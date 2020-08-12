using UnityEngine;

public delegate void ConsumableEventListener(Consumable consumable);

/// <summary>Defines how agents categorize this consumable</summary>
public enum ConsumableType : byte
{
    Peppers, Lettuce, Grass
}

/// <summary>Defines an item that can be consumed by an agent</summary>
public sealed class Consumable : MonoBehaviour
{
    #region Inspector References
    [Tooltip("Defines how this consumable renders as it is being consumed")]
    [SerializeField] private Drawer drawingImplementation = null;
    [Tooltip("What the agent identifies this consumable as")]
    [SerializeField] private ConsumableType consumableType = ConsumableType.Peppers;
    public ConsumableType ConsumableType { get { return consumableType; } }
    [Tooltip("On generation, the minimum food units")]
    [SerializeField] private float minUnits = 1;
    [Tooltip("On generation, the maximum food units")]
    [SerializeField] private float maxUnits = 1.5f;
    #endregion
    #region Accessible Fields
    /// <summary>The food units remaining in this consumable item</summary>
    public float ConsumableUnits
    { 
        get { return consumableUnits; }
        set
        {
            consumableUnits = value;

            // Redraw this consumable in its eaten state.
            drawingImplementation.Redraw(ConsumableUnits / initialUnits);
            // If this item has been consumed, make it un-observable.
            if(consumableUnits < 0)
            {
                GetComponent<Collider>().enabled = false;
                // Notify listeners that this item has been consumed.
                onConsumedCompletely?.Invoke(this);
            }
        }
    }
    /// <summary>Fired once when this consumable has been exhausted</summary>
    public event ConsumableEventListener onConsumedCompletely;
    #endregion
    #region Private Fields
    // Remember the initial saturation of this consumable.
    private float initialUnits;
    private float consumableUnits;
    #endregion

    #region Monobehavior Implementation
    private void Start()
    {
        // Add some visual variance by rotating it.
        transform.Rotate(transform.up, Random.value * 360);
        // Create a random size for this consumable.
        ConsumableUnits = initialUnits = Mathf.Lerp(minUnits, maxUnits, Random.value);
        transform.localScale = Vector3.one * initialUnits;
    }
    #endregion
}
