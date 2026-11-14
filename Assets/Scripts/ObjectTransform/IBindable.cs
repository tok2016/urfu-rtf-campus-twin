using UnityEngine;

/// <summary>
/// Поведение для объектов, способных к привязке к поверхности.
/// </summary>
public interface IBindable : IHandleable
{
    /// <summary>
    /// Совершает действия при прекращении вращения.
    /// </summary>
    /// <param name="isIntersecting">Пересекается ли объект с другими или нет.</param>
    public void OnRelease(bool isIntersecting);

    /// <summary>
    /// Проверяет, пересекается ли объект с другими.
    /// </summary>
    /// <param name="surface">Поверхность, на которой размещён объект.</param>
    /// <param name="localScale">Масштаб родительского объекта.</param>
    /// <returns>True, если объект пересекается с хотя бы одним другим объектом или если переданная поверхность пуста.</returns>
    public bool CheckIntersection(Surface surface, Vector3 localScale);

    /// <summary>
    /// Находит под объектом (в направлении локального пространства) поверхность.
    /// </summary>
    /// <param name="localScale">Масштаб родительского объекта.</param>
    /// <param name="surfaceLayers">Слои поверхности, на которых объект может находиться.</param>
    /// <returns>
    /// Поверхность с нормалью и Transform.
    /// Если поверхность не найдена, возвращает пустой Transform и нулевой вектор нормали.
    /// </returns>
    public Surface GetNearestSurface(Vector3 localScale, LayerMask surfaceLayers);

    /// <summary>
    /// Совершает действия при вращении объекта.
    /// </summary>
    /// <param name="surfaceNormal">Поверхность, на которой размещён объект.</param>
    public void OnRotate(Vector3 surfaceNormal);
}
