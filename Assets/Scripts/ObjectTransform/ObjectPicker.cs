using RuntimeHandle;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPicker : MonoBehaviour
{
    [SerializeField] private RuntimeTransformController _transformController;
    [SerializeField] private InputActionReference _selectAction, _dragAction;
    [SerializeField] private LayerMask _draggableMask, _wallHandlersMask, _wallMask;
    [SerializeField] private float _maxDistance;

    private Transform _hoveredTarget;
    private IDraggable _draggableObject;

    void Update()
    {
        if (_transformController.IsInteracting) return;

        if(_selectAction.action.IsPressed())
        {
            if (_draggableObject != null)
            {
                RaycastHit hit;
                var hitted = Physics.Raycast(
                    Camera.main.ScreenPointToRay(_dragAction.action.ReadValue<Vector2>()), 
                    out hit, 
                    _maxDistance, 
                    _wallHandlersMask);

                if (hitted && DoesHaveTheLayer(_wallMask, hit.transform.gameObject.layer))
                {
                    _draggableObject.Drag(hit.point, new Surface(hit.normal, hit.transform));
                }
            }
        }
        else
        {
            RaycastHit hit = new RaycastHit();
            var hitted = Physics.Raycast(
                Camera.main.ScreenPointToRay(_dragAction.action.ReadValue<Vector2>()),
                out hit);

            _hoveredTarget = hit.transform;
        }
        

        if (_selectAction.action.WasPerformedThisFrame())
        {
            if (_hoveredTarget && DoesHaveTheLayer(_draggableMask, _hoveredTarget.gameObject.layer))
            {
                _draggableObject = _hoveredTarget.parent.GetComponent<IDraggable>();
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
            if (_draggableObject != null)
            {
                _draggableObject.LetGo();
                _transformController.AttachTarget(_draggableObject);
            }
            _draggableObject = null;
        }
    }

    private bool DoesHaveTheLayer(LayerMask mask, int layer) => (mask & (1 << layer)) != 0;
}
