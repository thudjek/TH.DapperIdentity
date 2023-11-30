using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TH.DapperIdentity.BaseRepositories;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.SqlServer.Repositories;

public class UserRepository<TUser, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken> : IdentityRepositoryBase, IUserRepository<TUser, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
{
    private readonly IdentitySqlPropertiesProvider _propertiesProvider;

    public UserRepository(
        IDbConnectionFactory dbConnectionFactory,
        DapperStoresOptions identityTablesOptions,
        IdentitySqlPropertiesProvider propertiesProvider) : base(dbConnectionFactory, identityTablesOptions)
    {
        _propertiesProvider = propertiesProvider;
    }

    public async Task<bool> CreateAsync(TUser user)
    {
        var sql = $@"INSERT INTO [dbo].[{DapperStoreOptions.TableNames.UsersTableName}]
                    VALUES ({_propertiesProvider.InsertUserSqlProperties})";

        var p = new DynamicParameters(user);

        var rowsInserted = await DbConnection.ExecuteAsync(sql, p);

        return rowsInserted == 1;
    }

    public Task<bool> UpdateAsync(TUser user, IList<TUserClaim> userClaims, IList<TUserLogin> userLogins, IList<TUserToken> userTokens) => UpdateAsync(user, null, userClaims, userLogins, userTokens);

    public async Task<bool> UpdateAsync(TUser user, IList<TUserRole> userRoles, IList<TUserClaim> userClaims, IList<TUserLogin> userLogins, IList<TUserToken> userTokens)
    {
        var sql = $@"UPDATE [dbo].[{DapperStoreOptions.TableNames.UsersTableName}]
                     SET {_propertiesProvider.UpdateUserSqlProperties}
                     WHERE [Id] = @Id";

        using var transaction = DbConnection.BeginTransaction();

        var p = new DynamicParameters(user);

        await DbConnection.ExecuteAsync(sql, p, transaction);

        if (userRoles?.Count > 0)
        {
            sql = $@"DELETE FROM [dbo].[{DapperStoreOptions.TableNames.UserRolesTableName}]
                     WHERE [UserId] = @UserId";

            await DbConnection.ExecuteAsync(sql, new { UserId = user.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{DapperStoreOptions.TableNames.UserRolesTableName}] ([UserId], [RoleId])
                     VALUES (@UserId, @RoleId)";

            await DbConnection.ExecuteAsync(sql, userRoles.Select(r => new
            {
                UserId = user.Id,
                r.RoleId
            }), transaction);
        }

        if (userClaims?.Count > 0)
        {
            sql = $@"DELETE FROM [dbo].[{DapperStoreOptions.TableNames.UserClaimsTableName}]
                     WHERE [UserId] = @UserId";

            await DbConnection.ExecuteAsync(sql, new { UserId = user.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{DapperStoreOptions.TableNames.UserClaimsTableName}] ([UserId], [ClaimType], [ClaimValue])
                     VALUES (@UserId, @ClaimType, @ClaimValue)";

            await DbConnection.ExecuteAsync(sql, userClaims.Select(c => new
            {
                UserId = user.Id,
                c.ClaimType,
                c.ClaimValue
            }), transaction);
        }

        if (userLogins?.Count > 0)
        {
            sql = $@"DELETE FROM [dbo].[{DapperStoreOptions.TableNames.UserLoginsTableName}]
                     WHERE [UserId] = @UserId";

            await DbConnection.ExecuteAsync(sql, new { UserId = user.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{DapperStoreOptions.TableNames.UserLoginsTableName}] ([LoginProvider], [ProviderKey], [ProviderDisplayName], [UserId])
                     VALUES (@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId)";

            await DbConnection.ExecuteAsync(sql, userLogins.Select(l => new
            {
                l.LoginProvider,
                l.ProviderKey,
                l.ProviderDisplayName,
                UserId = user.Id
            }), transaction);
        }

        if (userTokens?.Count > 0)
        {
            sql = $@"DELETE FROM [dbo].[{DapperStoreOptions.TableNames.UserTokensTableName}]
                     WHERE [UserId] = @UserId";

            await DbConnection.ExecuteAsync(sql, new { UserId = user.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{DapperStoreOptions.TableNames.UserTokensTableName}] ([UserId], [LoginProvider], [Name], [Value])
                     VALUES (@UserId, @LoginProvider, @Name, @Value)";

            await DbConnection.ExecuteAsync(sql, userTokens.Select(t => new
            {
                UserId = user.Id,
                t.LoginProvider,
                t.Name,
                t.Value
            }), transaction);
        }

        try
        {
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteAsync(TKey userId)
    {
        var sql = $@"DELETE FROM [dbo].[{DapperStoreOptions.TableNames.UsersTableName}]
                     WHERE [Id] = @UserId";

        var rowsDeleted = await DbConnection.ExecuteAsync(sql, new { UserId = userId });

        return rowsDeleted == 1;
    }

    public async Task<TUser> FindByEmailAsync(string normalizedEmail)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UsersTableName}]
                     WHERE [NormalizedEmail] = @NormalizedEmail";

        return await DbConnection.QuerySingleOrDefaultAsync<TUser>(sql, new { NormalizedEmail = normalizedEmail });
    }

    public async Task<TUser> FindByIdAsync(TKey id)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UsersTableName}]
                     WHERE [Id] = @Id";

        return await DbConnection.QuerySingleOrDefaultAsync<TUser>(sql, new { Id = id });
    }

    public async Task<TUser> FindByUserNameAsync(string normalizedUserName)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.UsersTableName}]
                     WHERE [NormalizedUserName] = @NormalizedUserName";

        return await DbConnection.QuerySingleOrDefaultAsync<TUser>(sql, new { NormalizedUserName = normalizedUserName });
    }

    public async Task<IEnumerable<TUser>> GetUsersForClaimAsync(Claim claim)
    {
        var sql = $@"SELECT u.* FROM [dbo].[{DapperStoreOptions.TableNames.UsersTableName}] AS u
                     INNER JOIN [dbo].[{DapperStoreOptions.TableNames.UserClaimsTableName}] AS uc ON u.[UserId] = uc.[UserId]
                     WHERE uc.[ClaimType] = @ClaimType
                     AND uc.[ClaimValue] = @ClaimValue";

        return await DbConnection.QueryAsync<TUser>(sql, new { ClaimType = claim.Type, ClaimValue = claim.Value });
    }

    public async Task<IEnumerable<TUser>> GetUsersInRoleAsync(TKey roleId)
    {
        var sql = $@"SELECT u.* FROM [dbo].[{DapperStoreOptions.TableNames.UsersTableName}] AS u
                     INNER JOIN [dbo].[{DapperStoreOptions.TableNames.UserRolesTableName}] AS ur ON u.[UserId] = ur.[UserId]
                     WHERE ur.[RoleId] = @RoleId";

        return await DbConnection.QueryAsync<TUser>(sql, new { RoleId = roleId });
    }
}
