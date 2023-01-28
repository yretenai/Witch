using Serilog;

namespace Scarlet;

public sealed class PerformanceCounter : IDisposable {
    private readonly string Name;
    private readonly DateTimeOffset StartTime;
    private readonly bool Write;

    public PerformanceCounter(string name, bool write = true) {
        Name = name;
        StartTime = DateTimeOffset.Now;
        Write = write;
    }

    public void Dispose() {
        var endTime = DateTimeOffset.Now;
        var elapsed = endTime - StartTime;
        if (Write) {
            Log.Debug($"[PERF] {Name}: {elapsed.TotalNanoseconds}ns");
        }
    }
}
