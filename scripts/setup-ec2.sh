#!/bin/bash
set -e

# FamilyRelocation EC2 Setup Script
# Run this script on a fresh Amazon Linux 2023 EC2 instance

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if running as root or with sudo
if [ "$EUID" -ne 0 ]; then
    log_error "Please run this script with sudo"
    exit 1
fi

log_info "Starting EC2 setup for FamilyRelocation..."

# Update system packages
log_info "Updating system packages..."
dnf update -y

# Install Docker
log_info "Installing Docker..."
dnf install -y docker
systemctl enable docker
systemctl start docker

# Add ec2-user to docker group
usermod -aG docker ec2-user

# Install additional utilities
log_info "Installing utilities..."
dnf install -y jq htop git

# Install nginx for reverse proxy
log_info "Installing nginx..."
dnf install -y nginx
systemctl enable nginx

# Configure nginx
log_info "Configuring nginx..."
cat > /etc/nginx/conf.d/familyrelocation.conf << 'NGINX'
# FamilyRelocation API Reverse Proxy
server {
    listen 80;
    server_name _;

    # Increase max body size for file uploads
    client_max_body_size 50M;

    # API endpoints
    location /api {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
    }

    # Health check endpoint
    location /health {
        proxy_pass http://localhost:8080/health;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
    }

    # Default - return 404
    location / {
        return 404;
    }
}
NGINX

# Remove default nginx config
rm -f /etc/nginx/conf.d/default.conf 2>/dev/null || true

# Test nginx configuration
nginx -t

# Start nginx
systemctl start nginx

# Configure CloudWatch agent for logging (optional)
log_info "Setting up CloudWatch logging..."
cat > /opt/aws/amazon-cloudwatch-agent/etc/amazon-cloudwatch-agent.json << 'CLOUDWATCH'
{
    "logs": {
        "logs_collected": {
            "files": {
                "collect_list": [
                    {
                        "file_path": "/var/log/messages",
                        "log_group_name": "/familyrelocation/dev/system",
                        "log_stream_name": "{instance_id}/messages"
                    },
                    {
                        "file_path": "/var/log/nginx/access.log",
                        "log_group_name": "/familyrelocation/dev/nginx",
                        "log_stream_name": "{instance_id}/access"
                    },
                    {
                        "file_path": "/var/log/nginx/error.log",
                        "log_group_name": "/familyrelocation/dev/nginx",
                        "log_stream_name": "{instance_id}/error"
                    }
                ]
            }
        }
    }
}
CLOUDWATCH

# Create helper scripts
log_info "Creating helper scripts..."

# Script to view API logs
cat > /usr/local/bin/api-logs << 'SCRIPT'
#!/bin/bash
docker logs -f familyrelocation-api
SCRIPT
chmod +x /usr/local/bin/api-logs

# Script to restart API
cat > /usr/local/bin/api-restart << 'SCRIPT'
#!/bin/bash
docker restart familyrelocation-api
SCRIPT
chmod +x /usr/local/bin/api-restart

# Script to check API health
cat > /usr/local/bin/api-health << 'SCRIPT'
#!/bin/bash
curl -s http://localhost:8080/health | jq .
SCRIPT
chmod +x /usr/local/bin/api-health

# Script to pull and restart API
cat > /usr/local/bin/api-update << 'SCRIPT'
#!/bin/bash
set -e

AWS_REGION="${AWS_REGION:-us-east-1}"
ECR_URI="$1"

if [ -z "$ECR_URI" ]; then
    echo "Usage: api-update <ecr-uri>"
    exit 1
fi

echo "Logging into ECR..."
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $(echo $ECR_URI | cut -d'/' -f1)

echo "Pulling latest image..."
docker pull $ECR_URI:latest

echo "Stopping current container..."
docker stop familyrelocation-api || true
docker rm familyrelocation-api || true

echo "Getting secrets..."
DB_CONNECTION=$(aws secretsmanager get-secret-value --secret-id familyrelocation/dev/database --query SecretString --output text | jq -r '.ConnectionStrings__DefaultConnection')
COGNITO_POOL_ID=$(aws secretsmanager get-secret-value --secret-id familyrelocation/dev/cognito --query SecretString --output text | jq -r '.UserPoolId')
COGNITO_CLIENT_ID=$(aws secretsmanager get-secret-value --secret-id familyrelocation/dev/cognito --query SecretString --output text | jq -r '.ClientId')
S3_BUCKET=$(aws secretsmanager get-secret-value --secret-id familyrelocation/dev/s3 --query SecretString --output text | jq -r '.BucketName')

echo "Starting new container..."
docker run -d \
    --name familyrelocation-api \
    --restart unless-stopped \
    -p 8080:8080 \
    -e "ConnectionStrings__DefaultConnection=$DB_CONNECTION" \
    -e "AWS__CognitoUserPoolId=$COGNITO_POOL_ID" \
    -e "AWS__CognitoClientId=$COGNITO_CLIENT_ID" \
    -e "AWS__S3BucketName=$S3_BUCKET" \
    -e "AWS__Region=$AWS_REGION" \
    -e "ASPNETCORE_ENVIRONMENT=Production" \
    $ECR_URI:latest

echo "Cleaning up old images..."
docker image prune -af

echo "Waiting for health check..."
sleep 10
curl -f http://localhost:8080/health && echo "API is healthy!" || echo "Health check failed!"
SCRIPT
chmod +x /usr/local/bin/api-update

# Set up automatic security updates
log_info "Enabling automatic security updates..."
dnf install -y dnf-automatic
sed -i 's/apply_updates = no/apply_updates = yes/' /etc/dnf/automatic.conf
systemctl enable dnf-automatic.timer
systemctl start dnf-automatic.timer

# Create swap file for t3.micro (1GB RAM)
log_info "Creating swap file..."
if [ ! -f /swapfile ]; then
    dd if=/dev/zero of=/swapfile bs=128M count=16
    chmod 600 /swapfile
    mkswap /swapfile
    swapon /swapfile
    echo '/swapfile swap swap defaults 0 0' >> /etc/fstab
fi

# Print summary
log_info "Setup complete!"
echo ""
echo "=========================================="
echo "EC2 Setup Summary"
echo "=========================================="
echo ""
echo "Installed:"
echo "  - Docker"
echo "  - nginx (reverse proxy)"
echo "  - jq, htop, git"
echo ""
echo "Helper commands:"
echo "  - api-logs      : View API container logs"
echo "  - api-restart   : Restart API container"
echo "  - api-health    : Check API health"
echo "  - api-update    : Pull and restart API"
echo ""
echo "Nginx config: /etc/nginx/conf.d/familyrelocation.conf"
echo ""
echo "Next steps:"
echo "  1. Create secrets in AWS Secrets Manager"
echo "  2. Push Docker image to ECR"
echo "  3. Run: api-update <ecr-uri>"
echo ""
echo "Don't forget to log out and back in for docker group to take effect!"
