using UnityEngine;

namespace RuntimeModelImport
{
    /// <summary>
    /// Материал для импортированных моделей. AsImpL создаёт материалы только через IMaterialFactory
    /// (для OBJ без .mtl берёт built-in "VertexLit", не спрашивая IShaderSelector), поэтому подменяем фабрику целиком:
    /// имя шейдера игнорируем, отдаём URP Lit. URP-материал запоминаем, чтобы remap-пост-проход
    /// (AsImpL пишет в "_MainTex"/"_Glossiness", у URP Lit это "_BaseMap"/"_Smoothness") применялся к его материалам.
    /// </summary>
    public class UrpMaterialFactory : AsImpL.IMaterialFactory
    {
        private const string UrpLitShader = "Universal Render Pipeline/Lit";

        public Material Create(string shaderName)
        {
            Shader shader = Shader.Find(UrpLitShader);
            if (shader == null)
            {
                Debug.LogWarning($"[{nameof(UrpMaterialFactory)}] Shader '{UrpLitShader}' not found — модель будет розовой. " +
                                 "Назначьте материал, ссылающийся на URP Lit, в собираемой сцене.");
                shader = Shader.Find("Legacy Shaders/Diffuse");
            }
            return new Material(shader);
        }
    }

    /// <summary>
    /// Пост-фикс свойств: AsImpL адресует built-in пайплайн (_MainTex, _Glossiness),
    /// URP Lit использует _BaseMap/_Smoothness (_BumpMap/_MetallicGlossMap совпадают — их не трогаем).
    /// </summary>
    public static class UnityPropertyRemap
    {
        public static void Apply(Material material)
        {
            if (material == null || material.shader == null || material.shader.name != UrpMaterialFactoryLit)
                return;

            Texture mainTex = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
            if (mainTex != null && material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", mainTex);

            if (material.HasProperty("_Glossiness") && material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", material.GetFloat("_Glossiness"));
        }

        private const string UrpMaterialFactoryLit = "Universal Render Pipeline/Lit";
    }
}
