using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aether.Expressions;

/// <summary>
/// Represents a compact, highly efficient unique identifier.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct Identity : IEquatable<Identity>, IComparable<Identity>
{
  [FieldOffset(0)] private readonly ulong _value;

  [FieldOffset(0)] private readonly uint _index;         // 32 bits
  [FieldOffset(4)] private readonly ushort _generation;  // 16 bits
  [FieldOffset(6)] private readonly ushort _typeId;      // 16 bits

  /// <summary>
  /// Gets the index component of identifier.
  /// The index is a 32-bit unsigned integer used to differentiate instances within a generation.
  /// </summary>
  public uint Index => _index;

  /// <summary>
  /// Gets the generation component of the identifier.
  /// The generation is a 16-bit unsigned integer used to represent the version or lifecycle stage
  /// of the identity, allowing for incremental versions of an entity.
  /// </summary>
  public ushort Generation => _generation;

  /// <summary>
  /// Gets the type identifier component of the identifier.
  /// The type identifier is a 16-bit unsigned integer that distinguishes different types
  /// of entities, useful for grouping or identifying entities of a certain type.
  /// </summary>
  public ushort TypeId => _typeId;

  #region Constructors

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Identity(uint index, ushort generation, ushort typeId)
  {
    _value = (ulong)index | ((ulong)generation << 32) | ((ulong)typeId << 48);
  }

  /// <summary>
  /// Constructs an identity directly from a raw 64-bit value. This is useful when you already have the raw data
  /// for the identifier and want to quickly construct an instance of <see cref="Identity"/>.
  /// </summary>
  /// <param name="value">A raw 64-bit unsigned integer representing the identity.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Identity(ulong value) => _value = value;

  #endregion

  #region Successor Logic

  /// <summary>
  /// Generates a new identity with the next generation value. This is useful when creating a new version
  /// of an entity while keeping the same index and type identifier.
  /// </summary>
  /// <returns>A new <see cref="Identity"/> instance with the incremented generation value.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the maximum generation value has been reached
  /// (i.e., when <see cref="Generation"/> is equal to <see cref="ushort.MaxValue"/>).</exception>
  public unsafe Identity Successor()
  {
    if (Generation == ushort.MaxValue)
      throw new InvalidOperationException("Maximum generation reached. Identity generation cannot be incremented beyond its limit.");

    ulong nextValue;
    fixed (ulong* ptr = &_value)
    {
      // unsafe manipulation to increment the generation part directly
      nextValue = *ptr + (1UL << 32); // incrementing the generation part by one
    }
    return new Identity(nextValue);
  }

  #endregion

  #region Serialization

  /// <summary>
  /// Converts the identity to a 64-bit unsigned integer representation. This is useful for storage or transmission
  /// where a raw byte stream or numerical representation of the identity is needed.
  /// </summary>
  /// <returns>A 64-bit unsigned integer representing the identity.</returns>
  public ulong ToUInt64() => _value;


  /// <summary>
  /// Serializes the identity into a byte buffer. The buffer must be large enough to hold the serialized value
  /// (i.e., at least 8 bytes). This is useful for transferring or storing the identity in a binary format.
  /// </summary>
  /// <param name="buffer">A span of bytes that will receive the serialized identity.</param>
  /// <exception cref="ArgumentException">Thrown if the provided buffer is too small to hold the serialized identity.</exception>
  public unsafe void Serialize(Span<byte> buffer)
  {
    if (buffer.Length < sizeof(ulong))
      throw new ArgumentException($"Buffer size is insufficient. Expected size: {sizeof(ulong)}, received: {buffer.Length}.", nameof(buffer));

    fixed (byte* ptr = buffer)
    {
      *(ulong*)ptr = _value; // direct memory write
    }
  }

  /// <summary>
  /// Deserializes an identity from a byte buffer. The buffer must contain at least 8 bytes representing
  /// the raw 64-bit identity value.
  /// </summary>
  /// <param name="buffer">A read-only span of bytes containing the serialized identity.</param>
  /// <returns>A new <see cref="Identity"/> instance created from the raw data.</returns>
  /// <exception cref="ArgumentException">Thrown if the provided buffer is too small to contain the identity.</exception>
  public static unsafe Identity Deserialize(ReadOnlySpan<byte> buffer)
  {
    if (buffer.Length < sizeof(ulong))
      throw new ArgumentException($"Buffer size is insufficient. Expected size: {sizeof(ulong)}, received: {buffer.Length}.", nameof(buffer));

    fixed (byte* ptr = buffer)
    {
      return new Identity(*(ulong*)ptr);
    }
  }

  #endregion

  #region Unsafe Operations

  /// <summary>
  /// Combines two identities into a new identity using an XOR operation.
  /// This can be useful when you want to create a combined identifier for a group of identities,
  /// or to generate a unique identifier from two existing identifiers.
  /// </summary>
  /// <param name="a">The first identity to combine.</param>
  /// <param name="b">The second identity to combine.</param>
  /// <returns>A new <see cref="Identity"/> that represents the combined value.</returns>
  public static Identity Combine(Identity a, Identity b)
  {
    unsafe
    {
      ulong combined = a._value ^ b._value;
      return new Identity(combined);
    }
  }

  /// <summary>
  /// Creates an identity directly from the provided components (index, generation, and typeId)
  /// using unsafe pointer dereferencing. This method allows direct construction of the identity from raw data,
  /// and it is mainly intended for low-level operations where performance is critical.
  /// </summary>
  /// <param name="index">The index component (32 bits).</param>
  /// <param name="generation">The generation component (16 bits).</param>
  /// <param name="typeId">The type identifier component (16 bits).</param>
  /// <returns>A new <see cref="Identity"/> instance constructed from the raw values.</returns>
  public static unsafe Identity CreateUnsafe(uint index, ushort generation, ushort typeId)
  {
    ulong value = index | ((ulong)generation << 32) | ((ulong)typeId << 48);
    return *(Identity*)&value; // direct pointer dereferencing
  }

  #endregion

  #region TypeID Structures

  /// <summary>
  /// Represents a compact, immutable type identifier, which is derived from the <see cref="MetadataToken"/>
  /// of a given type. The <see cref="TypeID"/> allows for efficient type-based comparisons and operations.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public readonly struct TypeID(ushort value) : IEquatable<TypeID>
  {
    public readonly ushort Value = value;

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(TypeID other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is TypeID other && Equals(other);

    /// <summary>
    /// Retrieves the type identifier for a specific type <typeparamref name="T"/>.
    /// This uses the <see cref="MetadataToken"/> of the type to derive the unique identifier.
    /// </summary>
    /// <typeparam name="T">The type for which to retrieve the identifier.</typeparam>
    /// <returns>A <see cref="TypeID"/> for the specified type.</returns>
    public static TypeID From<T>() => new((ushort)typeof(T).MetadataToken);
  }

  /// <summary>
  /// A static cache for type-based IDs. This is a fast and efficient way to store and retrieve type identifiers
  /// for specific types at runtime.
  /// </summary>
  /// <typeparam name="T">The type for which the <see cref="TypeID"/> is cached.</typeparam>
  public static class TypeIDCache<T>
  {
    public static readonly TypeID Id = TypeID.From<T>();
  }

  #endregion

  #region Hashing and Equality

  public override int GetHashCode()
  {
    // FNV-1a fast, well-distributed, and easy to implement
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
