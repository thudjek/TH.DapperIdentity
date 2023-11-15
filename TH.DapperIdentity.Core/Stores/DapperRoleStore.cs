using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TH.DapperIdentity.Core.Contracts;

namespace TH.DapperIdentity.Core.Stores;

public class DapperRoleStore<TRole, TKey, TUserRole, TRoleClaim> : RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
    where TRole : IdentityRole<TKey>, new()
    where TKey : IEquatable<TKey>
    where TUserRole : IdentityUserRole<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    private readonly IRoleRepository<TRole, TKey, TRoleClaim> _roleRepository;
    private readonly IRoleClaimRepository<TKey, TRoleClaim> _roleClaimRepository;

    public DapperRoleStore(
        IRoleRepository<TRole, TKey, TRoleClaim> roleRepository,
        IRoleClaimRepository<TKey, TRoleClaim> roleClaimRepository,
        IdentityErrorDescriber describer) : base(describer)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _roleClaimRepository = roleClaimRepository ?? throw new ArgumentNullException(nameof(roleClaimRepository));
    }

    private List<TRoleClaim> RoleClaims { get; set; }

    public override IQueryable<TRole> Roles => throw new NotSupportedException();

    public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        var isCreated = await _roleRepository.CreateAsync(role);

        return isCreated ? IdentityResult.Success : IdentityResult.Failed(new IdentityError()
        {
            Code = string.Empty,
            Description = "Role insert failed."
        });
    }

    public override async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        role.ConcurrencyStamp = Guid.NewGuid().ToString();
        var isUpdated = await _roleRepository.UpdateAsync(role, RoleClaims);

        return isUpdated ? IdentityResult.Success : IdentityResult.Failed(new IdentityError()
        {
            Code = string.Empty,
            Description = "Role update failed."
        });
    }

    public override async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        var isDeleted = await _roleRepository.DeleteAsync(role.Id);

        return isDeleted ? IdentityResult.Success : IdentityResult.Failed(new IdentityError()
        {
            Code = string.Empty,
            Description = "User delete failed."
        });
    }

    public override async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(roleId, nameof(roleId));

        var id = ConvertIdFromString(roleId);
        return await _roleRepository.FindByIdAsync(id);
    }

    public override async Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(normalizedName, nameof(normalizedName));

        return await _roleRepository.FindByNameAsync(normalizedName);
    }

    public override async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        return (await _roleClaimRepository.GetClaimsAsync(role.Id)).Select(rc => rc.ToClaim()).ToList();
    }

    public override async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role, nameof(role));
        ArgumentNullException.ThrowIfNull(claim, nameof(claim));

        RoleClaims ??= (await _roleClaimRepository.GetClaimsAsync(role.Id)).ToList();

        RoleClaims.Add(CreateRoleClaim(role, claim));
    }

    public override async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role, nameof(role));
        ArgumentNullException.ThrowIfNull(claim, nameof(claim));

        RoleClaims ??= (await _roleClaimRepository.GetClaimsAsync(role.Id)).ToList();

        var matchedClaims = RoleClaims.Where(rc => rc.RoleId.Equals(role.Id) && rc.ClaimType == claim.Type && rc.ClaimValue == claim.Value);

        foreach (var c in matchedClaims)
        {
            RoleClaims.Remove(c);
        }
    }
}
