using System.Linq;
using UnityEngine;
public struct Surface
{
    public Surface(Vector3 normal, Transform transform)
    {
        this.normal = normal;
        this.transform = transform;
    }

    public Vector3 normal;
    public Transform transform;
}

public class DraggableObject : MonoBehaviour, IDraggable
{
    [SerializeField] private RotatableObject _rotatable;
    private Material[] _materials;

    private Vector3? _prevTarget;
    private Surface _surface;
    public float minScale, maxScale;
    private Vector2 xScaleLimit, yScaleLimit, zScaleLimit;

    [ColorUsage(false, true)]
    public Color intersectionColor;

    private Vector3 _prevScale, _prevPosition;
    private Quaternion _prevRotation;

    private bool _isIntersecting = false;

    public bool IsIntersecting
    {
        get
        {
            return _isIntersecting;
        }
        set
        {
            _isIntersecting = value;
            foreach (var material in _materials)
                material.SetColor("_EmissionColor", value ? intersectionColor : Color.black);
        }
    }

    private void Awake()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();
        _materials = renderers.Select(x => x.material).ToArray();
    }

    private void Start()
    {
        _surface = new Surface(Vector3.zero, null);

        _prevScale = transform.localScale;
        _prevPosition = transform.position;
        _prevRotation = transform.rotation;

        xScaleLimit = new Vector2(transform.localScale.x * minScale, transform.localScale.x * maxScale);
        yScaleLimit = new Vector2(transform.localScale.y * minScale, transform.localScale.y * maxScale);
        zScaleLimit = new Vector2(transform.localScale.z * minScale, transform.localScale.z * maxScale);
    }

    public Transform ObjectTransform => transform;

    public void Drag(Vector3 target, Surface surface)
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, surface.normal);
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.ProjectOnPlane(transform.right, surface.normal);

        transform.rotation = Quaternion.LookRotation(forward, surface.normal);

        var worldDiff = target - (_prevTarget ?? target);
        var localDiff = transform.InverseTransformDirection(worldDiff);
        localDiff.y = 0;

        transform.Translate(localDiff, Space.Self);

        var currentToTarget = transform.position - target;
        var offset = Vector3.ProjectOnPlane(currentToTarget, surface.normal);
        transform.position = target + offset;

        _prevTarget = target;
        _surface = surface;

        IsIntersecting = _rotatable.CheckIntersection(_surface, transform.localScale);
    }

    public void LetGo()
    {
        _prevTarget = null;

        if (IsIntersecting)
        {
            transform.position = _prevPosition;
            transform.rotation = _prevRotation;
            transform.localScale = _prevScale;
        }
        else
        {
            _prevPosition = transform.position;
            _prevRotation = transform.rotation;
            _prevScale = transform.localScale;
        }

        _rotatable.LetGo(IsIntersecting);
        IsIntersecting = false;
    }

    public void Hover()
    {

    }

    public void OnMove()
    {
        var surface = _rotatable.GetNearestSurface(transform.localScale);

        if(surface.transform)
        {
            _surface.normal = surface.normal;
            _surface.transform = surface.transform;
        } 
        else
        {
            _surface.normal = Vector3.zero;
            _surface.transform = null;
        }

        IsIntersecting = _rotatable.CheckIntersection(surface, transform.localScale);
    }

    public void OnRotate()
    {
        _rotatable.OnRotate(_surface.normal);
        IsIntersecting = _rotatable.CheckIntersection(_surface, transform.localScale);
    }

    public void OnScale()
    {
        //RealtimeTransformHandle при изменении масштаба вдоль одной оси сохраняет первоначальне значения масштаба других осей.
        var scaleDiff = new Vector3(
            transform.localScale.x / _prevScale.x,
            transform.localScale.y / _prevScale.y,
            transform.localScale.z / _prevScale.z);

        var scaleMult = Mathf.Max(scaleDiff.x, scaleDiff.y, scaleDiff.z);
        if (scaleMult == 1)
            scaleMult = Mathf.Min(scaleDiff.x, scaleDiff.y, scaleDiff.z);

        transform.localScale = new Vector3(
            Mathf.Clamp(transform.localScale.x * scaleMult / scaleDiff.x, xScaleLimit.x, xScaleLimit.y),
            Mathf.Clamp(transform.localScale.y * scaleMult / scaleDiff.y, yScaleLimit.x, yScaleLimit.y),
            Mathf.Clamp(transform.localScale.z * scaleMult / scaleDiff.z, zScaleLimit.x, zScaleLimit.y));

        IsIntersecting = _rotatable.CheckIntersection(_surface, transform.localScale);
    }
}
