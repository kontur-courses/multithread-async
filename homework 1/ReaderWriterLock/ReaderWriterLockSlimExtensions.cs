using System;
using System.Threading;

namespace ReaderWriterLock;

public static class ReaderWriterLockSlimExtensions
{
    public static ReadLockToken ReadLock(this ReaderWriterLockSlim rwLock) => new(rwLock);
    public static WriteLockToken WriteLock(this ReaderWriterLockSlim rwLock) => new(rwLock);
}

public sealed class WriteLockToken : IDisposable
{
    private ReaderWriterLockSlim _lock;

    public WriteLockToken(ReaderWriterLockSlim rwLock)
    {
        _lock = rwLock;
        _lock.EnterWriteLock();
    }

    public void Dispose()
    {
        _lock?.ExitWriteLock();
        _lock = null;
    }
}

public sealed class ReadLockToken : IDisposable
{
    private ReaderWriterLockSlim _lock;

    public ReadLockToken(ReaderWriterLockSlim rwLock)
    {
        _lock = rwLock;
        _lock.EnterReadLock();
    }

    public void Dispose()
    {
        _lock?.ExitReadLock();
        _lock = null;
    }
}