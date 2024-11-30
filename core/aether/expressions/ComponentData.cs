using System;
using System.Buffers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace aether.expressions;

public unsafe struct ComponentData(uint sizeInBytes, byte alignment)
{
  /// <summary>
  /// The alignment of the component in the memory.
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
  /// The memory block that holds the component's data.
  /// </summary>
  private Memory<byte> _data = Memory<byte>.Empty;

  /// <summary>
  /// The mapping of entity ID to the location of data in the memory block.
  /// </summary>
  private Memory<uint> _sparse = Memory<uint>.Empty;

  public void AllocateData()
  {
    // use arraypool to rent memory
    _data = ArrayPool<byte>.Shared.Rent((int)SizeInBytes);
    _sparse = ArrayPool<uint>.Shared.Rent((int)DataLength);
  }

  public void FreeData()
  {
    if (_data.Length > 0)
    {
      ArrayPool<byte>.Shared.Return(_data.Span.ToArray()); // return the rented memory
      _data = Memory<byte>.Empty;
    }

    if (_sparse.Length > 0)
    {
      ArrayPool<uint>.Shared.Return(_sparse.Span.ToArray());
      _sparse = Memory<uint>.Empty;
    }
  }

  public void SetEntityDataLocation(uint entityId, uint dataLocation)
  {
    if (entityId < DataLength)
    {
      _sparse.Span[(int)entityId] = dataLocation;
    }
    else
    {
      throw new ArgumentOutOfRangeException(nameof(entityId), "Entity ID is out of bounds.");
    }
  }

  private uint GetDataLocation(uint entityId)
  {
    uint dataLocation = _sparse.Span[(int)entityId];

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
  public byte this[uint entityId, uint offset]
  {
    get
    {
      uint dataLocation = GetDataLocation(entityId);
      return _data.Span[(int)(dataLocation + offset)];
    }
    set
    {
      uint dataLocation = GetDataLocation(entityId);
      _data.Span[(int)(dataLocation + offset)] = value;
    }
  }

}

