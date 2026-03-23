namespace ResourceService.Api.Models;

public sealed class ResourceResponse
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Status { get; init; }
    public string? OfficeAddress { get; init; }
    public int? Floor { get; init; }
    public string? Description { get; init; }
    public required BookingRulesResponse BookingRules { get; init; }
}

public sealed class BookingRulesResponse
{
    public int MaxDurationHours { get; init; }
    public required string[] AllowedRoles { get; init; }
}
