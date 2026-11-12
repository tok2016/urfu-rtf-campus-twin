using RuntimeHandle;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RuntimeTransformController : MonoBehaviour
{
    [SerializeField] private RuntimeTransformHandle _transformHandle, _additionalHandle;
    [SerializeField] private InputActionReference _positionButton, _rotationButton, _scaleButton;
    private HandleAxes _movementAxes = HandleAxes.XZ;

    private IDraggable _currentTarget;

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

    public HandleType Type
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

    public bool IsInteracting { get; private set; } = false;

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
    }

    private void ChangeChildrenLayers(Transform parent)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.layer = gameObject.layer;
            ChangeChildrenLayers(child);
        }
    }

    public void AttachTarget(IDraggable target)
    {
        if (target == null)
            DetachTarget();
        else
        {
            _currentTarget = target;
            _transformHandle.target = _currentTarget.ObjectTransform;
            _additionalHandle.target = _currentTarget.ObjectTransform.GetChild(0);

            _transformHandle.gameObject.SetActive(true);
            _additionalHandle.gameObject.SetActive(_transformHandle.gameObject.activeInHierarchy && Type == HandleType.ROTATION);

            AttachHandleDragEvents(target);

            StartCoroutine(WaitForLayersToChange());
        }
    }

    public void DetachTarget()
    {
        DetachHandleDragEvents();

        _transformHandle.gameObject.SetActive(false);
        _additionalHandle.gameObject.SetActive(false);

        _transformHandle.target = null;
        _additionalHandle.target = null;

        _currentTarget = null;
    }

    public void SetPositionType()
    {
        Type = HandleType.POSITION;
    }

    public void SetPositionType(InputAction.CallbackContext context) => SetPositionType();

    public void SetRotationType()
    {
        Type = HandleType.ROTATION;
    }

    public void SetRotationType(InputAction.CallbackContext context) => SetRotationType();

    public void SetScaleType()
    {
        Type = HandleType.SCALE;
    }

    public void SetScaleType(InputAction.CallbackContext context) => SetScaleType();

    private void OnDisable()
    {
        _positionButton.action.performed -= SetPositionType;
        _rotationButton.action.performed -= SetRotationType;
        _scaleButton.action.performed -= SetScaleType;
    }

    private IEnumerator WaitForLayersToChange()
    {
        yield return new WaitForSeconds(1);
        ChangeChildrenLayers(_transformHandle.transform);
        ChangeChildrenLayers(_additionalHandle.transform);
    }

    private void AttachHandleDragEvents(IDraggable target)
    {
        _transformHandle.startedDraggingHandle.AddListener(OnHandleGrabStart);
        _additionalHandle.startedDraggingHandle.AddListener(OnHandleGrabStart);

        switch(Type)
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

    private void DetachHandleDragEvents()
    {
        _transformHandle.startedDraggingHandle.RemoveAllListeners();
        _additionalHandle.startedDraggingHandle.RemoveAllListeners();

        _transformHandle.isDraggingHandle.RemoveAllListeners();
        _additionalHandle.isDraggingHandle.RemoveAllListeners();

        _transformHandle.endedDraggingHandle.RemoveAllListeners();
        _additionalHandle.endedDraggingHandle.RemoveAllListeners();
    }

    private void OnHandleGrabStart()
    {
        IsInteracting = true;
    }

    private void OnHandleGrabEnd()
    {
        IsInteracting = false;
        _currentTarget?.LetGo();
    }
}
