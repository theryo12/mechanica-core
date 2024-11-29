using System.Collections.Generic;
using System.Linq;
using log4net.Repository.Hierarchy;
using MechanicaCore.Core.ECS;
using MechanicaCore.Core.ECS.Components;
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
		public override void Load()
		{
			Logger.Info("Hello, Mechanica World!");
		}



		public override void Unload()
		{
			Logger.Info("Goodbye, Mechanica World!");
		}
	}

	public class MechanicaCoreSystem : ModSystem
	{
		private void DrawEntityDebug(SpriteBatch spriteBatch)
		{
			EntityManager.Instance.DrawDebug(spriteBatch, FontAssets.MouseText.Value, Mod);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"MechanicaCore: ECSDebug",
					delegate
					{
						DrawEntityDebug(Main.spriteBatch);
						return true;
					},
					InterfaceScaleType.Game)
				);
			}
		}

		public override void OnWorldLoad()
		{
			var entityManager = EntityManager.Instance;

			var entity = entityManager.CreateEntity();
			entity.Components =
			[
				new TransformComponent(Vector2.Zero, new Vector2(1000, 1000)),
				new DebugComponent("TestEntity", Color.Red)
			];

			entityManager.UpdateEntityKey(entity);
		}
	}
}
