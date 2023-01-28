using System.Diagnostics;
using Serilog;

namespace Scarlet;

[DebuggerNonUserCode, DebuggerStepThrough]
public sealed class PerformanceCounter : IDisposable {
    private readonly string Name;
    private readonly DateTimeOffset StartTime;

    public PerformanceCounter(string name) {
        Name = name;
        StartTime = DateTimeOffset.Now;
    }

    public void Dispose() {
        var endTime = DateTimeOffset.Now;
        var elapsed = endTime - StartTime;
        Log.Debug($"[PERF] {Name}: {elapsed.TotalNanoseconds}ns");
    }
}
