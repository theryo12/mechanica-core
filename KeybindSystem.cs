using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using static Microsoft.Xna.Framework.Input.Keys;

namespace MechanicaCore
{
  public class KeybindSystem : ModSystem
  {
    public static ModKeybind AdditionalDebugKeybind { get; private set; }
    public static ModKeybind DebugKeybind { get; private set; }

    public override void Load()
    {
      AdditionalDebugKeybind = KeybindLoader.RegisterKeybind(Mod, "AdditionalDebug", LeftAlt);
      DebugKeybind = KeybindLoader.RegisterKeybind(Mod, "Debug", Q);
    }

    public override void Unload()
    {
      AdditionalDebugKeybind = null;
      DebugKeybind = null;
    }
  }

  public partial class MechanicaCorePlayer : ModPlayer
  {
    public bool DebugMode { get; private set; }
    public bool AdditionalDebugMode { get; private set; }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
      if (KeybindSystem.DebugKeybind.JustPressed)
      {
        var debugMode = DebugMode ? "OFF" : "ON";
        Main.NewText($"Debug Mode: {debugMode}");
        DebugMode = !DebugMode;
      }

      if (KeybindSystem.AdditionalDebugKeybind.Current)
        AdditionalDebugMode = true;
      else
        AdditionalDebugMode = false;
    }
  }
}