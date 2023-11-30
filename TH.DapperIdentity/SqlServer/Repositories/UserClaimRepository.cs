using Dapper;
using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.BaseRepositories;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.SqlServer.Repositories;

public class UserClaimRepository<TKey, TUserClaim> : IdentityRepositoryBase, IUserClaimRepository<TKey, TUserClaim>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
{
    public UserClaimRepository(IDbConnectionFactory dbConnectionFactory, DapperStoresOptions identityTablesOptions) : base(dbConnectionFactory, identityTablesOptions)
    {
    }

    public async Task<IEnumerable<TUserClaim>> GetClaimsAsync(TKey userId)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UserClaimsTableName}]
                     WHERE [UserId] = @UserId";

        return await DbConnection.QueryAsync<TUserClaim>(sql, new { UserId = userId });
    }
}
