using System.ComponentModel.DataAnnotations;
using ResourceService.Api.Domain;

namespace ResourceService.Api.Models;

public sealed class CreateResourceRequest
{
    [Required]
    public Guid OrganizationId { get; init; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }

    [Required]
    public required string Type { get; init; }

    [Required]
    public string Status { get; init; } = "available";

    [MaxLength(300)]
    public string? OfficeAddress { get; init; }

    public int? Floor { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Required]
    public required BookingRulesDto BookingRules { get; init; }
}
