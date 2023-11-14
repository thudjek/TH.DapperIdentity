using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TH.DapperIdentity.Core.Contracts;
using TH.DapperIdentity.Core.Repositories;
using TH.DapperIdentity.Core.Stores;

namespace TH.DapperIdentity.Core;

public static class IdentityBuilderExtenstions
{
    public static IdentityBuilder AddDapperStores<TDbConnectionFactory>(this IdentityBuilder builder, string connectionString, Action<IdentityTablesOptions> tablesNamesOptions)
    {
        AddStores<TDbConnectionFactory>(builder.Services, builder.UserType, builder.RoleType, connectionString, tablesNamesOptions);
        return builder;
    }

    private static void AddStores<TDbConnectionFactory>(IServiceCollection services, Type userType, Type roleType, string connectionString, Action<IdentityTablesOptions> tablesNamesOptions)
    {
        var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<>));
        if (identityUserType == null)
        {
            throw new InvalidOperationException("User type must inherit from type IdentityUser");
        }

        var keyType = identityUserType.GenericTypeArguments[0];
        var userClaimType = typeof(IdentityUserClaim<>).MakeGenericType(keyType);
        var userRoleType = typeof(IdentityUserRole<>).MakeGenericType(keyType);
        var userLoginType = typeof(IdentityUserLogin<>).MakeGenericType(keyType);
        var roleClaimType = typeof(IdentityRoleClaim<>).MakeGenericType(keyType);
        var userTokenType = typeof(IdentityUserToken<>).MakeGenericType(keyType);

        services.TryAddScoped(typeof(IDbConnectionFactory), serviceProvider => Activator.CreateInstance(typeof(TDbConnectionFactory), connectionString));

        var identityTableOptions = new IdentityTablesOptions();
        tablesNamesOptions?.Invoke(identityTableOptions);
        services.TryAddSingleton(identityTableOptions);

        Type userStoreType;

        if (roleType != null)
        {
            services.TryAddScoped(
                typeof(IUserRepository<,,,,,>).MakeGenericType(userType, keyType, userClaimType, userRoleType, userLoginType, userTokenType),
                typeof(UserRepository<,,,,,>).MakeGenericType(userType, keyType, userClaimType, userRoleType, userLoginType, userTokenType)
            );

            services.TryAddScoped(
                typeof(IRoleRepository<,,>).MakeGenericType(roleType, keyType, roleClaimType),
                typeof(RoleRepository<,,>).MakeGenericType(roleType, keyType, roleClaimType)
            );

            services.TryAddScoped(
                typeof(IUserRoleRepository<,,>).MakeGenericType(roleType, keyType, userRoleType),
                typeof(UserRoleRepository<,,>).MakeGenericType(roleType, keyType, userRoleType)
            );

            services.TryAddScoped(
                typeof(IRoleClaimRepository<,>).MakeGenericType(keyType, roleClaimType),
                typeof(RoleClaimRepository<,>).MakeGenericType(keyType, roleClaimType)
            );

            var identityRoleType = FindGenericBaseType(roleType, typeof(IdentityRole<>));
            if (identityRoleType == null)
            {
                throw new InvalidOperationException("Role type must inherit from type IdentityRole");
            }

            userStoreType = typeof(DapperUserStore<,,,,,,,>).MakeGenericType(userType, roleType, keyType, userClaimType, userRoleType, userLoginType, userTokenType, roleClaimType);
            services.TryAddScoped(
                typeof(IRoleStore<>).MakeGenericType(roleType),
                typeof(DapperRoleStore<,,,>).MakeGenericType(roleType, keyType, userRoleType, roleClaimType)
            );
        }
        else
        {
            services.TryAddScoped(
                typeof(IUserOnlyRepository<,,,,>).MakeGenericType(userType, keyType, userClaimType, userLoginType, userTokenType),
                typeof(UserRepository<,,,,,>).MakeGenericType(userType, keyType, userClaimType, userRoleType, userLoginType, userTokenType)
            );

            userStoreType = typeof(DapperUserOnlyStore<,,,,>).MakeGenericType(userType, keyType, userClaimType, userLoginType, userTokenType);
        }

        services.TryAddScoped(
            typeof(IUserClaimRepository<,>).MakeGenericType(keyType, userClaimType),
            typeof(UserClaimRepository<,>).MakeGenericType(keyType, userClaimType)
        );

        services.TryAddScoped(
            typeof(IUserLoginRepository<,,>).MakeGenericType(userType, keyType, userLoginType),
            typeof(UserLoginRepository<,,>).MakeGenericType(userType, keyType, userLoginType)
        );

        services.TryAddScoped(
            typeof(IUserTokenRepository<,>).MakeGenericType(keyType, userTokenType),
            typeof(UserTokenRepository<,>).MakeGenericType(keyType, userTokenType)
        );

        services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
    }

    private static Type FindGenericBaseType(Type currentType, Type genericBaseType)
    {
        Type type = currentType;
        while (type != null)
        {
            var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            if (genericType != null && genericType == genericBaseType)
            {
                return type;
            }
            type = type.BaseType;
        }
        return null;
    }
}
