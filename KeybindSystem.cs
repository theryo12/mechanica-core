using Terraria.ModLoader;

namespace MechanicaCore
{
  public class KeybindSystem : ModSystem
  {
    public static ModKeybind QuestCanvasKeybind { get; private set; }

    public override void Load()
    {
      QuestCanvasKeybind = KeybindLoader.RegisterKeybind(Mod, "QuestMenuToggle", "Q");
    }

    public override void Unload()
    {
      QuestCanvasKeybind = null;
    }
  }
}