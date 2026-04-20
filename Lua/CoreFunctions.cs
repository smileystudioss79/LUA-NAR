using MoonSharp.Interpreter;
using UnityEngine;
using LUNAR.Logging;

namespace LUNAR.Lua
{
    public static class CoreFunctions
    {
        public static void Register(Script script)
        {
            script.Globals["print"] = (System.Action<DynValue>)LuaPrint;
            script.Globals["logToFile"] = (System.Action<DynValue>)LuaLogToFile;
        }

        private static void LuaPrint(DynValue val)
        {
            string msg = val.CastToString() ?? "(nil)";
            Debug.Log($"[LUA-NAR][Lua] {msg}");
        }

        private static void LuaLogToFile(DynValue val)
        {
            string msg = val.CastToString() ?? "(nil)";
            LuaNarLog.Append($"[LUA]   {msg}");
        }
    }
}
