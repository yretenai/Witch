using Scarlet.Structures.Math;

namespace Scarlet.Structures.Gfx.Model;

public interface IGraphicsModelData {
    public AABB BoundingBox { get; set; }
    public IGraphicsModelBone[] Bones { get; set; }
}

public record struct GraphicsModelData_Scarlet : IGraphicsModelData {
    public AABB BoundingBox { get; set; }
    [MessagePackRedirect<GraphicsModelBone_Scarlet[]>] public IGraphicsModelBone[] Bones { get; set; }
}
