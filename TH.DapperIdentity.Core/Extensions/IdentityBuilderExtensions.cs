using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TH.DapperIdentity.Core.Contracts;
using TH.DapperIdentity.Core.Stores;

namespace TH.DapperIdentity.Core.Extensions;

public static class IdentityBuilderExtensions
{
    public static IdentityBuilder AddDapperStores<TDbConnectionFactory>(this IdentityBuilder builder, string connectionString, Action<DapperStoresOptions> configureOptionsAction)
    {
        AddStores<TDbConnectionFactory>(builder.Services, builder.UserType, builder.RoleType, connectionString, configureOptionsAction);
        return builder;
    }

    private static void AddStores<TDbConnectionFactory>(IServiceCollection services, Type userType, Type roleType, string connectionString, Action<DapperStoresOptions> configureOptionsAction)
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

        var options = new DapperStoresOptions()
        {
            Services = services
        };

        configureOptionsAction?.Invoke(options);
        options.Services = null;

        services.TryAddSingleton(options);

        services.TryAddSingleton(typeof(IdentitySqlPropertiesProvider), serviceProvider => Activator.CreateInstance(typeof(IdentitySqlPropertiesProvider), userType, roleType));

        Type userStoreType;

        if (roleType != null)
        {
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
            userStoreType = typeof(DapperUserOnlyStore<,,,,>).MakeGenericType(userType, keyType, userClaimType, userLoginType, userTokenType);
        }

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
