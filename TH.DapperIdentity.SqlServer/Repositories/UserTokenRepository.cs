using Dapper;
using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.Core;
using TH.DapperIdentity.Core.BaseRepositories;
using TH.DapperIdentity.Core.Contracts;

namespace TH.DapperIdentity.SqlServer.Repositories;

public class UserTokenRepository<TKey, TUserToken> : IdentityRepositoryBase, IUserTokenRepository<TKey, TUserToken>
    where TKey : IEquatable<TKey>
    where TUserToken : IdentityUserToken<TKey>, new()
{
    public UserTokenRepository(IDbConnectionFactory dbConnectionFactory, DapperStoresOptions identityTablesOptions) : base(dbConnectionFactory, identityTablesOptions)
    {
    }

    public async Task<IEnumerable<TUserToken>> GetTokensAsync(TKey userId)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UserTokensTableName}]
                     WHERE [UserId] = @UserId";

        return await DbConnection.QueryAsync<TUserToken>(sql, new { UserId = userId });
    }

    public async Task<TUserToken> FindTokenAsync(TKey userId, string loginProvider, string name)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UserTokensTableName}]
                     WHERE [UserId] = @UserId
                     AND [LoginProvider] = @LoginProvider
                     AND [Name] = @Name";

        return await DbConnection.QuerySingleOrDefaultAsync<TUserToken>(sql, new
        {
            UserId = userId,
            LoginProvider = loginProvider,
            Name = name
        });
    }
}
