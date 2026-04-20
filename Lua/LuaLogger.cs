using MoonSharp.Interpreter;
using LUNAR.Logging;

namespace LUNAR.Lua
{
    public static class LuaLogger
    {
        public static void LogScriptError(string scriptName, InterpreterException ex)
        {
            string msg = $"Script '{scriptName}' error at {ex.DecoratedMessage ?? ex.Message}";
            LuaNarLog.AppendError(msg);
        }

        public static void LogScriptError(string scriptName, System.Exception ex)
        {
            string msg = $"Script '{scriptName}' unhandled exception: {ex.Message}";
            LuaNarLog.AppendError(msg);
        }

        public static void LogScriptLoaded(string scriptName)
        {
            LuaNarLog.AppendInfo($"Loaded script: {scriptName}");
        }
    }
}
