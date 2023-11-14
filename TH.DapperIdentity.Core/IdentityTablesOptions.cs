namespace TH.DapperIdentity.Core;

public class IdentityTablesOptions
{
    public string UsersTableName { get; set; } = "AspNetUsers";
    public string RolesTableName { get; set; } = "AspNetRoles";
    public string UserRolesTableName { get; set; } = "AspNetUserRoles";
    public string UserClaimsTableName { get; set; } = "AspNetUserClaims";
    public string RoleClaimsTableName { get; set; } = "AspNetRoleClaims";
    public string UserLoginsTableName { get; set; } = "AspNetUserLogins";
    public string UserTokensTableName { get; set; } = "AspNetUserTokens";
}
