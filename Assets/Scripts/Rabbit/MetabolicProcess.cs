using System.Collections.Generic;
using UnityEngine;

/// <summary>Represents a reaction creating energy from components</summary>
public sealed class MetabolicProcess : MonoBehaviour
{
    // Define this process via the inspector.
    [SerializeField] private ConsumableType[] componentsRequired = null;
    [SerializeField] private float[] quantitiesRequired = null;
    [SerializeField] private float energyProduced = 0;

    /// <summary>Contains the required components for this reaction</summary>
    public Dictionary<ConsumableType, float> Requirements { get; private set; }
    /// <summary>How many units of energy does this reaction produce(per second)</summary>
    public float EnergyProduced { get { return energyProduced; } }

    private void Start()
    {
        // Notify invalid inspector values.
        if(componentsRequired.Length != quantitiesRequired.Length)
            Debug.LogError(@"Fields `Components Required` and `Quantities Required` must have the same length!");

        // Create a dictionary to expose this process to other classes.
        Dictionary<ConsumableType, float> requirements = new Dictionary<ConsumableType, float>();
        for(int i = 0; i < Mathf.Min(componentsRequired.Length, quantitiesRequired.Length); i++)
            requirements.Add(componentsRequired[i], quantitiesRequired[i]);
        Requirements = requirements;
    }
}