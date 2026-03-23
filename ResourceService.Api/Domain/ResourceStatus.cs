using System.Text.Json.Serialization;

namespace ResourceService.Api.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceStatus
{
    Available = 1,
    TemporarilyUnavailable = 2,
    OutOfService = 3
}
