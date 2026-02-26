using System.Security.Cryptography;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using FamilyRelocation.Application.Auth;
using FamilyRelocation.Application.Auth.Models;
using FamilyRelocation.Infrastructure.AWS.Helpers;
using Microsoft.Extensions.Configuration;

namespace FamilyRelocation.Infrastructure.AWS;

public class CognitoAuthenticationService : IAuthenticationService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly string _clientId;
    private readonly string? _clientSecret;
    private readonly string _userPoolId;

    public CognitoAuthenticationService(
        IAmazonCognitoIdentityProvider cognitoClient,
        IConfiguration configuration)
    {
        _cognitoClient = cognitoClient;
        _clientId = configuration["AWS:Cognito:ClientId"]
            ?? throw new InvalidOperationException("AWS:Cognito:ClientId configuration is required");
        _clientSecret = configuration["AWS:Cognito:ClientSecret"];
        _userPoolId = configuration["AWS:Cognito:UserPoolId"]
            ?? throw new InvalidOperationException("AWS:Cognito:UserPoolId configuration is required");
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var authParameters = new Dictionary<string, string>
            {
                { "USERNAME", email },
                { "PASSWORD", password }
            };

            AddSecretHashIfRequired(email, authParameters);

            var request = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                ClientId = _clientId,
                AuthParameters = authParameters
            };

            var response = await _cognitoClient.InitiateAuthAsync(request);

            if (!string.IsNullOrEmpty(response.ChallengeName))
            {
                var challengeInfo = ChallengeMetadata.GetInfo(response.ChallengeName);
                return AuthResult.ChallengeResult(new ChallengeInfo
                {
                    ChallengeName = response.ChallengeName,
                    Session = response.Session,
                    Message = challengeInfo.Message,
                    RequiredFields = challengeInfo.RequiredFields
                });
            }

            return AuthResult.SuccessResult(MapToAuthTokens(response.AuthenticationResult));
        }
        catch (NotAuthorizedException)
        {
            return AuthResult.ErrorResult("Invalid email or password", AuthErrorType.InvalidCredentials);
        }
        catch (UserNotFoundException)
        {
            return AuthResult.ErrorResult("Invalid email or password", AuthErrorType.InvalidCredentials);
        }
        catch (UserNotConfirmedException)
        {
            return AuthResult.ErrorResult(
                "Email not verified. Use the resend-confirmation endpoint to receive a new code.",
                AuthErrorType.UserNotConfirmed);
        }
        catch (PasswordResetRequiredException)
        {
            return AuthResult.ErrorResult(
                "Password reset is required. Use the forgot-password endpoint to initiate reset.",
                AuthErrorType.PasswordResetRequired);
        }
    }

    public async Task<AuthResult> RespondToChallengeAsync(ChallengeResponseRequest request)
    {
        try
        {
            var cognitoResponses = ChallengeMetadata.MapResponsesToCognito(
                request.ChallengeName,
                request.Responses);

            cognitoResponses["USERNAME"] = request.Email;
            AddSecretHashIfRequired(request.Email, cognitoResponses);

            var cognitoRequest = new RespondToAuthChallengeRequest
            {
                ClientId = _clientId,
                ChallengeName = request.ChallengeName,
                Session = request.Session,
                ChallengeResponses = cognitoResponses
            };

            var response = await _cognitoClient.RespondToAuthChallengeAsync(cognitoRequest);

            if (!string.IsNullOrEmpty(response.ChallengeName))
            {
                var challengeInfo = ChallengeMetadata.GetInfo(response.ChallengeName);
                return AuthResult.ChallengeResult(new ChallengeInfo
                {
                    ChallengeName = response.ChallengeName,
                    Session = response.Session,
                    Message = challengeInfo.Message,
                    RequiredFields = challengeInfo.RequiredFields
                });
            }

            return AuthResult.SuccessResult(MapToAuthTokens(response.AuthenticationResult));
        }
        catch (InvalidPasswordException ex)
        {
            return AuthResult.ErrorResult(ex.Message, AuthErrorType.InvalidPassword);
        }
        catch (NotAuthorizedException)
        {
            return AuthResult.ErrorResult("Session expired. Please login again.", AuthErrorType.InvalidSession);
        }
        catch (InvalidParameterException)
        {
            return AuthResult.ErrorResult("Invalid session. Please login again.", AuthErrorType.InvalidSession);
        }
        catch (CodeMismatchException)
        {
            return AuthResult.ErrorResult("Invalid or expired session. Please login again.", AuthErrorType.InvalidSession);
        }
    }

    public async Task<TokenRefreshResult> RefreshTokensAsync(string username, string refreshToken)
    {
        try
        {
            var authParameters = new Dictionary<string, string>
            {
                { "REFRESH_TOKEN", refreshToken }
            };

            AddSecretHashIfRequired(username, authParameters);

            var request = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                ClientId = _clientId,
                AuthParameters = authParameters
            };

            var response = await _cognitoClient.InitiateAuthAsync(request);

            return TokenRefreshResult.SuccessResult(new AuthTokens
            {
                AccessToken = response.AuthenticationResult.AccessToken,
                IdToken = response.AuthenticationResult.IdToken,
                RefreshToken = null, // Refresh tokens are not rotated by default
                ExpiresIn = response.AuthenticationResult.ExpiresIn ?? 3600
            });
        }
        catch (NotAuthorizedException)
        {
            return TokenRefreshResult.ErrorResult("Invalid or expired refresh token", AuthErrorType.InvalidCredentials);
        }
    }

    public async Task<OperationResult> RequestPasswordResetAsync(string email)
    {
        try
        {
            var request = new ForgotPasswordRequest
            {
                ClientId = _clientId,
                Username = email
            };

            AddSecretHashIfRequired(email, request);

            await _cognitoClient.ForgotPasswordAsync(request);

            return OperationResult.SuccessResult(
                "If an account exists with this email, a password reset code has been sent.");
        }
        catch (UserNotFoundException)
        {
            // Return same message to prevent email enumeration
            return OperationResult.SuccessResult(
                "If an account exists with this email, a password reset code has been sent.");
        }
        catch (LimitExceededException)
        {
            return OperationResult.ErrorResult("Too many requests. Please try again later.", AuthErrorType.TooManyRequests);
        }
        catch (InvalidParameterException)
        {
            // User has no verified email/phone
            return OperationResult.SuccessResult(
                "If an account exists with this email, a password reset code has been sent.");
        }
        catch (NotAuthorizedException)
        {
            // User is in FORCE_CHANGE_PASSWORD state - must complete initial login first
            return OperationResult.ErrorResult(
                "Password reset is not available for this account. Please sign in with your temporary password to set a new password.",
                AuthErrorType.PasswordResetRequired);
        }
        catch (AmazonCognitoIdentityProviderException)
        {
            return OperationResult.ErrorResult(
                "An error occurred processing your request. Please try again later.",
                AuthErrorType.Unknown);
        }
    }

    public async Task<OperationResult> ConfirmPasswordResetAsync(string email, string code, string newPassword)
    {
        try
        {
            var request = new ConfirmForgotPasswordRequest
            {
                ClientId = _clientId,
                Username = email,
                ConfirmationCode = code,
                Password = newPassword
            };

            AddSecretHashIfRequired(email, request);

            await _cognitoClient.ConfirmForgotPasswordAsync(request);

            return OperationResult.SuccessResult("Password has been reset successfully. You can now log in.");
        }
        catch (CodeMismatchException)
        {
            return OperationResult.ErrorResult("Invalid verification code", AuthErrorType.InvalidCode);
        }
        catch (ExpiredCodeException)
        {
            return OperationResult.ErrorResult("Verification code has expired. Please request a new one.", AuthErrorType.ExpiredCode);
        }
        catch (InvalidPasswordException ex)
        {
            return OperationResult.ErrorResult(ex.Message, AuthErrorType.InvalidPassword);
        }
        catch (NotAuthorizedException)
        {
            return OperationResult.ErrorResult(
                "Password reset is not available for this account. Please sign in with your temporary password to set a new password.",
                AuthErrorType.PasswordResetRequired);
        }
        catch (AmazonCognitoIdentityProviderException)
        {
            return OperationResult.ErrorResult(
                "An error occurred processing your request. Please try again later.",
                AuthErrorType.Unknown);
        }
    }

    public async Task<OperationResult> ResendConfirmationCodeAsync(string email)
    {
        try
        {
            var request = new ResendConfirmationCodeRequest
            {
                ClientId = _clientId,
                Username = email
            };

            AddSecretHashIfRequired(email, request);

            await _cognitoClient.ResendConfirmationCodeAsync(request);

            return OperationResult.SuccessResult(
                "If an account exists with this email, a confirmation code has been sent.");
        }
        catch (UserNotFoundException)
        {
            // Return same message to prevent email enumeration
            return OperationResult.SuccessResult(
                "If an account exists with this email, a confirmation code has been sent.");
        }
        catch (InvalidParameterException)
        {
            // User is already confirmed
            return OperationResult.ErrorResult("This email is already verified.", AuthErrorType.Unknown);
        }
        catch (LimitExceededException)
        {
            return OperationResult.ErrorResult("Too many requests. Please try again later.", AuthErrorType.TooManyRequests);
        }
    }

    public async Task<OperationResult> ConfirmEmailAsync(string email, string code)
    {
        try
        {
            var request = new ConfirmSignUpRequest
            {
                ClientId = _clientId,
                Username = email,
                ConfirmationCode = code
            };

            AddSecretHashIfRequired(email, request);

            await _cognitoClient.ConfirmSignUpAsync(request);

            return OperationResult.SuccessResult("Email verified successfully. You can now log in.");
        }
        catch (CodeMismatchException)
        {
            return OperationResult.ErrorResult("Invalid verification code", AuthErrorType.InvalidCode);
        }
        catch (ExpiredCodeException)
        {
            return OperationResult.ErrorResult(
                "Verification code has expired. Use resend-confirmation to get a new one.",
                AuthErrorType.ExpiredCode);
        }
        catch (NotAuthorizedException)
        {
            return OperationResult.ErrorResult("This email is already verified.", AuthErrorType.Unknown);
        }
    }

    private void AddSecretHashIfRequired(string username, Dictionary<string, string> parameters)
    {
        if (!string.IsNullOrEmpty(_clientSecret))
        {
            parameters["SECRET_HASH"] = HmacHelper.ComputeSecretHash(username, _clientId, _clientSecret);
        }
    }

    private void AddSecretHashIfRequired(string username, ForgotPasswordRequest request)
    {
        if (!string.IsNullOrEmpty(_clientSecret))
        {
            request.SecretHash = HmacHelper.ComputeSecretHash(username, _clientId, _clientSecret);
        }
    }

    private void AddSecretHashIfRequired(string username, ConfirmForgotPasswordRequest request)
    {
        if (!string.IsNullOrEmpty(_clientSecret))
        {
            request.SecretHash = HmacHelper.ComputeSecretHash(username, _clientId, _clientSecret);
        }
    }

    private void AddSecretHashIfRequired(string username, ResendConfirmationCodeRequest request)
    {
        if (!string.IsNullOrEmpty(_clientSecret))
        {
            request.SecretHash = HmacHelper.ComputeSecretHash(username, _clientId, _clientSecret);
        }
    }

    private void AddSecretHashIfRequired(string username, ConfirmSignUpRequest request)
    {
        if (!string.IsNullOrEmpty(_clientSecret))
        {
            request.SecretHash = HmacHelper.ComputeSecretHash(username, _clientId, _clientSecret);
        }
    }

    public async Task<RegisterUserResult> RegisterUserAsync(string email, string? temporaryPassword = null)
    {
        try
        {
            // Generate a temporary password if not provided
            var tempPassword = temporaryPassword ?? GenerateTemporaryPassword();

            var request = new AdminCreateUserRequest
            {
                UserPoolId = _userPoolId,
                Username = email,
                TemporaryPassword = tempPassword,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = email },
                    new AttributeType { Name = "email_verified", Value = "true" }
                },
                // Suppress the welcome email since we'll handle communication ourselves
                MessageAction = MessageActionType.SUPPRESS
            };

            var response = await _cognitoClient.AdminCreateUserAsync(request);

            // Get the 'sub' attribute which is the unique Cognito user ID
            // This must match what appears in JWT tokens for role lookups to work
            var cognitoUserId = response.User.Attributes
                .FirstOrDefault(a => a.Name == "sub")?.Value
                ?? response.User.Username;

            return RegisterUserResult.SuccessResult(
                cognitoUserId,
                tempPassword,
                "User created successfully. They will need to change their password on first login.");
        }
        catch (UsernameExistsException)
        {
            return RegisterUserResult.ErrorResult("A user with this email already exists.", AuthErrorType.UserAlreadyExists);
        }
        catch (InvalidPasswordException ex)
        {
            return RegisterUserResult.ErrorResult(ex.Message, AuthErrorType.InvalidPassword);
        }
        catch (InvalidParameterException ex)
        {
            return RegisterUserResult.ErrorResult(ex.Message, AuthErrorType.Unknown);
        }
    }

    private static string GenerateTemporaryPassword()
    {
        // Generate a cryptographically secure password that meets Cognito requirements
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";

        var password = new char[12];

        // Ensure at least one of each required character type
        password[0] = GetSecureRandomChar(upper);
        password[1] = GetSecureRandomChar(lower);
        password[2] = GetSecureRandomChar(digits);
        password[3] = GetSecureRandomChar(special);

        // Fill the rest randomly
        var allChars = upper + lower + digits + special;
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = GetSecureRandomChar(allChars);
        }

        // Shuffle the password using cryptographically secure random
        return new string(password.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }

    private static char GetSecureRandomChar(string chars)
    {
        return chars[RandomNumberGenerator.GetInt32(chars.Length)];
    }

    private static AuthTokens MapToAuthTokens(AuthenticationResultType result) => new()
    {
        AccessToken = result.AccessToken,
        IdToken = result.IdToken,
        RefreshToken = result.RefreshToken,
        ExpiresIn = result.ExpiresIn ?? 3600
    };

    public async Task<UserListResult> ListUsersAsync(
        string? filter = null,
        int limit = 60,
        string? paginationToken = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListUsersRequest
            {
                UserPoolId = _userPoolId,
                Limit = Math.Min(limit, 60),
                PaginationToken = paginationToken
            };

            if (!string.IsNullOrEmpty(filter))
            {
                request.Filter = filter;
            }

            var response = await _cognitoClient.ListUsersAsync(request, cancellationToken);

            var users = new List<UserDto>();
            foreach (var user in response.Users)
            {
                var groups = await GetUserGroupsAsync(user.Username, cancellationToken);
                users.Add(MapToUserDto(user, groups));
            }

            return UserListResult.SuccessResult(users, response.PaginationToken);
        }
        catch (Exception ex)
        {
            return UserListResult.ErrorResult($"Failed to list users: {ex.Message}", AuthErrorType.Unknown);
        }
    }

    public async Task<GetUserResult> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AdminGetUserRequest
            {
                UserPoolId = _userPoolId,
                Username = userId
            };

            var response = await _cognitoClient.AdminGetUserAsync(request, cancellationToken);
            var groups = await GetUserGroupsAsync(userId, cancellationToken);

            var user = new UserDto
            {
                Id = GetAttributeValue(response.UserAttributes, "sub") ?? userId,
                Email = GetAttributeValue(response.UserAttributes, "email") ?? string.Empty,
                Name = GetAttributeValue(response.UserAttributes, "name"),
                Roles = groups,
                Status = response.UserStatus.ToString(),
                EmailVerified = GetAttributeValue(response.UserAttributes, "email_verified") == "true",
                MfaEnabled = response.MFAOptions?.Any() == true ||
                             response.UserMFASettingList?.Any() == true,
                CreatedAt = response.UserCreateDate ?? DateTime.UtcNow,
                LastLogin = response.UserLastModifiedDate
            };

            return GetUserResult.SuccessResult(user);
        }
        catch (UserNotFoundException)
        {
            return GetUserResult.ErrorResult("User not found", AuthErrorType.UserNotFound);
        }
        catch (Exception ex)
        {
            return GetUserResult.ErrorResult($"Failed to get user: {ex.Message}", AuthErrorType.Unknown);
        }
    }

    public async Task<List<string>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AdminListGroupsForUserRequest
            {
                UserPoolId = _userPoolId,
                Username = userId
            };

            var response = await _cognitoClient.AdminListGroupsForUserAsync(request, cancellationToken);
            return response.Groups.Select(g => g.GroupName).ToList();
        }
        catch (UserNotFoundException)
        {
            // User doesn't exist in this pool - return empty (expected for mismatched pools)
            return new List<string>();
        }
        catch (Exception ex)
        {
            // Log unexpected errors but don't break the user list
            Console.WriteLine($"[CognitoAuth] Failed to get groups for user {userId}: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<OperationResult> UpdateUserRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetRoles = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var currentRoles = await GetUserGroupsAsync(userId, cancellationToken);
            var currentRolesSet = currentRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Roles to add
            var rolesToAdd = targetRoles.Except(currentRolesSet, StringComparer.OrdinalIgnoreCase);
            foreach (var role in rolesToAdd)
            {
                await _cognitoClient.AdminAddUserToGroupAsync(new AdminAddUserToGroupRequest
                {
                    UserPoolId = _userPoolId,
                    Username = userId,
                    GroupName = role
                }, cancellationToken);
            }

            // Roles to remove
            var rolesToRemove = currentRolesSet.Except(targetRoles, StringComparer.OrdinalIgnoreCase);
            foreach (var role in rolesToRemove)
            {
                await _cognitoClient.AdminRemoveUserFromGroupAsync(new AdminRemoveUserFromGroupRequest
                {
                    UserPoolId = _userPoolId,
                    Username = userId,
                    GroupName = role
                }, cancellationToken);
            }

            return OperationResult.SuccessResult("User roles updated successfully");
        }
        catch (UserNotFoundException)
        {
            return OperationResult.ErrorResult("User not found", AuthErrorType.UserNotFound);
        }
        catch (ResourceNotFoundException)
        {
            return OperationResult.ErrorResult("One or more specified roles do not exist", AuthErrorType.Unknown);
        }
        catch (Exception ex)
        {
            return OperationResult.ErrorResult($"Failed to update user roles: {ex.Message}", AuthErrorType.Unknown);
        }
    }

    public async Task<OperationResult> DisableUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cognitoClient.AdminDisableUserAsync(new AdminDisableUserRequest
            {
                UserPoolId = _userPoolId,
                Username = userId
            }, cancellationToken);

            return OperationResult.SuccessResult("User disabled successfully");
        }
        catch (UserNotFoundException)
        {
            return OperationResult.ErrorResult("User not found", AuthErrorType.UserNotFound);
        }
        catch (Exception ex)
        {
            return OperationResult.ErrorResult($"Failed to disable user: {ex.Message}", AuthErrorType.Unknown);
        }
    }

    public async Task<OperationResult> EnableUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cognitoClient.AdminEnableUserAsync(new AdminEnableUserRequest
            {
                UserPoolId = _userPoolId,
                Username = userId
            }, cancellationToken);

            return OperationResult.SuccessResult("User enabled successfully");
        }
        catch (UserNotFoundException)
        {
            return OperationResult.ErrorResult("User not found", AuthErrorType.UserNotFound);
        }
        catch (Exception ex)
        {
            return OperationResult.ErrorResult($"Failed to enable user: {ex.Message}", AuthErrorType.Unknown);
        }
    }

    private static UserDto MapToUserDto(UserType user, List<string> groups) => new()
    {
        Id = GetAttributeValue(user.Attributes, "sub") ?? user.Username,
        Email = GetAttributeValue(user.Attributes, "email") ?? string.Empty,
        Name = GetAttributeValue(user.Attributes, "name"),
        Roles = groups,
        Status = user.UserStatus.ToString(),
        EmailVerified = GetAttributeValue(user.Attributes, "email_verified") == "true",
        MfaEnabled = user.MFAOptions?.Any() == true,
        CreatedAt = user.UserCreateDate ?? DateTime.UtcNow,
        LastLogin = user.UserLastModifiedDate
    };

    private static string? GetAttributeValue(List<AttributeType> attributes, string name)
    {
        return attributes.FirstOrDefault(a => a.Name == name)?.Value;
    }
}
