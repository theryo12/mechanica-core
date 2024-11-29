using System.Linq;
using MechanicaCore.Core.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;

namespace MechanicaCore.Core.ECS.Systems
{
  public class DebugSystem : SystemBase
  {
    public DebugSystem() : base(typeof(TransformComponent), typeof(DebugComponent))
    {
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
      foreach (var entity in Entities)
      {
        var transform = entity.GetComponent<TransformComponent>();
        var debug = entity.GetComponent<DebugComponent>();

        var position = transform.Position - Main.screenPosition;
        var size = transform.Size;
        var color = debug.DebugColor;

        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y),
            color * 0.5f
        );

        var info = $"{debug.EntityName}\nComponents: {string.Join(", ", entity.GetComponents().Select(t => t.Name))}";
        spriteBatch.DrawString(FontAssets.MouseText.Value, info, position + new Vector2(5, 5), Color.White);
      }
    }
  }
}