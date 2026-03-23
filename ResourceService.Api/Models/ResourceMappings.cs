using ResourceService.Api.Domain;

namespace ResourceService.Api.Models;

public static class ResourceMappings
{
    public static ResourceResponse ToResponse(this Resource resource)
    {
        return new ResourceResponse
        {
            Id = resource.Id,
            OrganizationId = resource.OrganizationId,
            Name = resource.Name,
            Type = resource.Type.ToApiValue(),
            Status = resource.Status.ToApiValue(),
            OfficeAddress = resource.OfficeAddress,
            Floor = resource.Floor,
            Description = resource.Description,
            BookingRules = new BookingRulesResponse
            {
                MaxDurationHours = resource.MaxDurationHours,
                AllowedRoles = resource.AllowedRoles
            }
        };
    }

    public static string ToApiValue(this ResourceType type)
    {
        return type switch
        {
            ResourceType.MeetingRoom => "meeting_room",
            ResourceType.Workplace => "workplace",
            ResourceType.Equipment => "equipment",
            ResourceType.Office => "office",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static string ToApiValue(this ResourceStatus status)
    {
        return status switch
        {
            ResourceStatus.Available => "available",
            ResourceStatus.TemporarilyUnavailable => "temporarily_unavailable",
            ResourceStatus.OutOfService => "out_of_service",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static bool TryParseType(string? value, out ResourceType type)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "meeting_room":
                type = ResourceType.MeetingRoom;
                return true;
            case "workplace":
                type = ResourceType.Workplace;
                return true;
            case "equipment":
                type = ResourceType.Equipment;
                return true;
            case "office":
                type = ResourceType.Office;
                return true;
            default:
                type = default;
                return false;
        }
    }

    public static bool TryParseStatus(string? value, out ResourceStatus status)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "available":
                status = ResourceStatus.Available;
                return true;
            case "temporarily_unavailable":
                status = ResourceStatus.TemporarilyUnavailable;
                return true;
            case "out_of_service":
                status = ResourceStatus.OutOfService;
                return true;
            default:
                status = default;
                return false;
        }
    }
}
