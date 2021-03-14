using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Encapsulates a collection metabolic processes.
/// </summary>
[CreateAssetMenu(fileName = "MetabolismProfile", menuName = "ScriptableObjects/MetabolicProcessProfile", order = 1)]
public sealed class MetabolicProcessProfile : ScriptableObject
{
    #region Inspector Fields
    [Tooltip("The metabolic processes in this profile.")]
    [SerializeField] private MetabolicProcessField[] processes = default;
    [Serializable]
    private sealed class MetabolicProcessField
    {
        [HideInInspector] public string name;
        public float energyProduced = 0;
        public ProcessEntry[] processRequirements = default;
        [Serializable]
        public sealed class ProcessEntry
        {
            [HideInInspector] public string name;
            public ConsumableType consumable;
            public float quantityRequired;
        }
    }
    #endregion
    #region Inspector Validation
    private void OnValidate()
    {
        if (processes != null)
        {
            // Make the editor more legible
            // by overwriting element labels.
            for (int i = 0; i < processes.Length; i++)
            {
                processes[i].name = $"Process {i + 1}";
                foreach (MetabolicProcessField.ProcessEntry entry in processes[i].processRequirements)
                    entry.name = ObjectNames.NicifyVariableName(entry.consumable.ToString());
            }
        }
    }
    #endregion
    #region Initialization
    private void OnEnable()
    {
        // Convert the inspector fields into a collection
        // of immutable metabolic processes.
        Processes = new MetabolicProcess[processes.Length];
        for (int i = 0; i < Processes.Length; i++)
        {
            var requirements = new Dictionary<ConsumableType, float>();
            foreach (MetabolicProcessField.ProcessEntry entry in processes[i].processRequirements)
                requirements.Add(entry.consumable, entry.quantityRequired);
            Processes[i] = new MetabolicProcess(requirements, processes[i].energyProduced);
        }
    }
    #endregion
    #region Accessors
    /// <summary>
    /// Collection of the metabolic processes in this profile.
    /// </summary>
    public MetabolicProcess[] Processes { get; private set; }
    #endregion
}
