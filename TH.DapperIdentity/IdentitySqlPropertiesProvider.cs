namespace TH.DapperIdentity;
public class IdentitySqlPropertiesProvider
{
    public IdentitySqlPropertiesProvider(Type userType, Type roleType)
    {
        string[] userProperties = null;
        string[] roleProperties = null;

        if (userType != null)
        {
            userProperties = userType.GetProperties().Select(p => p.Name).ToArray();
            InsertUserSqlPropertiesTemplate = SetSqlProperties(userProperties, true);
            InsertUserSqlPropertyNames = InsertUserSqlPropertiesTemplate.Replace("@", "");
            UpdateUserSqlPropertiesTemplate = SetSqlProperties(userProperties, false);
        }

        if (roleType != null)
        {
            roleProperties = roleType.GetProperties().Select(p => p.Name).ToArray();
            InsertRoleSqlPropertiesTemplate = SetSqlProperties(roleProperties, true);
            InsertRoleSqlPropertyNames = InsertRoleSqlPropertiesTemplate.Replace("@", "");
            UpdateRoleSqlPropertiesTemplate = SetSqlProperties(roleProperties, false);
        }
    }

    public string InsertUserSqlPropertiesTemplate { get; init; }
    public string InsertUserSqlPropertyNames { get; init; }
    public string UpdateUserSqlPropertiesTemplate { get; init; }
    public string InsertRoleSqlPropertiesTemplate { get; init; }
    public string InsertRoleSqlPropertyNames { get; init; }
    public string UpdateRoleSqlPropertiesTemplate { get; init; }

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