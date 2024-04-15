using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.Stores;

public class DapperUserOnlyStore<TUser, TKey, TUserClaim, TUserLogin, TUserToken> : UserStoreBase<TUser, TKey, TUserClaim, TUserLogin, TUserToken>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
{
    private readonly IUserOnlyRepository<TUser, TKey, TUserClaim, TUserLogin, TUserToken> _userRepository;
    private readonly IUserClaimRepository<TKey, TUserClaim> _userClaimRepository;
    private readonly IUserLoginRepository<TUser, TKey, TUserLogin> _userLoginRepository;
    private readonly IUserTokenRepository<TKey, TUserToken> _userTokenRepository;

    public DapperUserOnlyStore(
        IUserOnlyRepository<TUser, TKey, TUserClaim, TUserLogin, TUserToken> userRepository,
        IUserClaimRepository<TKey, TUserClaim> userClaimRepository,
        IUserLoginRepository<TUser, TKey, TUserLogin> userLoginRepository,
        IUserTokenRepository<TKey, TUserToken> userTokenRepository,
        IdentityErrorDescriber describer) : base(describer)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _userClaimRepository = userClaimRepository ?? throw new ArgumentNullException(nameof(userClaimRepository));
        _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _userTokenRepository = userTokenRepository ?? throw new ArgumentNullException(nameof(userTokenRepository));
    }

    private List<TUserClaim> UserClaims { get; set; }
    private List<TUserLogin> UserLogins { get; set; }
    private List<TUserToken> UserTokens { get; set; }

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
}
