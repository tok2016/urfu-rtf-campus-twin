using UnityEngine;

namespace RuntimeModelImport
{
    /// <summary>
    /// Общие операции для обоих форматов: куда поставить модель и как посчитать статистику.
    /// </summary>
    public static class ModelSpawn
    {
        /// <summary>Ставит модель перед камерой: низ модели — на уровень "пола" под камерой.</summary>
        /// <param name="distance">Расстояние вперёд от камеры, м.</param>
        /// <param name="floorOffset">Насколько "пол" ниже камеры, м.</param>
        public static void PlaceInFrontOfCamera(GameObject model, float distance, float floorOffset)
        {
            Camera camera = Camera.main;
            if (camera == null)         // в тестовых сценах проекта Player-префаба с камерой нет
                return;

            Transform root = model.transform;
            root.position = camera.transform.position + camera.transform.forward * distance;

            Renderer firstRenderer = model.GetComponentInChildren<Renderer>();
            if (firstRenderer == null)
                return;

            float groundY = camera.transform.position.y - floorOffset;
            Vector3 position = root.position;
            position.y -= firstRenderer.bounds.min.y - groundY;
            root.position = position;
        }

        public static int CountVertices(GameObject root)
        {
            int count = 0;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh != null)
                    count += filter.sharedMesh.vertexCount;
            }
            return count;
        }

        public static int CountTriangles(GameObject root)
        {
            int count = 0;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null)
                    continue;

                for (int i = 0; i < mesh.subMeshCount; i++)
                    count += (int)mesh.GetIndexCount(i) / 3;
            }
            return count;
        }
    }
}
