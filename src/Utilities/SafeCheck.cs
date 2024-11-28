using System;

namespace MechanicaCore.Utilities
{
  /// <summary>
  /// Utility class for performing safe checks and validations
  /// </summary>
  public static class SafeCheck
  {
    /// <summary>
    /// Checks if a condition is true, otherwise throws an exception.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="errorMessage">The error message if the condition fails.</param>
    public static void Ensure(bool condition, string errorMessage)
    {
      if (!condition)
        throw new ArgumentException(errorMessage);
    }

    /// <summary>
    /// Checks if a value is within the specified range, otherwise throws an exception.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <param name="paramName">The parameter name for better error context.</param>
    public static void EnsureInRange(int value, int min, int max, string paramName)
    {
      if (value < min || value > max)
        throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max}. Received: {value}");
    }

    /// <summary>
    /// Checks if an object is not null, otherwise throws an exception.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <param name="paramName">The parameter name for better error context.</param>
    public static void EnsureNotNull(object obj, string paramName)
    {
      if (obj == null)
        throw new ArgumentNullException(paramName, "Value cannot be null.");
    }
  }
}