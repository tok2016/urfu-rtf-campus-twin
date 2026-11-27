using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RuntimeModelImport
{
    /// <summary>
    /// Нативный диалог выбора файла (comdlg32) для Windows Standalone. Без пакетов.
    /// </summary>
    public static class Win32OpenFileDialog
    {
        public static string Open(string title, string filter)
        {
            var dialog = new OpenFileNameW
            {
                structSize = Marshal.SizeOf(typeof(OpenFileNameW)),
                filter = filter.Replace("|", "\0") + "\0",
                file = new string('\0', 4096),
                maxFile = 4096,
                fileTitle = new string('\0', 260),
                maxFileTitle = 260,
                initialDir = null,
                dialogTitle = title,
                flags = unchecked((int)(OpenFileNameFlags.OFN_EXPLORER
                        | OpenFileNameFlags.OFN_FILEMUSTEXIST
                        | OpenFileNameFlags.OFN_PATHMUSTEXIST
                        | OpenFileNameFlags.OFN_NOCHANGEDIR
                        | OpenFileNameFlags.OFN_HIDEREADONLY))
            };

            if (!GetOpenFileNameW(ref dialog))
            {
                // 0 = пользователь отменил, но при ошибке тоже вернётся 0 — логируем код
                int error = CommDlgExtendedError();
                if (error != 0)
                    Debug.LogWarning($"OpenFileDialog failed, CommDlgExtendedError={error}");
                return null;
            }

            // Маршаллер Mono убирает завершающий '\0' из буфера, поэтому IndexOf может вернуть -1
            string file = dialog.file;
            if (string.IsNullOrEmpty(file))
                return null;

            int terminator = file.IndexOf('\0');
            string selected = terminator >= 0 ? file.Substring(0, terminator) : file;
            return selected.Length > 0 && System.IO.File.Exists(selected) ? selected : null;
        }

        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetOpenFileNameW(ref OpenFileNameW ofn);

        [DllImport("comdlg32.dll")]
        private static extern int CommDlgExtendedError();

        [Flags]
        private enum OpenFileNameFlags
        {
            OFN_READONLY = 0x1,
            OFN_HIDEREADONLY = 0x4,
            OFN_FILEMUSTEXIST = 0x1000,
            OFN_PATHMUSTEXIST = 0x800,
            OFN_NOCHANGEDIR = 0x8,
            OFN_EXPLORER = 0x80000
        }

        // Layout должен совпадать с WIN32 OPENFILENAMEW: все поля указателей — IntPtr, размер фиксируется Pack=1 по умолчанию для x64
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct OpenFileNameW
        {
            public int structSize;
            public IntPtr owner;
            public IntPtr instance;
            public string filter;
            public string customFilter;
            public int maxCustFilter;
            public int filterIndex;
            public string file;
            public int maxFile;
            public string fileTitle;
            public int maxFileTitle;
            public string initialDir;
            public string dialogTitle;
            public int flags;            // на x64 после строк-указателей выравнивание даёт 4 байта паддинга — int совпадает с нативным DWORD
            public int fileOffset;
            public string defExt;
            public IntPtr custData;
            public IntPtr hook;
            public string templateName;
            public IntPtr reservedPtr;
            public int reservedInt;
            public int flagsEx;
        }
    }
}
