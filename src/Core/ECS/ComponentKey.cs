using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MechanicaCore.Core.ECS;

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