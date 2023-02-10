namespace Scarlet.Structures.Gfx.Model;

public interface IGraphicsModelBone {
    public string Name { get; set; }
    public uint LOD { get; set; }
}

public record struct GraphicsModelBone_Scarlet : IGraphicsModelBone {
    public string Name { get; set; }
    public uint LOD { get; set; }
    public uint Index { get; set; }
}

public record struct GraphicsModelBone_Black : IGraphicsModelBone {
    public string Name { get; set; }
    public uint LOD { get; set; }
}
