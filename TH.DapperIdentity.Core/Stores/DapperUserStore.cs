using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TH.DapperIdentity.Core.Contracts;

namespace TH.DapperIdentity.Core.Stores;

public class DapperUserStore<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim> : UserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>, IProtectedUserStore<TUser>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    private readonly IUserRepository<TUser, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken> _userRepository;
    private readonly IRoleRepository<TRole, TKey, TRoleClaim> _roleRepository;
    private readonly IUserClaimRepository<TKey, TUserClaim> _userClaimRepository;
    private readonly IUserLoginRepository<TUser, TKey, TUserLogin> _userLoginRepository;
    private readonly IUserTokenRepository<TKey, TUserToken> _userTokenRepository;
    private readonly IUserRoleRepository<TRole, TKey, TUserRole> _userRoleRepository;

    public DapperUserStore(
        IUserRepository<TUser, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken> userRepository,
        IRoleRepository<TRole, TKey, TRoleClaim> roleRepository,
        IUserClaimRepository<TKey, TUserClaim> userClaimRepository,
        IUserLoginRepository<TUser, TKey, TUserLogin> userLoginRepository,
        IUserTokenRepository<TKey, TUserToken> userTokenRepository,
        IUserRoleRepository<TRole, TKey, TUserRole> userRoleRepository,
        IdentityErrorDescriber describer) : base(describer)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _userClaimRepository = userClaimRepository ?? throw new ArgumentNullException(nameof(userClaimRepository));
        _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _userTokenRepository = userTokenRepository ?? throw new ArgumentNullException(nameof(userTokenRepository));
        _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
    }

    private IList<TUserRole> UserRoles { get; set; }
    private IList<TUserClaim> UserClaims { get; set; }
    private IList<TUserLogin> UserLogins { get; set; }
    private IList<TUserToken> UserTokens { get; set; }

    public override IQueryable<TUser> Users => throw new NotSupportedException();

    public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var isCreated = await _userRepository.CreateAsync(user);

        return isCreated ? IdentityResult.Success : IdentityResult.Failed(new IdentityError()
        {
            Code = string.Empty,
            Description = "User insert failed."
        });
    }

    public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        var isUpdated = await _userRepository.UpdateAsync(user, UserClaims, UserLogins, UserTokens);

        return isUpdated ? IdentityResult.Success : IdentityResult.Failed(new IdentityError()
        {
            Code = string.Empty,
            Description = "User update failed."
        });
    }

    public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var isDeleted = await _userRepository.DeleteAsync(user.Id);

        return isDeleted ? IdentityResult.Success : IdentityResult.Failed(new IdentityError()
        {
            Code = string.Empty,
            Description = "User delete failed."
        });
    }

    public override async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(normalizedEmail, nameof(normalizedEmail));

        return await _userRepository.FindByEmailAsync(normalizedEmail);
    }

    public override async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));

        var id = ConvertIdFromString(userId);
        return await _userRepository.FindByIdAsync(id);
    }

    public override async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(normalizedUserName, nameof(normalizedUserName));

        return await _userRepository.FindByUserNameAsync(normalizedUserName);
    }

    protected override async Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return await _userRepository.FindByIdAsync(userId);
    }

    public override async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        return (await _userClaimRepository.GetClaimsAsync(user.Id)).Select(uc => uc.ToClaim()).ToList();

    }

    public override async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(claims, nameof(claims));

        UserClaims ??= (await _userClaimRepository.GetClaimsAsync(user.Id)).ToList();

        foreach (var claim in claims)
        {
            UserClaims.Add(CreateUserClaim(user, claim));
        }
    }

    public override async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(claims, nameof(claims));

        UserClaims ??= (await _userClaimRepository.GetClaimsAsync(user.Id)).ToList();

        foreach (var claim in claims)
        {
            var matchedClaims = UserClaims.Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value);

            foreach (var c in matchedClaims)
            {
                UserClaims.Remove(c);
            }
        }
    }

    public override async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(claim, nameof(claim));
        ArgumentNullException.ThrowIfNull(newClaim, nameof(newClaim));

        UserClaims ??= (await _userClaimRepository.GetClaimsAsync(user.Id)).ToList();

        var matchedClaims = UserClaims.Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value).ToList();

        foreach (var matchedClaim in matchedClaims)
        {
            matchedClaim.ClaimValue = newClaim.Value;
            matchedClaim.ClaimType = newClaim.Type;
        }
    }

    public override async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(claim, nameof(claim));

        return (await _userRepository.GetUsersForClaimAsync(claim)).ToList();
    }

    public override async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        return (await _userLoginRepository.GetLoginsAsync(user.Id)).Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey, ul.ProviderDisplayName)).ToList();
    }

    public override async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(login, nameof(login));

        UserLogins ??= (await _userLoginRepository.GetLoginsAsync(user.Id)).ToList();

        UserLogins.Add(CreateUserLogin(user, login));
    }

    public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        UserLogins ??= (await _userLoginRepository.GetLoginsAsync(user.Id)).ToList();

        var userLogin = await _userLoginRepository.FindUserLoginAsync(user.Id, loginProvider, providerKey);

        if (userLogin != null)
        {
            UserLogins.Remove(userLogin);
        }
    }

    protected override async Task AddUserTokenAsync(TUserToken token)
    {
        ArgumentNullException.ThrowIfNull(token, nameof(token));

        UserTokens ??= (await _userTokenRepository.GetTokensAsync(token.UserId)).ToList();
        UserTokens.Add(token);
    }

    protected override async Task RemoveUserTokenAsync(TUserToken token)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(token, nameof(token));

        UserTokens ??= (await _userTokenRepository.GetTokensAsync(token.UserId)).ToList();
        UserTokens.Remove(token);
    }

    protected override async Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(loginProvider, nameof(loginProvider));
        ArgumentNullException.ThrowIfNull(providerKey, nameof(providerKey));

        return await _userLoginRepository.FindUserLoginAsync(userId, loginProvider, providerKey);
    }

    protected override async Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(loginProvider, nameof(loginProvider));
        ArgumentNullException.ThrowIfNull(providerKey, nameof(providerKey));

        return await _userLoginRepository.FindUserLoginAsync(loginProvider, providerKey);
    }

    protected override async Task<TUserToken> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(loginProvider, nameof(loginProvider));
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        return await _userTokenRepository.FindTokenAsync(user.Id, loginProvider, name);
    }

    public override async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        return (await _userRoleRepository.GetRolesAsync(user.Id)).Select(r => r.Name).ToList();
    }

    protected override async Task<TRole> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(normalizedRoleName, nameof(normalizedRoleName));

        return await _roleRepository.FindByNameAsync(normalizedRoleName);
    }

    protected override async Task<TUserRole> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return await _userRoleRepository.FindUserRoleAsync(userId, roleId);
    }

    public override async Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(normalizedRoleName, nameof(normalizedRoleName));

        var role = await _roleRepository.FindByNameAsync(normalizedRoleName);
        var users = new List<TUser>();
        if (role != null)
        {
            users = (await _userRepository.GetUsersInRoleAsync(role.Id)).ToList();
        }

        return users;
    }

    public override async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(normalizedRoleName, nameof(normalizedRoleName));

        var role = await _roleRepository.FindByNameAsync(normalizedRoleName);

        if (role != null)
        {
            UserRoles.Add(CreateUserRole(user, role));
        }
    }

    public override async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(normalizedRoleName, nameof(normalizedRoleName));

        var role = await _roleRepository.FindByNameAsync(normalizedRoleName);

        if (role != null)
        {
            var userRole = await FindUserRoleAsync(user.Id, role.Id, cancellationToken);
            if (userRole != null)
            {
                UserRoles.Remove(userRole);
            }
        }
    }

    public override async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(normalizedRoleName, nameof(normalizedRoleName));

        var role = await _roleRepository.FindByNameAsync(normalizedRoleName);

        if (role != null)
        {
            var userRole = await FindUserRoleAsync(user.Id, role.Id, cancellationToken);
            return userRole != null;
        }

        return false;
    }
}
