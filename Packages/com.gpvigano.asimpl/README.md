# AsImpL (embedded copy)

Runtime-загрузчик OBJ + MTL для Unity. Это локальная embed-копия
[gpvigano/AsImpL](https://github.com/gpvigano/AsImpL), коммит `f9b3dc8a41a7` (MIT, `LICENSE.md`).

Git-установка оригинального репозитория невозможна: это Unity-проект, а не пакет
(`package.json` в корне рядом с `Assets/` и `ProjectSettings/`), Unity игнорирует вложенную
папку `Assets/` и сборка проекта падает. Поэтому копия переложена в структуру пакета вручную.

## Что изменено относительно оригинала

- `Assets/AsImpL/Scripts/**/*.cs` → `Runtime/` (без изменений кода), добавлен `Runtime/AsImpL.asmdef`.
- Не скопированы: `Scripts/Editor/` (окно импорта в редакторе), `Examples/` (используют legacy `Input`),
  `Assets/AsImpL/Resources/` (материалы под built-in Standard и legacy-шейдер), `ProjectSettings/`,
  `models/`, `images/`, `Documentation/`, `.github/`.

## Обновление

Вручную: заменить содержимое `Runtime/` на файлы нового коммита оригинала (код не патчим —
все наши точки расширения (`IMaterialFactory`, `LoaderObj`) подключаются снаружи).
