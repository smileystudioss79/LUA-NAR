-- auto_throttle.lua
-- LUA-NAR example script
-- Demonstrates setThrottle() — cuts throttle above 10,000 m, full throttle below.
-- NOTE: This runs ONCE on load. For continuous control use a flight-loop mod or
--       call LuaManager.Instance.ExecuteFile() repeatedly from C#.

print("auto_throttle.lua loaded!")

local alt = getAlt()

if alt > 10000 then
    setThrottle(0.0)
    local msg = string.format("Alt %.0fm > 10000m — throttle CUT to 0%%", alt)
    print(msg)
    logToFile(msg)
    showGUI("AUTO THROTTLE\nAltitude exceeded 10 000 m\nThrottle set to 0%")
else
    setThrottle(1.0)
    local msg = string.format("Alt %.0fm <= 10000m — throttle FULL 100%%", alt)
    print(msg)
    logToFile(msg)
    showGUI("AUTO THROTTLE\nBelow 10 000 m\nThrottle set to 100%")
end
