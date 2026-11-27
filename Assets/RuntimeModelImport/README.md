# RuntimeModelImport

Импорт 3D-моделей с диска во время работы приложения.

## Как включить на сцене

1. Создать пустой GameObject.
2. Add Component → **ObjImportUi**.
3. Play.

`ObjModelFactory` и `GltfModelFactory` добавятся на этот объект сами. Фича работает
только в тех сценах, где есть `ObjImportUi`.

## Управление

- **F6** — диалог выбора файла и импорт (`.obj`, `.glb`, `.gltf`).
- **F7** — удалить последнюю импортированную модель.
- Статус (путь, вершины, треугольники, ошибка) — строка в левом верхнем углу.

## Настройки в Inspector

| Поле | Что делает |
| --- | --- |
| Spawn Distance | расстояние вперёд от камеры, м (по умолчанию 3) |
| Floor Offset Below Camera | насколько «пол» ниже камеры, м (по умолчанию 1.5) |
| Obj Is Z Up | включить, если OBJ лежит на боку |

## Как работает

1. Выбранный файл и его зависимости копируются в
   `Application.persistentDataPath/ImportedModels/<имя модели>/`:
   - `.obj` → `.mtl` и текстуры из `map_*` (в копии `.mtl` убираются опции `-s`/`-o`,
     ключ приводится к виду, который понимает AsImpL);
   - `.gltf` → внешние `.bin` и картинки, на которые ссылается `"uri"`;
   - `.glb` → один файл.
2. Модель импортируется из копии и появляется в 3 м перед камерой, низом на уровне «пола».
3. Если `Camera.main` нет — модель остаётся в начале координат.

Папка на Windows: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\urfu-rtf-campus-twin\ImportedModels`.
Повторный импорт файла с тем же именем перезаписывает папку.

## Ограничения

- Draco/KTX2-сжатые `.glb` не поддерживаются (нужны пакеты `com.unity.cloud.draco` / `ktx`).
- Имена текстур с пробелами не поддерживаются.
- В собранном билде материалы могут стать розовыми: URP Lit и шейдеры glTFast вырезаются,
  если на них нет ссылок из сцен build-а.
- Большие модели грузятся в главном потоке: `couch` (1 млн вершин) — около 38 с подвисания.
- Лимитов по полигонам/размеру пока нет.

## Файлы

- `ObjImportUi.cs` — точка входа, хоткеи, статус, настройки спавна.
- `ObjModelFactory.cs` — OBJ через AsImpL (`Packages/com.gpvigano.asimpl`).
- `UrpMaterialFactory.cs` — материалы OBJ на URP Lit + перенос `_MainTex`→`_BaseMap`.
- `GltfModelFactory.cs` — GLB/glTF через glTFast.
- `ModelStorage.cs` — копирование модели с зависимостями в папку пользователя.
- `ModelSpawn.cs` — позиция спавна и подсчёт вершин/треугольников.
- `Win32OpenFileDialog.cs` — нативный диалог выбора файла (comdlg32).

## Зависимости

- **AsImpL** — `Packages/com.gpvigano.asimpl` (embed-копия 1.0.0, MIT, сборка `AsImpL`), OBJ.
- **glTFast** (`com.unity.cloud.gltfast` 6.20.0, UPM assembly `glTFast`) — GLB/glTF.
- **Unity Input System** (`com.unity.inputsystem`) — хоткеи.
- **URP** (`com.unity.render-pipelines.universal`) — материалы.

