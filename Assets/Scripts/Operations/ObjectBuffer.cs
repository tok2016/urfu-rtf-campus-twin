using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Реализует буффер объектов: копирование, вставка, дублирование, удаление.
/// </summary>
public class ObjectBuffer : MonoBehaviour
{
    [SerializeField] private InputActionReference _copyInput, _pasteInput, _duplicateInput, _deleteInput;

    public static Action<ITransformable> ObjectPasted;
    public static Action ObjectDeleted;

    private ITransformable _objectToCopy, _objectToPaste;
    private float neighbourAngle = 0;
    private int startAnglesPerCircle, anglesPerCircle = 3;

    private void OnEnable()
    {
        ObjectPicker.ObjectSelected += OnObjectSelect;
        ObjectPicker.ObjectReleased += OnObjectRelease;

        _copyInput.action.performed += OnCopyClick;
        _pasteInput.action.performed += OnPasteClick;
        _duplicateInput.action.performed += OnDuplicateClick;
        _deleteInput.action.performed += OnDeleteClick;
    }

    /// <summary>
    /// Сохраняет ссылку на объект, который можно скопировать, когда он выбран.
    /// </summary>
    /// <param name="objectToCopy">Объект, который можно скопировать.</param>
    public void OnObjectSelect(ITransformable objectToCopy)
    {
        _objectToCopy = objectToCopy;
    }

    /// <summary>
    /// Сбрасывает ссылку на объект, делая невозможным его копирование при отпускании.
    /// </summary>
    public void OnObjectRelease()
    {
        _objectToCopy = null;
        anglesPerCircle = startAnglesPerCircle;
        neighbourAngle = 0;
    }

    /// <summary>
    /// Копирует выбранный объект и мохраняет ссылку на него, чтобы потом его вставить.
    /// </summary>
    public void OnCopyClick()
    {
        _objectToPaste = _objectToCopy;
    }

    public void OnCopyClick(InputAction.CallbackContext context) => OnCopyClick();

    /// <summary>
    /// Вставляет скопированный объект.
    /// </summary>
    public void OnPasteClick()
    {
        PasteObject(_objectToPaste);
    }

    public void OnPasteClick(InputAction.CallbackContext context) => OnPasteClick();

    /// <summary>
    /// Дублирует выбранный объект.
    /// </summary>
    public void OnDuplicateClick()
    {
        PasteObject(_objectToCopy);
    }

    public void OnDuplicateClick(InputAction.CallbackContext context) => OnDuplicateClick();

    /// <summary>
    /// Вставляет копию переданного объекта рядом с ним в разных направлениях относительно оригинала.
    /// </summary>
    /// <param name="objectToPaste">Объект, чья копия будет создана и размещена.</param>
    private void PasteObject(ITransformable objectToPaste)
    {
        if (objectToPaste != null)
        {
            var objectToInstantiate = objectToPaste.ObjectTransform;
            var direction = new Vector2(Mathf.Cos(neighbourAngle), Mathf.Sin(neighbourAngle));
            var targetPosition = objectToPaste.GetNeighbourPosition(direction);

            var copy = Instantiate(objectToInstantiate, targetPosition, objectToInstantiate.rotation);
            var transformable = copy.GetComponent<ITransformable>();

            anglesPerCircle += neighbourAngle % 360 == 0 ? 1 : 0;
            neighbourAngle += 360 / anglesPerCircle;

            ObjectPasted?.Invoke(transformable);
        }
    }

    /// <summary>
    /// Удаляет выбранный объект.
    /// </summary>
    public void OnDeleteClick()
    {
        if (_objectToCopy != null)
        {
            if (_objectToPaste == _objectToCopy)
                _objectToPaste = null;

            Destroy(_objectToCopy.ObjectTransform.gameObject);

            ObjectDeleted?.Invoke();
            _objectToCopy = null;            
        }
    }

    public void OnDeleteClick(InputAction.CallbackContext context) => OnDeleteClick();

    private void OnDisable()
    {
        ObjectPicker.ObjectSelected -= OnObjectSelect;
        ObjectPicker.ObjectReleased -= OnObjectRelease;

        _copyInput.action.performed -= OnCopyClick;
        _pasteInput.action.performed -= OnPasteClick;
        _duplicateInput.action.performed -= OnDuplicateClick;
        _deleteInput.action.performed -= OnDeleteClick;
    }
}
