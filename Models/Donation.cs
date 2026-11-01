using System;

namespace gifters.Models;

public class Donation
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public string DonationType { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public bool Anonymous { get; set; }

    // Who submitted this donation (user name)
    public string SubmittedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}