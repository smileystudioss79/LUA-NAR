using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;
using UnityEngine;
using LUNAR.Data;
using LUNAR.Lua;
using LUNAR.Logging;

namespace LUNAR.Management
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class LuaManager : MonoBehaviour
    {
        public static LuaManager Instance { get; private set; }

        private Script _script;
        private readonly List<string> _loadedScripts = new List<string>();
        private string _scriptsDir;
        private bool _ready = false;
        private float _tickInterval = 0.25f;
        private float _tickTimer = 0f;
        private bool _hasOnTick = false;
        private LuaFileWatcher _watcher;
        private GameObject _autopilotGO;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _scriptsDir = Path.Combine(KSPUtil.ApplicationRootPath, "GameData", "LUA-NAR", "Scripts");
            Directory.CreateDirectory(_scriptsDir);

            _autopilotGO = new GameObject("LUA-NAR_Autopilot");
            _autopilotGO.AddComponent<LuaAutopilotAPI>();
            DontDestroyOnLoad(_autopilotGO);

            InitScript();
            StartCoroutine(LoadAllScripts());

            _watcher = gameObject.AddComponent<LuaFileWatcher>();
            _watcher.StartWatching(_scriptsDir);
        }

        private void Update()
        {
            if (!_ready || !_hasOnTick) return;
            _tickTimer += Time.deltaTime;
            if (_tickTimer < _tickInterval) return;
            _tickTimer = 0f;

            try
            {
                DynValue fn = _script.Globals.Get("onTick");
                if (fn.Type == DataType.Function)
                    _script.Call(fn);
                else
                    _hasOnTick = false;
            }
            catch (InterpreterException ex) { LuaLogger.LogScriptError("onTick", ex); _hasOnTick = false; }
            catch (Exception ex) { LuaLogger.LogScriptError("onTick", ex); _hasOnTick = false; }
        }

        private void OnGUI()
        {
            LuaUIAPI.DrawGUI();
        }

        private void InitScript()
        {
            try
            {
                _script = new Script(CoreModules.Preset_SoftSandbox);
                CoreFunctions.Register(_script);
                LuaRegistry.RegisterAll(_script);
                LuaVesselAPI.Register(_script);
                LuaUIAPI.SetScript(_script);

                _script.Globals["setTickRate"] = (Action<float>)SetTickRate;

                _ready = true;
                LuaNarLog.AppendInfo("MoonSharp Script engine initialised.");
            }
            catch (Exception ex)
            {
                LuaNarLog.AppendError($"Failed to initialise MoonSharp: {ex.Message}");
                _ready = false;
            }
        }

        private void SetTickRate(float interval)
        {
            _tickInterval = Mathf.Max(0.05f, interval);
        }

        private IEnumerator LoadAllScripts()
        {
            yield return new WaitForSeconds(1f);
            if (!_ready || !Directory.Exists(_scriptsDir)) yield break;

            string[] files = Directory.GetFiles(_scriptsDir, "*.lua", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                ExecuteFile(file);
                yield return null;
            }

            _hasOnTick = _script.Globals.Get("onTick").Type == DataType.Function;
        }

        public void ExecuteFile(string path)
        {
            if (!_ready) { LuaNarLog.AppendError("Attempted ExecuteFile before engine ready."); return; }
            string name = Path.GetFileName(path);
            try
            {
                _script.DoString(File.ReadAllText(path), null, name);
                if (!_loadedScripts.Contains(name)) _loadedScripts.Add(name);
                LuaLogger.LogScriptLoaded(name);
                _hasOnTick = _script.Globals.Get("onTick").Type == DataType.Function;
            }
            catch (InterpreterException ex) { LuaLogger.LogScriptError(name, ex); }
            catch (Exception ex) { LuaLogger.LogScriptError(name, ex); }
        }

        public void ExecuteString(string code, string sourceName = "<inline>")
        {
            if (!_ready) return;
            try
            {
                _script.DoString(code, null, sourceName);
                _hasOnTick = _script.Globals.Get("onTick").Type == DataType.Function;
            }
            catch (InterpreterException ex) { LuaLogger.LogScriptError(sourceName, ex); }
            catch (Exception ex) { LuaLogger.LogScriptError(sourceName, ex); }
        }

        public DynValue CallFunction(string funcName, params object[] args)
        {
            if (!_ready) return DynValue.Nil;
            try
            {
                DynValue func = _script.Globals.Get(funcName);
                if (func.Type != DataType.Function) return DynValue.Nil;
                return _script.Call(func, args);
            }
            catch (InterpreterException ex) { LuaLogger.LogScriptError(funcName, ex); return DynValue.Nil; }
            catch (Exception ex) { LuaLogger.LogScriptError(funcName, ex); return DynValue.Nil; }
        }

        public IReadOnlyList<string> LoadedScripts => _loadedScripts;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_autopilotGO != null) Destroy(_autopilotGO);
        }
    }
}
