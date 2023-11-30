using Microsoft.AspNetCore.Identity;

namespace TH.DapperIdentity.Contracts;
public interface IRoleRepository<TRole, TKey, TRoleClaim>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    Task<bool> CreateAsync(TRole role);
    Task<bool> UpdateAsync(TRole role, IList<TRoleClaim> roleClaims);
    Task<bool> DeleteAsync(TKey roleId);
    Task<TRole> FindByIdAsync(TKey roleId);
    Task<TRole> FindByNameAsync(string normalizedRoleName);
}