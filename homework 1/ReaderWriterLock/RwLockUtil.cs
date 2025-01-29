using System;
using System.Threading;

namespace ReaderWriterLock;

public class RwLockUtil : IDisposable
{
    public class ReaderLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        public ReaderLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            rwLock.EnterReadLock();
        }
#pragma warning disable CA1816
        public void Dispose() => _rwLock.ExitReadLock();
#pragma warning restore CA1816
    }

    public class WriterLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        public WriterLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            rwLock.EnterWriteLock();
        }

#pragma warning disable CA1816
        public void Dispose() => _rwLock.ExitWriteLock();
#pragma warning restore CA1816
    }

    private readonly ReaderWriterLockSlim _lock = new();

    public ReaderLock ReadLock() => new(_lock);
    public WriterLock WriteLock() => new(_lock);

#pragma warning disable CA1816
    public void Dispose() => _lock.Dispose();
#pragma warning restore CA1816
}