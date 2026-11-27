using System;
using System.Collections;
using UnityEngine;

namespace RuntimeModelImport
{
    /// <summary>
    /// Обвязка AsImpL: грузит OBJ-файл с диска в сцену через LoaderObj.
    /// AsImpL работает корутинами на главном потоке, поэтому запуск только через StartCoroutine.
    /// </summary>
    public class ObjModelFactory : MonoBehaviour
    {
        private AsImpL.LoaderObj _loader;
        private AsImpL.ImportOptions _options;

        public bool IsBusy { get; private set; }
        public string LastError { get; private set; }

        /// <summary>Вертикальная ось исходника: false — Y-up (стандарт OBJ), true — Z-up (часть экспортов 3ds Max).</summary>
        public bool ZUp { get; set; }

        public Action<GameObject> Loaded;
        public Action<string> Failed;

        private void Awake()
        {
            _options = new AsImpL.ImportOptions
            {
                use32bitIndices = true,     // модели >65k вершин
                buildColliders = true,
                modelScaling = 1f,
                inheritLayer = false
            };
        }

        public void Import(string objPath, string objectName, Transform parent)
        {
            if (IsBusy)
            {
                LastError = "Предыдущая загрузка ещё не завершена";
                Failed?.Invoke(LastError);
                return;
            }

            IsBusy = true;
            LastError = null;
            _options.zUp = ZUp;

            _loader = gameObject.AddComponent<AsImpL.LoaderObj>();
            _loader.buildOptions = _options;
            _loader.Builder.MaterialFactory = new UrpMaterialFactory();
            _loader.ModelLoaded += OnModelLoaded;
            _loader.ModelError += OnModelError;

            // AsImpL разрешает .mtl и текстуры относительно папки .obj (LoaderObj.cs:110-150)
            StartCoroutine(LoadRoutine(objectName, objPath, parent));
        }

        /// <summary>
        /// Крутим корутину библиотеки вручную: при yield return на чужой IEnumerator Unity не даёт
        /// перехватить исключение парсера, и тогда IsBusy навсегда остаётся true (импорт умирает до перезапуска).
        /// </summary>
        private IEnumerator LoadRoutine(string name, string path, Transform parent)
        {
            IEnumerator inner = _loader.Load(name, path, parent);
            while (true)
            {
                object current = null;
                bool moved;
                try
                {
                    moved = inner.MoveNext();
                    if (moved)
                        current = inner.Current;
                }
                catch (Exception exception)
                {
                    IsBusy = false;
                    LastError = $"Исключение при разборе {name}: {exception.Message}";
                    CleanupLoader();
                    Failed?.Invoke(LastError);
                    Debug.LogException(exception);
                    yield break;
                }

                if (!moved)
                    yield break;

                yield return current;
            }
        }

        private void OnModelLoaded(GameObject loadedObject, string objectName)
        {
            IsBusy = false;

            foreach (var renderer in loadedObject.GetComponentsInChildren<Renderer>())
                foreach (var material in renderer.sharedMaterials)
                    UnityPropertyRemap.Apply(material);

            CleanupLoader();
            Loaded?.Invoke(loadedObject);
        }

        private void OnModelError(string errorMessage)
        {
            IsBusy = false;
            LastError = errorMessage;
            CleanupLoader();
            Failed?.Invoke(errorMessage);
        }

        private void CleanupLoader()
        {
            if (_loader == null)
                return;

            _loader.ModelLoaded -= OnModelLoaded;
            _loader.ModelError -= OnModelError;
            Destroy(_loader);
            _loader = null;
        }
    }
}
