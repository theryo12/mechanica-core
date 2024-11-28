using MechanicaCore.Quests.UI;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace MechanicaCore.Quests.Systems
{
  public partial class MechanicaCorePlayer : ModPlayer
  {
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
      if (KeybindSystem.QuestCanvasKeybind.JustReleased)
      {
        QuestCanvas.IsVisible = !QuestCanvas.IsVisible;
      }
    }
  }
}