using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RotatableObject : MonoBehaviour
{
    private Transform _child;
    private Vector3 _startChildScale;
    private BoxCollider _collider;
    private Vector3 _prevChildPosition;
    private Quaternion _prevRotation;
    public LayerMask walls, objects;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    void Start()
    {
        _child = transform.GetChild(0);
        _startChildScale = _collider.bounds.size;
        _prevRotation = transform.rotation;
        _prevChildPosition = _child?.localPosition ?? Vector3.zero;
    }

    public void LetGo(bool isIntersecting)
    {
        if(isIntersecting)
        {
            transform.rotation = _prevRotation;
            if(_child) _child.localPosition = _prevChildPosition;
        }
        else
        {
            _prevRotation = transform.rotation;
            _prevChildPosition = _child?.localPosition ?? Vector3.zero;
        }
    }

    public bool CheckIntersection(Surface surface, Vector3 localScale)
    {
        if (surface.transform == null)
            return true;

        var objectsHits = Physics.OverlapBox(
            _collider.bounds.center,
            GetCurrentRealScale(localScale) / 2,
            transform.rotation,
            objects);

        foreach (var obj in objectsHits)
        {
            Debug.Log($"{obj} normal {surface.normal}");
        }

        return objectsHits.Any(hit =>
            hit.transform != surface.transform && hit.transform != transform);
    }

    public Surface GetNearestSurface(Vector3 localScale)
    {
        RaycastHit wallBoxHit;
        var wallHit = Physics.Raycast(
            _collider.bounds.center, 
            -transform.up, 
            out wallBoxHit,
            _startChildScale.y * localScale.y, 
            walls);

        return new Surface(wallBoxHit.normal, wallBoxHit.transform);
    }

    public void OnRotate(Vector3 surfaceNormal)
    {
        if (!_child) return;

        var extents = _startChildScale / 2;
        var localNormal = transform.InverseTransformDirection(surfaceNormal);

        _child.localPosition = new Vector3(
            extents.x * localNormal.x,
            extents.y * localNormal.y,
            extents.z * localNormal.z);

        _collider.center = _child.localPosition;
    }

    private void OnDrawGizmos()
    {
        if(_collider)
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_collider.center, _collider.size);
        }
    }

    private Vector3 GetCurrentRealScale(Vector3 parentLocalScale) =>
        new Vector3(_startChildScale.x * parentLocalScale.x,
            _startChildScale.y * parentLocalScale.y,
            _startChildScale.z * parentLocalScale.z);
}
