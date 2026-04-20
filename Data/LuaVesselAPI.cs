using MoonSharp.Interpreter;
using UnityEngine;

namespace LUNAR.Data
{
    public static class LuaVesselAPI
    {
        public static void Register(Script script)
        {
            script.Globals["getOrbitApoapsis"] = (System.Func<double>)GetApoapsis;
            script.Globals["getOrbitPeriapsis"] = (System.Func<double>)GetPeriapsis;
            script.Globals["getOrbitInclination"] = (System.Func<double>)GetInclination;
            script.Globals["getFuelPercent"] = (System.Func<double>)GetFuelPercent;
            script.Globals["getElectricPercent"] = (System.Func<double>)GetElectricPercent;
            script.Globals["isInAtmosphere"] = (System.Func<bool>)IsInAtmosphere;
            script.Globals["isLanded"] = (System.Func<bool>)IsLanded;
        }

        private static Vessel V() => FlightGlobals.ActiveVessel;

        private static double GetApoapsis()
        {
            Vessel v = V();
            if (v == null || v.orbit == null) return 0.0;
            return v.orbit.ApA;
        }

        private static double GetPeriapsis()
        {
            Vessel v = V();
            if (v == null || v.orbit == null) return 0.0;
            return v.orbit.PeA;
        }

        private static double GetInclination()
        {
            Vessel v = V();
            if (v == null || v.orbit == null) return 0.0;
            return v.orbit.inclination;
        }

        private static double GetFuelPercent()
        {
            Vessel v = V();
            if (v == null) return 0.0;
            double current = 0.0, max = 0.0;
            foreach (Part p in v.parts)
            {
                foreach (PartResource r in p.Resources)
                {
                    if (r.resourceName == "LiquidFuel")
                    {
                        current += r.amount;
                        max += r.maxAmount;
                    }
                }
            }
            return max > 0.0 ? (current / max) * 100.0 : 0.0;
        }

        private static double GetElectricPercent()
        {
            Vessel v = V();
            if (v == null) return 0.0;
            double current = 0.0, max = 0.0;
            foreach (Part p in v.parts)
            {
                foreach (PartResource r in p.Resources)
                {
                    if (r.resourceName == "ElectricCharge")
                    {
                        current += r.amount;
                        max += r.maxAmount;
                    }
                }
            }
            return max > 0.0 ? (current / max) * 100.0 : 0.0;
        }

        private static bool IsInAtmosphere()
        {
            Vessel v = V();
            return v != null && v.atmDensity > 0.0;
        }

        private static bool IsLanded()
        {
            Vessel v = V();
            return v != null && (v.situation == Vessel.Situations.LANDED || v.situation == Vessel.Situations.SPLASHED);
        }
    }
}
