using UnityEngine;

/// <summary>
/// Поведение для классов, чьи объекты могут быть трансформированы в реальном времени.
/// </summary>
public interface ITransformable : IHandleable
{
    /// <value>Объект, который можно перетаскивать.</value>
    public IDraggable ObjectToDrag { get; }

    /// <value>True, если объект пересекается с другими или не стоит на поверхности.</value>
    public bool IsIntersecting { get; set; }

    /// <summary>
    /// Совершает действия, когда объект перестаёт трансформироваться.
    /// </summary>
    public void OnTransformStop();

    /// <summary>
    /// Совершает действия при выборе объекта.
    /// </summary>
    public void OnSelect();

    /// <summary>
    /// Совершает действия при перемещении объекта.
    /// </summary>
    public void OnMove();

    /// <summary>
    /// Совершает действия при вращении объекта.
    /// </summary>
    public void OnRotate();

    /// <summary>
    /// Совершает действия при масштабировании объекта.
    /// </summary>
    public void OnScale();
}
