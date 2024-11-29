
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MechanicaCore.Core.ECS.Interfaces;
using MechanicaCore.Core.ECS.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace MechanicaCore.Core.ECS.Managers;

/// <summary>
/// Manages all systems, delegating updates and drawing to each system.
/// </summary>
public class SystemManager
{
  private readonly List<ISystem> _systems = [];

  public SystemManager()
  {
    EntityManager.Instance.OnEntityChanged += HandleEntityChange;
  }

  /// <summary>
  /// Automatically adds every system to SystemManager.
  /// </summary>
  public void AddSystems(Mod mod)
  {
    var systemTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => typeof(SystemBase).IsAssignableFrom(t) && !t.IsAbstract);

    foreach (var systemType in systemTypes)
    {
      var systemInstance = (ISystem)Activator.CreateInstance(systemType);
      AddSystem(systemInstance);
      mod.Logger.Info($"Added system: {systemType.Name}");
    }
  }

  public void AddSystem(ISystem system)
  {
    _systems.Add(system);
    if (system is SystemBase baseSystem)
      baseSystem.Refresh(EntityManager.Instance);
  }

  public void UpdateAll(GameTime gameTime)
  {
    foreach (var system in _systems)
      system.Update(gameTime);
  }

  public void DrawAll(SpriteBatch spriteBatch)
  {
    foreach (var system in _systems)
      system.Draw(spriteBatch);
  }

  private void HandleEntityChange(IEntity entity)
  {
    foreach (var system in _systems.OfType<SystemBase>())
      system.OnEntityChanged(entity);
  }
}
