using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace TH.DapperIdentity.Contracts;

public interface IUserOnlyRepository<TUser, TKey, TUserClaim, TUserLogin, TUserToken>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
{
    Task<bool> CreateAsync(TUser user);
    Task<bool> UpdateAsync(TUser user, IList<TUserClaim> userClaims, IList<TUserLogin> userLogins, IList<TUserToken> userTokens);
    Task<bool> DeleteAsync(TKey userId);
    Task<TUser> FindByEmailAsync(string normalizedEmail);
    Task<TUser> FindByIdAsync(TKey id);
    Task<TUser> FindByUserNameAsync(string normalizedUserName);
    Task<IEnumerable<TUser>> GetUsersForClaimAsync(Claim claim);
}

public interface IUserRepository<TUser, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken> : IUserOnlyRepository<TUser, TKey, TUserClaim, TUserLogin, TUserToken>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
{
    Task<bool> UpdateAsync(TUser user, IList<TUserRole> userRoles, IList<TUserClaim> userClaims, IList<TUserLogin> userLogins, IList<TUserToken> userTokens);
    Task<IEnumerable<TUser>> GetUsersInRoleAsync(TKey roleId);
}
