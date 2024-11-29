using System;
using System.Collections.Generic;

namespace MechanicaCore.core.utils;
/// <summary>
/// A lightweight, optimized test runner.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TestRunner"/> struct with a predefined capacity.
/// </remarks>
/// <param name="capacity">Initial capacity for test storage.</param>
public struct TestRunner(int capacity = 16)
{
  private Test[] _tests = new Test[capacity];
  private int _testCount = 0;

  private int _passed = 0;
  private int _failed = 0;

  /// <summary>
  /// Adds a test to the runner.
  /// </summary>
  /// <param name="name">The name of the test.</param>
  /// <param name="test">The action to execute as the test.</param>
  public void AddTest(string name, Action test)
  {
    if (test == null)
      throw new ArgumentNullException(nameof(test), "Test action cannot be null.");

    if (_testCount >= _tests.Length)
    {
      Array.Resize(ref _tests, _tests.Length * 2); // Expand array if necessary
    }
    _tests[_testCount++] = new Test(name, test);
  }

  /// <summary>
  /// Runs all registered tests and reports results.
  /// </summary>
  public void RunAll()
  {
    Console.WriteLine("[MECHANICACORE] Running tests...\n");
    for (int i = 0; i < _testCount; i++)
    {
      ref readonly var test = ref _tests[i];
      try
      {
        test.Action.Invoke();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[PASS] {test.Name}");
        Console.ResetColor();
        _passed++;
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FAIL] {test.Name}");
        Console.WriteLine($"       {ex.Message}");
        Console.ResetColor();
        _failed++;
      }
    }

    Console.WriteLine("\nTest Summary:");
    Console.WriteLine($"  Passed: {_passed}");
    Console.WriteLine($"  Failed: {_failed}");
    Console.WriteLine($"  Total: {_testCount}");
  }

  /// <summary>
  /// Asserts that two values are equal.
  /// </summary>
  /// <typeparam name="T">The type of the values being compared.</typeparam>
  /// <param name="expected">The expected value.</param>
  /// <param name="actual">The actual value.</param>
  /// <param name="message">An optional message for the assertion.</param>
  /// <exception cref="Exception">Thrown when the values are not equal.</exception>
  public static void AssertEqual<T>(T expected, T actual, string message = "")
  {
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
      throw new Exception($"Assertion failed. {message} Expected: {expected}, Actual: {actual}");
    }
  }

  /// <summary>
  /// Asserts that a condition is true.
  /// </summary>
  /// <param name="condition">The condition to evaluate.</param>
  /// <param name="message">An optional message for the assertion.</param>
  /// <exception cref="Exception">Thrown when the condition is false.</exception>
  public static void AssertTrue(bool condition, string message = "")
  {
    if (!condition)
    {
      throw new Exception($"Assertion failed. {message} Condition was false.");
    }
  }

  /// <summary>
  /// Asserts that a condition is false.
  /// </summary>
  /// <param name="condition">The condition to evaluate.</param>
  /// <param name="message">An optional message for the assertion.</param>
  /// <exception cref="Exception">Thrown when the condition is true.</exception>
  public static void AssertFalse(bool condition, string message = "")
  {
    if (condition)
    {
      throw new Exception($"Assertion failed. {message} Condition was true.");
    }
  }

  /// <summary>
  /// Represents a single test with a name and an associated action.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="Test"/> struct.
  /// </remarks>
  /// <param name="name">The name of the test.</param>
  /// <param name="action">The action to execute.</param>
  private readonly struct Test(string name, Action action)
  {
    public readonly string Name = name;
    public readonly Action Action = action;
  }
}