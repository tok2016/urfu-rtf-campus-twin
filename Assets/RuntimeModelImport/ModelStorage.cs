using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RuntimeModelImport
{
    /// <summary>
    /// Копирует модель и её зависимости (.mtl, текстуры, .bin) в папку пользовательских данных,
    /// чтобы импорт не зависел от исходного расположения файлов и переживал перезапуск приложения.
    /// </summary>
    public static class ModelStorage
    {
        private static readonly Regex UriRegex = new Regex("\"uri\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);

        // AsImpL понимает только эти ключи и с точным регистром (switch в LoaderObj.cs),
        // поэтому при копировании приводим ключ к каноническому написанию.
        private static readonly string[] TextureKeys =
        {
            "map_kd", "map_ka", "map_ks", "map_d", "map_opacity", "map_bump", "bump", "refl", "map_ns", "norm"
        };

        private static readonly string[] CanonicalKeys =
        {
            "map_Kd", "map_Ka", "map_Ks", "map_d", "map_opacity", "map_bump", "bump", "refl", "map_Ns", "map_bump"
        };

        public static string RootFolder => Path.Combine(Application.persistentDataPath, "ImportedModels");

        /// <summary>Сохраняет модель с зависимостями и возвращает путь к её копии.</summary>
        public static string Save(string sourcePath)
        {
            string sourceDir = Path.GetDirectoryName(sourcePath);
            string targetDir = Path.Combine(RootFolder, Sanitize(Path.GetFileNameWithoutExtension(sourcePath)));
            Directory.CreateDirectory(targetDir);

            string target = Path.Combine(targetDir, Path.GetFileName(sourcePath));
            File.Copy(sourcePath, target, true);

            string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension == ".obj")
                CopyObjDependencies(sourcePath, sourceDir, targetDir);
            else if (extension == ".gltf")
                CopyGltfDependencies(sourcePath, sourceDir, targetDir);

            return target;
        }

        private static void CopyObjDependencies(string objPath, string sourceDir, string targetDir)
        {
            foreach (string line in File.ReadAllLines(objPath))
            {
                string trimmed = line.Trim();
                if (!trimmed.StartsWith("mtllib", StringComparison.OrdinalIgnoreCase))
                    continue;

                string mtlRelative = trimmed.Substring("mtllib".Length).Trim();
                if (mtlRelative.Length == 0)
                    continue;

                string mtlPath = Resolve(sourceDir, mtlRelative);
                if (mtlPath == null)
                {
                    Debug.LogWarning($"[{nameof(ModelStorage)}] .mtl не найден: {mtlRelative} — модель будет без материалов");
                    continue;
                }

                CopyAndRewriteMtl(mtlPath, mtlRelative, sourceDir, targetDir);
            }
        }

        /// <summary>
        /// Копирует .mtl, приводя строки текстур к виду "ключ файл". AsImpL не поддерживает опции
        /// (-s, -o, -bm): он берёт весь остаток строки как путь (LoaderObj.cs:471-480) и получает 404.
        /// </summary>
        private static void CopyAndRewriteMtl(string mtlPath, string mtlRelative, string sourceDir, string targetDir)
        {
            string[] lines = File.ReadAllLines(mtlPath);
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.Length == 0 || trimmed[0] == '#')
                    continue;

                string[] parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    continue;

                int keyIndex = Array.IndexOf(TextureKeys, parts[0].ToLowerInvariant());
                if (keyIndex < 0)
                    continue;

                // имя файла — последний токен, до него идут опции (-s, -o, -bm)
                string textureRelative = parts[parts.Length - 1];
                string texturePath = Resolve(sourceDir, textureRelative);
                if (texturePath == null)
                {
                    Debug.LogWarning($"[{nameof(ModelStorage)}] текстура не найдена: {textureRelative} (в {mtlRelative})");
                    continue;
                }

                CopyRelative(sourceDir, targetDir, textureRelative, texturePath);
                lines[i] = CanonicalKeys[keyIndex] + " " + textureRelative;
            }

            string destination = Path.Combine(targetDir, mtlRelative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.WriteAllLines(destination, lines);
        }

        private static void CopyGltfDependencies(string gltfPath, string sourceDir, string targetDir)
        {
            string json = File.ReadAllText(gltfPath);
            foreach (Match match in UriRegex.Matches(json))
            {
                string relative = Uri.UnescapeDataString(match.Groups[1].Value);
                if (relative.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (relative.Contains("://"))
                    continue;

                string dependency = Resolve(sourceDir, relative);
                if (dependency != null)
                    CopyRelative(sourceDir, targetDir, relative, dependency);
            }
        }

        private static string Resolve(string sourceDir, string relative)
        {
            string candidate = Path.Combine(sourceDir, relative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
                return candidate;

            candidate = Path.Combine(sourceDir, Path.GetFileName(relative));
            return File.Exists(candidate) ? candidate : null;
        }

        private static void CopyRelative(string sourceDir, string targetDir, string relative, string actualPath)
        {
            string destination = Path.Combine(targetDir, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destination));

            if (string.Equals(Path.GetFullPath(destination), Path.GetFullPath(actualPath), StringComparison.OrdinalIgnoreCase))
                return;

            File.Copy(actualPath, destination, true);
        }

        private static string Sanitize(string name)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');

            return string.IsNullOrWhiteSpace(name) ? "model" : name;
        }
    }
}
