# Summit / Larp Mod Feature & Developer Map

This guide maps every page, switch, slider, and cheat mechanic in `Summit_Decompiled/Summit/` to its exact code location, allowing rapid modifications and feature additions.

---

## 1. Quick Development Workflow

### How to Request Edits in Chat
Simply tell me what you want to change:
- *"Add a 10x speed toggle to the Movement page"*
- *"Add an infinite rope toggle to the Scout page"*
- *"Change the default menu key to F2"*
- *"Add an auto-revive feature"*

I will make the code changes, compile, and deploy directly to your game!

### How to Build Manually
Whenever you make edits in `Summit_Decompiled/`, run:
* **Option A**: Double-click [`build_summit.bat`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/build_summit.bat) in the workspace folder.
* **Option B**: In PowerShell:
  ```powershell
  .\build_summit.ps1
  ```
> [!NOTE]
> If PEAK is currently running, Windows locks loaded DLLs. Close PEAK before running the build to update the active in-game DLL. Automatic timestamped backups are preserved in `backups/`.

---

## 2. Codebase Architecture

| File | Purpose | Key Responsibilities |
|---|---|---|
| [`Plugin.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Plugin.cs) | Mod Entry Point | BepInEx lifecycle, Awake, default bindings, Harmony patch registration, Game object instantiation. |
| [`Menu.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Menu.cs) | UI System | IMGUI layout engine, tab drawer methods (`DrawScout`, `DrawMovement`, `DrawItems`, etc.), search bar, responsive scaling. |
| [`Features.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Features.cs) | Core Logic | Mechanics implementation: Godmode, warp/teleport, item spawning, fly loop, multiplayer RPC triggers. |
| [`Patches.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Patches.cs) | Harmony Patches | 25 prefix/postfix hooks into PEAK's `Assembly-CSharp.dll` (fall damage, carry weight, climbing grip, weather, camera). |
| [`State.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/State.cs) | Toggle States | Public boolean flags and float multipliers representing the state of all menu switches and sliders. |
| [`Live.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Live.cs) | Transient State | Free camera angles/coordinates and non-persisted runtime tracking variables. |
| [`Theme.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Theme.cs) | UI Styling | Color definitions, button styles, custom GUI textures, and font sizing. |
| [`Picker.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Picker.cs) | Item Grid | Scrollable visual grid for browsing and selecting all items in the game. |
| [`Keys.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Keys.cs) | Keybinding Manager | Binds hotkeys to switches and actions; handles key rebinding listeners. |
| [`Lang.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Lang.cs) | Localization | Multilingual strings (English, Spanish, Chinese). |
| [`Settings.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Settings.cs) | Persistence | Saves/loads switch and slider states to `BepInEx/config/duskidle.peak.summit.cfg`. |

---

## 3. Menu Tab Locations in `Menu.cs`

| Tab | Draw Method in `Menu.cs` | Associated Logics & Patches |
|---|---|---|
| **Scout** | `DrawScout` (Line ~930) | Godmode (`State.GodMode`), infinite stamina, no status effects, infinite item uses, outfit saver. |
| **Movement** | `DrawMovement` (Line ~1050) | Walk speed (`State.WalkSpeed`), jump height (`State.JumpMult`), mid-air jump, fly (`State.Fly`), noclip, climb grip (`State.ClimbAnything`), rope length. |
| **Items** | `DrawItems` (Line ~1170) | Item grid browser (`Picker`), search filtering, direct-to-hand item spawner. |
| **Teleport** | `DrawTeleport` (Line ~1230) | Warp to look direction (`Features.WarpToAim`), warp to campfire, warp to statue, 3 saved teleport waypoints. |
| **World** | `DrawWorld` (Line ~1340) | Fog removal (`State.NoFog`), stop lava, remove hazards, clear weather, skip mountain section. |
| **Camera** | `DrawCamera` (Line ~1420) | Third-person camera distance slider, FOV slider, free camera detached flight (`Live.FreeCam`). |
| **Help** | `DrawHelp` (Line ~1490) | Teammate actions: remote revive, status clearing, teleport to scout, summon scout. |
| **Troll** | `DrawTroll` (Line ~1540) | Network RPCs: launch teammate, throw teammate, swap player locations, drop item on head. |
| **About** | `DrawCredits` (Line ~1880) | Mod title, version, author info, menu key changer (`DrawMenuKey`). |

---

## 4. How to Add New Features

### Pattern A: Adding a Simple Toggle Switch
1. In [`State.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/State.cs), declare the variable:
   ```csharp
   public static bool MyFeature = false;
   ```
2. In [`Menu.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Menu.cs) inside the target tab drawer (e.g. `DrawScout`):
   ```csharp
   Sw("my_feature_id", "My Feature Name", "Tooltip hint for footer bar", () => State.MyFeature, (val) => State.MyFeature = val);
   ```
3. Use `State.MyFeature` in [`Patches.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Patches.cs) or [`Features.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Features.cs) to alter gameplay!

### Pattern B: Adding a Slider
1. In `State.cs`:
   ```csharp
   public static float MyMultiplier = 1f;
   ```
2. In `Menu.cs`:
   ```csharp
   Sl("my_slider_id", "Multiplier Name", "Tooltip hint", 1f, 10f, () => State.MyMultiplier, (val) => State.MyMultiplier = val, "x{0:0.#}");
   ```

### Pattern C: Adding an Action Button
In `Menu.cs`:
```csharp
Bu("my_action_id", "Execute Action", "Tooltip description", () => {
    Features.MyCustomAction();
});
```

### Pattern D: Adding a New Harmony Hook
In [`Plugin.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Plugin.cs) under `Awake`:
```csharp
Patch(h, typeof(TargetGameClass), "TargetMethodName", "MyPrefixMethod", "MyPostfixMethod");
```
Then define the prefix/postfix in [`Patches.cs`](file:///c:/Users/feldring/Documents/PEAK%20Plugins%20Dev/Summit_Decompiled/Summit/Patches.cs).
