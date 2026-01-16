namespace FamilyRelocation.Domain.ValueObjects;

public enum Gender
{
    Male,
    Female
}

/// <summary>
/// Child value object representing family member information
/// </summary>
public sealed record Child
{
    public string Name { get; }
    public int Age { get; }
    public Gender Gender { get; }
    public string? School { get; }
    public string? Grade { get; }
    public string? Notes { get; }

    // Private parameterless constructor for EF Core
    private Child()
    {
        Name = string.Empty;
        Age = 0;
        Gender = Gender.Male;
    }

    public Child(string name, int age, Gender gender, string? school = null, string? grade = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Child name is required", nameof(name));

        if (age < 0 || age > 50)
            throw new ArgumentException("Age must be between 0 and 50", nameof(age));

        Name = name.Trim();
        Age = age;
        Gender = gender;
        School = school?.Trim();
        Grade = grade?.Trim();
        Notes = notes?.Trim();
    }

    public bool IsSchoolAge => Age >= 5 && Age <= 18;

    public override string ToString() => $"{Name} ({Age}, {Gender})";
}
