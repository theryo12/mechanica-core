using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using MechanicaCore.Utilities;
using Microsoft.Xna.Framework;

namespace MechanicaCore.Core.ECS
{
  /// <summary>
  /// Represents a data-only component
  /// Components are used to store data for entities
  /// </summary>
  public interface IComponent { }

  /// <summary>
  /// Represents an entity that manages a collection of components
  /// </summary>
  public interface IEntity
  {
    /// <summary>
    /// Adds a component to the entity
    /// </summary>
    /// <typeparam name="T">The type of component</typeparam>
    /// <param name="component">The instance of the component</param>
    void AddComponent<T>(T component) where T : IComponent;

    /// <summary>
    /// Retrieves a component from the entity
    /// </summary>
    /// <typeparam name="T">The type of component</typeparam>
    /// <returns>The component instance, if it exists</returns>
    T? GetComponent<T>() where T : IComponent;

    /// <summary>
    /// Retrieves the types of all components currently associated with this entity.
    /// </summary>
    /// <returns>An enumerable of component types.</returns>
    IEnumerable<Type> GetComponents();

    /// <summary>
    /// Removes a component from the entity
    /// </summary>
    /// <typeparam name="T">The type of the component</typeparam>
    void RemoveComponent<T>() where T : IComponent;
  }

  /// <summary>
  /// Represents a system that processes entities with specific components
  /// </summary>
  public interface ISystem
  {
    void Update(GameTime gameTime);
  }


  /// <summary>
  /// Represents an entity in the ECS system
  /// Entities are containers for components that define their behavior and data
  /// </summary>
  public class Entity : IEntity
  {
    // stores components using their type as the key for quick lookup
    private readonly Dictionary<Type, object> _components = [];

    /// <summary>
    /// Gets or sets the components of the entity in bulk.
    /// Setting this property will replace all existing components.
    /// </summary>
    public IEnumerable<IComponent> Components
    {
      get => _components.Values.Cast<IComponent>();
      set
      {
        _components.Clear();

        foreach (var component in value)
        {
          AddComponent(component);
        }
      }
    }

    /// <summary>
    /// Adds a component to this entity.
    /// If a component of the same type already exists, it will be replaced
    /// </summary>
    /// <typeparam name="T">The type of component you're adding</typeparam>
    /// <param name="component">The actual component instance to add</param>
    /// <exception cref="ArgumentNullException">Thrown if the component is null</exception>
    public void AddComponent<T>(T component) where T : IComponent
    {
      SafeCheck.EnsureNotNull(component, nameof(component));

      _components[typeof(T)] = component;
    }

    /// <summary>
    /// Retrieves a component of specific type from this entity
    /// If the component isn't found, returns null
    /// </summary>
    /// <typeparam name="T">The type of component you're looking for</typeparam>
    /// <returns>The component if it exists, or null if it doesn't</returns>
    public T? GetComponent<T>() where T : IComponent
    {
      if (_components.TryGetValue(typeof(T), out var component))
        return (T)component;

      return default;
    }

    /// <summary>
    /// Retrieves the types of all components currently associated with this entity.
    /// </summary>
    /// <returns>An enumerable of component types.</returns>
    public IEnumerable<Type> GetComponents()
    {
      return _components.Keys;
    }

    /// <summary>
    /// Removes a component of specific type from this entity
    /// If entity doesn't have that component, nothing happens
    /// </summary>
    /// <typeparam name="T">The type of component you want to remove</typeparam>
    public void RemoveComponent<T>() where T : IComponent
    {
      _components.Remove(typeof(T));
    }
  }

  /// <summary>
  /// Represents a unique indentifier for a set of components
  /// Used to organize entities into efficient processing groups
  /// </summary>
  public struct ComponentKey : IEquatable<ComponentKey>
  {
    private static readonly Dictionary<Type, int> _typeToBitMap = [];
    private static int _currentBitIndex = 0;

    // the actual bitmask data
    private readonly ulong[] _bitMask;

    /// <summary>
    /// Initializes a new ComponentKey with the specified bitmask data
    /// </summary>
    /// <param name="bitMask">The bitmask data representing this key</param>
    private ComponentKey(ulong[] bitMask)
    {
      _bitMask = bitMask;
    }

    /// <summary>
    /// Generates a ComponentKey for the specified component types
    /// </summary>
    /// <param name="types">The types of components to include in the key</param>
    /// <returns>A unique key representing the combination of components</returns>
    public static ComponentKey FromTypes(IEnumerable<Type> types)
    {
      var bitMask = new ulong[4]; // up to 256 component types

      foreach (var type in types)
      {
        if (!_typeToBitMap.TryGetValue(type, out var bitIndex))
        {
          bitIndex = _currentBitIndex++;
          _typeToBitMap[type] = bitIndex;
        }

        var maskIndex = bitIndex / 64;
        var bitPosition = bitIndex % 64;
        bitMask[maskIndex] |= 1UL << bitPosition;
      }

      return new ComponentKey(bitMask);
    }

