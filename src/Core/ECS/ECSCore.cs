// i hope this system works well and it was worth it.
// - ryo

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MechanicaCore.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace MechanicaCore.Core.ECS
{
  /// <summary>
  /// Represents a data-only component storing state and attributes of an entity.
  /// Components are modular and contain only data with no associated logic.
  /// They are the building blocks of the Entity Component System (ECS).
  /// </summary>
  public interface IComponent { }

  /// <summary>
  /// Represents a container for components, defining data and behavior.
  /// Entities in the ECS pattern are modular, combining components dynamically.
  /// </summary>
  public interface IEntity
  {
    /// <summary>
    /// Adds or replaces a component of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of component to add or replace.</typeparam>
    /// <param name="component">The component instance to associate with the entity.</param>
    void AddComponent<T>(T component) where T : IComponent;

    /// <summary>
    /// Retrieves a component of the specified type, or <c>null</c> if not present.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <returns>The retrieved component, or <c>null</c> if it does not exist.</returns>
    T? GetComponent<T>() where T : IComponent;

    /// <summary>
    /// Gets a component by its type.
    /// </summary>
    /// <param name="componentType">The type of component to retrieve.</param>
    /// <returns>The component instance, or <c>null</c> if it does not exist.</returns>
    IComponent? GetComponent(Type componentType);

    /// <summary>
    /// Enumerates all types of components currently associated with the entity.
    /// </summary>
    /// <returns>An enumeration of component types.</returns>
    IEnumerable<Type> GetComponents();

    /// <summary>
    /// Removes a component of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the component to remove.</typeparam>
    void RemoveComponent<T>() where T : IComponent;

    /// <summary>
    /// Replaces the entity's components in bulk.
    /// </summary>
    IEnumerable<IComponent> Components { get; set; }
  }

  /// <summary>
  /// A component capable of serialization/deserialization for networking.
  /// Ensures synchronization across server-client environments.
  /// </summary>
  public interface INetSerializableComponent : IComponent
  {
    /// <summary>
    /// Serializes the component's state into a binary stream for network transmission.
    /// </summary>
    /// <param name="writer">A binary writer to write the component's state.</param>
    void Serialize(BinaryWriter writer);

    /// <summary>
    /// Restores the component's state from a binary stream.
    /// </summary>
    /// <param name="reader">A binary reader to read the component's state.</param>
    void Deserialize(BinaryReader reader);

    event Action<IEntity> OnComponentsChanged;
  }

  /// <summary>
  /// Represents a system that operates on entities matchin certain component requirments.
  /// Each system encapsulates logic for a specific domain (e.g., transform, debug)
  /// </summary>
  public interface ISystem
  {
    void Update(GameTime gameTime);

    void Draw(SpriteBatch spriteBatch);
  }

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

  /// <summary>
  /// Represents a fundamental implementation of the <see cref="IEntity"/> interface.
  /// Manages a dynamic collection of components.
  /// </summary>
  public class Entity : IEntity
  {
    private readonly Dictionary<Type, object> _components = [];

    /// <inheritdoc />
    public IEnumerable<IComponent> Components
    {
      get => _components.Values.Cast<IComponent>();
      set
      {
        _components.Clear();
        foreach (var component in value)
        {
          var componentType = component.GetType();
          _components[componentType] = component;
          NotifyChanges();
        }
      }
    }

    public event Action<IEntity>? OnComponentsChanged;

    /// <inheritdoc />
    public void AddComponent<T>(T component) where T : IComponent
    {
      SafeCheck.EnsureNotNull(component, nameof(component));
      _components[typeof(T)] = component;
      NotifyChanges();
    }

    /// <inheritdoc />
    public T? GetComponent<T>() where T : IComponent
    {
      return _components.TryGetValue(typeof(T), out var component) ? (T)component : default;
    }

    /// <inheritdoc />
    public IComponent? GetComponent(Type componentType)
    {
      return _components.TryGetValue(componentType, out var component) ? (IComponent)component : null;
    }

    /// <inheritdoc />
    public IEnumerable<Type> GetComponents() => _components.Keys;

    /// <inheritdoc />
    public void RemoveComponent<T>() where T : IComponent
    {
      _components.Remove(typeof(T));
      NotifyChanges();
    }

    private void NotifyChanges()
    {
      OnComponentsChanged?.Invoke(this);
    }
  }

  /// <summary>
  /// Represents a unique identifier for a set of components, optimized for fast lookups.
  /// </summary>
  public readonly struct ComponentKey : IEquatable<ComponentKey>
  {
    private const int MaxComponentTypes = 256; // limit to 256 types for performance
    private static readonly ConcurrentDictionary<Type, int> TypeToBitIndex = new();
    private static int _currentBitIndex;
    private readonly ulong[] _bitMask;

    private ComponentKey(ulong[] bitMask) => _bitMask = bitMask;

    /// <summary>
    /// Generates a <see cref="ComponentKey"/> for the specified component types.
    /// </summary>
    /// <param name="types">The types of components to include.</param>
    /// <returns>A <see cref="ComponentKey"/> representing the components.</returns>
    public static ComponentKey FromTypes(IEnumerable<Type> types)
    {
      var bitMask = new ulong[(MaxComponentTypes + 63) / 64];
      foreach (var type in types)
      {
        var bitIndex = TypeToBitIndex.GetOrAdd(type, _ => _currentBitIndex++);
        var maskIndex = bitIndex / 64;
        var bitPosition = bitIndex % 64;
        bitMask[maskIndex] |= 1UL << bitPosition;
      }
      return new ComponentKey(bitMask);
    }

    public bool ContainsComponent(Type type)
    {
      if (!TypeToBitIndex.TryGetValue(type, out var bitIndex))
        return false;
      var maskIndex = bitIndex / 64;
      var bitPosition = bitIndex % 64;
      return (_bitMask[maskIndex] & (1UL << bitPosition)) != 0;
    }

    public bool Equals(ComponentKey other)
    {
      return _bitMask.AsSpan().SequenceEqual(other._bitMask.AsSpan());
    }

    public override bool Equals(object? obj)
    {
      return obj is ComponentKey other && Equals(other);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(_bitMask[0], _bitMask[1], _bitMask[2], _bitMask[3]);
    }

    public static bool operator ==(ComponentKey left, ComponentKey right) => left.Equals(right);
    public static bool operator !=(ComponentKey left, ComponentKey right) => !(left == right);
  }

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
}
