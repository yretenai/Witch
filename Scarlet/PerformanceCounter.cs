using System.Collections;
using System.Diagnostics;
using Serilog;

namespace Scarlet;

public interface IPerformanceCounter {
    void Print();

    public static Hashtable Counters { get; } = new();

    public static void PrintAll() {
    #if DEBUG
        foreach (IPerformanceCounter counter in Counters.Values) {
            counter.Print();
        }
    #endif
    }
}

[DebuggerNonUserCode, DebuggerStepThrough]
public sealed class PerformanceCounter<T> : IPerformanceCounter, IDisposable {
    // ReSharper disable once StaticMemberInGenericType
    private static int Count { get; set; }
    // ReSharper disable once StaticMemberInGenericType
    private static TimeSpan Elapsed { get; set; } = TimeSpan.Zero;
    private DateTimeOffset StartTime { get; set; }

    public PerformanceCounter() {
        Start();
    }


    [Conditional("DEBUG")]
    private void Start() {
        StartTime = DateTimeOffset.Now;
        IPerformanceCounter.Counters[typeof(T)] = this;
    }

    public void Dispose() {
        Stop();
    }

    [Conditional("DEBUG")]
    public void Stop() {
        if (StartTime == DateTimeOffset.MinValue) {
            return;
        }

        var endTime = DateTimeOffset.Now;
        var elapsed = endTime - StartTime;
        Elapsed += elapsed;
        Count++;

        StartTime = DateTimeOffset.MinValue;
    }

    public void Print() {
    #if DEBUG
        Log.Information("PerformanceCounter<{Type}>: {Average}ms avg ({Elapsed}ms total / {Count} runs)", typeof(T).FullName, (Elapsed / Count).TotalMilliseconds, Elapsed.TotalMilliseconds, Count);
    #endif
    }
}
