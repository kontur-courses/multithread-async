using System;
using System.Threading;

namespace ReaderWriterLock;

public class RwLockSugar : IDisposable
{
    public class ReaderLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        public ReaderLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            rwLock.EnterReadLock();
        }
        public void Dispose() => _rwLock.ExitReadLock();
    }

    public class WriterLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        public WriterLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            rwLock.EnterWriteLock();
        }

        public void Dispose() => _rwLock.ExitWriteLock();
    }
    
    private readonly ReaderWriterLockSlim _lock = new();
    
    public ReaderLock ReadLock() => new(_lock);
    public WriterLock WriteLock() => new(_lock);
    
    public void Dispose() => _lock.Dispose();
}