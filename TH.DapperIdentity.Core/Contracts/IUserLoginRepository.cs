using Microsoft.AspNetCore.Identity;

namespace TH.DapperIdentity.Core.Contracts;

public interface IUserLoginRepository<TUser, TKey, TUserLogin>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TUserLogin : IdentityUserLogin<TKey>, new()
{
    Task<IEnumerable<TUserLogin>> GetLoginsAsync(TKey userId);
    Task<TUser> FindByLoginAsync(string loginProvider, string providerKey);
    Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey);
    Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey);
}
