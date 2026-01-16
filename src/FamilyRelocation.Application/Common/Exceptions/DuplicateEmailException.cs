namespace FamilyRelocation.Application.Common.Exceptions;

public class DuplicateEmailException : Exception
{
    public string Email { get; }

    public DuplicateEmailException(string email)
        : base($"An applicant with email '{email}' already exists.")
    {
        Email = email;
    }
}
