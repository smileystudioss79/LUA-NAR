using System;
using System.Collections;
using MoonSharp.Interpreter;
using UnityEngine;
using LUNAR.Logging;
using KSP.UI.Screens;

namespace LUNAR.Data
{
    public enum AutopilotState
    {
        Idle,
        Ascent,
        GravityTurn,
        CoastToApo,
        Circularise,
        HoldAltitude,
        Abort
    }

    public class LuaAutopilotAPI : MonoBehaviour
    {
        public static LuaAutopilotAPI Instance { get; private set; }

        public AutopilotState State { get; private set; } = AutopilotState.Idle;
        public double TargetAltitude { get; private set; } = 80000.0;
        public double TargetInclination { get; private set; } = 0.0;
        public string StatusMessage { get; private set; } = "Idle";

        private UnityEngine.Coroutine _routine;
        private bool _abortRequested = false;

        public static void Register(Script script)
        {
            script.Globals["apStartAscent"]     = (Action<double>)StartAscent;
            script.Globals["apAbort"]           = (Action)Abort;
            script.Globals["apGetState"]        = (Func<string>)GetState;
            script.Globals["apGetStatus"]       = (Func<string>)GetStatus;
            script.Globals["apIsRunning"]       = (Func<bool>)IsRunning;
            script.Globals["apCircularise"]     = (Action)Circularise;
            script.Globals["apSetTargetAlt"]    = (Action<double>)SetTargetAlt;
            script.Globals["apGetTargetAlt"]    = (Func<double>)GetTargetAlt;
        }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private static LuaAutopilotAPI AP()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("LUA-NAR_Autopilot");
                Instance = go.AddComponent<LuaAutopilotAPI>();
            }
            return Instance;
        }

        private static void StartAscent(double targetAltMetres)
        {
            LuaAutopilotAPI ap = AP();
            ap.TargetAltitude = targetAltMetres;
            ap._abortRequested = false;

            if (ap._routine != null) ap.StopCoroutine(ap._routine);
            ap._routine = ap.StartCoroutine(ap.AscentSequence());
            LuaNarLog.AppendInfo($"Autopilot: Ascent started — target {targetAltMetres / 1000.0:F0} km");
        }

        private static void Abort()
        {
            LuaAutopilotAPI ap = AP();
            ap._abortRequested = true;
            if (ap._routine != null) ap.StopCoroutine(ap._routine);
            ap.State = AutopilotState.Abort;
            ap.StatusMessage = "ABORTED by user";
            FlightInputHandler.state.mainThrottle = 0f;
            LuaNarLog.AppendInfo("Autopilot: ABORTED");
        }

        private static void Circularise()
        {
            LuaAutopilotAPI ap = AP();
            ap._abortRequested = false;
            if (ap._routine != null) ap.StopCoroutine(ap._routine);
            ap._routine = ap.StartCoroutine(ap.CirculariseBurn());
        }

        private static void SetTargetAlt(double alt) { AP().TargetAltitude = alt; }
        private static double GetTargetAlt() => AP().TargetAltitude;
        private static string GetState() => AP().State.ToString();
        private static string GetStatus() => AP().StatusMessage;
        private static bool IsRunning() => AP().State != AutopilotState.Idle && AP().State != AutopilotState.Abort;

        private IEnumerator AscentSequence()
        {
            Vessel v = FlightGlobals.ActiveVessel;
            if (v == null) { StatusMessage = "No active vessel"; yield break; }

            State = AutopilotState.Ascent;
            StatusMessage = "Pre-launch checks...";

            v.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
            yield return new WaitForSeconds(0.5f);

            if (_abortRequested) yield break;

            StatusMessage = "Ignition — full throttle";
            LuaNarLog.AppendInfo("Autopilot: Ignition");
            FlightInputHandler.state.mainThrottle = 1f;
            StageManager.ActivateNextStage();

            yield return new WaitForSeconds(1f);

            StatusMessage = "Vertical ascent...";
            while (v.altitude < 500.0)
            {
                if (_abortRequested) { FlightInputHandler.state.mainThrottle = 0f; yield break; }
                StatusMessage = $"Vertical — Alt {v.altitude:F0} m";
                yield return new WaitForSeconds(0.1f);
            }

            State = AutopilotState.GravityTurn;
            StatusMessage = "Gravity turn...";
            LuaNarLog.AppendInfo("Autopilot: Gravity turn");

            if (v.Autopilot != null)
                v.Autopilot.SetMode(VesselAutopilot.AutopilotMode.Prograde);

            while (v.orbit.ApA < TargetAltitude)
            {
                if (_abortRequested) { FlightInputHandler.state.mainThrottle = 0f; yield break; }

                double apo = v.orbit.ApA;
                double ratio = apo / TargetAltitude;

                float throttle = ratio < 0.95f ? 1f : Mathf.Lerp(0.1f, 1f, (float)((1.0 - ratio) / 0.05));
                FlightInputHandler.state.mainThrottle = throttle;
                StatusMessage = $"Gravity turn — Apo {apo / 1000.0:F1} / {TargetAltitude / 1000.0:F0} km  Throttle {throttle * 100:F0}%";
                yield return new WaitForSeconds(0.2f);
            }

            FlightInputHandler.state.mainThrottle = 0f;
            State = AutopilotState.CoastToApo;
            StatusMessage = "Coasting to apoapsis...";
            LuaNarLog.AppendInfo("Autopilot: Coasting");

            while (v.orbit.timeToAp > 30.0)
            {
                if (_abortRequested) yield break;
                double eta = v.orbit.timeToAp;
                StatusMessage = $"Coast — ETA Apo {eta:F0}s  Alt {v.altitude / 1000.0:F1} km";

                if (eta > 120.0 && TimeWarp.CurrentRate < 4f)
                    TimeWarp.SetRate(3, false);
                else if (eta <= 120.0)
                    TimeWarp.SetRate(0, false);

                yield return new WaitForSeconds(0.5f);
            }

            TimeWarp.SetRate(0, false);
            yield return new WaitForSeconds(1f);

            _routine = StartCoroutine(CirculariseBurn());
        }

        private IEnumerator CirculariseBurn()
        {
            Vessel v = FlightGlobals.ActiveVessel;
            if (v == null) yield break;

            State = AutopilotState.Circularise;
            StatusMessage = "Circularise burn...";
            LuaNarLog.AppendInfo("Autopilot: Circularise burn");

            if (v.Autopilot != null)
                v.Autopilot.SetMode(VesselAutopilot.AutopilotMode.Prograde);

            v.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
            yield return new WaitForSeconds(1f);

            FlightInputHandler.state.mainThrottle = 1f;

            while (true)
            {
                if (_abortRequested) { FlightInputHandler.state.mainThrottle = 0f; yield break; }

                double apo = v.orbit.ApA;
                double peri = v.orbit.PeA;
                double diff = Math.Abs(apo - peri);
                double target = TargetAltitude;

                StatusMessage = $"Circularise — Pe {peri / 1000.0:F1} km  Apo {apo / 1000.0:F1} km";

                if (diff < 2000.0 && peri > target * 0.95)
                {
                    FlightInputHandler.state.mainThrottle = 0f;
                    State = AutopilotState.Idle;
                    StatusMessage = $"Circular orbit achieved!\nApo {apo / 1000.0:F1} km  Pe {peri / 1000.0:F1} km";
                    LuaNarLog.AppendInfo($"Autopilot: Orbit achieved — Apo {apo / 1000.0:F1} km Pe {peri / 1000.0:F1} km");
                    yield break;
                }

                float throttle = diff > 50000.0 ? 1f : Mathf.Lerp(0.05f, 1f, (float)(diff / 50000.0));
                FlightInputHandler.state.mainThrottle = throttle;

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
