using UnityEngine;

public sealed class BerryDrawer : Drawer
{
    // Get scene references.
    [SerializeField] private Transform[] berries = null;

    private Vector3[] berriesInitialScale;
    private void Start()
    {
        // Remember the initial scale of the berries.
        berriesInitialScale = new Vector3[berries.Length];
        for(int i = 0; i < berries.Length; i++)
            berriesInitialScale[i] = berries[i].localScale;
    }

    public override void Redraw(float interpolant)
    {
        // If this bush has been consumed only remove the berry
        // berry objects, not the bushes.
        if(interpolant >= 0)
        {
            for(int i = 0; i < berries.Length; i++)
            {
                // Localize the interpolant for each berry.
                // This appears as if the berries are being consumed one by one.
                float subRangeLeft = (float)i / berries.Length;
                float subRangeRight = (float)(i + 1) / berries.Length;
                float subInterpolant = Mathf.Clamp(Mathf.InverseLerp(subRangeLeft, subRangeRight, interpolant), 0, 1);
                // Updater this berry relative to its initial size.
                if(berries[i] != null)
                    berries[i].localScale = berriesInitialScale[i] * subInterpolant;
            }
        }
    }
}
