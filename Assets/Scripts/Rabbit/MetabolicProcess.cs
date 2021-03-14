using System;
using System.Collections.Generic;

/// <summary>
/// Represents a reaction creating energy from components.
/// </summary>
[Serializable]
public sealed class MetabolicProcess
{
    #region Fields
    private readonly Dictionary<ConsumableType, float> requirements;
    private readonly float energyProduced;
    #endregion
    #region Constructors
    /// <summary>
    /// Creates a new metabolic process.
    /// </summary>
    /// <param name="requirements">The process requirements.</param>
    /// <param name="energyProduced">The energy produced from the reaction.</param>
    public MetabolicProcess(Dictionary<ConsumableType, float> requirements, float energyProduced)
    {
        this.requirements = requirements;
        this.energyProduced = energyProduced;
    }
    #endregion
    #region Accessors
    /// <summary>
    /// The required components and their quantities for this reaction.
    /// </summary>
    public Dictionary<ConsumableType, float> Requirements => requirements;
    /// <summary>
    /// How many units of energy this reaction produces.
    /// </summary>
    public float EnergyProduced => energyProduced;
    #endregion
}
