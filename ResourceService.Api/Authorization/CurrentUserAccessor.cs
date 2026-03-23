using System.Security.Claims;

namespace ResourceService.Api.Authorization;

public interface ICurrentUserAccessor
{
    string GetRequiredIdentityId();
}

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetRequiredIdentityId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var identityId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(identityId))
        {
            throw new InvalidOperationException("Authenticated user identityId was not found in JWT.");
        }

        return identityId;
    }
}
