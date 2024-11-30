using System;
using System.Buffers;

namespace aether.expressions;
public unsafe struct ComponentData(uint sizeInBytes, byte alignment)
{
  /// <summary>
  /// The alignment of the component in memory.
  /// </summary>
  public byte Alignment = alignment;

  /// <summary>
  /// The size of the component in bytes, including padding.
  /// </summary>
  public uint SizeInBytes = sizeInBytes;

  /// <summary>
  /// The length of component's data (in terms of the number of items).
  /// </summary>
  public uint DataLength = 0;

  /// <summary>
  /// The memory block that holds the component's data
  /// </summary>
  private byte[]? _data = null;

  /// <summary>
  /// The mapping of entity ID to the location of data in the memory block 
  /// </summary>
  private uint[]? _sparse = null;

  public void AllocateData()
  {
    // use ArrayPool to rent memory
    _data = ArrayPool<byte>.Shared.Rent((int)SizeInBytes);
    _sparse = ArrayPool<uint>.Shared.Rent((int)DataLength);
  }

  public void FreeData()
  {
    if (_data != null)
    {
      ArrayPool<byte>.Shared.Return(_data, clearArray: true);
      _data = null;
    }

    if (_sparse != null)
    {
      ArrayPool<uint>.Shared.Return(_sparse, clearArray: true);
      _sparse = null;
    }
  }

  public readonly void SetEntityDataLocation(uint entityId, uint dataLocation)
  {
    if (_sparse == null)
      throw new InvalidOperationException("Sparse array is not allocated.");

    if (entityId >= DataLength)
      throw new ArgumentOutOfRangeException(nameof(entityId), "Entity ID is out of bounds.");

    _sparse[(int)entityId] = dataLocation;
  }

  private readonly uint GetDataLocation(uint entityId)
  {
    if (_sparse == null)
      throw new InvalidOperationException("Sparse array is not allocated.");

    uint dataLocation = _sparse[(int)entityId];

    if (dataLocation == 0)
      throw new InvalidOperationException("Invalid entity ID.");

    return dataLocation;
  }

  /// <summary>
  /// Indexer for accessing component data using entity ID and offset.
  /// </summary>
  /// <param name="entityId">The ID of entity.</param>
  /// <param name="offset">The offset within the component's data.</param>
  /// <returns>The byte value at the specified location.</returns>
  public readonly byte this[uint entityId, uint offset]
  {
    get
    {
      if (_data == null)
        throw new InvalidOperationException("Data array is not allocated.");

      uint dataLocation = GetDataLocation(entityId);
      return _data[dataLocation + offset];
    }
    set
    {
      if (_data == null)
        throw new InvalidOperationException("Data array is not allocated.");

      uint dataLocation = GetDataLocation(entityId);
      _data[dataLocation + offset] = value;
    }
  }
}
