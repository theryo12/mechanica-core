using System;
using System.Collections.Generic;
using System.IO;

namespace MechanicaCore.Core.ECS.Interfaces;

/// <summary>
/// Represents a data-only component storing state and attributes of an entity.
/// Components are modular and contain only data with no associated logic.
/// They are the building blocks of the Entity Component System (ECS).
/// </summary>
public interface IComponent { }

/// <summary>
/// Defines a contract for components that expose their internal state and attributes 
/// for debugging purposes. Implementing this interface allows a component to provide 
/// a structured representation of its public fields and properties, facilitating the 
/// inspection of its data during runtime.
/// </summary>

public interface IDebuggableComponent : IComponent
{
  /// <summary>
  /// Returns a dictionary of field names and their values for debugging purposes.
  /// </summary>
  /// <returns>A dictionary where the key is the field name and the value is the field's value.</returns>
  Dictionary<string, object> GetDebugInfo();
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