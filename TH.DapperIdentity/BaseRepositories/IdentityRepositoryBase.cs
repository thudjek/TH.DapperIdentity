using System.Data;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.BaseRepositories;

public class IdentityRepositoryBase : DisposableRepositoryBase
{
    public IdentityRepositoryBase(
        IDbConnectionFactory dbConnectionFactory,
        DapperStoresOptions dapperStoreOptions)
    {
        DbConnection = dbConnectionFactory.Create();
        DapperStoreOptions = dapperStoreOptions;
    }
    protected IDbConnection DbConnection { get; }
    protected DapperStoresOptions DapperStoreOptions { get; }

    public override void OnDispose()
    {
        DbConnection?.Dispose();
    }
}
