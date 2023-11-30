using System.Data.Common;

namespace TH.DapperIdentity.Contracts;

public interface IDbConnectionFactory
{
    DbConnection Create();
}
