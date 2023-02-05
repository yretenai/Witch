using Scarlet.Structures.Math;

namespace Scarlet.Structures.Gfx.Model;

public interface IGraphicsModelData {
    public AABB BoundingBox { get; set; }
    public IGraphicsModelBone[] Bones { get; set; }
}

public record struct GraphicsModelData_Witch : IGraphicsModelData {
    public AABB BoundingBox { get; set; }
    [MessagePackRedirect<GraphicsModelBone_Witch[]>] public IGraphicsModelBone[] Bones { get; set; }
}
