using System;
using MoonSharp.Interpreter;
using UnityEngine;

namespace LUNAR.Data
{
    public static class LuaNavAPI
    {
        public static void Register(Script script)
        {
            script.Globals["getLatitude"]       = (Func<double>)GetLatitude;
            script.Globals["getLongitude"]      = (Func<double>)GetLongitude;
            script.Globals["getHeading"]        = (Func<double>)GetHeading;
            script.Globals["getPitchAngle"]     = (Func<double>)GetPitchAngle;
            script.Globals["getRollAngle"]      = (Func<double>)GetRollAngle;
            script.Globals["getOrbVelocity"]    = (Func<double>)GetOrbitalVelocity;
            script.Globals["getSurfVelocity"]   = (Func<double>)GetSurfaceVelocity;
            script.Globals["getOrbPeriod"]      = (Func<double>)GetOrbitalPeriod;
            script.Globals["getTimeToApo"]      = (Func<double>)GetTimeToApoapsis;
            script.Globals["getTimeToPe"]       = (Func<double>)GetTimeToPeriapsis;
            script.Globals["getSOI"]            = (Func<double>)GetSOIRadius;
            script.Globals["getBodyRadius"]     = (Func<double>)GetBodyRadius;
            script.Globals["getBodyGravity"]    = (Func<double>)GetBodyGravity;
            script.Globals["getBodyAtmHeight"]  = (Func<double>)GetBodyAtmosphereHeight;
            script.Globals["hasAtmosphere"]     = (Func<bool>)HasAtmosphere;
            script.Globals["getAngleToHorizon"] = (Func<double>)GetAngleToHorizon;
            script.Globals["getOrbitEcc"]       = (Func<double>)GetEccentricity;
            script.Globals["getSemiMajorAxis"]  = (Func<double>)GetSemiMajorAxis;
            script.Globals["addManeuverNode"]   = (Action<double, double, double, double>)AddManeuverNode;
            script.Globals["clearManeuverNodes"]= (Action)ClearManeuverNodes;
            script.Globals["getUniversalTime"]  = (Func<double>)GetUniversalTime;
        }

        private static Vessel V() => FlightGlobals.ActiveVessel;

        private static double GetLatitude()
        {
            Vessel v = V();
            return v != null ? v.latitude : 0.0;
        }

        private static double GetLongitude()
        {
            Vessel v = V();
            return v != null ? v.longitude : 0.0;
        }

        private static double GetHeading()
        {
            Vessel v = V();
            if (v == null) return 0.0;
            Vector3d nrm = v.mainBody.GetSurfaceNVector(v.latitude, v.longitude);
            Vector3d east = Vector3d.Cross(nrm, v.mainBody.GetSurfaceNVector(v.latitude + 0.001, v.longitude)).normalized;
            Vector3d north = Vector3d.Cross(east, nrm);
            Vector3d fwd = v.GetTransform().up;
            double heading = Math.Atan2(Vector3d.Dot(fwd, east), Vector3d.Dot(fwd, north)) * (180.0 / Math.PI);
            return (heading + 360.0) % 360.0;
        }

        private static double GetPitchAngle()
        {
            Vessel v = V();
            if (v == null) return 0.0;
            Vector3d nrm = v.mainBody.GetSurfaceNVector(v.latitude, v.longitude);
            Vector3d fwd = v.GetTransform().up;
            return 90.0 - Vector3d.Angle(fwd, nrm);
        }

        private static double GetRollAngle()
        {
            Vessel v = V();
            if (v == null) return 0.0;
            Vector3d nrm = v.mainBody.GetSurfaceNVector(v.latitude, v.longitude);
            Vector3d right = v.GetTransform().right;
            return Vector3d.Angle(right, Vector3d.Exclude(nrm, right).normalized) * Math.Sign(Vector3d.Dot(nrm, right));
        }

        private static double GetOrbitalVelocity()
        {
            Vessel v = V();
            return v != null ? v.obt_speed : 0.0;
        }

        private static double GetSurfaceVelocity()
        {
            Vessel v = V();
            return v != null ? v.srfSpeed : 0.0;
        }

        private static double GetOrbitalPeriod()
        {
            Vessel v = V();
            if (v == null || v.orbit == null) return 0.0;
            return v.orbit.period;
        }

        private static double GetTimeToApoapsis()
        {
            Vessel v = V();
            if (v == null || v.orbit == null) return 0.0;
            return v.orbit.timeToAp;
        }

        private static double GetTimeToPeriapsis()
        {
            Vessel v = V();
            if (v == null || v.orbit == null) return 0.0;
            return v.orbit.timeToPe;
        }

        private static double GetSOIRadius()
        {
            Vessel v = V();
            if (v == null || v.mainBody == null) return 0.0;
            return v.mainBody.sphereOfInfluence;
        }

        private static double GetBodyRadius()
        {
            Vessel v = V();
            if (v == null || v.mainBody == null) return 0.0;
            return v.mainBody.Radius;
        }

        private static double GetBodyGravity()
        {
            Vessel v = V();
            if (v == null || v.mainBody == null) return 0.0;
            return v.mainBody.gravParameter;
        }

        private static double GetBodyAtmosphereHeight()
        {
            Vessel v = V();
            if (v == null || v.mainBody == null) return 0.0;
            return v.mainBody.atmosphereDepth;
        }

        private static bool HasAtmosphere()
        {
            Vessel v = V();
            return v != null && v.mainBody != null && v.mainBody.atmosphere;
        }

        private static double GetAngleToHorizon()
        {
            Vessel v = V();
            if (v == null) return 0.0;
            return (v.horizontalSrfSpeed > 0.01) ? Math.Atan2(v.verticalSpeed, v.horizontalSrfSpeed) * (180.0 / Math.PI) : 0.0;
        }

        private static double GetEccentricity()
        {
            Vessel v = V();
            if (v == null || v.orbit == null) return 0.0;
            return v.orbit.eccentricity;
        }

        private static double GetSemiMajorAxis()
        {
            Vessel v = V();
            if (v == null || v.orbit == null) return 0.0;
            return v.orbit.semiMajorAxis;
        }

        private static void AddManeuverNode(double ut, double prograde, double normal, double radial)
        {
            Vessel v = V();
            if (v == null || v.orbit == null) return;
            ManeuverNode node = v.patchedConicSolver.AddManeuverNode(ut);
            node.DeltaV = new Vector3d(radial, normal, prograde);
            v.patchedConicSolver.UpdateFlightPlan();
        }

        private static void ClearManeuverNodes()
        {
            Vessel v = V();
            if (v == null || v.patchedConicSolver == null) return;
            v.patchedConicSolver.maneuverNodes.Clear();
        }

        private static double GetUniversalTime()
        {
            return Planetarium.GetUniversalTime();
        }
    }
}
