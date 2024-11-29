using System.Collections.Generic;
using System.IO;
using MechanicaCore.Core.ECS.Interfaces;
using Microsoft.Xna.Framework;

namespace MechanicaCore.Core.ECS.Components
{
  public class TransformComponent : IDebuggableComponent
  {
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }

    public TransformComponent(Vector2 position, Vector2 size)
    {
      Position = position;
      Size = size;
    }

    public Dictionary<string, object> GetDebugInfo()
    {
      return new Dictionary<string, object>
        {
            { nameof(Position), Position },
            { nameof(Size), Size }
        };
    }
  }

}