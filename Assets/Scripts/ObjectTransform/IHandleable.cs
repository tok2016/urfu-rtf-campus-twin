using UnityEngine;

/// <summary>
/// Поведение для объектов, которыми можно физически управлять.
/// </summary>
public interface IHandleable
{
    /// <value>Объект, который нужно трансформировать.</value>
    public ITransformable ObjectToTransform { get; }

    /// <value>Transform объекта.</value>
    public Transform ObjectTransform { get; }
}
