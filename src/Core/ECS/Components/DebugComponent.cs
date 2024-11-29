using System.Collections.Generic;
using MechanicaCore.Core.ECS.Interfaces;
using Microsoft.Xna.Framework;

namespace MechanicaCore.Core.ECS.Components
{
  public class DebugComponent : IDebuggableComponent
  {
    public string EntityName { get; }
    public Color DebugColor { get; }

    public DebugComponent(string entityName, Color debugColor)
    {
      EntityName = entityName;
      DebugColor = debugColor;
    }

    public Dictionary<string, object> GetDebugInfo()
    {
      return new Dictionary<string, object>
        {
            { nameof(DebugColor), DebugColor },
            { nameof(EntityName), EntityName }
        };
    }
  }
}