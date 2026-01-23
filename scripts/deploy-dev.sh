#!/bin/bash
set -e

# FamilyRelocation Dev Deployment Script
# This script deploys both frontend and backend to the dev environment

# Configuration
AWS_REGION="${AWS_REGION:-us-east-1}"
ECR_REPOSITORY="${ECR_REPOSITORY:-familyrelocation-api-dev}"
S3_BUCKET="${S3_BUCKET:-dev-unionvaad-web}"
CLOUDFRONT_DISTRIBUTION_ID="${CLOUDFRONT_DISTRIBUTION_ID}"
EC2_HOST="${EC2_HOST}"
SSH_KEY_PATH="${SSH_KEY_PATH:-~/.ssh/familyrelocation-dev.pem}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Helper functions
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check required environment variables
check_requirements() {
    log_info "Checking requirements..."

    if [ -z "$CLOUDFRONT_DISTRIBUTION_ID" ]; then
        log_error "CLOUDFRONT_DISTRIBUTION_ID is not set"
        exit 1
    fi

    if [ -z "$EC2_HOST" ]; then
        log_error "EC2_HOST is not set"
        exit 1
    fi

    # Check AWS CLI is installed
    if ! command -v aws &> /dev/null; then
        log_error "AWS CLI is not installed"
        exit 1
    fi

    # Check Docker is installed
    if ! command -v docker &> /dev/null; then
        log_error "Docker is not installed"
        exit 1
    fi

    # Check Node.js is installed
    if ! command -v node &> /dev/null; then
        log_error "Node.js is not installed"
        exit 1
    fi

    log_info "All requirements met"
}

# Build and push Docker image
deploy_api() {
    log_info "Building API Docker image..."

    # Get ECR login
    aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $(aws sts get-caller-identity --query Account --output text).dkr.ecr.$AWS_REGION.amazonaws.com

    # Build image
    docker build -t $ECR_REPOSITORY:latest .

    # Tag for ECR
    ECR_URI=$(aws ecr describe-repositories --repository-names $ECR_REPOSITORY --region $AWS_REGION --query 'repositories[0].repositoryUri' --output text)
    docker tag $ECR_REPOSITORY:latest $ECR_URI:latest
    docker tag $ECR_REPOSITORY:latest $ECR_URI:$(git rev-parse --short HEAD)

    # Push to ECR
    log_info "Pushing image to ECR..."
    docker push $ECR_URI:latest
    docker push $ECR_URI:$(git rev-parse --short HEAD)

    # Deploy to EC2
    log_info "Deploying to EC2..."
    ssh -i $SSH_KEY_PATH -o StrictHostKeyChecking=no ec2-user@$EC2_HOST << 'ENDSSH'
        # Login to ECR
        aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $(aws sts get-caller-identity --query Account --output text).dkr.ecr.$AWS_REGION.amazonaws.com

        # Pull latest image
        docker pull $ECR_URI:latest

        # Stop existing container
        docker stop familyrelocation-api || true
        docker rm familyrelocation-api || true

        # Get secrets from Secrets Manager
        DB_CONNECTION=$(aws secretsmanager get-secret-value --secret-id familyrelocation/dev/database --query SecretString --output text | jq -r '.ConnectionStrings__DefaultConnection')
        COGNITO_POOL_ID=$(aws secretsmanager get-secret-value --secret-id familyrelocation/dev/cognito --query SecretString --output text | jq -r '.UserPoolId')
        COGNITO_CLIENT_ID=$(aws secretsmanager get-secret-value --secret-id familyrelocation/dev/cognito --query SecretString --output text | jq -r '.ClientId')
        S3_BUCKET_NAME=$(aws secretsmanager get-secret-value --secret-id familyrelocation/dev/s3 --query SecretString --output text | jq -r '.BucketName')

        # Run new container
        docker run -d \
            --name familyrelocation-api \
            --restart unless-stopped \
            -p 8080:8080 \
            -e "ConnectionStrings__DefaultConnection=$DB_CONNECTION" \
            -e "AWS__CognitoUserPoolId=$COGNITO_POOL_ID" \
            -e "AWS__CognitoClientId=$COGNITO_CLIENT_ID" \
            -e "AWS__S3BucketName=$S3_BUCKET_NAME" \
            -e "AWS__Region=$AWS_REGION" \
            -e "ASPNETCORE_ENVIRONMENT=Production" \
            $ECR_URI:latest

        # Clean up old images
        docker image prune -af

        # Health check
        sleep 10
        curl -f http://localhost:8080/health || exit 1
ENDSSH

    log_info "API deployed successfully"
}

# Build and deploy frontend
deploy_frontend() {
    log_info "Building frontend..."

    cd src/FamilyRelocation.Web

    # Install dependencies
    npm ci

    # Build with production API URL
    VITE_API_URL=https://dev.unionvaad.com/api npm run build

    # Sync to S3
    log_info "Uploading to S3..."
    aws s3 sync dist/ s3://$S3_BUCKET --delete

    # Invalidate CloudFront cache
    log_info "Invalidating CloudFront cache..."
    aws cloudfront create-invalidation \
        --distribution-id $CLOUDFRONT_DISTRIBUTION_ID \
        --paths "/*"

    cd ../..

    log_info "Frontend deployed successfully"
}

# Main deployment
main() {
    log_info "Starting FamilyRelocation dev deployment..."

    check_requirements

    # Parse arguments
    DEPLOY_API=false
    DEPLOY_FRONTEND=false

    while [[ $# -gt 0 ]]; do
        case $1 in
            --api)
                DEPLOY_API=true
                shift
                ;;
            --frontend)
                DEPLOY_FRONTEND=true
                shift
                ;;
            --all)
                DEPLOY_API=true
                DEPLOY_FRONTEND=true
                shift
                ;;
            *)
                log_error "Unknown argument: $1"
                echo "Usage: $0 [--api] [--frontend] [--all]"
                exit 1
                ;;
        esac
    done

    # Default to all if no specific flag
    if [ "$DEPLOY_API" = false ] && [ "$DEPLOY_FRONTEND" = false ]; then
        DEPLOY_API=true
        DEPLOY_FRONTEND=true
    fi

    if [ "$DEPLOY_API" = true ]; then
        deploy_api
    fi

    if [ "$DEPLOY_FRONTEND" = true ]; then
        deploy_frontend
    fi

    log_info "Deployment complete!"
    echo ""
    echo "Frontend: https://dev.unionvaad.com"
    echo "API: https://dev.unionvaad.com/api"
}

main "$@"
