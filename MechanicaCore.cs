using Terraria.ModLoader;

namespace MechanicaCore
{
	public class MechanicaCore : Mod
	{
		public override void Load()
		{
			Logger.Info("Hello, Mechanica World!");
		}

		public override void Unload()
		{
			Logger.Info("Goodbye, Mechanica World!");
		}
	}
}
