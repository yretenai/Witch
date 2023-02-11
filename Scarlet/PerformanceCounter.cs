using Serilog;

namespace Scarlet;

public interface IPerformanceCounter {
    public static Dictionary<Type, IPerformanceCounter> Counters { get; } = new();
    void Print();

    public static void PrintAll() {
    #if DEBUG
        foreach (var counter in Counters.Values) {
            counter.Print();
        }
    #endif
    }
}

[DebuggerNonUserCode] [DebuggerStepThrough]
public sealed class PerformanceCounter<T> : IPerformanceCounter, IDisposable {
    public PerformanceCounter() {
        Start();
    }

    // ReSharper disable once StaticMemberInGenericType
    private static int Count { get; set; }

    // ReSharper disable once StaticMemberInGenericType
    private static TimeSpan Elapsed { get; set; } = TimeSpan.Zero;
    private DateTimeOffset StartTime { get; set; }

    public void Dispose() {
        Stop();
    }

    public void Print() {
    #if DEBUG
        Log.Information("PerformanceCounter<{Type}>: {Average}ms avg ({Elapsed}ms total / {Count} runs)", typeof(T).FullName, (Elapsed / Count).TotalMilliseconds, Elapsed.TotalMilliseconds, Count);
    #endif
    }


    [Conditional("DEBUG")]
    private void Start() {
        StartTime = DateTimeOffset.Now;
        IPerformanceCounter.Counters[typeof(T)] = this;
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
}
