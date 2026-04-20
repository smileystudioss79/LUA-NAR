-- ============================================================
--  LUA-NAR  |  Live HUD
--  Persistent heads-up display. Refreshes every 0.5 seconds.
--  Shows vessel state, orbit, resources, and autopilot status.
-- ============================================================

local WIN = "hud"
local TICK = 0

local SIT = {
    LANDED    = "LANDED",
    SPLASHED  = "SPLASHED",
    PRELAUNCH = "PRE-LAUNCH",
    FLYING    = "FLYING",
    SUB_ORBITAL = "SUB-ORBITAL",
    ORBITING  = "ORBITING",
    ESCAPING  = "ESCAPING",
    DOCKED    = "DOCKED",
}

local function situation()
    local v = FlightGlobals and FlightGlobals.ActiveVessel
    if isLanded() then return "LANDED" end
    if isInAtmosphere() then
        if getAlt() < 100 then return "PRE-LAUNCH" end
        return "FLYING"
    end
    local apo = getOrbitApoapsis()
    local peri = getOrbitPeriapsis()
    if peri > getBodyAtmHeight() then return "ORBITING" end
    return "SUB-ORBITAL"
end

local function buildHUD()
    guiClear(WIN)

    local sit  = situation()
    local alt  = getAlt()
    local spd  = getSpeed()
    local vs   = getVertSpeed()
    local mach = getMach()
    local thr  = getThrottle() * 100
    local twr  = getTWR()
    local apo  = getOrbitApoapsis() / 1000
    local peri = getOrbitPeriapsis() / 1000
    local ecc  = getOrbitEcc()
    local inc  = getOrbitInclination()
    local fuel = getFuelPercent()
    local elec = getElectricPercent()
    local met  = getMET()
    local body = getBodyName()
    local hdg  = getHeading()
    local pitch= getPitchAngle()
    local lat  = getLatitude()
    local lon  = getLongitude()
    local ap_s = apGetState()

    guiLabel(WIN, string.format("%-12s  %s", sit, body))
    guiSeparator(WIN)
    guiLabel(WIN, string.format("Alt   %10.1f m", alt))
    guiLabel(WIN, string.format("Speed %10.1f m/s", spd))
    guiLabel(WIN, string.format("Vert  %10.1f m/s", vs))
    if mach > 0.1 then
        guiLabel(WIN, string.format("Mach  %10.2f", mach))
    end
    guiSeparator(WIN)
    guiLabel(WIN, string.format("Hdg   %10.1f °", hdg))
    guiLabel(WIN, string.format("Pitch %10.1f °", pitch))
    guiLabel(WIN, string.format("Lat   %10.4f °", lat))
    guiLabel(WIN, string.format("Lon   %10.4f °", lon))
    guiSeparator(WIN)
    guiLabel(WIN, string.format("Apo   %10.1f km", apo))
    guiLabel(WIN, string.format("Pe    %10.1f km", peri))
    guiLabel(WIN, string.format("Ecc   %10.4f", ecc))
    guiLabel(WIN, string.format("Inc   %10.2f °", inc))
    guiSeparator(WIN)
    guiLabel(WIN, string.format("Thr   %10.0f %%", thr))
    guiLabel(WIN, string.format("TWR   %10.2f", twr))
    guiSeparator(WIN)
    guiLabel(WIN, string.format("Fuel  %10.1f %%", fuel))
    guiLabel(WIN, string.format("Elec  %10.1f %%", elec))
    guiSeparator(WIN)
    guiLabel(WIN, string.format("MET   %10.1f s", met))
    if ap_s ~= "Idle" then
        guiSeparator(WIN)
        guiLabel(WIN, "AP: " .. ap_s)
        guiLabel(WIN, apGetStatus())
    end
    guiSpace(WIN, 2)
    guiButton(WIN, "✕ Close HUD", function()
        guiHide(WIN)
        hudRunning = false
    end)
end

hudRunning = true

guiCreate(WIN, "LUA-NAR HUD", 20, 60, 240, 600)
setTickRate(0.5)
buildHUD()
guiShow(WIN)

local _oldTick = onTick

function onTick()
    if _oldTick then _oldTick() end
    if hudRunning then buildHUD() end
end

print("[HUD] Live HUD active.")
logToFile("live_hud.lua initialised.")
