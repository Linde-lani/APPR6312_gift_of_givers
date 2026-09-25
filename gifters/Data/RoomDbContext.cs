using System;

namespace gifters.Data;

/// <summary>
/// Placeholder type retained to avoid compilation errors when EF Core is not used.
/// The project now uses <see cref="IRoomDb"/>/ <see cref="RoomDb"/> (file-backed) for storage.
/// Remove this file when all references to <c>RoomDbContext</c> have been updated.
/// </summary>
[Obsolete("RoomDbContext is no longer used. Use IRoomDb/RoomDb instead.")]
public sealed class RoomDbContext
{
}