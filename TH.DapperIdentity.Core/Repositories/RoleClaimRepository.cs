using Dapper;
using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.Core.Contracts;

namespace TH.DapperIdentity.Core.Repositories;
public class RoleClaimRepository<TKey, TRoleClaim> : IdentityRepositoryBase, IRoleClaimRepository<TKey, TRoleClaim>
    where TKey : IEquatable<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    public RoleClaimRepository(IDbConnectionFactory dbConnectionFactory, IdentityTablesOptions identityTablesOptions) : base(dbConnectionFactory, identityTablesOptions)
    {
    }

    public virtual async Task<IEnumerable<TRoleClaim>> GetClaimsAsync(TKey roleId)
    {
        var sql = $@"SELECT * FROM [dbo].[{IdentityTablesOptions.RoleClaimsTableName}]
                     WHERE [RoleId] = @RoleId";

        return await DbConnection.QueryAsync<TRoleClaim>(sql, new { RoleId = roleId });
    }
}