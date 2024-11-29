using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net.Repository.Hierarchy;
using MechanicaCore.Core.ECS;
using MechanicaCore.Core.ECS.Components;
using MechanicaCore.Core.ECS.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace MechanicaCore
{
	public class MechanicaCore : Mod
	{
		public static SystemManager SystemManager { get; private set; }

		public override void Load()
		{
			Logger.Info("Hello, Mechanica World!");

			SystemManager = new();
			SystemManager.AddSystems(this);
		}

		public override void Unload()
		{
			SystemManager = null;
			Logger.Info("Goodbye, Mechanica World!");
		}
	}

	public class MechanicaCoreSystem : ModSystem
	{
		private void DrawEntities(SpriteBatch spriteBatch)
		{
			MechanicaCore.SystemManager.DrawAll(spriteBatch);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"MechanicaCore: ECS",
					delegate
					{
						DrawEntities(Main.spriteBatch);
						return true;
					},
					InterfaceScaleType.Game)
				);
			}
		}

		public override void PostUpdateWorld()
		{
			MechanicaCore.SystemManager.UpdateAll(new GameTime());
		}
	}
}
