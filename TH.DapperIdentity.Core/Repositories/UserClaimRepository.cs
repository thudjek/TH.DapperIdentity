using Dapper;
using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.Core.Contracts;

namespace TH.DapperIdentity.Core.Repositories;

public class UserClaimRepository<TKey, TUserClaim> : IdentityRepositoryBase, IUserClaimRepository<TKey, TUserClaim>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
{
    public UserClaimRepository(IDbConnectionFactory dbConnectionFactory, IdentityTablesOptions identityTablesOptions) : base(dbConnectionFactory, identityTablesOptions)
    {
    }

    public virtual async Task<IEnumerable<TUserClaim>> GetClaimsAsync(TKey userId)
    {
        var sql = $@"SELECT * FROM [dbo].[{IdentityTablesOptions.UserClaimsTableName}]
                     WHERE [UserId] = @UserId";

        return await DbConnection.QueryAsync<TUserClaim>(sql, new { UserId = userId });
    }
}
