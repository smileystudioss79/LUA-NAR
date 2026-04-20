# LUA-NAR — Lua Scripting for Kerbal Space Program

**Version:** 1.0.1  
**Engine:** MoonSharp 2.x (Lua 5.2 compatible)  
**Requires:** KSP 1.8+, .NET Framework 4.7.2

---

## What is LUA-NAR?

LUA-NAR lets you write Lua scripts that control and read data from your KSP vessel at runtime.  
Drop a `.lua` file into `GameData/LUA-NAR/Scripts/` and it runs automatically when you enter the flight scene.  
No restarting KSP. No recompiling. Just Lua.

---

## Project Structure

```
LUA-NAR/
├── LUA-NAR.csproj
├── README.md
├── Management/
│   ├── LuaManager.cs          # Core MonoBehaviour, script loader
│   └── LuaFileWatcher.cs      # Hot-reload watcher
├── Data/
│   ├── LuaRegistry.cs         # Core API: getAlt, getSpeed, setThrottle, etc.
│   ├── LuaVesselAPI.cs        # Extended API: orbit, fuel, resources
│   └── LuaUIAPI.cs            # IMGUI overlay: showGUI / hideGUI
├── Lua/
│   ├── CoreFunctions.cs       # print() -> Debug.Log, logToFile()
│   └── LuaLogger.cs           # Error reporting helpers
├── Logging/
│   └── LuaNarLog.cs           # Writes Lua-Nar.log and Lua-Nar.lua on launch
├── Startup/
│   └── LuaNarStartup.cs       # KSPAddon(MainMenu) — initialises logging
├── Libs/                      # Place your DLLs here (not included)
│   ├── Assembly-CSharp.dll
│   ├── UnityEngine.dll
│   ├── UnityEngine.CoreModule.dll
│   ├── UnityEngine.IMGUIModule.dll
│   └── MoonSharp.Interpreter.dll
└── GameData/
    └── LUA-NAR/
        └── Scripts/
            ├── hello_world.lua     # Displays "Hello World from Lua!" GUI
            ├── telemetry.lua       # Logs flight telemetry
            ├── auto_throttle.lua   # Sets throttle based on altitude
            └── orbit_report.lua    # Prints full orbital parameters
```

---

## Installation

### Step 1 — Build the DLL

1. Copy the following DLLs from your KSP install into `Libs/`:
   - `KSP_Data/Managed/Assembly-CSharp.dll`
   - `KSP_Data/Managed/UnityEngine.dll`
   - `KSP_Data/Managed/UnityEngine.CoreModule.dll`
   - `KSP_Data/Managed/UnityEngine.IMGUIModule.dll`

2. Download MoonSharp from https://www.moonsharp.org/ and copy `MoonSharp.Interpreter.dll` into `Libs/`.

3. Build with the dotnet CLI:

```bash
dotnet build LUA-NAR.csproj -c Release
```

The output will be at `bin/Release/LUA-NAR.dll`.

### Step 2 — Deploy to KSP

Copy the following into your KSP `GameData/LUA-NAR/` folder:

```
GameData/
└── LUA-NAR/
    ├── Plugins/
    │   ├── LUA-NAR.dll
    │   └── MoonSharp.Interpreter.dll   ← must be alongside LUA-NAR.dll
    └── Scripts/
        ├── hello_world.lua
        ├── telemetry.lua
        ├── auto_throttle.lua
        └── orbit_report.lua
```

> **Important:** `MoonSharp.Interpreter.dll` must be in `GameData/LUA-NAR/Plugins/`  
> alongside your built `LUA-NAR.dll`. KSP will not find it otherwise.

### Step 3 — Launch KSP

On the Main Menu, LUA-NAR will write two files into `GameData/LUA-NAR/`:

- **`Lua-Nar.log`** — Session log with TOS, Privacy Policy, and runtime messages
- **`Lua-Nar.lua`** — Auto-generated API reference stub (only written once)

When you enter a Flight scene, all `.lua` files in `GameData/LUA-NAR/Scripts/` are loaded automatically.

---

## Lua API Reference

### Vessel Data

| Function | Returns | Description |
|---|---|---|
| `getAlt()` | `number` | Altitude above sea level (metres) |
| `getSpeed()` | `number` | Surface speed (m/s) |
| `getVertSpeed()` | `number` | Vertical speed (m/s) |
| `getMach()` | `number` | Current Mach number |
| `getDynPressure()` | `number` | Dynamic pressure in Pascals |
| `getGForce()` | `number` | Current G-force |
| `getMET()` | `number` | Mission elapsed time (seconds) |
| `getBodyName()` | `string` | Name of current celestial body |
| `getOrbitApoapsis()` | `number` | Apoapsis altitude (metres) |
| `getOrbitPeriapsis()` | `number` | Periapsis altitude (metres) |
| `getOrbitInclination()` | `number` | Orbital inclination (degrees) |
| `getFuelPercent()` | `number` | LiquidFuel remaining (0–100) |
| `getElectricPercent()` | `number` | ElectricCharge remaining (0–100) |
| `isInAtmosphere()` | `boolean` | True if in atmosphere |
| `isLanded()` | `boolean` | True if landed or splashed |

### Control

| Function | Description |
|---|---|
| `setThrottle(val)` | Set main throttle. `val` is clamped to `0.0–1.0` |

### UI

| Function | Description |
|---|---|
| `showGUI(text)` | Show a draggable HUD window with the given text |
| `hideGUI()` | Hide the HUD window |

### Logging

| Function | Description |
|---|---|
| `print(text)` | Write to KSP's `output_log.txt` / `Player.log` |
| `logToFile(text)` | Append a timestamped line to `Lua-Nar.log` |

---

## Writing Your Own Scripts

Create a `.lua` file in `GameData/LUA-NAR/Scripts/`. It will be loaded automatically next time you enter a Flight scene.

**Minimum working example:**

```lua
-- my_script.lua
print("My script loaded!")
showGUI("Hello from my script!\nAlt: " .. math.floor(getAlt()) .. " m")
```

**Error handling:** All scripts run inside a C# `try-catch`. If your Lua has a syntax error or runtime error, KSP will not crash. The error is written to `Lua-Nar.log` and `output_log.txt`.

**Hot reload:** The file watcher detects when you save a `.lua` file while KSP is running. The changed script is re-executed automatically without restarting KSP.

---

## Building from CLI (Quick Reference)

```bash
# Release build
dotnet build LUA-NAR.csproj -c Release

# Debug build (includes .pdb)
dotnet build LUA-NAR.csproj -c Debug

# Clean
dotnet clean LUA-NAR.csproj
```

---

## License & Legal

PROVIDED BY LUA-NAR — ALL RIGHTS RESERVED.  
See `Lua-Nar.log` for full Terms of Service and Privacy Policy.

LUA-NAR is not affiliated with Intercept Games or Take-Two Interactive.  
Kerbal Space Program is a trademark of Take-Two Interactive Software.
