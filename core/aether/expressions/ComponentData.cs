using System;

namespace Aether.Expressions;

/// <summary>
/// Represents a collection of component data indexed by entity IDs.
/// This structure maps each entity ID to specific data while ensuring efficient
/// access and growth as new data is added.
/// </summary>
/// <typeparam name="T">The type of data associated with each entity. Must be unmanaged.</typeparam>
public struct ComponentData<T>(uint size) where T : unmanaged
{
  /// <summary>
  /// Raw data block that stores the component data.
  /// The data is stored in a contiguous array, with each entry corresponding
  /// to an index assigned via the sparse array.
  /// </summary>
  private T[] _data = new T[size];

  /// <summary>
  /// Sparse array mapping entity IDs to the index location in the _data array.
  /// A value of zero means the entity does not yet have associated data.
  /// </summary>
  private uint[] _sparse = new uint[size];

  /// <summary>
  /// Tracks the next avaliable index for new data.
  /// </summary>
  private uint _nextAvailableIndex = 0;

  /// <summary>
  /// Ensures that the data and sparse arrays have enough capacity to accommodate the required size.
  /// If either array is smaller than the required size, it is resized by doubling its current length.
  /// </summary>
  /// <param name="requiredSize">The minimum size required for both arrays.</param>
  private void EnsureCapacity(uint requiredSize)
  {
    if (requiredSize > _data.Length)
      ResizeData(requiredSize * 2);

    if (requiredSize > _sparse.Length)
      ResizeSparse(requiredSize * 2);
  }

  /// <summary>
  /// Resizes the data array to the specified size.
  /// This method is called when the data array is not large enough to accommodate new entries.
  /// </summary>
  /// <param name="newSize">The new size for the data array.</param>
  public void ResizeData(uint newSize)
  {
    Array.Resize(ref _data, (int)newSize);
  }

  /// <summary>
  /// Resizes the sparse array to the specified size.
  /// This method is called when the sparse array is not large enough to accommodate new entity IDs.
  /// </summary>
  /// <param name="newSize">The new size for the sparse array.</param>
  public void ResizeSparse(uint newSize)
  {
    Array.Resize(ref _sparse, (int)newSize);
  }

  /// <summary>
  /// Retrieves the component data associated with the specified entity ID.
  /// If the entity does not have assigned data, an <see cref="ArgumentOutOfRangeException"/> is thrown.
  /// </summary>
  /// <param name="entityId">The ID of the entity whose data is being retrieved.</param>
  /// <returns>A reference to the component data for the specified entity.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if the entity ID does not have associated data.</exception>
  public readonly ref T Get(uint entityId)
  {
    if (entityId >= _sparse.Length || _sparse[entityId] == 0)
      throw new ArgumentOutOfRangeException(nameof(entityId), "Entity does not have assigned data.");
    return ref _data[_sparse[entityId]];
  }

  /// <summary>
  /// Sets the component data for a specific entity ID.
  /// If the entity does not yet have assigned data, it will be allocated and the data will be set.
  /// </summary>
  /// <param name="entityId">The ID of the entity whose data is being set.</param>
  /// <param name="value">The component data value to assign to the entity.</param>
  public void Set(uint entityId, T value)
  {
    EnsureCapacity(entityId + 1);

    if (_sparse[entityId] == 0 && _nextAvailableIndex < _data.Length)
      _sparse[entityId] = _nextAvailableIndex++;

    _data[_sparse[entityId]] = value;
  }
}