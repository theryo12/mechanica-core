using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MechanicaCore.Core.ECS.Components;
using MechanicaCore.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MechanicaCore.Core.ECS
{
  /// <summary>
  /// Represents a data-only component used to store state and attributes of an entity.
  /// Components are the building blocks of the ECS system, containing only data 
  /// without logic, making entities modular and reusable.
  /// </summary>
  public interface IComponent { }

  /// <summary>
  /// Defines an entity that manages a collection of components.
  /// Entities are containers that aggregate components to define behavior and data.
  /// This interface provides methods for adding, retrieving, and removing components.
  /// </summary>
  public interface IEntity
  {
    /// <summary>
    /// Adds a component to the entity, replacing any existing component of the same type.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <param name="component">The instance of the component to add.</param>
    void AddComponent<T>(T component) where T : IComponent;

    /// <summary>
    /// Retrieves a specific component from the entity, if it exists.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <returns>The component instance, or <c>null</c> if not present.</returns>
    T? GetComponent<T>() where T : IComponent;

    /// <summary>
    /// Retrieves the types of all components currently associated with the entity.
    /// </summary>
    /// <returns>An enumerable collection of component types.</returns>
    IEnumerable<Type> GetComponents();

    /// <summary>
    /// Removes a specific component from the entity, if it exists.
    /// </summary>
    /// <typeparam name="T">The type of the component to remove.</typeparam>
    void RemoveComponent<T>() where T : IComponent;

    /// <summary>
    /// Gets or sets all components of the entity in bulk.
    /// Replacing components will overwrite the current set of components.
    /// </summary>
    IEnumerable<IComponent> Components { get; set; }
  }

  /// <summary>
  /// Represents a system that operates on entities containing specific components.
  /// Systems encapsulate logic for processing entities in the ECS framework.
  /// </summary>
  public interface ISystem
  {
    void Update(GameTime gameTime);
  }

  /// <summary>
  /// Represents a component that supports serialization and deserialization
  /// for network communication. This enables synchronization of component states
  /// across the server and clients in multiplayer environments.
  /// </summary>
  public interface INetSerializableComponent : IComponent
  {
    /// <summary>
    /// Serializes the component's state into a binary stream.
    /// Use this method to write all required fields for reconstructing the component
    /// during deserialization. This is typically used when sending data to clients.
    /// </summary>
    /// <param name="writer">A <see cref="BinaryWriter"/> used to write the component's data.</param>
    void Serialize(BinaryWriter writer);

    /// <summary>
    /// Deserializes the component's state from a binary stream.
    /// Use this method to restore the component's state using data received over the network.
    /// Ensure the deserialization order matches the serialization logic.
    /// </summary>
    /// <param name="reader">A <see cref="BinaryReader"/> used to read the component's data.</param>
    void Deserialize(BinaryReader reader);
  }


  /// <summary>
  /// Represents a basic entity implementation in the ECS system.
  /// Entities are containers for components, defining their data and behavior.
  /// </summary>
  public class Entity : IEntity
  {
    // stores components using their type as the key for quick lookup
    private readonly Dictionary<Type, object> _components = [];

    /// <inheritdoc/>
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
        }
      }
    }

    /// <inheritdoc/>
    public void AddComponent<T>(T component) where T : IComponent
    {
      SafeCheck.EnsureNotNull(component, nameof(component));

      _components[typeof(T)] = component;
    }

    /// <inheritdoc/>
    public T? GetComponent<T>() where T : IComponent
    {
      if (_components.TryGetValue(typeof(T), out var component))
        return (T)component;

      return default;
    }

    /// <inheritdoc/>
    public IEnumerable<Type> GetComponents()
    {
      return _components.Keys;
    }

    /// <inheritdoc/>
    public void RemoveComponent<T>() where T : IComponent
    {
      _components.Remove(typeof(T));
    }
  }

  /// <summary>
  /// Represents a unique identifier for a set of components.
  /// This structure is used to efficiently group and query entities
  /// with specific combinations of components.
  /// </summary>
  public struct ComponentKey : IEquatable<ComponentKey>
  {
    private static readonly Dictionary<Type, int> _typeToBitMap = [];
    private static int _currentBitIndex = 0;

    // the actual bitmask data
    private readonly ulong[] _bitMask;

    private ComponentKey(ulong[] bitMask)
    {
      _bitMask = bitMask;
    }

    /// <summary>
    /// Creates a unique <see cref="ComponentKey"/> for the specified component types.
    /// </summary>
    /// <param name="types">The types of components to include in the key.</param>
    /// <returns>A <see cref="ComponentKey"/> representing the specified components.</returns>
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
    /// Determines if this key contains the specified component type.
    /// </summary>
    /// <param name="type">The component type to check for.</param>
    /// <returns><c>true</c> if the component is present; otherwise, <c>false</c>.</returns>
    public bool ContainsComponent(Type type)
    {
      if (!_typeToBitMap.TryGetValue(type, out var bitIndex))
        return false;

      var maskIndex = bitIndex / 64;
      var bitPosition = bitIndex % 64;
      return (_bitMask[maskIndex] & (1UL << bitPosition)) != 0;
    }


    /// <summary>
    /// Checks if this key matches another, ensuring all bits in the other key
    /// are set in this key.
    /// </summary>
    /// <param name="other">The key to compare with.</param>
    /// <returns><c>true</c> if all bits in the other key are present; otherwise, <c>false</c>.</returns>
    public bool Matches(ComponentKey other)
    {
      for (int i = 0; i < _bitMask.Length; i++)
      {
        if ((other._bitMask[i] & _bitMask[i]) != other._bitMask[i])
          return false;
      }
      return true;
    }

    /// <inheritdoc/>
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
  /// Manages the lifecycle of entities and their components.
  /// Provides functionality for creating, retrieving, and removing entities.
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

    /// <summary>
    /// Singleton instance of the <see cref="EntityManager"/>.
    /// </summary>
    public static EntityManager Instance => _instance.Value;


    /// <summary>
    /// Creates a new entity and registers it within the manager.
    /// </summary>
    /// <returns>The newly created entity.</returns>
    public IEntity CreateEntity()
    {
      var entity = new Entity();
      var id = _nextEntityId++;

      _entities[id] = new EntityData(entity, GenerateComponentKey(entity));
      return entity;
    }

    public void DrawDebug(SpriteBatch spriteBatch, DynamicSpriteFont font, Mod mod)
    {
      foreach (var entry in _entities.Values)
      {
        var entity = entry.Entity;

        var transform = entity.GetComponent<TransformComponent>();
        var debugComponent = entity.GetComponent<DebugComponent>();

        if (transform == null || debugComponent == null)
          continue;

        var position = transform.Position - Main.screenPosition;
        var size = transform.Size;
        var color = debugComponent.DebugColor;
        var name = debugComponent.EntityName;

        spriteBatch.Draw(
          texture: TextureAssets.MagicPixel.Value,
          destinationRectangle: new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y),
          color: color * 0.5f
        );

        var info = $"{name}\nComponents: {string.Join(", ", entity.GetComponents().Select(t => t.Name))}";
        spriteBatch.DrawString(font, info, position + new Vector2(5, 5), Color.White);
      }
    }

    /// <summary>
    /// Deletes an entity and removes it from the manager.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
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