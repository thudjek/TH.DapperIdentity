using Microsoft.AspNetCore.Identity;

namespace TH.DapperIdentity.Contracts;

public interface IUserClaimRepository<TKey, TUserClaim>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
{
    Task<IEnumerable<TUserClaim>> GetClaimsAsync(TKey userId);
}
