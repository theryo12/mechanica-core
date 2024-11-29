using Microsoft.Xna.Framework;

namespace MechanicaCore.Core.ECS.Components
{
  /// <summary>
  /// Represents debug visualization settings for an entity
  /// </summary>
  public class DebugComponent : IComponent
  {
    public string EntityName { get; }
    public Color DebugColor { get; }

    public DebugComponent(string entityName, Color debugColor)
    {
      EntityName = entityName;
      DebugColor = debugColor;
    }
  }
}