    /// <summary>
    /// Checks if this key contains the specified component type
    /// </summary>
    /// <param name="type">The type of the component to check</param>
    /// <returns>True if the component is represented in the key; otherwise, false</returns>
    public bool ContainsComponent(Type type)
    {
      if (!_typeToBitMap.TryGetValue(type, out var bitIndex))
        return false;

      var maskIndex = bitIndex / 64;
      var bitPosition = bitIndex % 64;
      return (_bitMask[maskIndex] & (1UL << bitPosition)) != 0;
    }


    /// <summary>
    /// Checks if this key matches another, meaning all bits in the other key are set in this key
    /// </summary>
    /// <param name="other">The key to check against</param>
    /// <returns>True if this keys contains all bits in the other key; otherwise, false</returns>
    public bool Matches(ComponentKey other)
    {
      for (int i = 0; i < _bitMask.Length; i++)
      {
        if ((other._bitMask[i] & _bitMask[i]) != other._bitMask[i])
          return false;
      }
      return true;
    }

    /// <summary>
    /// Compares this key to another for equality
    /// </summary>
    /// <param name="other">The other key to compare</param>
    /// <returns>True if the keys are equal; otherwise, false</returns>
    public bool Equals(ComponentKey other)
    {
      for (int i = 0; i < _bitMask.Length; i++)
      {
        if (_bitMask[i] != other._bitMask[i])
          return false;
      }
      return true;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
      return obj is ComponentKey other && Equals(other);
    }

    public override int GetHashCode()
    {
      int hash = 17;
      foreach (var value in _bitMask)
      {
        hash = hash * 31 + value.GetHashCode();
      }
      return hash;
    }

    public override string ToString()
    {
      return string.Join(",", _bitMask);
    }

    public static bool operator ==(ComponentKey left, ComponentKey right) => left.Equals(right);

    public static bool operator !=(ComponentKey left, ComponentKey right) => !(left == right);
  }

  /// <summary>
  /// Manages entities and their components
  /// </summary>
  public class EntityManager
  {
    private readonly Dictionary<int, EntityData> _entities = [];

    private int _nextEntityId = 0;

    private static readonly Lazy<EntityManager> _instance = new(() => new EntityManager());

    /// <summary>
    /// Prevents external instantiation of the EntityManager
    /// Use <see cref="Instance"/> to access the singleton
    /// </summary>
    private EntityManager() { }

    public static EntityManager Instance => _instance.Value;


    /// <summary>
    /// Creates a new entity and registers it within the manager
    /// The entity starts with no components, but they can be added later
    /// </summary>
    /// <returns>A newly created entity</returns>
    public IEntity CreateEntity()
    {
      var entity = new Entity();
      var id = _nextEntityId++;

      _entities[id] = new EntityData(entity, GenerateComponentKey(entity));
      return entity;
    }

    /// <summary>
    /// Removes an entity from the manager, including all of its associated components
    /// </summary>
    /// <param name="entity">The entity to remove</param>
    public void RemoveEntity(IEntity entity)
    {
      var id = _entities.FirstOrDefault(e => e.Value.Entity == entity).Key;
      if (id != default)
      {
        _entities.Remove(id);
      }
    }

    /// <summary>
    /// Retrieves all entities that have the specified component type
    /// </summary>
    /// <typeparam name="T">The type of component to filter entities by</typeparam>
    /// <returns>An enumerable of entities containing the specified component</returns>
    public IEnumerable<IEntity> GetEntitiesWithComponent<T>() where T : IComponent
    {
      var targetType = typeof(T);

      foreach (var entry in _entities.Values)
      {
        if (entry.ComponentKey.ContainsComponent(targetType))
        {
          yield return entry.Entity;
        }
      }
    }

    /// <summary>
    /// Recalculates the ComponentKey for a given entity after components have been modified
    /// This ensures the entity remains correctly indexed for efficient querying
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public void UpdateEntityKey(IEntity entity)
    {
      var id = _entities.FirstOrDefault(e => e.Value.Entity == entity).Key;
      if (id != default)
      {
        _entities[id] = new EntityData(entity, GenerateComponentKey(entity));
      }
    }

    /// <summary>
    /// Generates a unique ComponentKey for an entity based on its current set of components
    /// This key is used to efficiently group and query entities
    /// </summary>
    /// <param name="entity">The entity for which to generate the key</param>
    /// <returns>A ComponentKey representing the entity's components</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ComponentKey GenerateComponentKey(IEntity entity)
    {
      var types = entity.GetComponents();
      return ComponentKey.FromTypes(types);
    }

    /// <summary>
    /// Internal data structure to associate entities with their ComponentKeys
    /// </summary>
    private record struct EntityData(IEntity Entity, ComponentKey ComponentKey);
  }
}