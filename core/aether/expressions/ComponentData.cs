using System;
using Terraria.DataStructures;

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
  private uint[] _sparse = new uint[size];

  /// <summary>
  /// Tracks the next avaliable index for new data.
  /// </summary>
  private uint _nextAvailableIndex = 0;

  private void EnsureCapacity(uint requiredSize)
  {
    if (requiredSize > _data.Length)
      ResizeData(requiredSize * 2);

    if (requiredSize > _sparse.Length)
      ResizeSparse(requiredSize * 2);
  }

  public void ResizeData(uint newSize)
  {
    Array.Resize(ref _data, (int)newSize);
  }

  public void ResizeSparse(uint newSize)
  {
    Array.Resize(ref _sparse, (int)newSize);
  }

  public readonly ref T Get(uint entityId)
  {
    if (entityId >= _sparse.Length || _sparse[entityId] == 0)
      throw new ArgumentOutOfRangeException(nameof(entityId), "Entity does not have assigned data.");
    return ref _data[_sparse[entityId]];
  }

  public void Set(uint entityId, T value)
  {
    EnsureCapacity(entityId + 1);

    if (_sparse[entityId] == 0 && _nextAvailableIndex < _data.Length)
      _sparse[entityId] = _nextAvailableIndex++;

    _data[_sparse[entityId]] = value;
  }
}