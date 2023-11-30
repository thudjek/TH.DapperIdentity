using Dapper;
using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.BaseRepositories;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.SqlServer.Repositories;

public class UserLoginRepository<TUser, TKey, TUserLogin> : IdentityRepositoryBase, IUserLoginRepository<TUser, TKey, TUserLogin>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TUserLogin : IdentityUserLogin<TKey>, new()
{
    public UserLoginRepository(IDbConnectionFactory dbConnectionFactory, DapperStoresOptions identityTablesOptions) : base(dbConnectionFactory, identityTablesOptions)
    {
    }

    public async Task<IEnumerable<TUserLogin>> GetLoginsAsync(TKey userId)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UserLoginsTableName}]
                     WHERE [UserId] = @UserId";

        return await DbConnection.QueryAsync<TUserLogin>(sql, new { UserId = userId });
    }

    public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey)
    {
        var sql = $@"SELECT u.* FROM [dbo].[{DapperStoreOptions.TableNames.UsersTableName}] AS u
                     INNER JOIN [dbo].[{DapperStoreOptions.TableNames.UserLoginsTableName}] AS ul ON u.[Id] = ul.[UserId]
                     WHERE ul.[LoginProvider] = @LoginProvider 
                     AND ul.[ProviderKey] = @ProviderKey;";

        return await DbConnection.QuerySingleOrDefaultAsync<TUser>(sql, new
        {
            LoginProvider = loginProvider,
            ProviderKey = providerKey
        });
    }

    public async Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UserLoginsTableName}]
                     WHERE [LoginProvider] = @LoginProvider 
                     AND [ProviderKey] = @ProviderKey;";

        return await DbConnection.QuerySingleOrDefaultAsync<TUserLogin>(sql, new
        {
            LoginProvider = loginProvider,
            ProviderKey = providerKey
        });
    }

    public async Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UserLoginsTableName}]
                     WHERE [UserId] = @UserId 
                     AND [LoginProvider] = @LoginProvider 
                     AND [ProviderKey] = @ProviderKey;";

        return await DbConnection.QuerySingleOrDefaultAsync<TUserLogin>(sql, new
        {
            UserId = userId,
            LoginProvider = loginProvider,
            ProviderKey = providerKey
        });
    }
}
