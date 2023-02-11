using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Archive;
using Scarlet.Structures;

namespace Scarlet.Tests.Archive;

public class EREPTests {
    [Fact]
    public void ShouldDeserializeEREPSingle() {
        using var owner = MemoryOwner<byte>.Allocate(16);
        owner.Span[0] = 1;
        owner.Span[8] = 2;

        var erep = new EbonyReplace(new AssetId(TypeIdRegistry.EREP), owner);

        Assert.Equal(1U, erep.Replacements.Keys.First().Value);
        Assert.Equal(2U, erep.Replacements.Values.First().Value);
    }

    [Fact]
    public void ShouldDeserializeEREPMultiple() {
        using var owner = MemoryOwner<byte>.Allocate(32);
        owner.Span[0] = 1;
        owner.Span[8] = 2;
        owner.Span[16] = 3;
        owner.Span[24] = 4;

        var erep = new EbonyReplace(new AssetId(TypeIdRegistry.EREP), owner);

        Assert.Equal(1U, erep.Replacements.Keys.First().Value);
        Assert.Equal(2U, erep.Replacements.Values.First().Value);
        Assert.Equal(3U, erep.Replacements.Keys.Last().Value);
        Assert.Equal(4U, erep.Replacements.Values.Last().Value);
    }

    [Fact]
    public void ShouldDeserializeEREPZero() {
        using var owner = MemoryOwner<byte>.Allocate(0);

        var erep = new EbonyReplace(new AssetId(TypeIdRegistry.EREP), owner);

        Assert.Empty(erep.Replacements);
    }
}
