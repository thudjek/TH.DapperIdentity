using Microsoft.AspNetCore.Identity;
using TH.DapperIdentity.Extensions;
using TH.DapperIdentity.SqlServer.Repositories;

namespace TH.DapperIdentity.SqlServer;
public static class DapperStoresOptionExtenstions
{
    public static void AddSqlServerIdentityRepositories<TUser, TKey>(this DapperStoresOptions options)
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
    {
        options.AddUserOnlyRepository<UserRepository<TUser, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>, TUser, TKey>();
        options.AddUserClaimRepository<UserClaimRepository<TKey, IdentityUserClaim<TKey>>, TKey>();
        options.AddUserLoginRepository<UserLoginRepository<TUser, TKey, IdentityUserLogin<TKey>>, TUser, TKey>();
        options.AddUserTokenRepository<UserTokenRepository<TKey, IdentityUserToken<TKey>>, TKey>();
    }

    public static void AddSqlServerIdentityRepositories<TUser, TRole, TKey>(this DapperStoresOptions options)
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TRole : IdentityRole<TKey>
    {
        options.AddUserRepository<UserRepository<TUser, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>, TUser, TKey>();
        options.AddUserClaimRepository<UserClaimRepository<TKey, IdentityUserClaim<TKey>>, TKey>();
        options.AddUserLoginRepository<UserLoginRepository<TUser, TKey, IdentityUserLogin<TKey>>, TUser, TKey>();
        options.AddUserTokenRepository<UserTokenRepository<TKey, IdentityUserToken<TKey>>, TKey>();
        options.AddRoleRepository<RoleRepository<TRole, TKey, IdentityRoleClaim<TKey>>, TRole, TKey>();
        options.AddUserRoleRepository<UserRoleRepository<TRole, TKey, IdentityUserRole<TKey>>, TRole, TKey>();
        options.AddRoleClaimRepository<RoleClaimRepository<TKey, IdentityRoleClaim<TKey>>, TKey>();
    }
}