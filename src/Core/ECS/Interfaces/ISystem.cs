using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MechanicaCore.Core.ECS.Interfaces;

/// <summary>
/// Represents a system that operates on entities matchin certain component requirments.
/// Each system encapsulates logic for a specific domain (e.g., transform, debug)
/// </summary>
public interface ISystem
{
  void Update(GameTime gameTime);

  void Draw(SpriteBatch spriteBatch);
}
