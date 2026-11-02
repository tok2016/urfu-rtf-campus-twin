using RuntimeHandle;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPicker : MonoBehaviour
{
    [SerializeField] private RuntimeTransformController _transformController;
    [SerializeField] private InputActionReference _selectAction, _dragAction;
    [SerializeField] private LayerMask _draggableMask, _wallMask;
    [SerializeField] private float _maxDistance;
    private DraggableObject _draggableObject;

    void Update()
    {
        if (_draggableObject != null && _selectAction.action.IsPressed())
        {
            RaycastHit ray = new RaycastHit();
            var hit = Physics.Raycast(
                Camera.main.ScreenPointToRay(_dragAction.action.ReadValue<Vector2>()), 
                out ray, 
                _maxDistance, 
                _wallMask);

            if (hit)
            {
                _draggableObject.Move(ray.point, ray.normal);
            }
        }

        if (_selectAction.action.WasPerformedThisFrame())
        {
            RaycastHit ray = new RaycastHit();
            var hit = Physics.Raycast(
                Camera.main.ScreenPointToRay(_dragAction.action.ReadValue<Vector2>()),
                out ray);

            if (hit && DoesMashHaveTheLayer(_draggableMask, ray.transform.gameObject.layer))
            {
                _draggableObject = ray.collider.GetComponent<DraggableObject>();
                if (_draggableObject)
                {
                    _transformController.AttachTarget(_draggableObject.transform);
                }
            }
            else
            {
                _draggableObject?.LetGo();
                _draggableObject = null;
                _transformController.DetachTarget();
            }
        }

        if (_selectAction.action.WasCompletedThisFrame())
        {
            _draggableObject?.LetGo();
            _draggableObject = null;
        }

        
    }

    private bool DoesMashHaveTheLayer(LayerMask mask, int layer) => (mask & (1 << layer)) != 0;
}
