using System.Data;
using TH.DapperIdentity.Core.Contracts;

namespace TH.DapperIdentity.Core.Repositories;

public class IdentityRepositoryBase : DisposableRepositoryBase
{
    public IdentityRepositoryBase(
        IDbConnectionFactory dbConnectionFactory,
        IdentityTablesOptions identityTablesOptions)
    {
        DbConnection = dbConnectionFactory.Create();
        IdentityTablesOptions = identityTablesOptions;
    }
    protected IDbConnection DbConnection { get; }
    protected IdentityTablesOptions IdentityTablesOptions { get; }

    public override void OnDispose()
    {
        DbConnection?.Dispose();
    }
}
