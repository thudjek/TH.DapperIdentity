using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TH.DapperIdentity.Core.Contracts;

namespace TH.DapperIdentity.Core.Repositories;

public class UserRepository<TUser, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken> : IdentityRepositoryBase, IUserRepository<TUser, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
{
    public UserRepository(IDbConnectionFactory dbConnectionFactory, IdentityTablesOptions identityTablesOptions) : base(dbConnectionFactory, identityTablesOptions)
    {
    }

    public virtual async Task<bool> CreateAsync(TUser user)
    {
        var sql = $@"INSERT INTO [dbo].[{IdentityTablesOptions.UsersTableName}]
                    VALUES (
                        @Id, 
                        @UserName, 
                        @NormalizedUserName,
                        @Email,
                        @NormalizedEmail,
                        @EmailConfirmed,
                        @PasswordHash,
                        @SecurityStamp,
                        @ConcurrencyStamp,
                        @PhoneNumber,
                        @PhoneNumberConfirmed,
                        @TwoFactorEnabled,
                        @LockoutEnd,
                        @LockoutEnabled,
                        @AccessFailedCount
                     )";

        var rowsInserted = await DbConnection.ExecuteAsync(sql, new
        {
            user.Id,
            user.UserName,
            user.NormalizedUserName,
            user.Email,
            user.NormalizedEmail,
            user.EmailConfirmed,
            user.PasswordHash,
            user.SecurityStamp,
            user.ConcurrencyStamp,
            user.PhoneNumber,
            user.PhoneNumberConfirmed,
            user.TwoFactorEnabled,
            user.LockoutEnd,
            user.LockoutEnabled,
            user.AccessFailedCount
        });

        return rowsInserted == 1;
    }

    public virtual Task<bool> UpdateAsync(TUser user, IList<TUserClaim> userClaims, IList<TUserLogin> userLogins, IList<TUserToken> userTokens) => UpdateAsync(user, null, userClaims, userLogins, userTokens);

