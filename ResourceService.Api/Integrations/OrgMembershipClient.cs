using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ResourceService.Api.Integrations;

public interface IOrgMembershipClient
{
    Task<bool> CheckPermissionAsync(
        string identityId,
        Guid organizationId,
        string permissionCode,
        CancellationToken cancellationToken);
}

public sealed class OrgMembershipClient : IOrgMembershipClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly OrgMembershipOptions _options;

    public OrgMembershipClient(HttpClient httpClient, IOptions<OrgMembershipOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<bool> CheckPermissionAsync(
        string identityId,
        Guid organizationId,
        string permissionCode,
        CancellationToken cancellationToken)
    {
        var request = new PermissionCheckRequest(identityId, organizationId, permissionCode);
        var response = await _httpClient.PostAsJsonAsync(
            $"{_options.BaseUrl.TrimEnd('/')}/api/internal/authorization/check",
            request,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PermissionCheckResponse>(
            JsonOptions,
            cancellationToken);

        return payload?.Allowed ?? false;
    }
}

public sealed class OrgMembershipOptions
{
    public const string SectionName = "OrgMembershipService";

    public string BaseUrl { get; set; } = string.Empty;
}

public sealed record PermissionCheckRequest(string IdentityId, Guid OrganizationId, string PermissionCode);

public sealed record PermissionCheckResponse(bool Allowed);
