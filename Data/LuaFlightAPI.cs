using System;
using MoonSharp.Interpreter;
using UnityEngine;
using LUNAR.Logging;
using KSP.UI.Screens;

namespace LUNAR.Data
{
    public static class LuaFlightAPI
    {
        public static void Register(Script script)
        {
            script.Globals["setSAS"]          = (Action<bool>)SetSAS;
            script.Globals["setRCS"]          = (Action<bool>)SetRCS;
            script.Globals["setGear"]         = (Action<bool>)SetGear;
            script.Globals["setBrakes"]       = (Action<bool>)SetBrakes;
            script.Globals["setLights"]       = (Action<bool>)SetLights;
            script.Globals["activateStage"]   = (Action)ActivateStage;
            script.Globals["setPitch"]        = (Action<float>)SetPitch;
            script.Globals["setYaw"]          = (Action<float>)SetYaw;
            script.Globals["setRoll"]         = (Action<float>)SetRoll;
            script.Globals["setPitchYawRoll"] = (Action<float, float, float>)SetPitchYawRoll;
            script.Globals["clearControls"]   = (Action)ClearControls;
            script.Globals["setWarp"]         = (Action<int>)SetWarp;
            script.Globals["getSASMode"]      = (Func<string>)GetSASMode;
            script.Globals["setSASMode"]      = (Action<string>)SetSASMode;
            script.Globals["getStage"]        = (Func<int>)GetCurrentStage;
            script.Globals["isEngineFlame"]   = (Func<bool>)IsEngineFlaming;
            script.Globals["getThrottle"]     = (Func<float>)GetThrottle;
            script.Globals["getMaxThrust"]    = (Func<double>)GetMaxThrust;
            script.Globals["getCurrentThrust"]= (Func<double>)GetCurrentThrust;
            script.Globals["getTWR"]          = (Func<double>)GetTWR;
            script.Globals["getMass"]         = (Func<double>)GetMass;
            script.Globals["actionGroup"]     = (Action<int, bool>)SetActionGroup;
        }

        private static Vessel V() => FlightGlobals.ActiveVessel;

        private static void SetSAS(bool on)
        {
            Vessel v = V();
            if (v == null) return;
            v.ActionGroups.SetGroup(KSPActionGroup.SAS, on);
        }

        private static void SetRCS(bool on)
        {
            Vessel v = V();
            if (v == null) return;
            v.ActionGroups.SetGroup(KSPActionGroup.RCS, on);
        }

        private static void SetGear(bool on)
        {
            Vessel v = V();
            if (v == null) return;
            v.ActionGroups.SetGroup(KSPActionGroup.Gear, on);
        }

        private static void SetBrakes(bool on)
        {
            Vessel v = V();
            if (v == null) return;
            v.ActionGroups.SetGroup(KSPActionGroup.Brakes, on);
        }

        private static void SetLights(bool on)
        {
            Vessel v = V();
            if (v == null) return;
            v.ActionGroups.SetGroup(KSPActionGroup.Light, on);
        }

        private static void ActivateStage()
        {
            StageManager.ActivateNextStage();
        }

        private static void SetPitch(float val)
        {
            FlightInputHandler.state.pitch = Mathf.Clamp(val, -1f, 1f);
        }

        private static void SetYaw(float val)
        {
            FlightInputHandler.state.yaw = Mathf.Clamp(val, -1f, 1f);
        }

        private static void SetRoll(float val)
        {
            FlightInputHandler.state.roll = Mathf.Clamp(val, -1f, 1f);
        }

        private static void SetPitchYawRoll(float p, float y, float r)
        {
            FlightInputHandler.state.pitch = Mathf.Clamp(p, -1f, 1f);
            FlightInputHandler.state.yaw   = Mathf.Clamp(y, -1f, 1f);
            FlightInputHandler.state.roll  = Mathf.Clamp(r, -1f, 1f);
        }

        private static void ClearControls()
        {
            FlightInputHandler.state.pitch = 0f;
            FlightInputHandler.state.yaw   = 0f;
            FlightInputHandler.state.roll  = 0f;
        }

        private static void SetWarp(int level)
        {
            int clamped = Mathf.Clamp(level, 0, 7);
            TimeWarp.SetRate(clamped, false);
        }

        private static string GetSASMode()
        {
            Vessel v = V();
            if (v == null) return "None";
            return v.Autopilot.Mode.ToString();
        }

        private static void SetSASMode(string mode)
        {
            Vessel v = V();
            if (v == null) return;
            if (!v.ActionGroups[KSPActionGroup.SAS]) return;
            if (Enum.TryParse(mode, true, out VesselAutopilot.AutopilotMode m))
                v.Autopilot.SetMode(m);
        }

        private static int GetCurrentStage()
        {
            Vessel v = V();
            return v != null ? v.currentStage : -1;
        }

        private static bool IsEngineFlaming()
        {
            Vessel v = V();
            if (v == null) return false;
            foreach (Part p in v.parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is ModuleEngines me && me.EngineIgnited && me.currentThrottle > 0f)
                        return true;
                }
            }
            return false;
        }

        private static float GetThrottle()
        {
            return FlightInputHandler.state.mainThrottle;
        }

        private static double GetMaxThrust()
        {
            Vessel v = V();
            if (v == null) return 0.0;
            double total = 0.0;
            foreach (Part p in v.parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is ModuleEngines me && me.EngineIgnited)
                        total += me.MaxThrustOutputVac(true);
                }
            }
            return total;
        }

        private static double GetCurrentThrust()
        {
            Vessel v = V();
            if (v == null) return 0.0;
            double total = 0.0;
            foreach (Part p in v.parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is ModuleEngines me && me.EngineIgnited)
                        total += me.finalThrust;
                }
            }
            return total;
        }

        private static double GetTWR()
        {
            Vessel v = V();
            if (v == null) return 0.0;
            double thrust = GetMaxThrust();
            double weight = v.totalMass * v.graviticAcceleration.magnitude;
            return weight > 0.0 ? thrust / weight : 0.0;
        }

        private static double GetMass()
        {
            Vessel v = V();
            return v != null ? v.totalMass : 0.0;
        }

        private static void SetActionGroup(int group, bool state)
        {
            Vessel v = V();
            if (v == null || group < 1 || group > 10) return;
            KSPActionGroup ag = (KSPActionGroup)(1 << (group + 3));
            v.ActionGroups.SetGroup(ag, state);
        }
    }
}
