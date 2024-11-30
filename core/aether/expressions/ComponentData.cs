using System;

namespace Aether.Expressions;

public struct ComponentData<T>(uint size) where T : unmanaged
{
  /// <summary>
  /// Raw data block.
  /// </summary>
  private T[] _data = new T[size];

  /// <summary>
  /// Sparse array mapping of entity ID to the location of data.
  /// </summary>
  private readonly uint[] _sparse = new uint[size];

  public void AllocateData(uint size)
  {
    _data = new T[size];
  }

  public void ResizeData(uint newSize)
  {
    Array.Resize(ref _data, (int)newSize);
  }

  public readonly void SetEntityDataLocation(uint entityId, uint location)
  {
    if (entityId >= _sparse.Length)
      throw new ArgumentOutOfRangeException(nameof(entityId));

    _sparse[entityId] = location;
  }

  public readonly T this[uint entityId, uint offset]
  {
    get
    {
      uint location = _sparse[entityId];
      return _data[location + offset];
    }
    set
    {
      uint location = _sparse[entityId];
      _data[location + offset] = value;
    }
  }
}