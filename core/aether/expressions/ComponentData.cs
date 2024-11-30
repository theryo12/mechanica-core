using System;
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
  /// The raw component data stored in a byte array.
  /// </summary>
  public byte* Data = null;

  /// <summary>
  /// The mapping of entity ID to the location of the component's data.
  /// </summary>
  public uint* Sparse = null;

  public void AllocateData()
  {
    // use fixed size buffers to avoid gc allocation
    Data = (byte*)Marshal.AllocHGlobal((int)SizeInBytes);
    Sparse = (uint*)Marshal.AllocHGlobal((int)(sizeof(uint) * DataLength));
  }

  public void FreeData()
  {
    if (Data != null)
    {
      Marshal.FreeHGlobal((IntPtr)Data);
      Data = null;
    }

    if (Sparse != null)
    {
      Marshal.FreeHGlobal((IntPtr)Sparse);
      Sparse = null;
    }
  }

  /// <summary>
  /// Sets a value at the specified entity location in the sparse array.
  /// </summary>
  /// <param name="entityId">The ID of the entity.</param>
  /// <param name="dataLocation">The location of the data for this entity.</param>
  public void SetEntityDataLocation(uint entityId, uint dataLocation)
  {
    Sparse[entityId] = dataLocation;
  }

  /// <summary>
  /// Gets a value from the data array using the entity's ID and offset.
  /// </summary>
  /// <param name="entityId">The ID of the entity.</param>
  /// <param name="offset">The offset in the component data.</param>
  /// <returns>The byte value at the specified location.</returns>
  public byte GetValue(uint entityId, uint offset)
  {
    uint dataLocation = Sparse[entityId];

    if (dataLocation == 0)
      throw new InvalidOperationException("Invalid entity ID.");

    return *(Data + dataLocation + offset);
  }

  /// <summary>
  /// Sets a value at a specific location in the data.
  /// </summary>
  /// <param name="entityId">The entity ID for the data.</param>
  /// <param name="offset">The offset within the component data.</param>
  /// <param name="value">The value to set.</param>
  public void SetValue(uint entityId, uint offset, byte value)
  {
    uint dataLocation = Sparse[entityId];

    if (dataLocation == 0)
      throw new InvalidOperationException("Invalid entity ID.");

    *(Data + dataLocation + offset) = value;
  }
}

