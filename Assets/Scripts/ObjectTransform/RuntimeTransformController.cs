using RuntimeHandle;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контролирует взаимодействие с RuntimeTransformHandle - внутриигровое гизмо для трансформации объектов.
/// </summary>
public class RuntimeTransformController : MonoBehaviour
{
    [Header("Transform handlers")]
    [SerializeField] private RuntimeTransformHandle _transformHandle;
    [SerializeField] private RuntimeTransformHandle _additionalHandle;

    [Header("Handle type hot keys")]
    [SerializeField] private InputActionReference _positionButton;
    [SerializeField] private InputActionReference _rotationButton, _scaleButton;

    private ITransformable _currentTarget;

    /// <value>Оси, которые можно трансформировать.</value>
    public HandleAxes Axes 
    { 
        get 
        { 
            return _transformHandle.axes; 
        } 
        private set
        {
            _transformHandle.axes = value;
            StartCoroutine(WaitForLayersToChange());
        }
    }

    private HandleAxes _movementAxes = HandleAxes.XZ;

    /// <value>Оси, вдоль которых можно перемещать объект.</value>
    public HandleAxes MovementAxes 
    {  
        get 
        { 
            return _movementAxes;
        } 
        set 
        {
            _movementAxes = value;
            _additionalHandle.axes = value;
            Axes = value;
        } 
    }

    /// <value>Тип трансформации.</value>
    public HandleType TransformType
    {
        get 
        { 
            return _transformHandle.type; 
        }
        set
        {
            _transformHandle.type = value;
            if (value == HandleType.SCALE)
                Axes = HandleAxes.XYZ;
            else if (value == HandleType.ROTATION)
                Axes = HandleAxes.XYZ ^ MovementAxes;
            else
                Axes = MovementAxes;

            _additionalHandle.gameObject.SetActive(
                _transformHandle.gameObject.activeInHierarchy 
                && value == HandleType.ROTATION);

            if(_currentTarget != null)
            {
                DetachHandleDragEvents();
                AttachHandleDragEvents(_currentTarget);
            }
        }
    }

    void Start()
    {
        MovementAxes = Axes;
        _additionalHandle.type = HandleType.ROTATION;
    }

    private void OnEnable()
    {
        _positionButton.action.performed += SetPositionType;
        _rotationButton.action.performed += SetRotationType;
        _scaleButton.action.performed += SetScaleType;

        ObjectPicker.ObjectSelected += AttachTarget;
        ObjectPicker.ObjectReleased += DetachTarget;
    }

    /// <summary>
    /// Изменяет слои всех гизмо.
    /// RuntimeTransformHandle создаёт гизмо только в режиме игре и с дефолтным слоем,
    /// поэтому это единственный способ поменять всем гизмо слои.
    /// </summary>
    /// <param name="parent">Родительский объект, у дочерних объектов которого нужно поменять слои.</param>
    private void ChangeChildrenLayers(Transform parent)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.layer = gameObject.layer;
            ChangeChildrenLayers(child);
        }
    }

    /// <summary>
    /// Назначает RuntimeTransformHandle переданный трансформируемый объект, чтобы отобразить гизмо.
    /// </summary>
    /// <param name="target">Объект, для которого нужно отобразить гизмо для трансформации.</param>
    public void AttachTarget(ITransformable target)
    {
        if (target == null)
            DetachTarget();
        else
        {
            _currentTarget = target;
            _transformHandle.target = _currentTarget.ObjectTransform;
            _additionalHandle.target = _currentTarget.ObjectTransform.GetChild(0);

            _transformHandle.gameObject.SetActive(true);
            _additionalHandle.gameObject.SetActive(_transformHandle.gameObject.activeInHierarchy && TransformType == HandleType.ROTATION);

            AttachHandleDragEvents(target);

            StartCoroutine(WaitForLayersToChange());
        }
    }

    /// <summary>
    /// Снимает объект с RuntimeTransformHandle, чтобы спрятать гизмо.
    /// </summary>
    public void DetachTarget()
    {
        DetachHandleDragEvents();

        _transformHandle.gameObject.SetActive(false);
        _additionalHandle.gameObject.SetActive(false);

        _transformHandle.target = null;
        _additionalHandle.target = null;

        _currentTarget = null;
    }

    /// <summary>
    /// Меняет тип трансформации на перемещение.
    /// </summary>
    public void SetPositionType()
    {
        TransformType = HandleType.POSITION;
    }

    /// <summary>
    /// Меняет тип трансформации на перемещение.
    /// </summary>
    public void SetPositionType(InputAction.CallbackContext context) => SetPositionType();

    /// <summary>
    /// Меняет тип трансформации на вращение.
    /// </summary>
    public void SetRotationType()
    {
        TransformType = HandleType.ROTATION;
    }

    /// <summary>
    /// Меняет тип трансформации на вращение.
    /// </summary>
    public void SetRotationType(InputAction.CallbackContext context) => SetRotationType();

    /// <summary>
    /// Меняет тип трансформации на масштабирование.
    /// </summary>
    public void SetScaleType()
    {
        TransformType = HandleType.SCALE;
    }

    /// <summary>
    /// Меняет тип трансформации на масштабирование.
    /// </summary>
    public void SetScaleType(InputAction.CallbackContext context) => SetScaleType();

    private void OnDisable()
    {
        _positionButton.action.performed -= SetPositionType;
        _rotationButton.action.performed -= SetRotationType;
        _scaleButton.action.performed -= SetScaleType;

        ObjectPicker.ObjectSelected -= AttachTarget;
        ObjectPicker.ObjectReleased -= DetachTarget;
    }

    /// <summary>
    /// Ждёт один кадр, чтобы поменять слои гизмо.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForLayersToChange()
    {
        yield return new WaitForSeconds(1);
        ChangeChildrenLayers(_transformHandle.transform);
        ChangeChildrenLayers(_additionalHandle.transform);
    }

    /// <summary>
    /// Назначает обработчики событий для RuntimeTransformHandle.
    /// </summary>
    /// <param name="target">Объект, для которого отображается гизмо для трансформации.</param>
    private void AttachHandleDragEvents(ITransformable target)
    {
        switch(TransformType)
        {
            case HandleType.POSITION:
                _transformHandle.isDraggingHandle.AddListener(target.OnMove);
                break;
            case HandleType.SCALE:
                _transformHandle.isDraggingHandle.AddListener(target.OnScale);
                break;
            default:
                break;
        }

        _additionalHandle.isDraggingHandle.AddListener(target.OnRotate);

        _transformHandle.endedDraggingHandle.AddListener(OnHandleGrabEnd);
        _additionalHandle.endedDraggingHandle.AddListener(OnHandleGrabEnd);
    }

    /// <summary>
    /// Снимает все обработчики событий с RuntimeTransformHandle.
    /// </summary>
    private void DetachHandleDragEvents()
    {
        _transformHandle.isDraggingHandle.RemoveAllListeners();
        _additionalHandle.isDraggingHandle.RemoveAllListeners();

        _transformHandle.endedDraggingHandle.RemoveAllListeners();
        _additionalHandle.endedDraggingHandle.RemoveAllListeners();
    }

    /// <summary>
    /// Передаёт прекрещение трансформирование целевому объекту.
    /// </summary>
    private void OnHandleGrabEnd()
    {
        _currentTarget?.OnTransformStop();
    }
}
