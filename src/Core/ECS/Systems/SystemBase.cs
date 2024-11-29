using System;
using System.Collections.Generic;
using System.Linq;
using MechanicaCore.Core.ECS;
using MechanicaCore.Core.ECS.Interfaces;
using MechanicaCore.Core.ECS.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MechanicaCore.Core.ECS.Systems;

/// <summary>
/// Represents a base system class, automatically managing matching entities.
/// Derived systems define their behavior by overriding <see cref="Update"/> or <see cref="Draw"/>
/// </summary>
public abstract class SystemBase(params Type[] requiredComponents) : ISystem
{
  private readonly List<IEntity> _matchingEntities = [];
  private readonly HashSet<Type> _requiredComponents = [.. requiredComponents];

  /// <summary>
  /// Updates the list of entities matching this system's requirements.
  /// </summary>
  /// <param name="entityManager">The entity manager providing entities to evaluate.</param>
  public void Refresh(EntityManager entityManager)
  {
    _matchingEntities.Clear();
    foreach (var entity in entityManager.Entities)
    {
      if (_requiredComponents.All(c => entity.GetComponent(c) != null))
      {
        _matchingEntities.Add(entity);
      }
    }
  }

  public void OnEntityChanged(IEntity entity)
  {
    if (_requiredComponents.All(c => entity.GetComponent(c) != null))
    {
      if (!_matchingEntities.Contains(entity))
        _matchingEntities.Add(entity);
    }
    else
    {
      _matchingEntities.Remove(entity);
    }
  }

  /// <summary>
  /// Gets the entities managed by this system.
  /// </summary>
  protected IReadOnlyList<IEntity> Entities => _matchingEntities;

  public abstract void Update(GameTime gameTime);
  public virtual void Draw(SpriteBatch spriteBatch) { }
}