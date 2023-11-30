# DapperIdentity

Custom implementation of .NET Core Identity User and Role stores using Dapper.

Contains implementation of stores and interfaces for repositories which are used within stores.

This package also includes Sql Server implementation of said repositories.

To use different database provider, provide implementation of repository interfaces and pass them in the DI configuration.

## Installing package

Install DapperIdentity with Package Manager Console using command

	Install-Package TH.DapperIdentity

Or via .NET CLI using command

	dotnet add package TH.DapperIdentity

Or just search for *TH.DapperIdentity* in Nuget Packages UI


## Using DapperIdentity

DapperIdentity provides extension method *AddDapperStores* on top of IdentityBuilder object while configuring .NET Core Identity.

*AddDapperStores* method takes 3 parameters:
 * Generic type parameter of class that implements IDbConnectionFactory (provided SqlServer factory or custom implementation for other database providers)
 * Connection string for database
 * Delegate to configure *DapperStoresOptions* (implementation of repositories are added here)

Using provided Sql Server repositories:

```csharp
services.AddIdentity<IdentityUser<int>, IdentityRole<int>>()
	.AddDapperStores<SqlServerDbConnectionFactory>(connectionString, options =>
	{
		options.AddSqlServerIdentityRepositories<IdentityUser<int>, int>();
	});
```

You can also set custom names of database tables through options:

```csharp
services.AddIdentity<IdentityUser<int>, IdentityRole<int>>()
	.AddDapperStores<SqlServerDbConnectionFactory>(connectionString, options =>
	{
		options.AddSqlServerIdentityRepositories<IdentityUser<int>, int>();
		options.TableNames.UsersTableName = "Users";
		options.TableNames.RolesTableName = "Roles";
		options.TableNames.UserRolesTableName = "UserRoles";
		options.TableNames.UserClaimsTableName = "UserClaims";
		options.TableNames.RoleClaimsTableName = "RoleClaims";
		options.TableNames.UserLoginsTableName = "UserLogins";
		options.TableNames.UserTokensTableName = "UserTokens";
	});
```

If you want to use custom implementation of repositories for different database provider you need to register them through options using extension methods:

```csharp
services.AddIdentity<IdentityUser<int>, IdentityRole<int>>()
	.AddDapperStores<SqlServerDbConnectionFactory>(connectionString, options =>
	{
		options.AddUserRepository<UserRepositoryImplementation, IdentityUser<int>, int>();
		options.AddRoleRepository<RoleRepositoryImplementation, IdentityRole<int>, int>();
		options.AddUserLoginRepository<UserLoginRepositoryImplementation, IdentityUser<int>, int>();
		...
	});
```

Keep in mind if you are not registering Role class with .NET Core Identity you should register only those repositories that don't use roles including UserOnlyRepository.
