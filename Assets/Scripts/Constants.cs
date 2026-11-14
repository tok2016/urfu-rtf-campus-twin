using UnityEngine;

/// <summary>
/// Содержит глобальные константы, доступные в любом месте.
/// </summary>
[CreateAssetMenu(fileName = "Constants", menuName = "Scriptable Objects/Constants")]
public class Constants : ScriptableObject
{
    public float MinObjectScale = 0.1f;
    public float MaxObjectScale = 5f;

    [ColorUsage(false, true)]
    public Color ObjectWarningHighlight;

    public LayerMask ObjectsIntersectionLayers;

    private static Constants _instance;

    public static Constants Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<Constants>("Constants");
            return _instance;
        }
    }

    /// <summary>
    /// Проверяет, есть ли в маске нужный слой.
    /// </summary>
    /// <param name="mask">Маска слоёв.</param>
    /// <param name="layer">Слой для проверки</param>
    /// <returns>True, если слой найден в маске.</returns>
    public bool DoesHaveTheLayer(LayerMask mask, int layer) => (mask & (1 << layer)) != 0;
}
