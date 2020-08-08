using UnityEngine;

public sealed class GrassDrawer : Drawer
{
    // Get scene references.
    [SerializeField] private Transform[] grassBlades = null;
    [Tooltip("The chance that each blade of grass will not be rendered(aesthetic only)")]
    [SerializeField][Range(0, 1)] private float bladeDespawnChance = 0.2f;

    private void Start()
    {
        // Randomly remove some of the grass blades to
        // add more visual variance.
        foreach(Transform blade in grassBlades)
            if(Random.value < bladeDespawnChance)
                foreach(Transform child in blade)
                    Destroy(child.gameObject);
    }

    public override void Redraw(float interpolant)
    {
        // Redraw the length of the grass.
        if(interpolant > 0)
            foreach(Transform blade in grassBlades)
                blade.localScale = new Vector3(blade.localScale.x, blade.localScale.y, interpolant);
    }
}
