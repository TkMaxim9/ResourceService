using ResourceService.Api.Integrations;

namespace ResourceService.Api.Authorization;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid organizationId, string permissionCode, CancellationToken cancellationToken);
}

public sealed class PermissionService : IPermissionService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IOrgMembershipClient _orgMembershipClient;

    public PermissionService(
        ICurrentUserAccessor currentUserAccessor,
        IOrgMembershipClient orgMembershipClient)
    {
        _currentUserAccessor = currentUserAccessor;
        _orgMembershipClient = orgMembershipClient;
    }

    public Task<bool> HasPermissionAsync(
        Guid organizationId,
        string permissionCode,
        CancellationToken cancellationToken)
    {
        var identityId = _currentUserAccessor.GetRequiredIdentityId();
        return _orgMembershipClient.CheckPermissionAsync(
            identityId,
            organizationId,
            permissionCode,
            cancellationToken);
    }
}
