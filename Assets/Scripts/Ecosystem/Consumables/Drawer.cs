using UnityEngine;

public abstract class Drawer : MonoBehaviour
{
    /// <summary>Updates the aesthetic of an object based on a completion interpolant</summary>
    /// <param name="interpolant">The length along the visual animation</param>
    public abstract void Redraw(float interpolant);
}
