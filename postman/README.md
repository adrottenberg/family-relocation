# FamilyRelocation Auth API - Postman Collection

This folder contains a Postman collection for testing the AWS Cognito authentication endpoints.

## Importing the Collection

### Step 1: Open Postman
Launch the Postman application on your computer.

### Step 2: Import the Collection
1. Click the **Import** button in the top-left corner of Postman
2. Select the **File** tab
3. Click **Upload Files**
4. Navigate to this folder and select `FamilyRelocation-Auth.postman_collection.json`
5. Click **Import**

The collection "FamilyRelocation Auth API" will appear in your Collections sidebar.

## Configuring Variables

Before running the tests, you need to set up the collection variables:

### Step 1: Open Collection Variables
1. Click on the collection name "FamilyRelocation Auth API" in the sidebar
2. Click the **Variables** tab

### Step 2: Set Required Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `baseUrl` | Your API base URL | `http://localhost:5000` |
| `testEmail` | Email of your test user | `testuser@example.com` |
| `testPassword` | Password for your test user | `TempPassword123!` |
| `newPassword` | New password (for challenges/resets) | `NewSecurePass123!` |

### Step 3: Save Variables
Click **Save** (Ctrl+S) after entering your values.

## Running the Tests

### Test Flow 1: New User (First Login)

When a user is created in AWS Cognito with a temporary password, they must change it on first login.

1. **Run "Login"**
   - This will return a `NEW_PASSWORD_REQUIRED` challenge
   - The session token is automatically saved to `challengeSession`

2. **Set `newPassword` variable**
   - Go to Variables tab and set `newPassword` to the desired new password
   - Must meet Cognito password policy (typically: 8+ chars, uppercase, lowercase, number, special char)

3. **Run "Respond to Challenge"**
   - This completes the password change
   - On success, tokens are automatically saved

### Test Flow 2: Existing User Login

1. **Run "Login"**
   - If successful, tokens are automatically saved to collection variables
   - `accessToken`, `idToken`, and `refreshToken` are populated

2. **Verify tokens are saved**
   - Check the Variables tab to confirm tokens are present

### Test Flow 3: Refresh Tokens

After a successful login:

1. **Run "Refresh Token"**
   - The pre-request script automatically extracts the Cognito username from the saved `idToken`
   - New `accessToken` and `idToken` are saved on success

### Test Flow 4: Password Reset

1. **Run "Forgot Password"**
   - Check the email inbox for a verification code

2. **Set variables**
   - Set `resetCode` to the code received via email
   - Set `newPassword` to the desired new password

3. **Run "Confirm Forgot Password"**
   - Password is reset on success

4. **Run "Login"** with new password

### Test Flow 5: Email Verification

For accounts that haven't verified their email:

1. **Run "Resend Confirmation"**
   - Check email inbox for verification code

2. **Set `confirmationCode` variable**
   - Enter the code received via email

3. **Run "Confirm Email"**
   - Email is verified on success

## Using Tokens for Authenticated Requests

After successful login, the `accessToken` is saved to collection variables. To use it in other requests:

### In Request Headers
Add an Authorization header:
```
Authorization: Bearer {{accessToken}}
```

### Creating Authenticated Requests
1. Create a new request in Postman
2. Go to the **Authorization** tab
3. Select **Bearer Token** as the type
4. Enter `{{accessToken}}` as the token value

## Automatic Test Scripts

The collection includes test scripts that automatically:
- Save tokens after successful login
- Save challenge session for NEW_PASSWORD_REQUIRED flow
- Extract Cognito username (sub) from idToken for refresh requests
- Validate responses and log helpful messages to the console

## Troubleshooting

### "Invalid email or password"
- Verify credentials are correct
- Check if the user exists in Cognito User Pool

### "Email not verified"
- Use "Resend Confirmation" then "Confirm Email" flow

### "Session expired" on Respond to Challenge
- Challenge sessions expire quickly (typically 3 minutes)
- Run "Login" again to get a fresh session

### "Invalid or expired refresh token"
- Refresh tokens have a longer lifespan but can expire
- Login again to get new tokens

### Token refresh fails
- Ensure you have a valid `idToken` saved (the username is extracted from it)
- Try logging in again to get fresh tokens

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Authenticate with email/password |
| POST | `/api/auth/respond-to-challenge` | Complete auth challenge (e.g., set new password) |
| POST | `/api/auth/refresh` | Refresh access tokens |
| POST | `/api/auth/forgot-password` | Request password reset code |
| POST | `/api/auth/confirm-forgot-password` | Complete password reset |
| POST | `/api/auth/resend-confirmation` | Resend email verification code |
| POST | `/api/auth/confirm-email` | Verify email address |
