using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.Extensions;
public static class DapperStoresOptionsExtenstions
{
    public static void AddUserOnlyRepository<TUserRepository, TUser, TKey>(this DapperStoresOptions options)
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TUserRepository : IUserOnlyRepository<TUser, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>
    {
        options.Services.AddScoped(
            typeof(IUserOnlyRepository<,,,,>).MakeGenericType(typeof(TUser), typeof(TKey), typeof(IdentityUserClaim<TKey>), typeof(IdentityUserLogin<TKey>), typeof(IdentityUserToken<TKey>)),
            typeof(TUserRepository)
        );
    }

    public static void AddUserRepository<TUserRepository, TUser, TKey>(this DapperStoresOptions options)
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TUserRepository : IUserRepository<TUser, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>
    {
        options.Services.AddScoped(
            typeof(IUserRepository<,,,,,>).MakeGenericType(typeof(TUser), typeof(TKey), typeof(IdentityUserClaim<TKey>), typeof(IdentityUserRole<TKey>), typeof(IdentityUserLogin<TKey>), typeof(IdentityUserToken<TKey>)),
            typeof(TUserRepository)
        );
    }

    public static void AddRoleRepository<TRoleRepository, TRole, TKey>(this DapperStoresOptions options)
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TRoleRepository : IRoleRepository<TRole, TKey, IdentityRoleClaim<TKey>>
    {
        options.Services.AddScoped(
            typeof(IRoleRepository<,,>).MakeGenericType(typeof(TRole), typeof(TKey), typeof(IdentityRoleClaim<TKey>)),
            typeof(TRoleRepository)
        );
    }

    public static void AddUserRoleRepository<TUserRoleRepository, TRole, TKey>(this DapperStoresOptions options)
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserRoleRepository : IUserRoleRepository<TRole, TKey, IdentityUserRole<TKey>>
    {
        options.Services.AddScoped(
            typeof(IUserRoleRepository<,,>).MakeGenericType(typeof(TRole), typeof(TKey), typeof(IdentityUserRole<TKey>)),
            typeof(TUserRoleRepository)
        );
    }

    public static void AddUserClaimRepository<TUserClaimRepository, TKey>(this DapperStoresOptions options)
        where TKey : IEquatable<TKey>
        where TUserClaimRepository : IUserClaimRepository<TKey, IdentityUserClaim<TKey>>
    {
        options.Services.AddScoped(
            typeof(IUserClaimRepository<,>).MakeGenericType(typeof(TKey), typeof(IdentityUserClaim<TKey>)),
            typeof(TUserClaimRepository)
        );
    }

    public static void AddRoleClaimRepository<TRoleClaimRepository, TKey>(this DapperStoresOptions options)
        where TKey : IEquatable<TKey>
        where TRoleClaimRepository : IRoleClaimRepository<TKey, IdentityRoleClaim<TKey>>
    {
        options.Services.AddScoped(
            typeof(IRoleClaimRepository<,>).MakeGenericType(typeof(TKey), typeof(IdentityRoleClaim<TKey>)),
            typeof(TRoleClaimRepository)
        );
    }

    public static void AddUserLoginRepository<TUserLoginRepository, TUser, TKey>(this DapperStoresOptions options)
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TUserLoginRepository : IUserLoginRepository<TUser, TKey, IdentityUserLogin<TKey>>
    {
        options.Services.AddScoped(
            typeof(IUserLoginRepository<,,>).MakeGenericType(typeof(TUser), typeof(TKey), typeof(IdentityUserLogin<TKey>)),
            typeof(TUserLoginRepository)
        );
    }

    public static void AddUserTokenRepository<TUserTokenRepository, TKey>(this DapperStoresOptions options)
        where TKey : IEquatable<TKey>
        where TUserTokenRepository : IUserTokenRepository<TKey, IdentityUserToken<TKey>>
    {
        options.Services.AddScoped(
            typeof(IUserTokenRepository<,>).MakeGenericType(typeof(TKey), typeof(IdentityUserToken<TKey>)),
            typeof(TUserTokenRepository)
        );
    }
}