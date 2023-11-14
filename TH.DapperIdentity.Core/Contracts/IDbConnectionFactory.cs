using System.Data.Common;

namespace TH.DapperIdentity.Core.Contracts;

public interface IDbConnectionFactory
{
    DbConnection Create();
}
