-- ============================================================
--  LUA-NAR  |  Stage Monitor
--  Tracks current stage, fuel levels, and warns on low fuel.
--  Optionally auto-stages when engines flame out.
-- ============================================================

local WIN = "stage_mon"
local autoStage = false
local lastStage = -1
local fuelWarnThreshold = 10.0
local warned = false

local function buildPanel()
    guiClear(WIN)
    guiLabel(WIN, "STAGE MONITOR  —  LUA-NAR")
    guiSeparator(WIN)
    guiLabel(WIN, "Stage:    " .. tostring(getStage()))
    guiLabel(WIN, string.format("Fuel:     %.1f %%", getFuelPercent()))
    guiLabel(WIN, string.format("Electric: %.1f %%", getElectricPercent()))
    guiLabel(WIN, string.format("Mass:     %.3f t",  getMass()))
    guiLabel(WIN, string.format("TWR:      %.2f",    getTWR()))
    guiLabel(WIN, string.format("Thrust:   %.1f kN", getCurrentThrust()))
    guiSeparator(WIN)
    guiLabel(WIN, "Auto-Stage: " .. (autoStage and "ON" or "OFF"))
    guiButton(WIN, "Toggle Auto-Stage", function()
        autoStage = not autoStage
        buildPanel()
    end)
    guiSeparator(WIN)
    guiLabel(WIN, string.format("Fuel warn at: %.0f%%", fuelWarnThreshold))
    guiButton(WIN, "Warn at 5%",  function() fuelWarnThreshold = 5;  warned = false end)
    guiButton(WIN, "Warn at 10%", function() fuelWarnThreshold = 10; warned = false end)
    guiButton(WIN, "Warn at 20%", function() fuelWarnThreshold = 20; warned = false end)
    guiSeparator(WIN)
    guiButton(WIN, "✕ Close", function() guiHide(WIN) end)
end

local _prevTick = onTick

function onTick()
    if _prevTick then _prevTick() end

    local fuel  = getFuelPercent()
    local stage = getStage()

    if autoStage and not isEngineFlame() and stage ~= lastStage then
        activateStage()
        logToFile(string.format("Auto-staged at stage %d — fuel %.1f%%", stage, fuel))
        lastStage = getStage()
    end

    if fuel < fuelWarnThreshold and not warned then
        warned = true
        showGUI(string.format(
            "⚠ LUA-NAR FUEL WARNING\nStage %d fuel below %.0f%%\nCurrent: %.1f%%",
            stage, fuelWarnThreshold, fuel))
        logToFile(string.format("FUEL WARN: Stage %d at %.1f%%", stage, fuel))
    end

    buildPanel()
end

guiCreate(WIN, "Stage Monitor", 580, 60, 230, 420)
buildPanel()
guiShow(WIN)
setTickRate(0.5)

print("[StageMonitor] Active.")
logToFile("stage_monitor.lua initialised.")
