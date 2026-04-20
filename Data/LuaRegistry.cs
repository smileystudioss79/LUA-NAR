using MoonSharp.Interpreter;
using UnityEngine;

namespace LUNAR.Data
{
    public static class LuaRegistry
    {
        public static void RegisterAll(Script script)
        {
            RegisterVesselAPI(script);
            LuaUIAPI.Register(script);
            LuaFlightAPI.Register(script);
            LuaNavAPI.Register(script);
            LuaAutopilotAPI.Register(script);
        }

        private static void RegisterVesselAPI(Script script)
        {
            script.Globals["getAlt"]          = (System.Func<double>)GetAltitude;
            script.Globals["getSpeed"]        = (System.Func<double>)GetSpeed;
            script.Globals["getVertSpeed"]    = (System.Func<double>)GetVerticalSpeed;
            script.Globals["getMach"]         = (System.Func<double>)GetMach;
            script.Globals["getDynPressure"]  = (System.Func<double>)GetDynamicPressure;
            script.Globals["getGForce"]       = (System.Func<double>)GetGForce;
            script.Globals["getMET"]          = (System.Func<double>)GetMET;
            script.Globals["getBodyName"]     = (System.Func<string>)GetBodyName;
            script.Globals["setThrottle"]     = (System.Action<float>)SetThrottle;
        }

        private static Vessel ActiveVessel() => FlightGlobals.ActiveVessel;

        private static double GetAltitude()
        {
            Vessel v = ActiveVessel(); return v != null ? v.altitude : 0.0;
        }
        private static double GetSpeed()
        {
            Vessel v = ActiveVessel(); return v != null ? v.srfSpeed : 0.0;
        }
        private static double GetVerticalSpeed()
        {
            Vessel v = ActiveVessel(); return v != null ? v.verticalSpeed : 0.0;
        }
        private static double GetMach()
        {
            Vessel v = ActiveVessel(); return v != null ? v.mach : 0.0;
        }
        private static double GetDynamicPressure()
        {
            Vessel v = ActiveVessel(); return v != null ? v.dynamicPressurekPa * 1000.0 : 0.0;
        }
        private static double GetGForce()
        {
            Vessel v = ActiveVessel(); return v != null ? v.geeForce : 0.0;
        }
        private static double GetMET()
        {
            Vessel v = ActiveVessel(); return v != null ? v.missionTime : 0.0;
        }
        private static string GetBodyName()
        {
            Vessel v = ActiveVessel();
            return (v != null && v.mainBody != null) ? v.mainBody.bodyName : "Unknown";
        }
        private static void SetThrottle(float value)
        {
            FlightInputHandler.state.mainThrottle = Mathf.Clamp01(value);
        }
    }
}
