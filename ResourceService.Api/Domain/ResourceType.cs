using System.Text.Json.Serialization;

namespace ResourceService.Api.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceType
{
    MeetingRoom = 1,
    Workplace = 2,
    Equipment = 3,
    Office = 4
}
