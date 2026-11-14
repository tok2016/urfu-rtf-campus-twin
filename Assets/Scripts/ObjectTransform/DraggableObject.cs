using System.Linq;
using UnityEngine;

/// <summary>
/// Представляет поверхность (плоскость) с нормалью и Transform.
/// </summary>
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

/// <summary>
/// Представляет объект, который можно перетаскивать и трансформировать.
/// </summary>
public class DraggableObject : MonoBehaviour, IDraggable
{
    private Material[] _materials;

    [field: SerializeField] public LayerMask SurfaceLayers { get; private set; }

    [SerializeField] private BindableObject _binding;
    private Surface _surface;

    private Vector3? _prevTarget;
    private Vector3 _startScale;
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
                material.SetColor("_EmissionColor", value ? Constants.Instance.ObjectWarningHighlight : Color.black);
        }
    }

    public Transform ObjectTransform => transform;

    public ITransformable ObjectToTransform => this;
    public IDraggable ObjectToDrag => this;

    private void Awake()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();
        _materials = renderers.Select(x => x.material).ToArray();
    }

    private void Start()
    {
        _surface = new Surface(Vector3.zero, null);

        _startScale = transform.localScale;

        _prevScale = _startScale;
        _prevPosition = transform.position;
        _prevRotation = transform.rotation;
    }

    /// <summary>
    /// Размещает объект в указанной точке переданной поверхности.
    /// Поворачивает объект так, чтобы он прикрепился нижней гранью к поверхности.
    /// </summary>
    /// <param name="target">Точка размещения объекта на поверхности.</param>
    /// <param name="surface">Поверхность, на которую нужно разместить объект.</param>
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

        IsIntersecting = _binding.CheckIntersection(_surface, transform.localScale);
    }

    /// <summary>
    /// Отменяет трансформацию, если объект был отпущен, когда он пересекается с другими или не размещён на поверхности.
    /// </summary>
    public void OnTransformStop()
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

        _binding.OnRelease(IsIntersecting);
        IsIntersecting = false;
    }

    public void OnSelect()
    {

    }

    /// <summary>
    /// Проверяет при перемещении, есть ли под объектом (в локальном пространстве) поверхность и пересекается ли он с другими.
    /// </summary>
    public void OnMove()
    {
        var surface = _binding.GetNearestSurface(transform.localScale, SurfaceLayers);

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

        IsIntersecting = _binding.CheckIntersection(surface, transform.localScale);
    }

    /// <summary>
    /// Вращает дочерний объект и проверяет пересечение с другими при вращении.
    /// </summary>
    public void OnRotate()
    {
        _binding.OnRotate(_surface.normal);
        IsIntersecting = _binding.CheckIntersection(_surface, transform.localScale);
    }

    /// <summary>
    /// Пропорционально увеличивает все измерения объекта при увеличении одной из них. Проверяет пересечение с другими объектами.
    /// </summary>
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
            ClampAxisScale(transform.localScale.x * scaleMult / scaleDiff.x, _startScale.x),
            ClampAxisScale(transform.localScale.y * scaleMult / scaleDiff.y, _startScale.y),
            ClampAxisScale(transform.localScale.z * scaleMult / scaleDiff.z, _startScale.z)
        );

        IsIntersecting = _binding.CheckIntersection(_surface, transform.localScale);
    }

    /// <summary>
    /// Ограничивает размер измерения размера объекта.
    /// </summary>
    /// <param name="axisScale">Значение размера измерения / оси размера.</param>
    /// <param name="startAxisScale">Изначальное значение размера измерения / оси размера.</param>
    /// <returns></returns>
    private float ClampAxisScale(float axisScale, float startAxisScale) => Mathf.Clamp(
            axisScale,
            startAxisScale * Constants.Instance.MinObjectScale,
            startAxisScale * Constants.Instance.MaxObjectScale
        );
}
