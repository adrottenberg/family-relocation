# FamilyRelocation Infrastructure

This directory contains AWS CloudFormation templates for deploying the FamilyRelocation application.

## Architecture Overview

```
                    ┌─────────────────┐
                    │   Route 53      │
                    │ dev.unionvaad.com│
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │   CloudFront    │
                    │  (SSL termination)│
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
     ┌────────▼────────┐    │     ┌────────▼────────┐
     │   S3 Bucket     │    │     │   EC2 t3.micro  │
     │  (React SPA)    │    │     │   (Docker API)  │
     │  /index.html    │    │     │   /api/*        │
     └─────────────────┘    │     └────────┬────────┘
                            │              │
                            │     ┌────────▼────────┐
                            │     │   RDS Postgres  │
                            │     │ (existing dev)  │
                            │     └─────────────────┘
                            │
                    ┌───────▼────────┐
                    │ Secrets Manager│
                    │  (DB password) │
                    └────────────────┘
```

## Prerequisites

Before deploying, you need:

1. **ACM Certificate** for `*.unionvaad.com` in `us-east-1` region
2. **Route 53 Hosted Zone** for `unionvaad.com`
3. **EC2 Key Pair** for SSH access
4. **Existing RDS PostgreSQL** instance
5. **VPC with public subnet**

## Deployment

### 1. Create ACM Certificate (if not exists)

```bash
# Request certificate in us-east-1 (required for CloudFront)
aws acm request-certificate \
  --domain-name "*.unionvaad.com" \
  --validation-method DNS \
  --region us-east-1
```

### 2. Create Secrets in Secrets Manager

```bash
# Database connection string
aws secretsmanager create-secret \
  --name "familyrelocation/dev/database" \
  --secret-string '{"ConnectionStrings__DefaultConnection":"Host=your-rds-endpoint;Database=familyrelocation;Username=admin;Password=yourpassword"}'

# Cognito configuration
aws secretsmanager create-secret \
  --name "familyrelocation/dev/cognito" \
  --secret-string '{"UserPoolId":"us-east-1_xxxxx","ClientId":"your-client-id"}'

# S3 bucket configuration
aws secretsmanager create-secret \
  --name "familyrelocation/dev/s3" \
  --secret-string '{"BucketName":"familyrelocation-dev-documents"}'
```

### 3. Update Parameters File

Edit `cloudformation/dev-parameters.json` with your actual values:

- `HostedZoneId`: Your Route 53 hosted zone ID
- `CertificateArn`: Your ACM certificate ARN
- `EC2KeyPairName`: Your EC2 key pair name
- `RDSSecurityGroupId`: Security group of your RDS instance
- `VpcId`: Your VPC ID
- `SubnetId`: A public subnet ID in your VPC

### 4. Deploy CloudFormation Stack

```bash
aws cloudformation create-stack \
  --stack-name familyrelocation-dev \
  --template-body file://cloudformation/dev-stack.yml \
  --parameters file://cloudformation/dev-parameters.json \
  --capabilities CAPABILITY_NAMED_IAM \
  --region us-east-1
```

Or update an existing stack:

```bash
aws cloudformation update-stack \
  --stack-name familyrelocation-dev \
  --template-body file://cloudformation/dev-stack.yml \
  --parameters file://cloudformation/dev-parameters.json \
  --capabilities CAPABILITY_NAMED_IAM \
  --region us-east-1
```

### 5. Configure RDS Security Group

Add the EC2 security group to your RDS security group's inbound rules:

```bash
# Get EC2 security group ID from stack outputs
EC2_SG=$(aws cloudformation describe-stacks \
  --stack-name familyrelocation-dev \
  --query "Stacks[0].Outputs[?OutputKey=='EC2SecurityGroupId'].OutputValue" \
  --output text)

# Add inbound rule to RDS security group
aws ec2 authorize-security-group-ingress \
  --group-id YOUR_RDS_SECURITY_GROUP_ID \
  --protocol tcp \
  --port 5432 \
  --source-group $EC2_SG
```

## GitHub Secrets Required

Configure these secrets in your GitHub repository:

| Secret Name | Description |
|-------------|-------------|
| `AWS_ACCESS_KEY_ID` | AWS access key with deployment permissions |
| `AWS_SECRET_ACCESS_KEY` | AWS secret access key |
| `EC2_HOST` | EC2 public IP or domain (from stack outputs) |
| `EC2_SSH_KEY` | Private key for SSH access to EC2 |
| `CLOUDFRONT_DISTRIBUTION_ID` | CloudFront distribution ID (from stack outputs) |

## Stack Outputs

After deployment, the stack provides these outputs:

- `ECRRepositoryUri`: URI for pushing Docker images
- `WebsiteBucketName`: S3 bucket for frontend assets
- `CloudFrontDistributionId`: For cache invalidation
- `EC2PublicIP`: For SSH access and GitHub Actions
- `WebsiteURL`: `https://dev.unionvaad.com`
- `APIURL`: `https://dev.unionvaad.com/api`

## Estimated Costs

- **EC2 t3.micro**: Free tier eligible (750 hours/month)
- **S3**: Free tier (5GB storage, 20,000 GET, 2,000 PUT)
- **CloudFront**: Free tier (1TB transfer, 10M requests)
- **Route 53**: ~$0.50/month (hosted zone)
- **Elastic IP**: Free when attached to running instance
- **ECR**: Free tier (500MB storage)

Total estimated cost: **~$0.50/month** + existing RDS costs

## Database Authentication

### Dev Environment
- Uses RDS-managed secret with rotation **disabled**
- Credentials are read at container startup and passed as environment variables

### Production (TODO)
Switch to **IAM Database Authentication** for production:
1. Enable IAM auth on RDS instance
2. Create IAM policy allowing `rds-db:connect`
3. Modify connection string to use IAM auth token
4. See: https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/UsingWithRDS.IAMDBAuth.html

Benefits:
- No passwords to manage or rotate
- Uses EC2 IAM role (already configured)
- Short-lived tokens (15 min) generated automatically

## Troubleshooting

### SSH to EC2

```bash
ssh -i your-key.pem ec2-user@<EC2_PUBLIC_IP>
```

### View Docker logs

```bash
docker logs familyrelocation-api
```

### Check nginx status

```bash
sudo systemctl status nginx
sudo nginx -t
```

### View application health

```bash
curl http://localhost:8080/health
```
