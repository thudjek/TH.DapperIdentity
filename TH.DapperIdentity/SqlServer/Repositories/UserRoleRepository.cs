using Dapper;
using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.BaseRepositories;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.SqlServer.Repositories;
public class UserRoleRepository<TRole, TKey, TUserRole> : IdentityRepositoryBase, IUserRoleRepository<TRole, TKey, TUserRole>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : IdentityUserRole<TKey>, new()
{
    public UserRoleRepository(IDbConnectionFactory dbConnectionFactory, DapperStoresOptions identityTablesOptions) : base(dbConnectionFactory, identityTablesOptions)
    {
    }

    public async Task<IEnumerable<TRole>> GetRolesAsync(TKey userId)
    {
        var sql = $@"SELECT r.* FROM [dbo].[{DapperStoreOptions.TableNames.RolesTableName}] AS r
                     INNER JOIN [dbo].[{DapperStoreOptions.TableNames.UserRolesTableName}] AS ur ON r.[UserId] = ur.[UserId]
                     WHERE ur.[UserId] = @UserId";

        return await DbConnection.QueryAsync<TRole>(sql, new { UserId = userId });
    }

    public async Task<TUserRole> FindUserRoleAsync(TKey userId, TKey roleId)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UserRolesTableName}]
                     WHERE [UserId] = @UserId
                     AND [RoleId] = @RoleId";

        return await DbConnection.QuerySingleOrDefaultAsync<TUserRole>(sql, new
        {
            UserId = userId,
            RoleId = roleId
        });
    }
}