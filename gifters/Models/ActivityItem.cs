using System;

namespace gifters.Models;

public class ActivityItem
{
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? Id { get; set; }
}