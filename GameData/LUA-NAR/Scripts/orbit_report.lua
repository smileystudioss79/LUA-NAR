-- orbit_report.lua
-- LUA-NAR example script
-- Prints a full orbital report to the KSP log and the LUA-NAR log file.

print("orbit_report.lua loaded!")

local body    = getBodyName()
local alt     = getAlt()
local apo     = getOrbitApoapsis()
local peri    = getOrbitPeriapsis()
local inc     = getOrbitInclination()
local inAtm   = isInAtmosphere()
local landed  = isLanded()
local fuel    = getFuelPercent()
local elec    = getElectricPercent()

local status
if landed then
    status = "LANDED"
elseif inAtm then
    status = "IN ATMOSPHERE"
else
    status = "IN ORBIT / SUBORBITAL"
end

local report = string.format([[
=== LUA-NAR ORBIT REPORT ===
Body        : %s
Status      : %s
Altitude    : %.1f m
Apoapsis    : %.1f m
Periapsis   : %.1f m
Inclination : %.2f deg
Fuel        : %.1f%%
Electric    : %.1f%%
============================
]], body, status, alt, apo, peri, inc, fuel, elec)

print(report)
logToFile(report)

showGUI(string.format(
    "ORBIT REPORT\n%s | %s\nApo: %.0fm  Pe: %.0fm\nFuel: %.0f%%  Elec: %.0f%%",
    body, status, apo, peri, fuel, elec
))
