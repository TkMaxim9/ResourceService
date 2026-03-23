using System.ComponentModel.DataAnnotations;
using ResourceService.Api.Domain;

namespace ResourceService.Api.Models;

public sealed class UpdateResourceStatusRequest
{
    [Required]
    public required string Status { get; init; }
}
