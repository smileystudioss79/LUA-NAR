-- ============================================================
--  LUA-NAR  |  API Explorer
--  Tap any button to read a live value from the API.
--  Useful for testing and learning the API.
-- ============================================================

local WIN = "api_explorer"
local OUT = "api_output"

local function show(label, val)
    guiClear(OUT)
    guiLabel(OUT, "── API Result ──")
    guiLabel(OUT, label)
    guiSeparator(OUT)
    guiLabel(OUT, tostring(val))
    guiShow(OUT)
end

local ENTRIES = {
    { "getAlt()",           function() return getAlt()           end },
    { "getSpeed()",         function() return getSpeed()         end },
    { "getVertSpeed()",     function() return getVertSpeed()      end },
    { "getMach()",          function() return getMach()           end },
    { "getDynPressure()",   function() return getDynPressure()    end },
    { "getGForce()",        function() return getGForce()         end },
    { "getMET()",           function() return getMET()            end },
    { "getBodyName()",      function() return getBodyName()       end },
    { "getHeading()",       function() return getHeading()        end },
    { "getPitchAngle()",    function() return getPitchAngle()     end },
    { "getRollAngle()",     function() return getRollAngle()      end },
    { "getLatitude()",      function() return getLatitude()       end },
    { "getLongitude()",     function() return getLongitude()      end },
    { "getOrbitApoapsis()", function() return getOrbitApoapsis()  end },
    { "getOrbitPeriapsis()",function() return getOrbitPeriapsis() end },
    { "getOrbitInclination()", function() return getOrbitInclination() end },
    { "getOrbitEcc()",      function() return getOrbitEcc()       end },
    { "getOrbVelocity()",   function() return getOrbVelocity()    end },
    { "getOrbPeriod()",     function() return getOrbPeriod()      end },
    { "getTimeToApo()",     function() return getTimeToApo()      end },
    { "getTimeToPe()",      function() return getTimeToPe()       end },
    { "getSemiMajorAxis()", function() return getSemiMajorAxis()  end },
    { "getTWR()",           function() return getTWR()            end },
    { "getMaxThrust()",     function() return getMaxThrust()      end },
    { "getCurrentThrust()", function() return getCurrentThrust()  end },
    { "getMass()",          function() return getMass()           end },
    { "getThrottle()",      function() return getThrottle()       end },
    { "getStage()",         function() return getStage()          end },
    { "getFuelPercent()",   function() return getFuelPercent()     end },
    { "getElectricPercent()",function() return getElectricPercent() end },
    { "isInAtmosphere()",   function() return isInAtmosphere()    end },
    { "isLanded()",         function() return isLanded()          end },
    { "isEngineFlame()",    function() return isEngineFlame()     end },
    { "hasAtmosphere()",    function() return hasAtmosphere()     end },
    { "getSASMode()",       function() return getSASMode()        end },
    { "getBodyRadius()",    function() return getBodyRadius()     end },
    { "getBodyAtmHeight()", function() return getBodyAtmHeight()  end },
    { "getUniversalTime()", function() return getUniversalTime()  end },
}

guiCreate(WIN, "LUA-NAR API Explorer", 580, 60, 260, 700)
guiCreate(OUT, "API Output",           860, 60, 260, 140)

guiClear(WIN)
guiLabel(WIN, "Click any function to read its live value.")
guiSeparator(WIN)

for _, entry in ipairs(ENTRIES) do
    local name = entry[1]
    local fn   = entry[2]
    guiButton(WIN, name, function()
        local ok, result = pcall(fn)
        if ok then
            show(name, result)
        else
            show(name, "ERROR: " .. tostring(result))
        end
    end)
end

guiSeparator(WIN)
guiButton(WIN, "✕ Close", function()
    guiHide(WIN)
    guiHide(OUT)
end)

guiShow(WIN)
print("[API Explorer] Ready — " .. #ENTRIES .. " functions available.")
logToFile("api_explorer.lua initialised.")
