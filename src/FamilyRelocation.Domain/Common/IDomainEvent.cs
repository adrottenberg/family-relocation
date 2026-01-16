namespace FamilyRelocation.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
