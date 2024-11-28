using System;
using MechanicaCore.Utilities;

namespace MechanicaCore.Energy
{
  /// <summary>
  /// Handles the storage and management of Energite with event notifications.
  /// </summary>
  public class EnergiteStorage
  {
    private int _currentEnergy;

    /// <summary>
    /// Maximum capacity of the storage.
    /// </summary>
    public int MaxEnergy { get; }

    /// <summary>
    /// Current amount of stored energy.
    /// </summary>
    public int CurrentEnergy
    {
      get => _currentEnergy;
      set
      {
        SafeCheck.EnsureInRange(value, 0, MaxEnergy, nameof(CurrentEnergy));

        int clampedValue = Math.Clamp(value, 0, MaxEnergy);

        if (clampedValue == _currentEnergy)
          return; // no changes in energy

        _currentEnergy = clampedValue;
        OnEnergyChanged?.Invoke(_currentEnergy);

        if (_currentEnergy == MaxEnergy)
          OnEnergyFull?.Invoke();

        if (_currentEnergy == 0)
          OnEnergyEmpty?.Invoke();
      }
    }

    /// <summary>
    /// Event triggered when the energy level changes.
    /// </summary>
    public event Action<int> OnEnergyChanged;

    /// <summary>
    /// Event triggered when the energy storage becomes full.
    /// </summary>
    public event Action OnEnergyFull;

    /// <summary>
    /// Event triggered when the energy storage becomes empty.
    /// </summary>
    public event Action OnEnergyEmpty;

    /// <summary>
    /// Initializes the energy storage with a specified capacity.
    /// </summary>
    /// <param name="maxEnergy">The maximum energy capacity of the storage.</param>
    public EnergiteStorage(int maxEnergy)
    {
      SafeCheck.Ensure(maxEnergy > 0, "Max energy must be greater than zero.");
      MaxEnergy = maxEnergy;
      _currentEnergy = 0;
    }
  }
}
