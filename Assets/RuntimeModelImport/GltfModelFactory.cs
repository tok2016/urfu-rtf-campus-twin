using System;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace RuntimeModelImport
{
    /// <summary>
    /// Загрузка GLB/glTF через glTFast. В отличие от AsImpL работает на Task:
    /// исполнение продолжается на главном потоке Unity, поэтому инстанцирование безопасно.
    /// </summary>
    public class GltfModelFactory : MonoBehaviour
    {
        public bool IsBusy { get; private set; }
        public string LastError { get; private set; }

        public Action<GameObject> Loaded;
        public Action<string> Failed;

        public void Import(string path, string objectName, Transform parent)
        {
            if (IsBusy)
            {
                Failed?.Invoke("Предыдущая загрузка ещё не завершена");
                return;
            }

            IsBusy = true;
            LastError = null;
            _ = ImportAsync(path, objectName, parent);
        }

        private async Task ImportAsync(string path, string objectName, Transform parent)
        {
            GameObject root = null;
            var gltf = new GltfImport();
            try
            {
                // Uri обязателен: по нему glTFast резолвит относительные .bin и текстуры
                bool loaded = await gltf.LoadFile(path, new Uri(path));
                if (!loaded)
                {
                    Fail(gltf, "не удалось разобрать файл");
                    return;
                }

                root = new GameObject(objectName);
                if (parent != null)
                    root.transform.SetParent(parent, false);

                bool instantiated = await gltf.InstantiateMainSceneAsync(root.transform);
                if (!instantiated || ModelSpawn.CountVertices(root) == 0)
                {
                    Fail(gltf, "в файле нет сцены с мешами");
                    if (root != null)
                        Destroy(root);
                    return;
                }

                // Меши и материалы принадлежат GltfImport — освобождаем их вместе с моделью
                root.AddComponent<GltfLifetime>().Attach(gltf);

                IsBusy = false;
                Loaded?.Invoke(root);
            }
            catch (Exception exception)
            {
                Fail(gltf, exception.Message);
                if (root != null)
                    Destroy(root);
            }
        }

        private void Fail(GltfImport gltf, string message)
        {
            IsBusy = false;
            LastError = $"glTF: {message}";
            gltf.Dispose();
            Failed?.Invoke(LastError);
        }
    }

    /// <summary>Держит GltfImport ровно столько, сколько живёт загруженная модель.</summary>
    internal class GltfLifetime : MonoBehaviour
    {
        private GltfImport _gltf;

        public void Attach(GltfImport gltf) => _gltf = gltf;

        private void OnDestroy()
        {
            _gltf?.Dispose();
            _gltf = null;
        }
    }
}
