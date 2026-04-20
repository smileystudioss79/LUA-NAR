-- ============================================================
--  LUA-NAR  |  Orbital Autopilot
--  Provides a GUI to select a target orbit and execute an
--  automated ascent + circularisation sequence.
-- ============================================================

print("[AP] orbital_autopilot.lua loading...")

-- ── State ────────────────────────────────────────────────────
local WIN_MAIN   = "ap_main"
local WIN_STATUS = "ap_status"

local PRESETS = {
    { label = "LKO  80 km",   alt = 80000  },
    { label = "LKO  100 km",  alt = 100000 },
    { label = "LKO  120 km",  alt = 120000 },
    { label = "LKO  150 km",  alt = 150000 },
    { label = "Sat  250 km",  alt = 250000 },
    { label = "Sat  400 km",  alt = 400000 },
    { label = "Relay 600 km", alt = 600000 },
    { label = "High  1000 km",alt = 1000000},
}

local selectedIndex = 1
local targetAlt     = PRESETS[1].alt
local tickEnabled   = false

-- ── Build main control window ─────────────────────────────────
local function buildMainWindow()
    guiClear(WIN_MAIN)
    guiLabel(WIN_MAIN, "ORBITAL AUTOPILOT  —  LUA-NAR")
    guiSeparator(WIN_MAIN)
    guiSpace(WIN_MAIN, 4)

    guiLabel(WIN_MAIN, string.format("Target orbit:  %s  (%.0f km)",
        PRESETS[selectedIndex].label,
        PRESETS[selectedIndex].alt / 1000))

    guiSpace(WIN_MAIN, 2)
    guiLabel(WIN_MAIN, "── Preset selector ──")

    for i, preset in ipairs(PRESETS) do
        local idx = i
        local marker = (idx == selectedIndex) and "► " or "  "
        guiButton(WIN_MAIN, marker .. preset.label, function()
            selectedIndex = idx
            targetAlt     = PRESETS[idx].alt
            apSetTargetAlt(targetAlt)
            buildMainWindow()
        end)
    end

    guiSeparator(WIN_MAIN)
    guiSpace(WIN_MAIN, 4)

    local state = apGetState()
    if state == "Idle" or state == "Abort" then
        guiButton(WIN_MAIN, "▶  LAUNCH AUTOPILOT", function()
            apSetTargetAlt(targetAlt)
            apStartAscent(targetAlt)
            tickEnabled = true
            guiShow(WIN_STATUS)
            buildStatusWindow()
        end)

        guiButton(WIN_MAIN, "↺  Circularise only", function()
            apSetTargetAlt(targetAlt)
            apCircularise()
            tickEnabled = true
            guiShow(WIN_STATUS)
            buildStatusWindow()
        end)
    else
        guiButton(WIN_MAIN, "■  ABORT AUTOPILOT", function()
            apAbort()
            tickEnabled = false
            buildMainWindow()
            buildStatusWindow()
        end)
    end

    guiSpace(WIN_MAIN, 4)
    guiSeparator(WIN_MAIN)
    guiLabel(WIN_MAIN, string.format(
        "Alt: %.1f km  |  Apo: %.1f km",
        getAlt() / 1000,
        getOrbitApoapsis() / 1000))
    guiLabel(WIN_MAIN, string.format(
        "Speed: %.0f m/s  |  TWR: %.2f",
        getSpeed(), getTWR()))
    guiSpace(WIN_MAIN, 2)
    guiButton(WIN_MAIN, "✕ Close panel", function()
        guiHide(WIN_MAIN)
    end)
end

-- ── Build status / telemetry window ──────────────────────────
local function buildStatusWindow()
    guiClear(WIN_STATUS)
    local state  = apGetState()
    local status = apGetStatus()
    guiLabel(WIN_STATUS, "── AUTOPILOT STATUS ──")
    guiLabel(WIN_STATUS, "State:  " .. state)
    guiLabel(WIN_STATUS, status)
    guiSeparator(WIN_STATUS)
    guiLabel(WIN_STATUS, string.format("Alt      : %8.1f m",  getAlt()))
    guiLabel(WIN_STATUS, string.format("Speed    : %8.1f m/s",getSpeed()))
    guiLabel(WIN_STATUS, string.format("Vert spd : %8.1f m/s",getVertSpeed()))
    guiLabel(WIN_STATUS, string.format("Throttle : %8.0f %%",  getThrottle() * 100))
    guiLabel(WIN_STATUS, string.format("Thrust   : %8.1f kN", getCurrentThrust()))
    guiLabel(WIN_STATUS, string.format("TWR      : %8.2f",    getTWR()))
    guiLabel(WIN_STATUS, string.format("Apoapsis : %8.1f km", getOrbitApoapsis() / 1000))
    guiLabel(WIN_STATUS, string.format("Periapsis: %8.1f km", getOrbitPeriapsis() / 1000))
    guiLabel(WIN_STATUS, string.format("Ecc      : %8.4f",    getOrbitEcc()))
    guiLabel(WIN_STATUS, string.format("Inc      : %8.2f °",  getOrbitInclination()))
    guiLabel(WIN_STATUS, string.format("MET      : %8.1f s",  getMET()))
    guiSeparator(WIN_STATUS)
    guiLabel(WIN_STATUS, string.format("Fuel     : %8.1f %%", getFuelPercent()))
    guiLabel(WIN_STATUS, string.format("Electric : %8.1f %%", getElectricPercent()))
    guiSpace(WIN_STATUS, 2)
    guiButton(WIN_STATUS, "✕ Close", function()
        guiHide(WIN_STATUS)
    end)
end

-- ── onTick — called every 0.25s by LuaManager ────────────────
function onTick()
    if not tickEnabled then return end

    local state = apGetState()

    buildStatusWindow()

    if state == "Idle" or state == "Abort" then
        tickEnabled = false
        buildMainWindow()
    end
end

-- ── Initialise windows ────────────────────────────────────────
guiCreate(WIN_MAIN,   "LUA-NAR Autopilot",   20, 60, 280, 500)
guiCreate(WIN_STATUS, "AP Telemetry",        320, 60, 260, 420)

apSetTargetAlt(targetAlt)
setTickRate(0.25)
buildMainWindow()
buildStatusWindow()
guiShow(WIN_MAIN)

print(string.format("[AP] Orbital autopilot ready — %d presets loaded.", #PRESETS))
logToFile("orbital_autopilot.lua initialised.")
