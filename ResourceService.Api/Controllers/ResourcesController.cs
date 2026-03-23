using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourceService.Api.Authorization;
using ResourceService.Api.Data;
using ResourceService.Api.Domain;
using ResourceService.Api.Models;

namespace ResourceService.Api.Controllers;

[ApiController]
[Route("api/resources")]
public sealed class ResourcesController : ControllerBase
{
    private readonly ResourceDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public ResourcesController(ResourceDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ResourceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _dbContext.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return resource is null ? NotFound() : Ok(resource.ToResponse());
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ResourceResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ResourceResponse>>> GetList(
        [FromQuery] Guid? organizationId,
        [FromQuery] string? status,
        [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Resources.AsNoTracking().AsQueryable();

        if (organizationId.HasValue)
        {
            query = query.Where(x => x.OrganizationId == organizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!ResourceMappings.TryParseStatus(status, out var parsedStatus))
            {
                return ValidationProblem($"Unsupported status '{status}'.");
            }

            query = query.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!ResourceMappings.TryParseType(type, out var parsedType))
            {
                return ValidationProblem($"Unsupported type '{type}'.");
            }

            query = query.Where(x => x.Type == parsedType);
        }

        var resources = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(resources.Select(x => x.ToResponse()).ToArray());
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType<ResourceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ResourceResponse>> Create(
        [FromBody] CreateResourceRequest request,
        CancellationToken cancellationToken)
    {
        var canCreate = await _permissionService.HasPermissionAsync(
            request.OrganizationId,
            PermissionCodes.ResourcesCreate,
            cancellationToken);

        if (!canCreate)
        {
            return Forbid();
        }

        if (!ResourceMappings.TryParseType(request.Type, out var type))
        {
            return ValidationProblem($"Unsupported type '{request.Type}'.");
        }

        if (!ResourceMappings.TryParseStatus(request.Status, out var status))
        {
            return ValidationProblem($"Unsupported status '{request.Status}'.");
        }

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            Name = request.Name.Trim(),
            Type = type,
            Status = status,
            OfficeAddress = request.OfficeAddress?.Trim(),
            Floor = request.Floor,
            Description = request.Description?.Trim(),
            MaxDurationHours = request.BookingRules.MaxDurationHours,
            AllowedRoles = request.BookingRules.AllowedRoles
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Resources.Add(resource);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource.ToResponse());
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ResourceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceResponse>> Update(
        Guid id,
        [FromBody] UpdateResourceRequest request,
        CancellationToken cancellationToken)
    {
        var resource = await _dbContext.Resources.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (resource is null)
        {
            return NotFound();
        }

        var canUpdate = await _permissionService.HasPermissionAsync(
            resource.OrganizationId,
            PermissionCodes.ResourcesUpdate,
            cancellationToken);

        if (!canUpdate)
        {
            return Forbid();
        }

        var canManageRules = await _permissionService.HasPermissionAsync(
            resource.OrganizationId,
            PermissionCodes.ResourcesRulesManage,
            cancellationToken);

        if (!canManageRules && request.BookingRules is not null)
        {
            return Forbid();
        }

        if (!ResourceMappings.TryParseType(request.Type, out var type))
        {
            return ValidationProblem($"Unsupported type '{request.Type}'.");
        }

        resource.Name = request.Name.Trim();
        resource.Type = type;
        resource.OfficeAddress = request.OfficeAddress?.Trim();
        resource.Floor = request.Floor;
        resource.Description = request.Description?.Trim();
        resource.UpdatedAtUtc = DateTime.UtcNow;

        if (request.BookingRules is not null)
        {
            resource.MaxDurationHours = request.BookingRules.MaxDurationHours;
            resource.AllowedRoles = request.BookingRules.AllowedRoles
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(resource.ToResponse());
    }

    [Authorize]
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<ResourceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateResourceStatusRequest request,
        CancellationToken cancellationToken)
    {
        var resource = await _dbContext.Resources.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (resource is null)
        {
            return NotFound();
        }

        var canChangeStatus = await _permissionService.HasPermissionAsync(
            resource.OrganizationId,
            PermissionCodes.ResourcesStatusChange,
            cancellationToken);

        if (!canChangeStatus)
        {
            return Forbid();
        }

        if (!ResourceMappings.TryParseStatus(request.Status, out var status))
        {
            return ValidationProblem($"Unsupported status '{request.Status}'.");
        }

        resource.Status = status;
        resource.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(resource.ToResponse());
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _dbContext.Resources.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (resource is null)
        {
            return NotFound();
        }

        var canDelete = await _permissionService.HasPermissionAsync(
            resource.OrganizationId,
            PermissionCodes.ResourcesDelete,
            cancellationToken);

        if (!canDelete)
        {
            return Forbid();
        }

        _dbContext.Resources.Remove(resource);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
