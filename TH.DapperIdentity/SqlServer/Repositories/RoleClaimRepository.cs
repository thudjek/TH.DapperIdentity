using Dapper;
using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.BaseRepositories;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.SqlServer.Repositories;
public class RoleClaimRepository<TKey, TRoleClaim> : IdentityRepositoryBase, IRoleClaimRepository<TKey, TRoleClaim>
    where TKey : IEquatable<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    public RoleClaimRepository(IDbConnectionFactory dbConnectionFactory, DapperStoresOptions identityTablesOptions) : base(dbConnectionFactory, identityTablesOptions)
    {
    }

    public async Task<IEnumerable<TRoleClaim>> GetClaimsAsync(TKey roleId)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.RoleClaimsTableName}]
                     WHERE [RoleId] = @RoleId";

        return await DbConnection.QueryAsync<TRoleClaim>(sql, new { RoleId = roleId });
    }
}