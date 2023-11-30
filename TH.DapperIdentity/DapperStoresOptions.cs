using Microsoft.Extensions.DependencyInjection;

namespace TH.DapperIdentity;

public class DapperStoresOptions
{
    public DapperStoresOptions()
    {
        TableNames = new();
    }

    public IdentityTableNamesOptions TableNames { get; set; }

    public IServiceCollection Services { get; set; }
}

public class IdentityTableNamesOptions
{
    public string UsersTableName { get; set; } = "AspNetUsers";
    public string RolesTableName { get; set; } = "AspNetRoles";
    public string UserRolesTableName { get; set; } = "AspNetUserRoles";
    public string UserClaimsTableName { get; set; } = "AspNetUserClaims";
    public string RoleClaimsTableName { get; set; } = "AspNetRoleClaims";
    public string UserLoginsTableName { get; set; } = "AspNetUserLogins";
    public string UserTokensTableName { get; set; } = "AspNetUserTokens";
}