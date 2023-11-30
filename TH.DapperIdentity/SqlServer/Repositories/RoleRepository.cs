using Dapper;
using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.BaseRepositories;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.SqlServer.Repositories;
public class RoleRepository<TRole, TKey, TRoleClaim> : IdentityRepositoryBase, IRoleRepository<TRole, TKey, TRoleClaim>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    private readonly IdentitySqlPropertiesProvider _propertiesProvider;

    public RoleRepository(
        IDbConnectionFactory dbConnectionFactory,
        DapperStoresOptions identityTablesOptions,
        IdentitySqlPropertiesProvider propertiesProvider) : base(dbConnectionFactory, identityTablesOptions)
    {
        _propertiesProvider = propertiesProvider;
    }

    public async Task<bool> CreateAsync(TRole role)
    {
        var sql = $@"INSERT INTO [dbo].[{DapperStoreOptions.TableNames.RolesTableName}]
                     VALUES ({_propertiesProvider.InsertRoleSqlProperties})";

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
        var sql = $@"UPDATE [dbo].[{DapperStoreOptions.TableNames.RolesTableName}]
                     SET {_propertiesProvider.UpdateRoleSqlProperties}
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
            sql = $@"DELETE FROM [dbo].[{DapperStoreOptions.TableNames.RoleClaimsTableName}]
                     WHERE [RoleId] = @RoleId";

            await DbConnection.ExecuteAsync(sql, new { RoleId = role.Id }, transaction);

            sql = $@"INSERT INTO [dbo].[{DapperStoreOptions.TableNames.RoleClaimsTableName}]
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

    public async Task<bool> DeleteAsync(TKey roleId)
    {
        var sql = $@"DELETE FROM [dbo].[{DapperStoreOptions.TableNames.RolesTableName}]
                     WHERE [RoleId] = @RoleId";

        var rowsDeleted = await DbConnection.ExecuteAsync(sql, new { RoleId = roleId });

        return rowsDeleted == 1;
    }

    public async Task<TRole> FindByIdAsync(TKey roleId)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.RolesTableName}]
                     WHERE [RoleId] = @RoleId";

        return await DbConnection.QuerySingleOrDefaultAsync<TRole>(sql, new { RoleId = roleId });
    }

    public async Task<TRole> FindByNameAsync(string normalizedRoleName)
    {
        var sql = $@"SELECT * FROM [dbo].[{DapperStoreOptions.TableNames.RolesTableName}]
                     WHERE [NormalizedName] = @NormalizedName";

        return await DbConnection.QuerySingleOrDefaultAsync<TRole>(sql, new { NormalizedName = normalizedRoleName });
    }
}