    public virtual async Task<bool> UpdateAsync(TUser user, IList<TUserRole> userRoles, IList<TUserClaim> userClaims, IList<TUserLogin> userLogins, IList<TUserToken> userTokens)
    {
        var sql = $@"UPDATE [dbo].[{IdentityTablesOptions.UsersTableName}]
                     SET [UserName] = @UserName, 
                         [NormalizedUserName] = @NormalizedUserName, 
                         [Email] = @Email, 
                         [NormalizedEmail] = @NormalizedEmail, 
                         [EmailConfirmed] = @EmailConfirmed, 
                         [PasswordHash] = @PasswordHash, 
                         [SecurityStamp] = @SecurityStamp, 
                         [ConcurrencyStamp] = @ConcurrencyStamp, 
                         [PhoneNumber] = @PhoneNumber, 
                         [PhoneNumberConfirmed] = @PhoneNumberConfirmed, 
                         [TwoFactorEnabled] = @TwoFactorEnabled, 
                         [LockoutEnd] = @LockoutEnd, 
                         [LockoutEnabled] = @LockoutEnabled, 
                         [AccessFailedCount] = @AccessFailedCount
                     WHERE [Id] = @Id";

        using var transaction = DbConnection.BeginTransaction();

        await DbConnection.ExecuteAsync(sql, new
        {
            user.Id,
            user.UserName,
            user.NormalizedUserName,
            user.Email,
            user.NormalizedEmail,
            user.EmailConfirmed,
            user.PasswordHash,
            user.SecurityStamp,
            user.ConcurrencyStamp,
            user.PhoneNumber,
            user.PhoneNumberConfirmed,
            user.TwoFactorEnabled,
            user.LockoutEnd,
            user.LockoutEnabled,
            user.AccessFailedCount
        }, transaction);

        if (userRoles?.Count > 0)
        {
            sql = $@"DELETE FROM [dbo].[{IdentityTablesOptions.UserRolesTableName}]
                     WHERE [UserId] = @UserId";

            await DbConnection.ExecuteAsync(sql, new { UserId = user.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{IdentityTablesOptions.UserRolesTableName}] ([UserId], [RoleId])
                     VALUES (@UserId, @RoleId)";

            await DbConnection.ExecuteAsync(sql, userRoles.Select(r => new
            {
                UserId = user.Id,
                r.RoleId
            }), transaction);
        }

        if (userClaims?.Count > 0)
        {
            sql = $@"DELETE FROM [dbo].[{IdentityTablesOptions.UserClaimsTableName}]
                     WHERE [UserId] = @UserId";

            await DbConnection.ExecuteAsync(sql, new { UserId = user.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{IdentityTablesOptions.UserClaimsTableName}] ([UserId], [ClaimType], [ClaimValue])
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
            sql = $@"DELETE FROM [dbo].[{IdentityTablesOptions.UserLoginsTableName}]
                     WHERE [UserId] = @UserId";

            await DbConnection.ExecuteAsync(sql, new { UserId = user.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{IdentityTablesOptions.UserLoginsTableName}] ([LoginProvider], [ProviderKey], [ProviderDisplayName], [UserId])
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
            sql = $@"DELETE FROM [dbo].[{IdentityTablesOptions.UserTokensTableName}]
                     WHERE [UserId] = @UserId";

            await DbConnection.ExecuteAsync(sql, new { UserId = user.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{IdentityTablesOptions.UserTokensTableName}] ([UserId], [LoginProvider], [Name], [Value])
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

    public virtual async Task<bool> DeleteAsync(TKey userId)
    {
        var sql = $@"DELETE FROM [dbo].[{IdentityTablesOptions.UsersTableName}]
                     WHERE [Id] = @UserId";

        var rowsDeleted = await DbConnection.ExecuteAsync(sql, new { UserId = userId });

        return rowsDeleted == 1;
    }

    public virtual async Task<TUser> FindByEmailAsync(string normalizedEmail)
    {
        var sql = $@"SELECT * FROM [dbo].[{IdentityTablesOptions.UsersTableName}]
                     WHERE [NormalizedEmail] = @NormalizedEmail";

        return await DbConnection.QuerySingleOrDefaultAsync<TUser>(sql, new { NormalizedEmail = normalizedEmail });
    }

    public virtual async Task<TUser> FindByIdAsync(TKey id)
    {
        var sql = $@"SELECT * FROM [dbo].[{IdentityTablesOptions.UsersTableName}]
                     WHERE [Id] = @Id";

        return await DbConnection.QuerySingleOrDefaultAsync<TUser>(sql, new { Id = id });
    }

    public virtual async Task<TUser> FindByUserNameAsync(string normalizedUserName)
    {
        var sql = $@"SELECT * FROM [dbo].[{IdentityTablesOptions.UsersTableName}]
                     WHERE [NormalizedUserName] = @NormalizedUserName";

        return await DbConnection.QuerySingleOrDefaultAsync<TUser>(sql, new { NormalizedUserName = normalizedUserName });
    }

    public async Task<IEnumerable<TUser>> GetUsersForClaimAsync(Claim claim)
    {
        var sql = $@"SELECT u.* FROM [dbo].[{IdentityTablesOptions.UsersTableName}] AS u
                     INNER JOIN [dbo].[{IdentityTablesOptions.UserClaimsTableName}] AS uc ON u.[UserId] = uc.[UserId]
                     WHERE uc.[ClaimType] = @ClaimType
                     AND uc.[ClaimValue] = @ClaimValue";

        return await DbConnection.QueryAsync<TUser>(sql, new { ClaimType = claim.Type, ClaimValue = claim.Value });
    }

    public async Task<IEnumerable<TUser>> GetUsersInRoleAsync(TKey roleId)
    {
        var sql = $@"SELECT u.* FROM [dbo].[{IdentityTablesOptions.UsersTableName}] AS u
                     INNER JOIN [dbo].[{IdentityTablesOptions.UserRolesTableName}] AS ur ON u.[UserId] = ur.[UserId]
                     WHERE ur.[RoleId] = @RoleId";

        return await DbConnection.QueryAsync<TUser>(sql, new { RoleId = roleId });
    }
}
