using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpriteGenerator
{
    internal static class NativeFolderPicker
    {
        internal static bool TrySelectFolder(
            string initialDirectory,
            out string selectedDirectory,
            out string errorMessage)
        {
            selectedDirectory = string.Empty;
            errorMessage = string.Empty;

#if UNITY_EDITOR
            try
            {
                string initial = ResolveInitialDirectory(initialDirectory);
                string selected = EditorUtility.OpenFolderPanel(
                    "Choose Sprite Export Folder",
                    initial,
                    string.Empty);
                if (string.IsNullOrWhiteSpace(selected))
                {
                    return false;
                }

                selectedDirectory = Path.GetFullPath(selected);
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = BuildErrorMessage(exception);
                return false;
            }
#elif UNITY_STANDALONE_WIN
            return TrySelectWindowsFolder(out selectedDirectory, out errorMessage);
#else
            errorMessage = "Folder browsing is unavailable on this platform. Paste a folder path into the export field.";
            return false;
#endif
        }

        private static string ResolveInitialDirectory(string initialDirectory)
        {
            if (!string.IsNullOrWhiteSpace(initialDirectory))
            {
                string candidate = initialDirectory.Trim().Trim('"', '\'');
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            return RuntimePresentationSettings.ResolveDefaultExportDirectory();
        }

        private static string BuildErrorMessage(Exception exception)
        {
            return $"Windows could not open the folder browser: {exception.Message} " +
                   "Paste a folder path into the export field instead.";
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const int DisplayNameCapacity = 512;
        private const int LongPathCapacity = 32768;
        private const uint BrowseReturnOnlyFileSystemDirectories = 0x0001;
        private const uint BrowseEditBox = 0x0010;
        private const uint BrowseNewDialogStyle = 0x0040;
        private const uint CoInitializeApartmentThreaded = 0x2;

        private static bool TrySelectWindowsFolder(
            out string selectedDirectory,
            out string errorMessage)
        {
            string selected = string.Empty;
            string error = string.Empty;
            bool succeeded = false;

            try
            {
                var pickerThread = new Thread(() =>
                {
                    try
                    {
                        // The Unity thread waits synchronously below, so the dialog must
                        // remain unowned to avoid cross-thread modal window messages.
                        succeeded = ShowWindowsFolderDialog(IntPtr.Zero, out selected);
                    }
                    catch (Exception exception)
                    {
                        error = BuildErrorMessage(exception);
                    }
                })
                {
                    IsBackground = true,
                    Name = "Sprite Export Folder Picker"
                };

                pickerThread.SetApartmentState(ApartmentState.STA);
                pickerThread.Start();
                pickerThread.Join();
            }
            catch (Exception exception)
            {
                error = BuildErrorMessage(exception);
            }

            selectedDirectory = selected;
            errorMessage = error;
            return succeeded;
        }

        private static bool ShowWindowsFolderDialog(IntPtr ownerWindow, out string selectedDirectory)
        {
            selectedDirectory = string.Empty;
            int initializeResult = CoInitializeEx(IntPtr.Zero, CoInitializeApartmentThreaded);
            if (initializeResult < 0)
            {
                Marshal.ThrowExceptionForHR(initializeResult);
            }

            IntPtr displayNameBuffer = IntPtr.Zero;
            IntPtr itemIdList = IntPtr.Zero;
            try
            {
                displayNameBuffer = Marshal.AllocHGlobal(DisplayNameCapacity * sizeof(char));
                var browseInfo = new BrowseInfo
                {
                    OwnerWindow = ownerWindow,
                    RootItemIdList = IntPtr.Zero,
                    DisplayName = displayNameBuffer,
                    Title = "Choose the folder where generated sprites will be saved",
                    Flags = BrowseReturnOnlyFileSystemDirectories | BrowseEditBox | BrowseNewDialogStyle,
                    Callback = IntPtr.Zero,
                    CallbackData = IntPtr.Zero,
                    ImageIndex = 0
                };

                itemIdList = SHBrowseForFolder(ref browseInfo);
                if (itemIdList == IntPtr.Zero)
                {
                    return false;
                }

                var pathBuffer = new StringBuilder(LongPathCapacity);
                bool pathResolved;
                try
                {
                    pathResolved = SHGetPathFromIDListEx(
                        itemIdList,
                        pathBuffer,
                        (uint)pathBuffer.Capacity,
                        0);
                }
                catch (EntryPointNotFoundException)
                {
                    pathResolved = SHGetPathFromIDList(itemIdList, pathBuffer);
                }

                if (!pathResolved || pathBuffer.Length == 0)
                {
                    throw new ExternalException("The selected shell item is not a file-system folder.");
                }

                selectedDirectory = Path.GetFullPath(pathBuffer.ToString());
                return true;
            }
            finally
            {
                if (itemIdList != IntPtr.Zero)
                {
                    CoTaskMemFree(itemIdList);
                }

                if (displayNameBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(displayNameBuffer);
                }

                CoUninitialize();
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct BrowseInfo
        {
            internal IntPtr OwnerWindow;
            internal IntPtr RootItemIdList;
            internal IntPtr DisplayName;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string Title;

            internal uint Flags;
            internal IntPtr Callback;
            internal IntPtr CallbackData;
            internal int ImageIndex;
        }

        [DllImport("shell32.dll", EntryPoint = "SHBrowseForFolderW", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHBrowseForFolder(ref BrowseInfo browseInfo);

        [DllImport("shell32.dll", EntryPoint = "SHGetPathFromIDListW", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SHGetPathFromIDList(IntPtr itemIdList, StringBuilder path);

        [DllImport("shell32.dll", EntryPoint = "SHGetPathFromIDListEx", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SHGetPathFromIDListEx(
            IntPtr itemIdList,
            StringBuilder path,
            uint pathCapacity,
            uint flags);

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr reserved, uint coInitialize);

        [DllImport("ole32.dll")]
        private static extern void CoUninitialize();

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr memory);
#endif
    }
}
