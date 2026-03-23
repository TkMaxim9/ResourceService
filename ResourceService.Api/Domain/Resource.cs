namespace ResourceService.Api.Domain;

public sealed class Resource
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public ResourceStatus Status { get; set; }
    public string? OfficeAddress { get; set; }
    public int? Floor { get; set; }
    public string? Description { get; set; }
    public int MaxDurationHours { get; set; }
    public string[] AllowedRoles { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
