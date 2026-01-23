#!/bin/bash
set -e

# FamilyRelocation - Create AWS Secrets Script
# This script creates the required secrets in AWS Secrets Manager

AWS_REGION="${AWS_REGION:-us-east-1}"
ENVIRONMENT="${ENVIRONMENT:-dev}"

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

# Check AWS CLI
if ! command -v aws &> /dev/null; then
    log_error "AWS CLI is not installed"
    exit 1
fi

echo "=========================================="
echo "FamilyRelocation Secrets Setup"
echo "=========================================="
echo ""
echo "Environment: $ENVIRONMENT"
echo "Region: $AWS_REGION"
echo ""

# Prompt for values
read -p "RDS Host (e.g., xxx.xxx.us-east-1.rds.amazonaws.com): " RDS_HOST
read -p "RDS Database Name [familyrelocation]: " RDS_DATABASE
RDS_DATABASE=${RDS_DATABASE:-familyrelocation}
read -p "RDS Username [admin]: " RDS_USERNAME
RDS_USERNAME=${RDS_USERNAME:-admin}
read -s -p "RDS Password: " RDS_PASSWORD
echo ""

read -p "Cognito User Pool ID (e.g., us-east-1_xxxxxx): " COGNITO_POOL_ID
read -p "Cognito Client ID: " COGNITO_CLIENT_ID

read -p "S3 Documents Bucket Name [familyrelocation-${ENVIRONMENT}-documents]: " S3_BUCKET
S3_BUCKET=${S3_BUCKET:-familyrelocation-${ENVIRONMENT}-documents}

# Build connection string
CONNECTION_STRING="Host=${RDS_HOST};Database=${RDS_DATABASE};Username=${RDS_USERNAME};Password=${RDS_PASSWORD}"

echo ""
log_info "Creating secrets..."

# Create database secret
log_info "Creating database secret..."
aws secretsmanager create-secret \
    --name "familyrelocation/${ENVIRONMENT}/database" \
    --description "FamilyRelocation ${ENVIRONMENT} database connection string" \
    --secret-string "{\"ConnectionStrings__DefaultConnection\":\"${CONNECTION_STRING}\"}" \
    --region $AWS_REGION 2>/dev/null || \
aws secretsmanager update-secret \
    --secret-id "familyrelocation/${ENVIRONMENT}/database" \
    --secret-string "{\"ConnectionStrings__DefaultConnection\":\"${CONNECTION_STRING}\"}" \
    --region $AWS_REGION

# Create Cognito secret
log_info "Creating Cognito secret..."
aws secretsmanager create-secret \
    --name "familyrelocation/${ENVIRONMENT}/cognito" \
    --description "FamilyRelocation ${ENVIRONMENT} Cognito configuration" \
    --secret-string "{\"UserPoolId\":\"${COGNITO_POOL_ID}\",\"ClientId\":\"${COGNITO_CLIENT_ID}\"}" \
    --region $AWS_REGION 2>/dev/null || \
aws secretsmanager update-secret \
    --secret-id "familyrelocation/${ENVIRONMENT}/cognito" \
    --secret-string "{\"UserPoolId\":\"${COGNITO_POOL_ID}\",\"ClientId\":\"${COGNITO_CLIENT_ID}\"}" \
    --region $AWS_REGION

# Create S3 secret
log_info "Creating S3 secret..."
aws secretsmanager create-secret \
    --name "familyrelocation/${ENVIRONMENT}/s3" \
    --description "FamilyRelocation ${ENVIRONMENT} S3 configuration" \
    --secret-string "{\"BucketName\":\"${S3_BUCKET}\"}" \
    --region $AWS_REGION 2>/dev/null || \
aws secretsmanager update-secret \
    --secret-id "familyrelocation/${ENVIRONMENT}/s3" \
    --secret-string "{\"BucketName\":\"${S3_BUCKET}\"}" \
    --region $AWS_REGION

log_info "All secrets created successfully!"
echo ""
echo "Secrets created:"
echo "  - familyrelocation/${ENVIRONMENT}/database"
echo "  - familyrelocation/${ENVIRONMENT}/cognito"
echo "  - familyrelocation/${ENVIRONMENT}/s3"
echo ""
echo "You can verify with:"
echo "  aws secretsmanager list-secrets --filters Key=name,Values=familyrelocation/${ENVIRONMENT}"
