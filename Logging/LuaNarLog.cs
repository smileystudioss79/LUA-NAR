using System;
using System.IO;
using UnityEngine;

namespace LUNAR.Logging
{
    public static class LuaNarLog
    {
        public static readonly string Version = "1.0.1";
        public static readonly string BuildDate = "2025";

        private static string _logPath;
        private static string _luaPath;

        public static void Initialize(string gameDataRoot)
        {
            string dir = Path.Combine(gameDataRoot, "LUA-NAR");
            Directory.CreateDirectory(dir);

            _logPath = Path.Combine(dir, "Lua-Nar.log");
            _luaPath = Path.Combine(dir, "Lua-Nar.lua");

            WriteLogHeader();
            WriteLuaStub();
        }

        private static void WriteLogHeader()
        {
            using (StreamWriter w = new StreamWriter(_logPath, false))
            {
                w.WriteLine("================================================================================");
                w.WriteLine("                          LUA-NAR MOD FOR KSP");
                w.WriteLine("                   PROVIDED BY LUA-NAR — ALL RIGHTS RESERVED");
                w.WriteLine("================================================================================");
                w.WriteLine();
                w.WriteLine($"  Version   : {Version}");
                w.WriteLine($"  Build Year: {BuildDate}");
                w.WriteLine($"  Launched  : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                w.WriteLine($"  Platform  : {Application.platform}");
                w.WriteLine($"  Unity     : {Application.unityVersion}");
                w.WriteLine($"  KSP       : {Application.version}");
                w.WriteLine();
                w.WriteLine("--------------------------------------------------------------------------------");
                w.WriteLine("  TERMS OF SERVICE");
                w.WriteLine("--------------------------------------------------------------------------------");
                w.WriteLine("  By using this mod you agree to the following terms:");
                w.WriteLine("  1. This software is provided \"as-is\" without warranty of any kind.");
                w.WriteLine("  2. The author is not liable for any damage to game saves, files, or data.");
                w.WriteLine("  3. Redistribution requires explicit written permission from the author.");
                w.WriteLine("  4. Modification of LUA-NAR source is permitted for personal use only.");
                w.WriteLine("  5. Commercial use is strictly prohibited without a signed licence.");
                w.WriteLine();
                w.WriteLine("--------------------------------------------------------------------------------");
                w.WriteLine("  PRIVACY POLICY");
                w.WriteLine("--------------------------------------------------------------------------------");
                w.WriteLine("  LUA-NAR does NOT collect, transmit, or store any personal data.");
                w.WriteLine("  All Lua scripts execute locally on your machine.");
                w.WriteLine("  No telemetry, analytics, or network requests are made by this mod.");
                w.WriteLine("  Log files written by LUA-NAR remain on your local filesystem only.");
                w.WriteLine();
                w.WriteLine("--------------------------------------------------------------------------------");
                w.WriteLine("  DISCLAIMER");
                w.WriteLine("--------------------------------------------------------------------------------");
                w.WriteLine("  LUA-NAR is a third-party mod and is not affiliated with, endorsed by,");
                w.WriteLine("  or supported by Intercept Games or Take-Two Interactive.");
                w.WriteLine("  Kerbal Space Program is a trademark of Take-Two Interactive Software.");
                w.WriteLine();
                w.WriteLine("================================================================================");
                w.WriteLine("  SESSION LOG");
                w.WriteLine("================================================================================");
                w.WriteLine();
            }
        }

        public static void Append(string message)
        {
            try
            {
                if (_logPath == null) return;
                using (StreamWriter w = new StreamWriter(_logPath, true))
                {
                    w.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
                }
            }
            catch { }
        }

        public static void AppendError(string message)
        {
            Append($"[ERROR] {message}");
            Debug.LogError($"[LUA-NAR] {message}");
        }

        public static void AppendInfo(string message)
        {
            Append($"[INFO]  {message}");
            Debug.Log($"[LUA-NAR] {message}");
        }

        private static void WriteLuaStub()
        {
            if (File.Exists(_luaPath)) return;

            using (StreamWriter w = new StreamWriter(_luaPath, false))
            {
                w.WriteLine("--[[");
                w.WriteLine("    Lua-Nar.lua — Auto-generated by LUA-NAR on first launch");
                w.WriteLine("    =========================================================");
                w.WriteLine($"    Version   : {Version}");
                w.WriteLine($"    Generated : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                w.WriteLine();
                w.WriteLine("    This file was created automatically by the LUA-NAR mod.");
                w.WriteLine("    It is a stub that demonstrates the LUA-NAR Lua API.");
                w.WriteLine("    Place your own .lua scripts in GameData/LUA-NAR/Scripts/");
                w.WriteLine("    and they will be loaded automatically on flight scene entry.");
                w.WriteLine();
                w.WriteLine("    PROVIDED BY LUA-NAR — ALL RIGHTS RESERVED");
                w.WriteLine("    For TOS and Privacy Policy, see Lua-Nar.log");
                w.WriteLine("--]]");
                w.WriteLine();
                w.WriteLine("-- LUA-NAR API Reference Stub");
                w.WriteLine("-- This file will NOT be auto-executed. It is documentation only.");
                w.WriteLine();
                w.WriteLine("-- Available API functions:");
                w.WriteLine("--   getAlt()           -> number   altitude above sea level in metres");
                w.WriteLine("--   getSpeed()         -> number   surface speed in m/s");
                w.WriteLine("--   getVertSpeed()     -> number   vertical speed in m/s");
                w.WriteLine("--   getMach()          -> number   current mach number");
                w.WriteLine("--   getDynPressure()   -> number   dynamic pressure (q) in Pa");
                w.WriteLine("--   getGForce()        -> number   current G-force");
                w.WriteLine("--   getMET()           -> number   mission elapsed time in seconds");
                w.WriteLine("--   getBodyName()      -> string   name of current body");
                w.WriteLine("--   setThrottle(val)   -> nil      set throttle 0.0 to 1.0");
                w.WriteLine("--   showGUI(text)      -> nil      display text on HUD");
                w.WriteLine("--   hideGUI()          -> nil      hide HUD text");
                w.WriteLine("--   print(text)        -> nil      write to KSP log");
                w.WriteLine("--   logToFile(text)    -> nil      write to Lua-Nar.log");
                w.WriteLine();
                w.WriteLine("print(\"LUA-NAR stub loaded — version " + Version + "\")");
            }
        }
    }
}
