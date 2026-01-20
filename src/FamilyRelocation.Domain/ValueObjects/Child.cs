namespace FamilyRelocation.Domain.ValueObjects;

public enum Gender
{
    Male,
    Female
}

/// <summary>
/// Child value object representing a child in the family
/// Only Age and Gender are required (from the application form)
/// </summary>
public sealed record Child
{
    public int Age { get; init; }
    public Gender Gender { get; init; }
    public string? Name { get; init; }
    public string? School { get; init; }

    // Private parameterless constructor for EF Core/JSON deserialization
    private Child()
    {
        Age = 0;
        Gender = Gender.Male;
    }

    public Child(int age, Gender gender, string? name = null, string? school = null)
    {
        if (age < 0 || age > 50)
            throw new ArgumentException("Age must be between 0 and 50", nameof(age));

        Age = age;
        Gender = gender;
        Name = name?.Trim();
        School = school?.Trim();
    }

    public bool IsSchoolAge => Age >= 5 && Age <= 18;

    public override string ToString() =>
        string.IsNullOrEmpty(Name)
            ? $"{Gender}, Age {Age}"
            : $"{Name} ({Age}, {Gender})";
}
