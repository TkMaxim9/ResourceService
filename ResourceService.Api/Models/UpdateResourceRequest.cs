using System.ComponentModel.DataAnnotations;
using ResourceService.Api.Domain;

namespace ResourceService.Api.Models;

public sealed class UpdateResourceRequest
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }

    [Required]
    public required string Type { get; init; }

    [MaxLength(300)]
    public string? OfficeAddress { get; init; }

    public int? Floor { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    public BookingRulesDto? BookingRules { get; init; }
}
