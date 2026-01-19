namespace FamilyRelocation.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when attempting to create an applicant with an email that already exists.
/// </summary>
public class DuplicateEmailException : Exception
{
    /// <summary>
    /// The duplicate email address.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Initializes a new instance of the exception.
    /// </summary>
    /// <param name="email">The duplicate email address.</param>
    public DuplicateEmailException(string email)
        : base($"An applicant with email '{email}' already exists.")
    {
        Email = email;
    }
}
