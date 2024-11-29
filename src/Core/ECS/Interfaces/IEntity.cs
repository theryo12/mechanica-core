using System;
using System.Collections.Generic;

namespace MechanicaCore.Core.ECS.Interfaces;

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
