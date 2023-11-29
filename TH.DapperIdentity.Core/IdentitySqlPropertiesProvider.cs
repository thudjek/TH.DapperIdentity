namespace TH.DapperIdentity.Core;
public class IdentitySqlPropertiesProvider
{
    public IdentitySqlPropertiesProvider(Type userType, Type roleType)
    {
        string[] userProperties = null;
        string[] roleProperties = null;

        if (userType != null)
        {
            userProperties = userType.GetProperties().Select(p => p.Name).ToArray();
        }

        if (roleType != null)
        {
            roleProperties = roleType.GetProperties().Select(p => p.Name).ToArray();
        }

        InsertUserSqlProperties = SetSqlProperties(userProperties, true);
        UpdateUserSqlProperties = SetSqlProperties(userProperties, false);
        InsertRoleSqlProperties = SetSqlProperties(roleProperties, true);
        UpdateRoleSqlProperties = SetSqlProperties(roleProperties, false);
    }

    public string InsertUserSqlProperties { get; init; }
    public string UpdateUserSqlProperties { get; init; }
    public string InsertRoleSqlProperties { get; init; }
    public string UpdateRoleSqlProperties { get; init; }

    private static string SetSqlProperties(string[] properties, bool isInsert)
    {
        if (properties == null)
        {
            return null;
        }

        if (isInsert)
        {
            return string.Join(", ", properties.Where(p => p != "Id").Select(p => $"@{p}"));
        }

        return string.Join(", ", properties.Where(p => p != "Id").Select(p => $"[{p}] = @{p}"));
    }
}