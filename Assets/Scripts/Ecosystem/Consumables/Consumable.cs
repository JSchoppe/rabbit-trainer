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
    [HideInInspector] public float consumableUnits;
    /// <summary>Fired once when this consumable has been exhausted</summary>
    public event ConsumableEventListener onConsumedCompletely;
    #endregion
    #region Private Fields
    // Remember the initial saturation of this consumable.
    private float initialUnits;
    #endregion

    #region Monobehavior Implementation
    private void Start()
    {
        // Add some visual variance by rotating it.
        transform.Rotate(transform.up, Random.value * 360);
        // Create a random size for this consumable.
        consumableUnits = initialUnits = Mathf.Lerp(minUnits, maxUnits, Random.value);
        transform.localScale = Vector3.one * initialUnits;
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Agent"))
        {
            // Since we only are using rabbits we can assume the agent
            // is a rabbit. We will have the rabbit begin to eat this consumable.
            RabbitAgent agent = other.gameObject.GetComponent<RabbitAgent>();
            agent.Eat(this);
            // Redraw this consumable in its eaten state.
            drawingImplementation.Redraw(consumableUnits / initialUnits);
            // If this item has been consumed, make it un-observable.
            if(consumableUnits == 0)
            {
                GetComponent<Collider>().enabled = false;
                // Notify listeners that this item has been consumed.
                onConsumedCompletely?.Invoke(this);
            }
        }
    }
    #endregion
}
