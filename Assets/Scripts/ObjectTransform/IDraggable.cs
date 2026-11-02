using RuntimeHandle;
using UnityEngine;

public interface IDraggable
{
    public void Move(Vector3 target, Vector3 surfaceNormal);
    public void Hover();
}
