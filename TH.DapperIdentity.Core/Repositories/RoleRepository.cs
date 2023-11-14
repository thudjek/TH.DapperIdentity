using Dapper;
using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.Core.Contracts;

namespace TH.DapperIdentity.Core.Repositories;
public class RoleRepository<TRole, TKey, TRoleClaim> : IdentityRepositoryBase, IRoleRepository<TRole, TKey, TRoleClaim>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    public RoleRepository(IDbConnectionFactory dbConnectionFactory, IdentityTablesOptions identityTablesOptions) : base(dbConnectionFactory, identityTablesOptions)
    {
    }

    public virtual async Task<bool> CreateAsync(TRole role)
    {
        var sql = $@"INSERT INTO [dbo].[{IdentityTablesOptions.RolesTableName}]
                     VALUES (
                        @Id,
                        @Name,
                        @NormalizedName,
                        @ConcurrencyStamp
                     )";

        var rowsInserted = await DbConnection.ExecuteAsync(sql, new
        {
            role.Id,
            role.Name,
            role.NormalizedName,
            role.ConcurrencyStamp
        });

        return rowsInserted == 1;
    }

    public async Task<bool> UpdateAsync(TRole role, IList<TRoleClaim> roleClaims)
    {
        var sql = $@"UPDATE [dbo].[{IdentityTablesOptions.RolesTableName}]
                     SET [Name] = @Name
                         [NormalizedName] = @NormalizedName
                         [ConcurrencyStamp] = @ConcurrencyStamp
                     WHERE [Id] = @Id";

        using var transaction = DbConnection.BeginTransaction();

        await DbConnection.ExecuteAsync(sql, new
        {
            role.Id,
            role.Name,
            role.NormalizedName,
            role.ConcurrencyStamp
        }, transaction);

        if (roleClaims?.Count > 0)
        {
            sql = $@"DELETE FROM [dbo].[{IdentityTablesOptions.RoleClaimsTableName}]
                     WHERE [RoleId] = @RoleId";

            await DbConnection.ExecuteAsync(sql, new { RoleId = role.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{IdentityTablesOptions.RoleClaimsTableName}]
                     VALUES (
                        @RoleId,
                        @ClaimType,
                        @ClaimValue
                     )";

            await DbConnection.ExecuteAsync(sql, roleClaims.Select(rc => new
            {
                RoleId = role.Id,
                rc.ClaimType,
                rc.ClaimValue
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

        return false;
    }

    public virtual async Task<bool> DeleteAsync(TKey roleId)
    {
        var sql = $@"DELETE FROM [dbo].[{IdentityTablesOptions.RolesTableName}]
                     WHERE [RoleId] = @RoleId";

        var rowsDeleted = await DbConnection.ExecuteAsync(sql, new { RoleId = roleId });

        return rowsDeleted == 1;
    }

    public virtual async Task<TRole> FindByIdAsync(TKey roleId)
    {
        var sql = $@"SELECT * FROM [dbo].[{IdentityTablesOptions.RolesTableName}]
                     WHERE [RoleId] = @RoleId";

        return await DbConnection.QuerySingleOrDefaultAsync<TRole>(sql, new { RoleId = roleId });
    }

    public virtual async Task<TRole> FindByNameAsync(string normalizedRoleName)
    {
        var sql = $@"SELECT * FROM [dbo].[{IdentityTablesOptions.RolesTableName}]
                     WHERE [NormalizedName] = @NormalizedName";

        return await DbConnection.QuerySingleOrDefaultAsync<TRole>(sql, new { NormalizedName = normalizedRoleName });
    }
}