using Microsoft.Xna.Framework;

namespace MechanicaCore.Core.ECS.Components
{
  public class TransformComponent : IComponent
  {
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }

    public TransformComponent(Vector2 position, Vector2 size)
    {
      Position = position;
      Size = size;
    }
  }
}