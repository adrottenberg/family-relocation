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

    public CognitoAuthenticationService(
        IAmazonCognitoIdentityProvider cognitoClient,
        IConfiguration configuration)
    {
        _cognitoClient = cognitoClient;
        _clientId = configuration["AWS:Cognito:ClientId"]
            ?? throw new InvalidOperationException("AWS:Cognito:ClientId configuration is required");
        _clientSecret = configuration["AWS:Cognito:ClientSecret"];
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

    private static AuthTokens MapToAuthTokens(AuthenticationResultType result) => new()
    {
        AccessToken = result.AccessToken,
        IdToken = result.IdToken,
        RefreshToken = result.RefreshToken,
        ExpiresIn = result.ExpiresIn ?? 3600
    };
}
