using System;
using System.Collections.Generic;
using System.Linq;
using MechanicaCore.Core.ECS.Interfaces;

namespace MechanicaCore.Core.ECS.Managers;

/// <summary>
/// Manages the lifecycle of entities and their components in the ECS framework.
/// Provides utilities for efficient creation, retrieval, and management of entities.
/// </summary>
public class EntityManager
{
  private readonly Dictionary<int, EntityData> _entities = [];
  private int _nextEntityId;

  /// <summary>
  /// Gets all entities managed by the EntityManager.
  /// </summary>
  public IEnumerable<IEntity> Entities => _entities.Values.Select(data => data.Entity);

  private static readonly Lazy<EntityManager> InstanceHolder = new(() => new EntityManager());

  public event Action<IEntity>? OnEntityChanged;

  /// <summary>
  /// Prevents external instantiation. Use <see cref="Instance"/> to access the singleton.
  /// </summary>
  private EntityManager() { }

  /// <summary>
  /// Singleton instance of the <see cref="EntityManager"/>.
  /// </summary>
  public static EntityManager Instance => InstanceHolder.Value;

  /// <summary>
  /// Creates and registers a new entity within the manager.
  /// </summary>
  public IEntity CreateEntity()
  {
    var entity = new Entity();
    var id = _nextEntityId++;
    entity.OnComponentsChanged += HandleEntityChange;
    _entities[id] = new EntityData(entity, GenerateComponentKey(entity));
    return entity;
  }

  public void RemoveEntity(IEntity entity)
  {
    var entry = _entities.FirstOrDefault(kvp => kvp.Value.Entity == entity);
    if (entry.Key != default)
      _entities.Remove(entry.Key);
  }

  public IEnumerable<IEntity> GetEntitiesWithComponent<T>() where T : IComponent
  {
    var type = typeof(T);
    foreach (var (_, value) in _entities)
    {
      if (value.ComponentKey.ContainsComponent(type))
        yield return value.Entity;
    }
  }

  private void HandleEntityChange(IEntity entity)
  {
    var entry = _entities.FirstOrDefault(kvp => kvp.Value.Entity == entity);
    if (!entry.Equals(default(KeyValuePair<int, EntityData>)))
    {
      var entityId = entry.Key;

      var updatedKey = GenerateComponentKey(entity);
      _entities[entityId] = new EntityData(entity, updatedKey);

      OnEntityChanged?.Invoke(entity);
    }
  }

  private ComponentKey GenerateComponentKey(IEntity entity)
  {
    return ComponentKey.FromTypes(entity.GetComponents());
  }

  private record struct EntityData(IEntity Entity, ComponentKey ComponentKey);
}