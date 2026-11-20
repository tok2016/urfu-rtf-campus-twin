using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPicker : MonoBehaviour
{
    [SerializeField] private float _rayMaxDistance = 1000;
    private bool _wasHoveredOnHandler = false;

    public static Action<ITransformable> ObjectSelected;
    public static Action ObjectReleased;

    [Header("Pick input")]
    [SerializeField] private InputActionReference _selectAction;
    [SerializeField] private InputActionReference _dragAction;

    [Header("Pick layers")]
    [SerializeField] private LayerMask _layersToSelect;
    [SerializeField] private LayerMask _handlersMask;

    private ITransformable _transformable;

    void Update()
    {
        if (IsInteractingWithHandlers()) return;

        CheckSelectDragging();
        CheckSelection();
        CheckSelectionRelease();
    }

    private void OnEnable()
    {
        ObjectBuffer.ObjectPasted += OnObjectPasted;
        ObjectBuffer.ObjectDeleted += OnObjectDeleted;
    }

    /// <summary>
    /// Проверяет, взаимодействует ли игрок с гизмо трансформации.
    /// </summary>
    /// <returns>True, если было наведение на гизмо и его зажатие.</returns>
    private bool IsInteractingWithHandlers()
    {
        if(!_selectAction.action.IsPressed())
            _wasHoveredOnHandler = Physics.Raycast(
                Camera.main.ScreenPointToRay(_dragAction.action.ReadValue<Vector2>()),
                _rayMaxDistance,
                _handlersMask
            );

        return _selectAction.action.IsPressed() && _wasHoveredOnHandler;
    }

    /// <summary>
    /// Проверяет, было ли наведение на поверхность, с которой может взаимодействовать выбранный объект, при удержании нажатия.
    /// Если да, переносит объект в точку пересечения с поверхностью и прикрепляет его е ней.
    /// </summary>
    private void CheckSelectDragging()
    {
        if (_selectAction.action.IsPressed())
        {
            if (_transformable?.ObjectToDrag != null)
            {
                RaycastHit hit;
                var hitted = Physics.Raycast(
                    Camera.main.ScreenPointToRay(_dragAction.action.ReadValue<Vector2>()),
                    out hit,
                    _rayMaxDistance,
                    _transformable.ObjectToDrag.SurfaceLayers | _handlersMask);

                if (hitted 
                    && Constants.Instance.DoesHaveTheLayer(
                        _transformable.ObjectToDrag.SurfaceLayers, 
                        hit.transform.gameObject.layer)
                    )
                    _transformable.ObjectToDrag.Drag(hit.point, new Surface(hit.normal, hit.transform));
            }
        }
    }

    /// <summary>
    /// Проверяет, было ли нажатие на какой-нибудь объект, который можно выбрать.
    /// </summary>
    private void CheckSelection()
    {
        if (_selectAction.action.WasPerformedThisFrame())
        {
            RaycastHit hit;
            var hitted = Physics.Raycast(
                Camera.main.ScreenPointToRay(_dragAction.action.ReadValue<Vector2>()),
                out hit,
                _rayMaxDistance,
                _layersToSelect);

            if (hitted)
            {
                var handleable = hit.transform.GetComponent<IHandleable>();
                _transformable = handleable?.ObjectToTransform;
            }
            else
                ReleaseCurrentObject();
        }
    }

    /// <summary>
    /// Проверяет, было ли снятие нажатия. Если да, прекращает перетаскивание объекта.
    /// </summary>
    private void CheckSelectionRelease()
    {
        if (_selectAction.action.WasCompletedThisFrame())
            SelectCurrentObject();   
    }

    /// <summary>
    /// Выбирает объект и удаляет ссылку на него, чтобы его не перетаскивали.
    /// </summary>
    private void SelectCurrentObject()
    {
        if (_transformable != null)
        {
            _transformable.OnTransformStop();
            ObjectSelected?.Invoke(_transformable);
        }

        _transformable = null;
    }

    /// <summary>
    /// Отпускает выбранный объект и удаляет ссылку на него.
    /// </summary>
    private void ReleaseCurrentObject()
    {
        _transformable?.OnTransformStop();
        _transformable = null;

        ObjectReleased?.Invoke();
    }

    /// <summary>
    /// Выбирает только что вставленный объект.
    /// </summary>
    /// <param name="newObject">Новый объект, который нужно выбрать.</param>
    private void OnObjectPasted(ITransformable newObject)
    {
        _transformable = newObject;
        SelectCurrentObject();
    }

    /// <summary>
    /// Отпускает текущий объект.
    /// </summary>
    private void OnObjectDeleted()
    {
        ReleaseCurrentObject();
    }

    private void OnDisable()
    {
        ObjectBuffer.ObjectPasted -= OnObjectPasted;
        ObjectBuffer.ObjectDeleted -= OnObjectDeleted;
    }
}
