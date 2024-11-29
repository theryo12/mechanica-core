using System.Linq;
using MechanicaCore.Core.ECS.Components;
using MechanicaCore.Core.ECS.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

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
      var player = Main.LocalPlayer.GetModPlayer<MechanicaCorePlayer>();
      foreach (var entity in Entities)
      {
        if (player.DebugMode)
        {
          var transform = entity.GetComponent<TransformComponent>();
          var debug = entity.GetComponent<DebugComponent>();

          var position = transform.Position - Main.screenPosition;
          var size = transform.Size;
          var color = debug.DebugColor;
          var entityRect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);

          spriteBatch.Draw(
              TextureAssets.MagicPixel.Value,
              entityRect,
              color * 0.5f
          );

          if (MouseUtils.MouseIntersects(entityRect))
          {
            var components = entity.Components
                .Select(component =>
                {
                  if (player.AdditionalDebugMode && component is IDebuggableComponent debuggable)
                  {
                    var debugInfo = debuggable.GetDebugInfo()
                        .Select(kvp => $"{kvp.Key} = {kvp.Value}")
                        .ToList();
                    return $"{component.GetType().Name} = {{ {string.Join("; ", debugInfo)} }}";
                  }

                  return $"{component.GetType().Name}";
                });


            var info = $"{debug.EntityName}\nComponents:\n{string.Join("\n", components)}";
            var assignedKeys = string.Join(", ", KeybindSystem.AdditionalDebugKeybind.GetAssignedKeys()
              .Select(key => key.ToString()));

            if (!player.AdditionalDebugMode)
            {
              info += $"\nHold <{assignedKeys}> for more info.";
            }

            Main.instance.MouseText(info, 0, 0);
          }
        }
      }
    }
  }
}