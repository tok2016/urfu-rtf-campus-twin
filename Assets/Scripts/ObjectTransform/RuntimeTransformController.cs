using RuntimeHandle;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RuntimeTransformController : MonoBehaviour
{
    [SerializeField] private RuntimeTransformHandle _transformHandle;
    [SerializeField] private InputActionReference _positionButton, _rotationButton, _scaleButton;
    private HandleAxes _movementAxes = HandleAxes.XZ;

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
        }
    }

    void Start()
    {
        MovementAxes = Axes;
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

    public void AttachTarget(Transform target)
    {
        if (target == null)
            DetachTarget();
        else
        {
            _transformHandle.target = target;
            _transformHandle.gameObject.SetActive(true);
            StartCoroutine(WaitForLayersToChange());
        }
    }

    public void DetachTarget()
    {
        _transformHandle.gameObject.SetActive(false);
        _transformHandle.target = null;
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
    }
}
