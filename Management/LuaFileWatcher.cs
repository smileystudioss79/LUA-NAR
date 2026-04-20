using System;
using System.IO;
using UnityEngine;
using LUNAR.Logging;

namespace LUNAR.Management
{
    public class LuaFileWatcher : MonoBehaviour
    {
        private FileSystemWatcher _watcher;
        private string _pendingFile;
        private bool _hasPending;

        public void StartWatching(string directory)
        {
            try
            {
                _watcher = new FileSystemWatcher(directory, "*.lua")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };

                _watcher.Created += OnFileEvent;
                _watcher.Changed += OnFileEvent;

                LuaNarLog.AppendInfo($"File watcher active on: {directory}");
            }
            catch (Exception ex)
            {
                LuaNarLog.AppendError($"File watcher failed to start: {ex.Message}");
            }
        }

        private void OnFileEvent(object sender, FileSystemEventArgs e)
        {
            lock (this)
            {
                _pendingFile = e.FullPath;
                _hasPending = true;
            }
        }

        private void Update()
        {
            string pending = null;
            lock (this)
            {
                if (_hasPending)
                {
                    pending = _pendingFile;
                    _hasPending = false;
                }
            }

            if (pending != null && LuaManager.Instance != null)
            {
                LuaNarLog.AppendInfo($"Hot-reloading: {Path.GetFileName(pending)}");
                LuaManager.Instance.ExecuteFile(pending);
            }
        }

        private void OnDestroy()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }
    }
}
