using System.Linq;
using UnityEngine;

/// <summary>
/// Представляет объект, способный привязываться к поверхности.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class BindableObject : MonoBehaviour, IBindable
{
    private BoxCollider _collider;
    private Quaternion _prevRotation;

    private Transform _child;
    private Vector3 _startChildScale;
    private Vector3 _prevChildPosition;

    public Transform ObjectTransform => transform;
    public ITransformable ObjectToTransform {  get; private set; }

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        ObjectToTransform = transform.GetComponentInParent<ITransformable>();
    }

    void Start()
    {
        _child = transform.GetChild(0);
        _startChildScale = _collider.bounds.size;
        _prevRotation = transform.rotation;
        _prevChildPosition = _child?.localPosition ?? Vector3.zero;
    }

    /// <summary>
    /// Возвращает объекту предыдущее значение угла поворота, если при прикращении вращения он пересекается с другими объектам.
    /// </summary>
    /// <param name="isIntersecting">Пересекается ли объект с другими или нет.</param>
    public void OnRelease(bool isIntersecting)
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
            GetCurrentGlobalScale(localScale) / 2,
            transform.rotation,
            Constants.Instance.ObjectsIntersectionLayers);

        return objectsHits.Any(hit =>
            hit.transform != surface.transform && hit.transform != transform);
    }

    public Surface GetNearestSurface(Vector3 localScale, LayerMask surfaceLayers)
    {
        RaycastHit wallBoxHit;
        Physics.Raycast(
            _collider.bounds.center, 
            -transform.up, 
            out wallBoxHit,
            _startChildScale.y * localScale.y,
            surfaceLayers);

        return new Surface(wallBoxHit.normal, wallBoxHit.transform);
    }

    /// <summary>
    /// Меняет локальное положение объекта внутри так, чтобы его нижняя граница находилась на поверхности.
    /// </summary>
    /// <param name="surfaceNormal">Поверхность, на которой размещён объект.</param>
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

    /// <summary>
    /// Рассчитывает реальный текущий размер объекта в глобальном пространстве.
    /// </summary>
    /// <param name="parentLocalScale">Локальный масштаб родительского объекта.</param>
    /// <returns>Размер объекта в глобальном пространстве.</returns>
    private Vector3 GetCurrentGlobalScale(Vector3 parentLocalScale) =>
        new Vector3(_startChildScale.x * parentLocalScale.x,
            _startChildScale.y * parentLocalScale.y,
            _startChildScale.z * parentLocalScale.z);
}
