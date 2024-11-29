using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MechanicaCore.core.ecs;

/// <summary>
/// Represents a compact, highly efficient unique identifier.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct Identity : IEquatable<Identity>, IComparable<Identity>
{
  [FieldOffset(0)] private readonly ulong _value;

  [FieldOffset(0)] private readonly uint _index;       // 32 bits
  [FieldOffset(4)] private readonly ushort _generation; // 16 bits
  [FieldOffset(6)] private readonly short _typeId;     // 16 bits

  /// <summary>
  /// The index component.
  /// </summary>
  public uint Index => _index;

  /// <summary>
  /// The generation component.
  /// </summary>
  public ushort Generation => _generation;

  /// <summary>
  /// The type identifier component.
  /// </summary>
  public short TypeId => _typeId;

  #region Constructors

  /// <summary>
  /// Constructs a new identity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Identity(uint index, ushort generation, short typeId)
  {
    _value = (ulong)index | ((ulong)generation << 32) | ((ulong)(ushort)typeId << 48);
  }

  /// <summary>
  /// Constructs an identity directly from a raw 64-bit value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Identity(ulong value) => _value = value;

  #endregion

  #region Successor Logic

  /// <summary>
  /// Generates a new identity with the next generation value.
  /// </summary>
  public unsafe Identity Successor()
  {
    if (Generation == ushort.MaxValue)
      throw new InvalidOperationException("Maximum generation reached.");

    ulong nextValue;
    fixed (ulong* ptr = &_value)
    {
      // Unsafe manipulation to increment the generation part directly
      // Incrementing generation directly in memory avoids additional decomposition/reconstruction of values
      nextValue = *ptr + (1UL << 32);
    }
    return new Identity(nextValue);
  }

  #endregion

  #region Serialization

  public ulong ToUInt64() => _value;

  public unsafe void Serialize(Span<byte> buffer)
  {
    if (buffer.Length < sizeof(ulong))
      throw new ArgumentException("Buffer too small.");

    fixed (byte* ptr = buffer)
    {
      *(ulong*)ptr = _value; // Direct memory write
    }
  }

  public static unsafe Identity Deserialize(ReadOnlySpan<byte> buffer)
  {
    if (buffer.Length < sizeof(ulong))
      throw new ArgumentException("Buffer too small.");

    fixed (byte* ptr = buffer)
    {
      return new Identity(*(ulong*)ptr);
    }
  }

  #endregion

  #region Unsafe Operations

  /// <summary>
  /// Combines two identities into a new identity using XOR.
  /// Useful for grouping identities or creating combined unique identifiers.
  /// </summary>
  public static Identity Combine(Identity a, Identity b)
  {
    unsafe
    {
      ulong combined = a._value ^ b._value;
      return new Identity(combined);
    }
  }
  public static unsafe Identity CreateUnsafe(uint index, ushort generation, short typeId)
  {
    ulong value = index | ((ulong)generation << 32) | ((ulong)(ushort)typeId << 48);
    return *(Identity*)&value; // Direct pointer dereferencing
  }

  #endregion

  #region TypeID Structures

  /// <summary>
  /// Represents a compact, immutable type identifier.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public readonly struct TypeID(ushort value) : IEquatable<TypeID>
  {
    public readonly ushort Value = value;

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(TypeID other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is TypeID other && Equals(other);

    public static TypeID From<T>() => new((ushort)typeof(T).MetadataToken);
  }

  /// <summary>
  /// Cache for type-based IDs.
  /// </summary>
  public static class TypeIDCache<T>
  {
    public static readonly TypeID Id = TypeID.From<T>();
  }

  #endregion

  #region Hashing and Equality

  public override int GetHashCode()
  {
    // FNV-1a Fast, well-distributed, and easy to implement.
    const ulong FnvPrime = 0x100000001b3;
    ulong hash = 0xcbf29ce484222325;

    hash ^= _value;
    hash *= FnvPrime;

    return unchecked((int)hash);
  }

  public bool Equals(Identity other) => _value == other._value;

  public int CompareTo(Identity other) => _value.CompareTo(other._value);
  public override bool Equals(object? obj) => obj is Identity other && Equals(other);

  public override string ToString() => $"Identity(Index: {_index}, Generation: {_generation}, TypeId: {_typeId})";

  public static bool operator ==(Identity left, Identity right) => left.Equals(right);

  public static bool operator !=(Identity left, Identity right) => !left.Equals(right);

  #endregion
}