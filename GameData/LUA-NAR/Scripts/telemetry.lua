-- telemetry.lua
-- LUA-NAR example script
-- Logs telemetry on load; a real polling loop would require coroutines or repeated calls.

print("telemetry.lua loaded!")

local alt   = getAlt()
local spd   = getSpeed()
local vs    = getVertSpeed()
local mach  = getMach()
local body  = getBodyName()
local met   = getMET()

local msg = string.format(
    "[Telemetry] Body: %s | MET: %.1fs | Alt: %.1fm | Spd: %.1fm/s | VS: %.1fm/s | Mach: %.2f",
    body, met, alt, spd, vs, mach
)

print(msg)
logToFile(msg)

showGUI(string.format(
    "LUA-NAR Telemetry\nBody: %s\nAlt: %.0f m\nSpeed: %.1f m/s\nMach: %.2f",
    body, alt, spd, mach
))
