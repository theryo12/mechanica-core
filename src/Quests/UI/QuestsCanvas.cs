using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace MechanicaCore.Quests.UI
{
  public class QuestCanvas : UIState
  {
    public static bool IsVisible { get; set; }

    public override void Draw(SpriteBatch spriteBatch)
    {
      Rectangle screenRectangle = new(0, 0, Main.screenWidth, Main.screenHeight);
      spriteBatch.Draw(TextureAssets.MagicPixel.Value, screenRectangle, Color.Black);
    }
  }
}