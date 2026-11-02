using RuntimeHandle;
using UnityEngine;
using UnityEngine.ProBuilder;

public class DraggableObject : MonoBehaviour, IDraggable
{
    private Vector3? _prevTarget;

    public void Move(Vector3 target, Vector3 surfaceNormal)
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, surfaceNormal);
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.ProjectOnPlane(transform.right, surfaceNormal);

        transform.rotation = Quaternion.LookRotation(forward, surfaceNormal);

        var worldDiff = target - (_prevTarget ?? target);
        var localDiff = transform.InverseTransformDirection(worldDiff);
        localDiff.y = 0;

        transform.Translate(localDiff, Space.Self);

        var currentToTarget = transform.position - target;
        var offset = Vector3.ProjectOnPlane(currentToTarget, surfaceNormal);
        transform.position = target + offset + (surfaceNormal * transform.lossyScale.y / 2);

        _prevTarget = target;
    }

    public void LetGo()
    {
        _prevTarget = null;
    }

    public void Hover()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
