namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Email delivery and engagement tracking
/// NEW: For email blast system (Correction #14)
/// </summary>
public enum EmailDeliveryStatus
{
    Pending,
    Sent,
    Delivered,
    Opened,
    Clicked,
    Bounced,
    /// <summary>Spam complaint</summary>
    Complained
}
