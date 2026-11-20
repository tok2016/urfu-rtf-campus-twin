using UnityEngine;

/// <summary>
/// Поведение для любых классов, чьи объекты можно перетаскивать.
/// </summary>
public interface IDraggable : ITransformable
{
    /// <value>Слои поверхностей, на которые объект может быть размещён.</value>
    public LayerMask SurfaceLayers { get; }

    /// <summary>
    /// Размещает объект в целевой точке переданной поверхности.
    /// </summary>
    /// <param name="target">Точка размещения объекта на поверхности.</param>
    /// <param name="surface">Поверхность, на которую нужно разместить объект.</param>
    public void Drag(Vector3 target, Surface surface);

    /// <summary>
    /// Assigns main dependencies.
    /// </summary>
    /// <param name="bindableObject">Object that binds to surface.</param>
    public void Initialize(BindableObject bindableObject);
}
