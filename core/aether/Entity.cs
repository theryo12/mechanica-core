using System;

namespace aether;

public readonly struct Entity : IComparable<Entity>
{
  #region Implementations: IComparable<Entity>
  public int CompareTo(Entity other)
  {
    throw new NotImplementedException();
  }

  #endregion
}