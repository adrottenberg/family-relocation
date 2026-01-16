using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Value object representing a board review decision
/// </summary>
public sealed record BoardReview
{
    public BoardDecision Decision { get; }
    public string? Notes { get; }
    public DateTime ReviewDate { get; }
    public Guid ReviewedByUserId { get; }

    // Private parameterless constructor for EF Core
    private BoardReview()
    {
        Decision = BoardDecision.Pending;
        ReviewDate = DateTime.UtcNow;
        ReviewedByUserId = Guid.Empty;
    }

    public BoardReview(
        BoardDecision decision,
        Guid reviewedByUserId,
        string? notes = null,
        DateTime? reviewDate = null)
    {
        if (reviewedByUserId == Guid.Empty)
            throw new ArgumentException("Reviewer user ID is required", nameof(reviewedByUserId));

        Decision = decision;
        ReviewedByUserId = reviewedByUserId;
        Notes = notes?.Trim();
        ReviewDate = reviewDate ?? DateTime.UtcNow;
    }

    public bool IsApproved => Decision == BoardDecision.Approved;
    public bool IsRejected => Decision == BoardDecision.Rejected;
    public bool IsPending => Decision == BoardDecision.Pending;
    public bool IsDeferred => Decision == BoardDecision.Deferred;

    public override string ToString() => $"{Decision} on {ReviewDate:d}";
}
