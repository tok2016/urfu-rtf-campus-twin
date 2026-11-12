using RuntimeHandle;
using UnityEngine;

public interface IDraggable
{
    public bool IsIntersecting { get; set; }
    public Transform ObjectTransform { get; }
    public void Drag(Vector3 target, Surface surface);
    public void LetGo();
    public void Hover();
    public void OnMove();
    public void OnRotate();
    public void OnScale();
}
