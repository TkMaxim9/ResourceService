using System.ComponentModel.DataAnnotations;

namespace ResourceService.Api.Models;

public sealed class BookingRulesDto
{
    [Range(1, 168)]
    public int MaxDurationHours { get; init; }

    [MinLength(1)]
    public required string[] AllowedRoles { get; init; }
}
