using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace LUNAR.Data
{
    public static class LuaUIAPI
    {
        private class LuaWindow
        {
            public int Id;
            public string Title;
            public Rect Rect;
            public bool Visible;
            public readonly List<LuaWidget> Widgets = new List<LuaWidget>();
        }

        private abstract class LuaWidget { }
        private class LabelWidget : LuaWidget { public string Text; }
        private class ButtonWidget : LuaWidget { public string Label; public DynValue Callback; }
        private class SpaceWidget : LuaWidget { public float Pixels; }
        private class SeparatorWidget : LuaWidget { }

        private static readonly Dictionary<string, LuaWindow> _windows = new Dictionary<string, LuaWindow>();
        private static Script _script;
        private static int _idCounter = 0x4C4E0000;

        public static void SetScript(Script s) { _script = s; }

        public static void Register(Script script)
        {
            _script = script;

            script.Globals["showGUI"]       = (Action<string>)ShowSimple;
            script.Globals["hideGUI"]       = (Action)HideSimple;
            script.Globals["guiCreate"]     = (Action<string, string, float, float, float, float>)CreateWindow;
            script.Globals["guiShow"]       = (Action<string>)ShowWindow;
            script.Globals["guiHide"]       = (Action<string>)HideWindow;
            script.Globals["guiSetTitle"]   = (Action<string, string>)SetTitle;
            script.Globals["guiClear"]      = (Action<string>)ClearWindow;
            script.Globals["guiLabel"]      = (Action<string, string>)AddLabel;
            script.Globals["guiButton"]     = (Action<string, string, DynValue>)AddButton;
            script.Globals["guiSpace"]      = (Action<string, float>)AddSpace;
            script.Globals["guiSeparator"]  = (Action<string>)AddSeparator;
            script.Globals["guiMove"]       = (Action<string, float, float>)MoveWindow;
        }

        private static void ShowSimple(string text)
        {
            const string id = "__simple__";
            if (!_windows.ContainsKey(id))
                CreateWindow(id, "LUA-NAR", 20, 20, 300, 120);
            LuaWindow w = _windows[id];
            w.Widgets.Clear();
            w.Widgets.Add(new LabelWidget { Text = text ?? "" });
            w.Widgets.Add(new ButtonWidget { Label = "Close", Callback = DynValue.Nil });
            w.Visible = true;
        }

        private static void HideSimple()
        {
            const string id = "__simple__";
            if (_windows.ContainsKey(id)) _windows[id].Visible = false;
        }

        private static void CreateWindow(string id, string title, float x, float y, float w, float h)
        {
            if (!_windows.ContainsKey(id))
            {
                _windows[id] = new LuaWindow
                {
                    Id = _idCounter++,
                    Title = title ?? id,
                    Rect = new Rect(x, y, w, h),
                    Visible = false
                };
            }
            else
            {
                _windows[id].Title = title ?? id;
                _windows[id].Rect = new Rect(x, y, w, h);
            }
        }

        private static void ShowWindow(string id)
        {
            if (_windows.TryGetValue(id, out LuaWindow w)) w.Visible = true;
        }

        private static void HideWindow(string id)
        {
            if (_windows.TryGetValue(id, out LuaWindow w)) w.Visible = false;
        }

        private static void SetTitle(string id, string title)
        {
            if (_windows.TryGetValue(id, out LuaWindow w)) w.Title = title ?? "";
        }

        private static void ClearWindow(string id)
        {
            if (_windows.TryGetValue(id, out LuaWindow w)) w.Widgets.Clear();
        }

        private static void AddLabel(string id, string text)
        {
            if (_windows.TryGetValue(id, out LuaWindow w))
                w.Widgets.Add(new LabelWidget { Text = text ?? "" });
        }

        private static void AddButton(string id, string label, DynValue callback)
        {
            if (_windows.TryGetValue(id, out LuaWindow w))
                w.Widgets.Add(new ButtonWidget { Label = label ?? "Button", Callback = callback });
        }

        private static void AddSpace(string id, float pixels)
        {
            if (_windows.TryGetValue(id, out LuaWindow w))
                w.Widgets.Add(new SpaceWidget { Pixels = pixels });
        }

        private static void AddSeparator(string id)
        {
            if (_windows.TryGetValue(id, out LuaWindow w))
                w.Widgets.Add(new SeparatorWidget());
        }

        private static void MoveWindow(string id, float x, float y)
        {
            if (_windows.TryGetValue(id, out LuaWindow w))
                w.Rect = new Rect(x, y, w.Rect.width, w.Rect.height);
        }

        public static void DrawGUI()
        {
            foreach (LuaWindow win in _windows.Values)
            {
                if (!win.Visible) continue;
                win.Rect = GUILayout.Window(win.Id, win.Rect, id => DrawWindow(win), win.Title);
            }
        }

        private static void DrawWindow(LuaWindow win)
        {
            GUILayout.BeginVertical();
            List<DynValue> pendingCallbacks = null;

            foreach (LuaWidget widget in win.Widgets)
            {
                if (widget is LabelWidget lw)
                {
                    GUILayout.Label(lw.Text);
                }
                else if (widget is ButtonWidget bw)
                {
                    if (GUILayout.Button(bw.Label))
                    {
                        if (bw.Callback == null || bw.Callback == DynValue.Nil)
                            win.Visible = false;
                        else
                        {
                            if (pendingCallbacks == null) pendingCallbacks = new List<DynValue>();
                            pendingCallbacks.Add(bw.Callback);
                        }
                    }
                }
                else if (widget is SpaceWidget sw)
                {
                    GUILayout.Space(sw.Pixels);
                }
                else if (widget is SeparatorWidget)
                {
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                }
            }

            GUILayout.EndVertical();
            GUI.DragWindow();

            if (pendingCallbacks != null && _script != null)
            {
                foreach (DynValue cb in pendingCallbacks)
                {
                    try { _script.Call(cb); }
                    catch (Exception ex) { Debug.LogError($"[LUA-NAR] GUI callback error: {ex.Message}"); }
                }
            }
        }

        public static void Show(string text) => ShowSimple(text);
        public static void Hide() => HideSimple();
    }
}
