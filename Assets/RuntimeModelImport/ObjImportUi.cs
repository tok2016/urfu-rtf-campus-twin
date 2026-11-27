using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RuntimeModelImport
{
    /// <summary>
    /// Точка входа фичи. Фича активна только в тех сценах, где есть этот компонент:
    /// добавьте его на любой GameObject (остальные компоненты создадутся сами).
    /// F6 — выбрать OBJ/GLB/glTF, сохранить в папку пользовательских данных и импортировать.
    /// F7 — удалить последний импорт. Статус и ошибки — в OnGUI (временный UI).
    /// </summary>
    public class ObjImportUi : MonoBehaviour
    {
        private const string FileFilter = "3D models|*.obj;*.glb;*.gltf";

        [Header("Спавн модели")]
        [Tooltip("Расстояние вперёд от камеры, метры")]
        [SerializeField] private float spawnDistance = 3f;

        [Tooltip("Насколько уровень \"пола\" ниже камеры, метры (низ модели ставится на этот уровень)")]
        [SerializeField] private float floorOffsetBelowCamera = 1.5f;

        [Header("Ориентация OBJ")]
        [Tooltip("Включите, если модель лежит на боку: часть экспортов (например, 3ds Max) пишет Z-up вместо стандартного Y-up")]
        [SerializeField] private bool objIsZUp;

        private ObjModelFactory _objFactory;
        private GltfModelFactory _gltfFactory;
        private string _status = "F6 — импорт модели (OBJ/GLB/glTF), F7 — удалить последнюю";
        private GameObject _lastImported;

        private void Awake()
        {
            _objFactory = GetComponent<ObjModelFactory>();
            if (_objFactory == null)
                _objFactory = gameObject.AddComponent<ObjModelFactory>();

            _gltfFactory = GetComponent<GltfModelFactory>();
            if (_gltfFactory == null)
                _gltfFactory = gameObject.AddComponent<GltfModelFactory>();

            _objFactory.Loaded += OnLoaded;
            _objFactory.Failed += OnFailed;
            _gltfFactory.Loaded += OnLoaded;
            _gltfFactory.Failed += OnFailed;
        }

        private void OnDestroy()
        {
            if (_objFactory != null)
            {
                _objFactory.Loaded -= OnLoaded;
                _objFactory.Failed -= OnFailed;
            }
            if (_gltfFactory != null)
            {
                _gltfFactory.Loaded -= OnLoaded;
                _gltfFactory.Failed -= OnFailed;
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.f6Key.wasPressedThisFrame)
                PickAndImport();

            if (keyboard.f7Key.wasPressedThisFrame && _lastImported != null)
            {
                Destroy(_lastImported);
                _lastImported = null;
                _status = "Последний импорт удалён";
            }
        }

        private bool IsBusy => _objFactory.IsBusy || _gltfFactory.IsBusy;

        private void PickAndImport()
        {
            if (IsBusy)
                return;

            string path = Win32OpenFileDialog.Open("Выбор 3D-модели", FileFilter);
            if (string.IsNullOrEmpty(path))
                return;

            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            string extension = System.IO.Path.GetExtension(path).ToLowerInvariant();

            string savedPath;
            try
            {
                savedPath = ModelStorage.Save(path);       // импортируем уже из папки пользовательских данных
            }
            catch (Exception exception)
            {
                OnFailed($"не удалось сохранить модель: {exception.Message}");
                return;
            }

            _status = $"Загрузка {name}… ({extension})";
            if (extension == ".obj")
            {
                _objFactory.ZUp = objIsZUp;
                _objFactory.Import(savedPath, name, null);
            }
            else
                _gltfFactory.Import(savedPath, name, null);
        }

        private void OnLoaded(GameObject model)
        {
            _lastImported = model;
            ModelSpawn.PlaceInFrontOfCamera(model, spawnDistance, floorOffsetBelowCamera);

            int vertices = ModelSpawn.CountVertices(model);
            int triangles = ModelSpawn.CountTriangles(model);
            _status = $"{model.name}: вершины={vertices}, треугольники={triangles}\n" +
                      $"сохранено: {ModelStorage.RootFolder}";

            if (vertices == 0)
                Debug.LogWarning($"[{nameof(ObjImportUi)}] {model.name}: вершин 0 — файл пустой или не разобрался");
        }

        private void OnFailed(string error)
        {
            _status = $"Ошибка: {error}";
            Debug.LogError($"[{nameof(ObjImportUi)}] {error}");
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, Screen.width - 20, 44), _status);
        }
    }
}
