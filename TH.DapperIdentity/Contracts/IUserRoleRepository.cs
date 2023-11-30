using Microsoft.AspNetCore.Identity;

namespace TH.DapperIdentity.Contracts;
public interface IUserRoleRepository<TRole, TKey, TUserRole>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : IdentityUserRole<TKey>, new()
{
    Task<IEnumerable<TRole>> GetRolesAsync(TKey userId);
    Task<TUserRole> FindUserRoleAsync(TKey userId, TKey roleId);
}