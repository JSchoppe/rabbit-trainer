using System.Collections.Generic;
using UnityEngine;

/// <summary>Represents a distribution of environment components</summary>
public sealed class Environment : MonoBehaviour
{
    #region Inspector References
    [Tooltip("A corner of the bounding rectangular prism to spawn components in")]
    [SerializeField] private Transform spawnMin = null;
    [Tooltip("Other corner of the bounding rectangular prism to spawn components in")]
    [SerializeField] private Transform spawnMax = null;
    [Tooltip("Instances to spawn inside the environment bounds")]
    [SerializeField] private GameObject[] spawnablePrefabs = null;
    [Tooltip("Defines how many of each object can spawn in the environment")]
    [SerializeField] private Vector2Int[] spawnQuantityRanges = null;
    [Tooltip("Minimum distance in meters between environment objects")]
    [Range(0, float.MaxValue)][SerializeField] private float minDistanceBetween = 1;
    #endregion
    #region Accessible Fields
    /// <summary>A collection of the consumables that have been generated</summary>
    public List<Consumable> spawnedConsumables { get; private set; }
    #endregion
    #region Private Fields
    // Prevent an infinite loop that might occur if there are too many items to pack in the space.
    private const uint maxAttempts = 1000;
    #endregion

    // Start is used to do some basic inspector error checking.
    private void Start()
    {
        if(spawnablePrefabs.Length != spawnQuantityRanges.Length)
            Debug.LogError(@"Fields `SpawnablePrefabs` and `Spawn Quantity Ranges` must have the same length!");
    }

    /// <summary>Clears any previous environment items and generates a new layout</summary>
    public void Generate()
    {
        // Generate the quantity of each item to spawn.
        int[] quantityEach = new int[spawnablePrefabs.Length];
        int quantityTotal = 0;
        for(int i = 0; i < quantityEach.Length; i++)
        {
            quantityEach[i] = Random.Range(spawnQuantityRanges[i].x, spawnQuantityRanges[i].y + 1);
            quantityTotal += quantityEach[i];
        }

        // Determine the locations to spawn the objects at.
        Vector3[] spawnLocations = new Vector3[quantityTotal];
        uint failedAttempts = 0;
        for(int i = 0; i < spawnLocations.Length;)
        {
            // Generate a random coordinate in the bounding region.
            spawnLocations[i] = new Vector3(
                Mathf.Lerp(spawnMin.position.x, spawnMax.position.x, Random.value),
                Mathf.Lerp(spawnMin.position.y, spawnMax.position.y, Random.value),
                Mathf.Lerp(spawnMin.position.z, spawnMax.position.z, Random.value)
            );
            // Check against all previous locations to ensure spacing.
            bool satisfiesSpacing = true;
            for(int j = 0; j < i; j++)
            {
                // True when spacing is violated:
                if(Vector3.Distance(spawnLocations[i], spawnLocations[j]) < minDistanceBetween)
                {
                    // Break out of the method if we get stuck in a loop.
                    if(failedAttempts > maxAttempts)
                    {
                        Debug.LogWarning("Environment failed to pack generated objects." + "\n" +
                        "Consider lowering spawn quantities, or reducing spacing requirements.");
                        return;
                    }
                    // Increment failsafe and stop checking.
                    failedAttempts++;
                    satisfiesSpacing = false;
                    break;
                }
            }
            // If this location works move onto the next one.
            // Otherwise try again.
            if(satisfiesSpacing){ i++; }
        }

        // Remove any previous environment objects.
        spawnedConsumables = new List<Consumable>();
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // Instantiate the environment objects.
        int prefabIndex = 0, objectIndex = 0;
        foreach(GameObject prefab in spawnablePrefabs)
        {
            for(int i = 0; i < quantityEach[prefabIndex]; i++)
            {
                // Create the object, organize it in the hierarchy, and set its position.
                GameObject newObject = Instantiate(prefab);
                newObject.transform.parent = transform;
                newObject.transform.position = spawnLocations[objectIndex];

                // If this prefab has a consumable add it to the collection.
                Consumable consumable = newObject.GetComponent<Consumable>();
                if(consumable != null)
                {
                    spawnedConsumables.Add(consumable);
                    // Remove the consumable once it has been completely consumed.
                    consumable.onConsumedCompletely += (Consumable item) =>
                    {
                        spawnedConsumables.Remove(item);
                    };
                }

                objectIndex++;
            }
            prefabIndex++;
        }
    }
}
