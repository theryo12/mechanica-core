using System;
using System.Collections.Generic;
using System.Linq;
using MechanicaCore.Core.ECS.Interfaces;
using MechanicaCore.Utilities;

namespace MechanicaCore.Core.ECS;

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