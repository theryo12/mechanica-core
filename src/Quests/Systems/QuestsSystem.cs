using MechanicaCore.Quests.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MechanicaCore.Quests.Systems
{
  public class QuestsSystem : ModSystem
  {
    private UserInterface questInterface;
    public static QuestCanvas QuestCanvas { get; private set; }


    public override void Load()
    {
      QuestCanvas = new QuestCanvas();
      questInterface = new UserInterface();
      questInterface.SetState(QuestCanvas);
    }

    public override void Unload()
    {
      questInterface.SetState(null);
      questInterface = null;
      QuestCanvas = null;
    }

    public override void UpdateUI(GameTime gameTime)
    {
      if (QuestCanvas.IsVisible)
      {
        questInterface?.Update(gameTime);
      }
    }

    public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
    {
      if (QuestCanvas.IsVisible)
      {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (index != -1)
        {
          layers.Insert(index, new LegacyGameInterfaceLayer(
              "MechanicaQuests: Quest Canvas",
              delegate
              {
                questInterface.Draw(Main.spriteBatch, new GameTime());
                return true;
              },
              InterfaceScaleType.UI)
          );
        }
      }
    }
  }
}