-- ==========================================================
-- SMART ASCENT & SAFETY MONITOR (LUA-NAR)
-- ==========================================================
local targetMach = 0.85
local lastUpdate = 0

print("[LUA-NAR] Smart Ascent Script Initialized.")

-- Main Loop Logic
local alt = getAlt()
local speed = getSpeed()
local mach = getMach()
local dynamicPressure = getDynPressure()
local met = getMET()

-- 1. AUTOMATIC THROTTLE CONTROL (TWR & Max Q Management)
-- We throttle down slightly as we approach Mach 1 to avoid high-G aerodynamic stress
if alt < 25000 then
    if mach > targetMach then
        setThrottle(0.6) -- Ease off to prevent burning up
    else
        setThrottle(1.0) -- Full power to orbit
    end
else
    setThrottle(1.0) -- Thin air, floor it!
end

-- 2. EMERGENCY ABORT / CHUTE LOGIC
-- If we are above 500m, falling fast, and not in space, save the crew!
if alt > 500 and alt < 30000 and getVertSpeed() < -20 then
    if getFuelPercent() < 1 then
        print("[LUA-NAR] EMERGENCY: No fuel and falling! Deploying Chutes.")
        stage()
        logToFile("Emergency chute deployment at MET: " .. met)
    end
end

-- 3. HUD TELEMETRY DATA
-- Formats a nice window so you don't have to look at the tiny KSP gauges
local hudText = string.format(
    "LUA-NAR MISSION CONTROL\n" ..
    "------------------------\n" ..
    "Altitude:  %d m\n" ..
    "Velocity:  %d m/s (Mach %.2f)\n" ..
    "Dyn. Pressure: %d Pa\n" ..
    "Fuel Level: %d%%\n" ..
    "Body: %s",
    alt, speed, mach, dynamicPressure, getFuelPercent(), getBodyName()
)

showGUI(hudText)

-- 4. LOGGING DATA EVERY 10 SECONDS (To avoid spamming the file)
if math.floor(met) % 10 == 0 and math.floor(met) ~= lastUpdate then
    logToFile("Status Update - Alt: " .. alt .. " | Mach: " .. mach)
    lastUpdate = math.floor(met)
end