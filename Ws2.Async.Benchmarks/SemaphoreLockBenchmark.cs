﻿using BenchmarkDotNet.Attributes;
using Medallion.Threading.FileSystem;
using Ws2.Async.DistributedLock;
using Ws2.Async.Locks;
using Ws2.Async.Locks.PooledLocks;

namespace Ws2.Async.Benchmarks;

[SimpleJob]
public class PooledSemaphoreLockBenchmark
{
    [Params(10, 100, 1000, 10_000)]
    public static int RequestCount { get; set; }

    [Params(128, 1024)]
    public static int PoolSize { get; set; }

    private PooledSemaphoreLockFactory lockFactoryFactory = null!;
    private DistributedLockFactory distributedLockFactoryFactory = null!;
    private Task<int>[] tasks = null!;
    private static readonly DirectoryInfo LockFilesDirectory = new("./locks");

    [IterationSetup]
    public void Setup()
    {
        lockFactoryFactory = new PooledSemaphoreLockFactory(new SemaphoreSlimPool { Size = PoolSize });
        distributedLockFactoryFactory = new DistributedLockFactory(new FileDistributedSynchronizationProvider(LockFilesDirectory));
        tasks = new Task<int>[RequestCount];
    }

    [IterationCleanup]
    public void Cleanup()
    {
        lockFactoryFactory.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Benchmark]
    public int RunWithLock()
    {
        for (var i = 0; i < RequestCount; i++)
        {
            tasks[i] = RunJobWithLock(i);
        }

        var result = Task.WhenAll(tasks).Result;
        return result[0];
    }

    [Benchmark]
    public int RunWithDistributedLock()
    {
        for (var i = 0; i < RequestCount; i++)
        {
            tasks[i] = RunJobWithDistributedLock(i);
        }

        var result = Task.WhenAll(tasks).Result;
        return result[0];
    }

    private async Task<int> RunJobWithLock(int id)
    {
        await using var _ = await lockFactoryFactory.AcquireAsync(id, Timeout.InfiniteTimeSpan);
        return await RunJob(id);
    }

    private async Task<int> RunJobWithDistributedLock(int id)
    {
        await using var _ = await distributedLockFactoryFactory.AcquireAsync(id, Timeout.InfiniteTimeSpan);
        return await RunJob(id);
    }

    private async Task<int> RunJob(int id)
    {
        await Task.Delay(10);
        return 0;
    }
}