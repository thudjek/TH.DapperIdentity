namespace TH.DapperIdentity.Core.Repositories;

public abstract class DisposableRepositoryBase : IDisposable
{
    private bool _disposed = false;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract void OnDispose();

    public virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            OnDispose();
        }

        _disposed = true;
    }
}
