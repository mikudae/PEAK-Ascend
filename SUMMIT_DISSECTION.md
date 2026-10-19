# Technical Reverse-Engineering Deep Dive: Summit Mod (v1.1.1)

**Document Classification**: Forensic Binary Analysis & Architecture Specification  
**Target Binary**: `Summit.dll` (v1.1.1)  
**Target Game**: PEAK (v2.6.b-OFME)  
**Modding Framework**: BepInEx 5.4.23.5 x64 / HarmonyX 2.9.0.0  
**Author of Mod**: DuskIdle (`https://discord.gg/Y6PWGsPADV`)  
**Analysis Date**: October 18, 2026  
**Analyst**: Worker 4 (Reverse Engineering Analyst & Technical Author)  

---

## Table of Contents

1. [Executive Summary & Binary Metadata](#1-executive-summary--binary-metadata)
2. [Architecture & Execution Lifecycle](#2-architecture--execution-lifecycle)
3. [IMGUI & Visual Subsystem Architecture](#3-imgui--visual-subsystem-architecture)
4. [Full Harmony Hook Inventory & Analysis](#4-full-harmony-hook-inventory--analysis)
5. [Deep Cheat Implementation Mechanics](#5-deep-cheat-implementation-mechanics)
6. [Multiplayer & Networking (Photon PUN Analysis)](#6-multiplayer--networking-photon-pun-analysis)
7. [Steam Integration & Cosmetics System](#7-steam-integration--cosmetics-system)
8. [Internal Self-Test Suite & Easter Egg](#8-internal-self-test-suite--easter-egg)
9. [Architectural Assessment & Developer Takeaways](#9-architectural-assessment--developer-takeaways)

---

## 1. Executive Summary & Binary Metadata

### 1.1 Overview & Purpose
`Summit.dll` (v1.1.1) is an advanced, production-grade utility and trainer plugin engineered for the co-operative mountain climbing game *PEAK*. Operating as a client-side BepInEx 5 plugin targeting `.NET Standard 2.1`, Summit hooks into the game's core assemblies (`Assembly-CSharp.dll`, `UnityEngine`, and `PhotonUnityNetworking.dll`) via HarmonyX to deliver real-time character manipulation, movement physics modifications, world and hazard suppressions, ESP overlays, procedural item spawning, Steam achievement management, cosmetic unlocks, and networked multiplayer interactions.

Beyond standard trainer capabilities, Summit incorporates an enterprise-grade embedded test harness (`SelfTest.cs` and `KitTest.cs`) consisting of 78 automated feature verifications, Win32 input simulation, and a trilingual localization system (English, Spanish, Simplified Chinese).

### 1.2 Binary File Specification
| Attribute | Static Analysis Value | Notes / Verification |
|---|---|---|
| **File Name** | `Summit.dll` | Shipped in NexusMods package ID 237 |
| **File Size** | `599,552` bytes (585.5 KB) | Exact byte length verified on disk |
| **Last Modified (UTC)** | `2026-10-13 15:02:16 UTC` | NexusMods upload timestamp |
| **Runtime Target** | `.NET Standard 2.1` | MSIL / AnyCPU |
| **Decompiled Source LOC** | `13,361` lines across 17 files | Extracted via `ilspycmd` 8.2.0 |
| **Plugin GUID** | `duskidle.peak.summit` | Declared in `Plugin.cs` |
| **Plugin Name** | `Summit` | Declared in `Plugin.cs` |
| **Plugin Version** | `1.1.1` | Constant in `Plugin.Version` and `Kit.Version` |
| **Default Activation Key** | `KeyCode.F1` (Value `282`) | Rebindable via config and About page |
| **Mod Author** | `DuskIdle` | Verified in `Summit_README.txt` and `Kit.Author` |
| **Author Contact** | `https://discord.gg/Y6PWGsPADV` | Embedded in `Kit.Discord` and UI button |

### 1.3 Assembly Dependency Graph
Inspection of `Summit.dll` metadata via Reflection (`GetReferencedAssemblies()`) reveals 15 external dependency assemblies. The plugin contains zero native code dependencies other than standard Win32 API P/Invokes (`user32.dll` and `winmm.dll`) used strictly for audio playback and UI test automation.

```
Summit.dll (v1.1.1)
├── netstandard (v2.1.0.0, PKT: cc7b13ffcd2ddd51)
├── BepInEx (v5.4.23.5)
│   ├── BepInEx.Configuration
│   └── BepInEx.Logging
├── 0Harmony (v2.9.0.0, HarmonyX)
├── Assembly-CSharp (PEAK game logic, classes: Character, Item, Rope, Campfire, etc.)
├── UnityEngine.CoreModule (GameObject, Transform, Vector3, Time, Mathf, Screen, Resources)
├── UnityEngine.IMGUIModule (GUI, GUILayout, GUIStyle, Event, Matrix4x4, Rect)
├── UnityEngine.TextRenderingModule (Font, TextAnchor, FontStyle)
├── UnityEngine.PhysicsModule (Physics, RaycastHit, LayerMask, SphereCast)
├── UnityEngine.ImageConversionModule (ImageConversion.LoadImage)
├── UnityEngine.UIModule (Canvas, RectTransform)
├── UnityEngine.ScreenCaptureModule (ScreenCapture.CaptureScreenshot)
├── Unity.InputSystem (v1.19.0.0, Keyboard, Mouse, Controls)
├── PhotonUnityNetworking (PhotonNetwork, PhotonView, MonoBehaviourPun, RpcTarget)
├── PhotonRealtime (Player, Room)
└── Zorro.Core.Runtime (Singleton, SingletonAsset, DatabaseAsset)
```

---

## 2. Architecture & Execution Lifecycle

### 2.1 Entry Point & BepInEx Lifecycle
Summit conforms strictly to the BepInEx 5 plugin paradigm. The primary class `Summit.Plugin` inherits from `BaseUnityPlugin` and is decorated with the `[BepInPlugin]` attribute:

```csharp
namespace Summit;

[BepInPlugin("duskidle.peak.summit", "Summit", "1.1.1")]
public class Plugin : BaseUnityPlugin
{
    public const string Version = "1.1.1";
    internal static ManualLogSource L;
    internal static ConfigEntry<string>[] Outfits;
    internal static List<string> FailedPatches => Kit.FailedPatches;
    ...
}
```

The plugin lifecycle follows a deterministic 4-stage pipeline:

```
[Game Boot]
     │
     ▼
Plugin.Awake()
     │
     ├── 1. Logger & Config Binding (Kit.BlockAchievements, Outfits[4])
     ├── 2. Direct Action Shortcut Registration (Keys.Job for Warp, Heal, FreeCam, Spots)
     ├── 3. Theme & Framework Bootstrap (Theme.Paint, Kit.Start, Language detection)
     ├── 4. Harmony Hook Injection (25 patch bindings across Assembly-CSharp)
     │
     ▼
GameObject Initialization
     │
     ├── Instantiate hidden GameObject ("Summit", hideFlags = 61: HideAndDontSave)
     ├── DontDestroyOnLoad(go)
     └── Attach Summit.Menu component
          │
          ▼
Menu Runtime Loop (Every Frame)
     │
     ├── Menu.Update()       --> Settings.Tick(), Lang.Start(), Menu.Build(), Menu.Tick()
     │                                                                 └── Features.Upkeep()
     ├── Menu.LateUpdate()   --> SelfTest.LateTick()
     └── Menu.OnGUI()        --> Event routing, Menu.Overlay() [ESP], Menu.Draw() [IMGUI]
```

### 2.2 Awake() Detailed Initialization
In `Plugin.Awake()`, Summit initializes its internal subsystems in precise order:
1. **Logging Initialization**: Assigns `Plugin.L = ((BaseUnityPlugin)this).Logger` for centralized diagnostics.
2. **Configuration Binding**:
   - `Kit.BlockAchievements`: Bound to `"General"`, `"BlockAchievements"`, default `true`.
   - `Outfits`: Array of 4 `ConfigEntry<string>` entries for saved customization presets.
3. **Global Action Shortcut Registration**:
   Registers six direct-action delegate jobs into `Keys`:
   - `"WarpAim"`: Teleports the local scout to the camera look-target via `Features.WarpToAim()`.
   - `"WarpFire"`: Warps to `MapHandler.CurrentCampfire` via `Features.WarpToCampfire()`.
   - `"Heal"`: Clears all afflictions and restores stamina via `Features.Heal()`.
   - `"FreeCam"`: Toggles detached camera flight via `Features.SetFreeCam(!Live.FreeCam)`.
   - `"SaveSpot"`: Caches current position into slot 0 via `Features.SaveSpot(0)`.
   - `"GoSpot"`: Recalls cached position from slot 0 via `Features.GoSpot(0)`.
4. **Theme Coloring**:
   Invokes `Theme.Paint(Theme.Rgb(47, 158, 143), Theme.Rgb(69, 191, 173))` to establish the mod's signature teal/cyan palette (`#2F9E8F` and `#45BFAD`).
5. **Kit Framework Bootstrap**:
   Executes `Kit.Start(...)`, generating a deterministic window ID (`0x4B495400 ^ title.GetHashCode()`), binding the `MenuKey` (default `KeyCode.F1`), binding `PinnedToHome`, and loading persisted configuration entries.
6. **Game State & Locale Detection**:
   - Predicate `Menu.Ready = () => Features.InGame;` checks whether `Character.localCharacter` and `CharacterMovement` are initialized.
   - Predicate `Lang.OfTheGame = () => ((object)(Language)(ref LocalizedText.CURRENT_LANGUAGE)).ToString();` ties Summit's language detection directly to the PEAK game locale.
7. **Harmony Patch Dispatch**:
   Instantiates `Harmony h = new Harmony("duskidle.peak.summit")` and invokes the internal `Patch()` wrapper 25 times to inject hooks.
8. **Persistent GameObject Attachment**:
   Creates `new GameObject("Summit")`, flags it with `DontDestroyOnLoad`, sets `hideFlags = (HideFlags)61` (`HideFlags.HideAndDontSave` = HideInHierarchy | DontSaveInEditor | NotEditable | DontSaveInBuild | DontUnloadUnusedAsset), and attaches `Menu`.

### 2.3 Component Interaction Model
Summit decouples its presentation, logic, patches, and persistence into clear architectural modules:

| Class | Type | Architectural Responsibility |
|---|---|---|
| `Plugin` | `BaseUnityPlugin` | Plugin entrypoint, lifecycle hooks, Harmony bootstrap, configuration schema. |
| `Menu` | `MonoBehaviour` | IMGUI controller, responsive window scaling, 10-tab navigation, trilingual search indexer, ESP drawing. |
| `Features` | `static class` | Runtime execution engine: physics overrides, coordinate teleportation, item giving, and Photon PUN RPC dispatch. |
| `Patches` | `static class` | Harmony prefix and postfix hook methods intercepting `Assembly-CSharp` logic. |
| `State` | `static class` | High-speed static state cache containing 32 public fields (toggles, sliders, multipliers). |
| `Live` | `static class` | Ephemeral runtime camera orientation (`CamPos`, `CamYaw`, `CamPitch`, `FreeCam`) and 3 spatial memory vectors. |
| `Settings` | `static class` | Reflection-driven persistence engine managing dynamic binding between `State` fields and BepInEx `ConfigFile`. |
| `Keys` | `static class` | Keybinding manager coordinating menu keys, toggle shortcuts, and direct action hotkeys. |
| `Theme` | `static class` | Procedural texture synthesis (anti-aliased rounded boxes), GUIStyles, color palette, and CJK font fallback. |
| `Lang` | `static class` | Trilingual dictionary (EN, ES, ZH), string interpolation formatters, and search keyword expansion. |
| `Picker` | `class` | Searchable grid modal for selecting items, cosmetics, and accessories with dynamic texture preview. |
| `Egg` | `static class` | Logo click easter egg, 110-particle physics confetti, and Win32 `PlaySoundW` audio playback. |
| `Kit` | `static class` | Core mod metadata (`DuskIdle`, Discord), window ID generation, and failed patch tracker. |
| `SelfTest` | `static class` | 78-check automated end-to-end regression test suite driving character actions and assertions. |
| `KitTest` | `static class` | Win32 input simulator automating mouse movements, clicks, and keystrokes for UI verification. |

---

## 3. IMGUI & Visual Subsystem Architecture

### 3.1 Immediate Mode GUI & Responsive Matrix Scaling
Summit utilizes Unity's Immediate Mode GUI (`OnGUI`) rendering pipeline. Rather than hardcoding fixed pixel coordinates that break on non-1080p resolutions, Summit implements a responsive GUI scaling matrix:

```csharp
internal static float Scale
{
    get
    {
        if (ForcedScale > 0f) return ForcedScale;
        float num = Mathf.Min((float)Screen.width / 1100f, (float)Screen.height / 760f);
        if (!(num < 1f))
        {
            return Mathf.Max(1f, (float)Screen.height / 1080f);
        }
        return Mathf.Max(0.55f, num);
    }
}
```

In `Menu.OnGUI()`, the transformation matrix is applied before drawing:
```csharp
Matrix4x4 matrix = GUI.matrix;
GUI.matrix = Matrix4x4.Scale(new Vector3(Scale, Scale, 1f));

// Smooth window height animation
if (Event.current.type == EventType.Layout)
{
    win.height = Mathf.MoveTowards(win.height, wantHeight, 4000f * Time.unscaledDeltaTime);
    win.x = Mathf.Clamp(win.x, 40f - win.width, (float)Screen.width / Scale - 40f);
    win.y = Mathf.Clamp(win.y, 0f, (float)Screen.height / Scale - 40f);
}

win = GUI.Window(Kit.WindowId, win, Draw, GUIContent.none, Theme.Window);
Egg.OnScreen();
GUI.matrix = matrix; // Restore matrix
```

This guarantees that on 720p, 1080p, 1440p, or 4K ultrawide displays, the menu window maintains exact physical proportionality without clipped controls or text overflow.

### 3.2 Procedural Texture Synthesis (`Theme.Round`)
Summit does not bundle external `.png` sprite assets for its UI elements (buttons, sliders, toggles, panels). Instead, `Theme.cs` programmatically generates anti-aliased rounded rectangles in memory using Euclidean distance math:

```csharp
private static Texture2D Round(int size, int radius, Color c)
{
    Texture2D val = new Texture2D(size, size, TextureFormat.RGBA32, false)
    {
        hideFlags = HideFlags.HideAndDontSave,
        filterMode = FilterMode.Bilinear,
        wrapMode = TextureWrapMode.Clamp
    };
    for (int i = 0; i < size; i++)
    {
        for (int j = 0; j < size; j++)
        {
            float num = Mathf.Clamp((float)j + 0.5f, (float)radius, (float)(size - radius));
            float num2 = Mathf.Clamp((float)i + 0.5f, (float)radius, (float)(size - radius));
            float dist = Vector2.Distance(new Vector2((float)j + 0.5f, (float)i + 0.5f), new Vector2(num, num2));
            float alpha = Mathf.Clamp01((float)radius - dist + 0.5f);
            val.SetPixel(j, i, new Color(c.r, c.g, c.b, c.a * alpha));
        }
    }
    val.Apply();
    return val;
}
```
These textures are assigned to `GUIStyle.normal.background` with `border = new RectOffset(...)` 9-slice stretch borders, yielding razor-sharp UI borders and smooth corners.

### 3.3 Dynamic Font Fallback Chain (CJK Support)
To natively support Simplified Chinese without rendering tofu/square glyphs, `Theme.Refont()` dynamically queries the operating system's installed fonts when `Lang.Now == 2` (Chinese):
```csharp
wide = Font.CreateDynamicFontFromOSFont(new string[7] 
{ 
    "Microsoft YaHei UI", 
    "Microsoft YaHei", 
    "DengXian", 
    "SimHei", 
    "SimSun", 
    "Noto Sans CJK SC", 
    "Arial Unicode MS" 
}, 13);
```
All GUIStyles are reflectively iterated and assigned this dynamic font.

### 3.4 Trilingual Fuzzy Search Engine
Summit includes a real-time search box at the top of the menu (`Menu.SearchBox`). During registration of every control (`Menu.Put`), the engine indexes the English, Spanish, and Chinese translations simultaneously:

```csharp
internal Control Put(Control c)
{
    c.In = building;
    string text = ((c.Buttons != null && !c.Raw) 
        ? string.Join(" ", c.Buttons.Select(Lang.Every).ToArray()) 
        : Lang.Every(c.Label));
    
    // c.Match concatenates multilingual labels, hints, card titles, and page names
    c.Match = (text + " " + Lang.Every(c.Hint) + " " + Lang.Every(building.Title) + " " + Lang.Every(building.Page)).ToLowerInvariant();
    building.Items.Add(c);
    controls[c.Id] = c;
    ...
    return c;
}
```

When typing in the search bar, the engine splits the query into individual words and performs an `All()` containment check:
```csharp
string[] array = find.ToLowerInvariant().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
foreach (Group group in groups)
{
    foreach (Control c in group.Items)
    {
        if (array.All(w => c.Match.Contains(w)))
        {
            matches.Add(c);
        }
    }
}
```
A user can type `"stamina"`, `"estamina"`, or `"体力"` and retrieve the exact same toggle immediately.

### 3.5 Pinned Home Tab & Live HUD Overlay
1. **Pinned Home Tab**:
   Every interactable control row features a small 15px pin square on its left edge (`Menu.PinHit`). Clicking toggles the control's ID in `pinned` (persisted to BepInEx config `PinnedToHome`). The Home tab renders all pinned controls in a compact quick-access layout alongside a `"Switched on now"` card displaying active cheat modifiers.
2. **Top-Screen Live HUD Overlay (`DrawActive`)**:
   When the menu is closed (`!IsOpen && Keys.ShowActive`), Summit renders a clean, horizontal status bar at the top of the screen:
   ```csharp
   float num = (float)Screen.width / Scale;
   Rect val = new Rect(num * 0.25f, 6f, num * 0.5f, 64f);
   string text = Keys.Active(); // e.g. "God mode  ·  Infinite stamina  ·  Fly 20 m/s"
   GUI.color = new Color(0f, 0f, 0f, 0.85f);
   GUI.Label(new Rect(val.x + 1f, val.y + 1f, val.width, val.height), text, activeStyle); // Shadow
   GUI.color = Color.white;
   GUI.Label(val, text, activeStyle);
   ```

---

## 4. Full Harmony Hook Inventory & Analysis

### 4.1 Master Harmony Patch Inventory Table
Summit registers 25 patch bindings across 23 target methods in `Assembly-CSharp.dll`. All hooks are declared in `Summit.Patches` and bound in `Plugin.Awake()`:

| # | Target Class | Target Method & Parameter Signature | Hook Type | Summit Hook Method | Behavioral Modification & Mechanics |
|---|---|---|---|---|---|
| **1** | `CharacterAfflictions` | `AddStatus(Status, Character)` | **Prefix** | `AddStatusPrefix` | **God Mode**: If `State.God` and character is local, sets `__result = false` and returns `false`. Completely blocks hunger, cold, injury, poison, and all affliction accumulation. |
| **2** | `CharacterMovement` | `CheckFallDamage(float)` | **Prefix** | `FallDamagePrefix` | **Fall Damage Immunity**: If `State.NoFall || State.Fly` and character is local, returns `false`. Prevents injury calculations and fall damage knockdowns. |
| **3** | `Character` | `Fall(float)` | **Prefix** | `FallPrefix` | **Fall Knockdown Suppression**: If `State.NoFall || State.Fly` and character is local, returns `false`. Suppresses ragdoll tumble reaction on high-speed impacts. |
| **4** | `CharacterMovement` | `GetMovementForce()` | **Postfix** | `MoveForcePostfix` | **Movement Speed Multiplier**: If `State.Speed != 1.0f` and character is local, multiplies return value `__result *= State.Speed` (up to 5x ground velocity). |
| **5** | `CharacterMovement` | `FixedUpdate()` | **Postfix** | `FlyPostfix` | **Free Flight Physics**: If `State.Fly` and character is local, computes directional flight velocity from camera vectors and directly overrides ragdoll `part.Rig.linearVelocity`. Resets `sinceGrounded = 0f`. |
| **6** | `Item` | `get_CarryWeight()` | **Postfix** | `CarryWeightPostfix` | **Zero Carry Weight**: If `State.NoWeight`, forces `__result = 0`. Eliminates all stamina penalties from carried backpacks and heavy gear. |
| **7** | `OrbFogHandler` | `Move()` | **Prefix** | `FogPrefix` | **Fog Movement Freeze**: Returns `!State.StopFog`. If true, aborts fog advance logic across the mountain. |
| **8** | `OrbFogHandler` | `WaitToMove()` | **Prefix** | `FogPrefix` | **Fog Advance Postponement**: Returns `!State.StopFog`. Freezes fog countdown timers. |
| **9** | `CustomizationOption`| `get_IsLocked()` | **Postfix** | `CosmeticLockPostfix`| **Session Wardrobe Unlocker**: If `State.Cosmetics`, overrides return value `__result = false`. Instantly unlocks all skins, hats, outfits, and accessories in the wardrobe. |
| **10**| `GUIManager` | `UpdateWindowStatus()` | **Postfix** | `WindowStatusPostfix`| **Input / Cursor Management**: If `Menu.IsOpen`, reflectively sets `windowShowingCursor = true` and `windowBlockingInput = true` on `GUIManager`. |
| **11**| `AchievementManager` | `ThrowAchievement(ACHIEVEMENTTYPE)` | **Prefix** | `AchievementPrefix` | **Steam Achievement Blocker**: If `!Granting`, returns `!Kit.BlockAchievements.Value`. Prevents cheat-induced achievements from being reported to Steam. |
| **12**| `Item` | `ConsumeDelayed(bool)` | **Prefix** | `ConsumePrefix` | **Infinite Item Use**: If `State.KeepItems` and item belongs to local player, replaces coroutine with `Kept()` iterator to execute effects without decrementing inventory. |
| **13**| `Action_ReduceUses` | `RunAction()` | **Prefix** | `ReduceUsesPrefix` | **Infinite Durability**: If `State.KeepItems` and item is held by local player, returns `false`. Prevents piton, medical, and tool use count depletion. |
| **14**| `RopeSpool` | `set_RopeFuel(float)` | **Prefix** | `RopeFuelPrefix` | **Infinite Rope Spool**: If `State.KeepItems` and `value < ___ropeFuel` on local item, returns `false`. Prevents spool fuel from decreasing. |
| **15**| `Rope` | `get_MaxSegments()` | **Postfix** | `MaxSegmentsPostfix` | **Rope Length Multiplier**: If `State.RopeLength > 1.0f`, multiplies `__result = Mathf.RoundToInt(__result * State.RopeLength)` (up to 3x longer ropes). |
| **16**| `CharacterClimbing` | `AcceptableGrabAngle(Vector3)` | **Postfix** | `GrabAnglePostfix` | **Climb Any Surface**: If `State.ClimbAnything` and character is local, overrides `__result = true`. Allows grabbing steep overhangs, ceilings, and flat ground. |
| **17**| `CharacterClimbing` | `set_playerSlide(Vector2)` | **Prefix** | `SlidePrefix` | **Slip Immunity**: If `State.NoSlip` and character is local, overrides `value = Vector2.zero`. Prevents sliding down icy or sheer surfaces. |
| **18**| `CharacterClimbing` | `TryToStartWallClimb()` | **Prefix** | `ReachPrefix` | **Extended Climb Reach**: If `State.Reach > 1.0f` and character is local, scales `raycastDistance *= State.Reach` (up to 4x normal grab distance). |
| **19**| `RunSettings` | `GetValue(string, bool)` | **Postfix** | `RunValuePostfix` | **Hazard & Weather Cancellation**: Overrides `__result = 0` if `State.NoHazards` (for `Hazard_*` keys) or `State.NoWeather` (for `Hazard_Rain`, `Hazard_Wind`, `ColdNight`, etc.). |
| **20**| `LavaRising` | `Update()` | **Prefix** | `LavaPrefix` | **Lava Rise Freeze**: Returns `!State.StopLava`. When active, completely halts rising lava level progression. |
| **21**| `MainCameraMovement` | `GetFov()` | **Postfix** | `FovPostfix` | **Custom Field of View**: If `State.Fov > 50f`, overrides return value `__result = State.Fov` (slider range 50° to 130°). |
| **22**| `MainCameraMovement` | `CharacterCam()` | **Postfix** | `ThirdPersonPostfix`| **Third-Person Perspective**: Offsets camera backwards by `State.CamDistance` with spherecast collision against terrain mask to prevent wall clipping. |
| **23**| `MainCameraMovement` | `LateUpdate()` | **Postfix** | `FreeCamPostfix` | **Detached Free Camera**: Completely overrides camera transform position and Euler rotation via `Keyboard.current` and `Mouse.current` controls. |
| **24**| `HideTheBody` | `Update()` | **Prefix** | `ShowBodyPrefix` | **Scout Head / Body Mesh Unhiding**: If in Third-Person or FreeCam, forces `ToggleBody.Invoke(instance, True)` so player head and torso are rendered instead of culled. |
| **25**| `CharacterInput` | `Sample()` | **Prefix** | `SamplePrefix` | **Player Input Freeze**: If `Live.FreeCam`, overrides `playerMovementActive = false`. Prevents player scout from moving while steering the free camera. |

---

## 5. Deep Cheat Implementation Mechanics

### 5.1 God Mode & Status Prevention
In PEAK, afflictions (hunger, cold, injury, exhaustion, poison) are driven by the `CharacterAfflictions` component. Many cheat trainers simply poll the health/status bars and set them to full in `Update()`. Summit employs a superior approach by intercepting afflictions at origin:

```csharp
public static bool AddStatusPrefix(CharacterAfflictions __instance, ref bool __result)
{
    if (!State.God || (Object)(object)__instance.character == null || !__instance.character.IsLocal)
    {
        return true;
    }
    __result = false;
    return false; // Aborts AddStatus execution completely
}
```
Because `AddStatus` returns false, status timers and intensity accumulators are never instantiated.
For active restoration, `Features.Heal()` directly invokes:
```csharp
Me.refs.afflictions.ClearAllStatus(false, false);
Character.GainFullStamina();
```

### 5.2 Infinite Stamina & Movement Physics Engine
Summit controls stamina and movement across three coordinated layers:
1. **Infinite Stamina**: In `Features.Upkeep()`, Summit sets the internal game flags:
   ```csharp
   if (State.InfStamina)
   {
       if (!stamTouched) me.data.currentStamina = 1f;
       me.infiniteStam = true;
       stamTouched = true;
   }
   ```
2. **Speed Multiplier**: Harmony Postfix on `CharacterMovement.GetMovementForce` multiplies ground acceleration:
   ```csharp
   public static void MoveForcePostfix(Character ___character, ref float __result)
   {
       if (State.Speed != 1f && ___character != null && ___character.IsLocal)
           __result *= State.Speed;
   }
   ```
3. **Jump & Climbing Impulses**: In `Features.Upkeep()`, base values are captured and dynamically scaled:
   ```csharp
   movement.jumpImpulse = jumpBase * State.Jump;
   climbing.climbSpeedMod = climbBase * State.Climb;
   ```
4. **Infinite Mid-Air Jump (`AirJump`)**:
   ```csharp
   if (State.AirJump)
   {
       me.data.jumpsRemaining = 1;
       me.data.extraJumps = 1;
       jumpTouched = true;
   }
   ```
5. **Surface Adhesion & Slip Cancelling**:
   - `GrabAnglePostfix`: forces `CharacterClimbing.AcceptableGrabAngle` to return `true`.
   - `SlidePrefix`: intercepts `CharacterClimbing.set_playerSlide` and forces `value = Vector2.zero`.
   - `ReachPrefix`: multiplies raycast distance by `State.Reach` (up to 4x).

### 5.3 Infinite Items & Durability Preservation
PEAK handles consumable items (food, medicine, flares) via `Item.ConsumeDelayed` (a Unity coroutine) and tool durability via `Action_ReduceUses.RunAction`. Summit defeats both mechanisms:

1. **Consumable Substitution Coroutine (`Kept`)**:
   When `Item.ConsumeDelayed` is called, `Patches.ConsumePrefix` cancels the game's coroutine and substitutes an iterator (`_003CKept_003Ed__17`):
   ```csharp
   public static bool ConsumePrefix(Item __instance, bool ignoreActions, ref IEnumerator __result)
   {
       if (!State.KeepItems || !Mine(__instance)) return true;
       __result = Kept(__instance, ignoreActions);
       return false; // Game's destruction coroutine is discarded
   }
   ```
   The `Kept` coroutine invokes `item.OnConsumed()` to grant the player the consumable's beneficial effects, yields `null`, and terminates without ever decrementing the stack or destroying the `Item` GameObject!

2. **Durability Protection**:
   `ReduceUsesPrefix` checks `if (State.KeepItems && Mine(component)) return false;`, preventing pitons, ropes, and tools from consuming charges.
3. **Rope Fuel Auto-Topup**:
   In `Features.Upkeep()`, rope spools are automatically replenished to match or exceed the modified max segments:
   ```csharp
   RopeSpool component = me.data.currentItem.GetComponent<RopeSpool>();
   float needed = (float)Rope.MaxSegments + 2f;
   if (component.RopeFuel < needed) component.RopeFuel = needed;
   ```

### 5.4 Flight Kinematics & Noclip Mechanics
When `State.Fly` is enabled, Summit overrides standard character physics in `Patches.FlyPostfix`:

```csharp
public static void FlyPostfix(Character ___character)
{
    if (!State.Fly || ___character == null || !___character.IsLocal || ___character.data.dead) return;

    CharacterInput input = ___character.input;
    // Calculate 3D fly direction based on camera view vectors
    Vector3 dir = ___character.data.lookDirection * input.movementInput.y + 
                  ___character.data.lookDirection_Right * input.movementInput.x;
    
    if (input.jumpIsPressed) dir += Vector3.up;
    if (input.crouchIsPressed) dir += Vector3.down;

    Vector3 velocity = Vector3.ClampMagnitude(dir, 1f) * State.FlySpeed * (input.sprintIsPressed ? 2.5f : 1f);

    // Apply linear velocity across all ragdoll body parts
    foreach (Bodypart part in ___character.refs.ragdoll.partList)
    {
        if (part.Rig != null) part.Rig.linearVelocity = velocity;
    }
    ___character.data.sinceGrounded = 0f; // Eliminates fall velocity accumulation
}
```

**Noclip Collider Disabling**:
When `State.NoClip` is paired with `State.Fly`, Summit calls `me.refs.ragdoll.ToggleCollision(false)`. Because PEAK periodically reenables colliders, Summit asserts collision suppression every 0.5 seconds:
```csharp
bool flag = State.Fly && State.NoClip;
if (flag && (!clipOff || Time.unscaledTime > clipAsserted + 0.5f))
{
    me.refs.ragdoll.ToggleCollision(false);
    clipOff = true;
    clipAsserted = Time.unscaledTime;
}
else if (!flag && clipOff)
{
    me.refs.ragdoll.ToggleCollision(true);
    clipOff = false;
}
```

### 5.5 Teleportation & Spatial Memory Coordinates
Summit provides 4 teleportation modes, all routed through `Features.Warp`:

```csharp
public static void Warp(Vector3 position, string what)
{
    ((MonoBehaviourPun)Me).photonView.RPC("WarpPlayerRPC", RpcTarget.All, new object[2] { position, true });
    Say(Lang.F("Teleported to {0}", what));
}
```

1. **Look-Target Raycast (`WarpToAim`)**:
   Casts a 2000m ray from the camera transform forward position:
   ```csharp
   if (Physics.Raycast(transform.position + transform.forward * 2f, transform.forward, out RaycastHit hit, 2000f, -1, QueryTriggerInteraction.Ignore))
   {
       Warp(hit.point + hit.normal * 1.5f, Lang.T("where you were looking"));
   }
   ```
2. **Campfire & Statue Teleport**: Queries `MapHandler.CurrentCampfire` and `MapHandler.CurrentScoutStatue`, adding `new Vector3(2f, 2f, 0f)`.
3. **Player Teleport (`DrawToScouts`)**: Warps to `targetPlayer.Center + Vector3.up * 1.5f`.
4. **Spatial Memory Spots (`Live.Spots[3]`)**:
   Stores `Vector3?` points in memory via `Live.Spots[i] = Me.Center;`. Recalls via `Warp(Live.Spots[i].Value + Vector3.up * 0.5f)`.

### 5.6 ESP (Extra Sensory Perception) World-to-Screen Projection
Summit's ESP engine (`Menu.Overlay` and `Menu.Tag`) converts 3D world space coordinates of mountain entities into 2D IMGUI screen space:

```csharp
private int Tag(Camera cam, Vector3 eye, Vector3 world, string text, Color color, float range)
{
    float dist = Vector3.Distance(eye, world);
    if (dist > range) return 0;

    Vector3 screenPoint = cam.WorldToScreenPoint(world);
    if (screenPoint.z <= 0f) return 0; // Behind the camera frustum

    // Invert Unity Y-axis for IMGUI screen space
    float screenX = screenPoint.x * (float)Screen.width / (float)cam.pixelWidth;
    float screenY = (float)Screen.height - screenPoint.y * (float)Screen.height / (float)cam.pixelHeight;

    Rect rect = new Rect(screenX - 100f, screenY - 10f, 200f, 20f);
    string label = text + "  " + dist.ToString("0") + " m";

    // Draw drop shadow followed by colored text
    GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), label, espShadow);
    espStyle.normal.textColor = color;
    GUI.Label(rect, label, espStyle);
    return 1;
}
```

**Entity Query & Discovery Engine (`Features.LookAround`)**:
To prevent frame drops from scanning game objects every frame, Summit throttles discovery to a 1.5-second timer (`nextLook = Time.unscaledTime + 1.5f`), caching targets into dedicated lists:
- `EspScouts`: queries `Features.Others()`, max distance 100,000m (Color: Teal `#45BFAD`).
- `EspLuggage`: queries `Luggage.ALL_LUGGAGE`, checks `!IsOpen && !(item is RespawnChest)`, max distance 250m (Color: Gold `#FFD14D`).
- `EspItems`: queries `Item.ALL_ACTIVE_ITEMS`, checks `itemState == 0` (loose on ground), max distance 80m (Color: White).
- `EspCampfires`: queries `Campfire.ALL_CAMPFIRES`, max distance 100,000m (Color: Orange `#FF9940`).
- `EspStatues`: queries `Object.FindObjectsByType<RespawnChest>()`, max distance 600m (Color: Sky Blue `#B3D9FF`).
- `EspHazards`: queries active instances of `Mob`, `Tornado`, `BeeSwarm`, `Antlion`, `MushroomZombie`, `Spider`, max distance 150m (Color: Coral Red `#FF6666`).

### 5.7 Third-Person & Free Camera Mechanics
1. **Third-Person Perspective (`ThirdPersonPostfix`)**:
   Applies a camera backward offset with a spherecast collision sweep against `HelperFunctions.terrainMapMask` (radius 0.25f) to ensure the camera never clips into rocks or cliffs:
   ```csharp
   Vector3 pov = ___targetPlayerPovPosition + Vector3.up * 0.35f;
   Vector3 targetPos = pov - transform.forward * State.CamDistance;
   Vector3 dir = targetPos - pov;
   if (Physics.SphereCast(pov, 0.25f, dir.normalized, out RaycastHit hit, State.CamDistance, HelperFunctions.terrainMapMask, QueryTriggerInteraction.Ignore))
   {
       targetPos = pov - transform.forward * Mathf.Max(0.4f, hit.distance - 0.1f);
   }
   transform.position = targetPos;
   ```
2. **Head & Torso Mesh Unhiding (`ShowBodyPrefix`)**:
   PEAK hides the local scout's head and body in first-person mode via `HideTheBody`. When entering third-person or freecam, Summit forces `ToggleBody.Invoke(__instance, True)`, ensuring the player model renders completely.
3. **Detached Free Camera (`FreeCamPostfix`)**:
   Takes full control of the main camera transform in `LateUpdate()`. Reads raw `Keyboard.current` (WASD + Space/Ctrl + Shift) and `Mouse.current` delta to steer the viewpoint through the mountain while `SamplePrefix` sets `playerMovementActive = false` to keep the physical scout frozen in place.

### 5.8 Item Catalog Reflection & Spawner
Summit reflects into `SingletonAsset<ItemDatabase>.Instance.Objects` to extract all `Item` prefabs registered in the game.
- **Solo / Host Mode**: Spawns items directly into the player's hands via `ItemDatabase.Add(item)`.
- **Guest / Multiplayer Mode**: Instantiates networked item prefabs on the ground or in front of targets via `PhotonNetwork.Instantiate("0_Items/" + item.name, ...)`.

---

## 6. Multiplayer & Networking (Photon PUN Analysis)

### 6.1 Authority Model in PEAK
PEAK uses Photon Unity Networking (PUN 2) for its multiplayer architecture. The game operates under a hybrid client-server model:
- **Master Client (Host)**: Maintains authority over world state, section progression (`MapHandler.JumpToNextSegment`), day/night time cycles (`DayNightManager`), weather hazard states (`RunSettings`), and global affliction clearing.
- **Remote Clients (Guests)**: Maintain local ownership over their own `Character` GameObject and `PhotonView`, but broadcast physics, damage, and interaction events via Remote Procedure Calls (RPCs).

Because PEAK accepts RPCs directed to `PhotonTargets.All` or `RpcTarget.All` without cryptographic signing or strict server validation, a modded client can invoke remote RPCs on foreign player objects.

### 6.2 Master Photon PUN RPC Inventory Table
Summit leverages 6 distinct PUN RPC methods across its Help and Troll systems in `Features.cs`:

| RPC Method Name | Photon Target Filter | Parameter Payload Signature | Sender Authority | Behavioral Impact & Network Effect |
|---|---|---|---|---|
| `WarpPlayerRPC` | `RpcTarget.All` (`(RpcTarget)0`) | `Vector3 position, bool playEffect` | Client / Host | Forces target scout to immediately warp to the target position coordinates. Used by `Warp`, `Bring`, `Swap`, and `WarpToAim`. |
| `RPCA_ReviveAtPosition` | `RpcTarget.All` (`(RpcTarget)0`) | `Vector3 position, bool playSound, int reviverId` | Client / Host | Remote Revive: Resurrects a fallen or passed-out player either where they fell or next to the local scout. |
| `RPC_ApplyStatusesFromFloatArray` | `RpcTarget.All` (`(RpcTarget)0`) | `float[] statusDeltas` | **MasterClient Only** | Remote Status Clear: Sends an array of `-5.0f` across all `STATUSTYPE` enum slots to purge poison, cold, hunger, and injury from remote scouts. |
| `RPCA_Fall` | `RpcTarget.All` (`(RpcTarget)0`) | `float durationSeconds, float unk` | Client / Host | Remote Trip: Knocks the target player over into a ragdoll tumble state for the specified duration (e.g. 1.5s or 3.0s). |
| `RPCA_AddForceAtPosition` | `RpcTarget.All` (`(RpcTarget)0`) | `Vector3 forceVector, Vector3 position, float radius` | Client / Host | Remote Launch: Calculates total ragdoll mass (`part.Rig.mass`) across all body parts and applies directional impulse force to throw or pull players. |
| `JumpRpc` | `RpcTarget.All` (`(RpcTarget)0`) | `bool isDoubleJump` | Client / Host | Remote Hop: Forces the target scout's character controller to execute a jump impulse. |
| `PhotonNetwork.Instantiate` | Network Object Broadcast | `string prefabPath, Vector3 pos, Quaternion rot, byte group, object[] data` | Client / Host | Network Item Delivery: Spawns item prefabs (`"0_Items/" + name`) directly above a player's head or in front of their hands. |

### 6.3 Troll Mechanics & Ragdoll Mass Integration
Summit's `Features.Launch()` calculates precise physics impulses by summing the target's ragdoll body part masses:

```csharp
public static void Launch(Character c, Vector3 direction, float speed)
{
    float totalMass = 0f;
    int parts = 0;
    foreach (Bodypart part in c.refs.ragdoll.partList)
    {
        if (part.Rig != null)
        {
            totalMass += part.Rig.mass;
            parts++;
        }
    }
    if (parts != 0)
    {
        Trip(c, 1.5f); // First trips the player into ragdoll mode
        ((MonoBehaviourPun)c).photonView.RPC("RPCA_AddForceAtPosition", RpcTarget.All, new object[3]
        {
            direction.normalized * speed * (totalMass / (float)parts),
            c.Center,
            40f
        });
    }
}
```
By tripping the player into ragdoll state prior to applying force, the game engine processes the vector as full kinetic velocity, launching the player through the air.

### 6.4 Desynchronization Hazards & Host Validation Limits
1. **Desync on Rapid Warping**: Repeatedly sending `WarpPlayerRPC` to other players while they are climbing can desynchronize their local `CharacterClimbing` state machine, leaving the remote player floating in climb pose while their root transform moves.
2. **Host-Only Guards**: Features such as `HealOther`, `NextSection`, and `SetTime` check `PhotonNetwork.IsMasterClient`. If a non-host attempts to skip sections or force time-of-day changes, the host's authoritative simulation loop overrides the client within several ticks.

---

## 7. Steam Integration & Cosmetics System

### 7.1 Steamworks Achievement Blocker
PEAK interacts with the Steamworks API via `AchievementManager`. When experimenting with cheat mods, players often risk unlocking legitimate Steam achievements (such as reaching mountain summits or completing ascents). Summit provides an active blocker:

```csharp
public static bool AchievementPrefix()
{
    if (!Granting)
    {
        return !Kit.BlockAchievements.Value; // When BlockAchievements is true, returns false
    }
    return true; // Allows bypass only during intentional cosmetic unlocking
}
```
By intercepting `AchievementManager.ThrowAchievement`, Summit blocks all calls to `SteamUserStats.SetAchievement` and `SteamUserStats.StoreStats`.

### 7.2 Session Wardrobe Unlocker (Non-Permanent)
PEAK evaluates wardrobe cosmetic locks via `CustomizationOption.IsLocked`. Summit intercepts the getter:

```csharp
public static void CosmeticLockPostfix(ref bool __result)
{
    if (State.Cosmetics)
    {
        __result = false; // Forces every cosmetic item to report as unlocked
    }
}
```
This enables players to equip any skin, hat, outfit, or accessory in the passport menu. Because it modifies only the in-memory boolean property during GUI checks, **no data is written to Steam or player save files**, and all locked items revert the instant the game restarts.

### 7.3 Permanent Cosmetic Unlocker Mechanics
For players who explicitly want to unlock all cosmetics permanently, Summit provides `Features.UnlockCosmeticsForever()`. PEAK's cosmetic unlock requirements are tied directly to Steam achievements and ascent statistics:

```csharp
public static List<ACHIEVEMENTTYPE> CosmeticRequirements(out int ascent, out int locked)
{
    List<ACHIEVEMENTTYPE> need = new List<ACHIEVEMENTTYPE>();
    ascent = 0; locked = 0;
    ...
    foreach (CustomizationOption val in Resources.FindObjectsOfTypeAll<CustomizationOption>())
    {
        if (val == null || !val.IsLocked) continue;
        locked++;
        if (val.requiresAscent)
            ascent = Mathf.Max(ascent, val.requiredAscent);
        else if ((int)val.customRequirement == 1)
            ascent = Mathf.Max(ascent, 8);
        else if ((int)val.customRequirement == 2)
            for (int j = 1; j <= 32; j++) Want(need, instance, (ACHIEVEMENTTYPE)j);
        else
            Want(need, instance, val.requiredAchievement);
    }
    return need;
}
```

When triggered:
1. `Patches.Granting = true;` temporarily disables the achievement blocker.
2. Iterates over missing `ACHIEVEMENTTYPE` entries and calls `AchievementManager.Grant(item)`.
3. If required ascent exceeds current max, invokes `instance.SetSteamStat((STEAMSTATTYPE)13, ascent)`.
4. Reenables `Patches.Granting = false;`.
5. Requires a two-stage user confirmation: the UI button requires a second confirmation click within a 6-second window (`foreverUntil = Time.unscaledTime + 6f`).

---

## 8. Internal Self-Test Suite & Easter Egg

### 8.1 Automated Self-Test Suite (`SelfTest.cs`)
One of Summit's most remarkable architectural features is its embedded regression test suite. Rather than relying on manual QA, the mod includes 78 automated test cases that execute directly inside the running game process.

```
Trigger Condition: Presence of "summit_selftest.flag" in BepInEx root folder
Execution Driver:  SelfTest.Tick(menu) -> Coroutine enumerator _003CBody_003Ed__34
Assertion Output:  "summit_selftest_report.txt" in BepInEx root folder
Teardown:          Application.Quit() upon completion
```

The test runner spans 18 distinct test sections:
1. `[Loading]`: Asserts game scene and local player initialization.
2. `[The mountain]`: Verifies mountain terrain, map handlers, and segment indexing.
3. `[Climbing]`: Tests climb speed modifiers, grab angles, and slip cancellation.
4. `[Camera]`: Asserts FOV bounds, third-person spherecast distances, and freecam state flags.
5. `[The menu]`: Verifies window matrix scaling, tab selection, and search filtering.
6. `[Menu, in the airport]`: Tests airport lobby state transitions.
7. `[Scout]`: Asserts God mode status immunity, instant heal, and bonus stamina injection.
8. `[Movement]`: Tests speed multipliers, jump impulses, air jump flags, and fly kinematics.
9. `[World]`: Verifies fog freeze, lava halt, and hazard/weather suppression flags.
10. `[Items, on the island]`: Verifies island item spawning and placement.
11. `[Help (sent to your own scout)]`: Asserts remote revive, status clear, and item handing RPCs.
12. `[Teleport, on the island]`: Tests coordinate warping and POI resolution.
13. `[Teleport]`: Validates raycast look targeting and 3-slot spatial memory persistence.
14. `[Left alone]`: Tests solo session isolation.
15. `[Items]`: Asserts infinite consumable use (`Kept`) and durability preservation.
16. `[Troll (each one sent to your own scout)]`: Tests launch, trip, pull, and swap RPCs on the local player.
17. `[Scout, on the island]`: Verifies island character state invariants.
18. `[Outfits]`: Tests wardrobe serialization, outfit preset saving, and slot reloading.

### 8.2 Win32 Input Simulation Test Harness (`KitTest.cs`)
To test the UI end-to-end, `KitTest.cs` uses Win32 API P/Invokes to simulate physical user input:

```csharp
[DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
[DllImport("user32.dll")] private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);
[DllImport("user32.dll")] private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
[DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr window, ref Point p);
[DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
[DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr window);
```
During automated testing, `KitTest` brings the PEAK window to the foreground, moves the mouse cursor across menu tabs and toggle buttons, clicks pins, types search queries, and verifies that UI states update in real time.

### 8.3 The Easter Egg ("Egg")
On the About page, clicking the Summit logo 7 times within 1.5 seconds triggers a hidden easter egg (`Egg.cs`):

```csharp
internal static void Tap()
{
    float unscaledTime = Time.unscaledTime;
    if (unscaledTime - lastTap > 1.5f) taps = 0;
    lastTap = unscaledTime;
    if (++taps >= 7 && !On)
    {
        taps = 0;
        Start(); // Triggers 6-second easter egg sequence
    }
}
```

**Visual & Audio Payload**:
1. **Confetti Particle Engine**: Simulates 110 physics confetti bits with randomized trajectories (`vx`, `vy`), gravity, and 6 vibrant colors (`Colours`).
2. **Audio Playback**: Extracts the embedded resource `egg.wav` into unmanaged memory and plays it via the Windows multimedia library:
   ```csharp
   [DllImport("winmm.dll", EntryPoint = "PlaySoundW")]
   private static extern bool PlaySound(IntPtr sound, IntPtr module, uint flags);
   ```
3. **Graphic & Text Overlay**: Loads embedded `egg.png` via `ImageConversion.LoadImage` and draws the creator's tag across the center of the screen:
   ```csharp
   internal const string Words = "duski was hereee";
   ```
4. **Auto-Dismiss**: The easter egg runs for exactly 6.0 seconds (`until = started + 6f`), automatically cleaning up all resources upon expiration.

---

## 9. Architectural Assessment & Developer Takeaways

### 9.1 Architectural Strengths
1. **Self-Contained Procedural UI**: By generating procedural rounded-rect textures at runtime (`Theme.Round`) and using OS font fallback chains, Summit eliminates external asset bundle dependencies, avoiding asset bundle version mismatches across Unity engine updates.
2. **Non-Destructive Hooks**: Patches consistently check `character.IsLocal` to isolate singleplayer/local cheats from contaminating remote players' physics or status accumulators.
3. **Upstream Interception over Post-Fix Healing**: Intercepting `AddStatus` at origin is significantly more robust than polling health values and healing damage post-facto.
4. **Exhaustive Automated Testing**: Incorporating an automated self-test suite (`SelfTest.cs` and `KitTest.cs`) inside a game mod is an exemplary software engineering practice that drastically reduces regression risk across game patches.

### 9.2 Anti-Patterns & Vulnerabilities
1. **Global Static State**: The entire state model (`State.cs`, `Live.cs`, `Features.cs`) is statically scoped. While performant, this tightly couples subsystems and prevents multi-instance execution or easy unit testing without game context.
2. **Unvalidated PUN RPC Broadcasting**: Invoking RPCs like `RPCA_Fall`, `RPCA_AddForceAtPosition`, and `WarpPlayerRPC` with `RpcTarget.All` demonstrates that the PEAK networking layer lacks server-side verification. Any client can trip, launch, or teleport other players in public lobbies. Mod developers should treat these capabilities with extreme ethical caution.
3. **Hardcoded String Identifiers**: RPC method names and item prefab strings (`"0_Items/" + name`) are hardcoded as raw strings. Any rename in PEAK's internal codebase will silently break functionality without compiler warnings.

### 9.3 Safe Modding Guidelines for PEAK Developers
- **Always Validate `IsLocal`**: When altering movement, stamina, fall damage, or inventory, ensure modifications check `character.IsLocal` to avoid polluting the host's simulation of other scouts.
- **Guard Achievement Calls**: Mod developers should follow Summit's example and provide an achievement block toggle (`AchievementManager.ThrowAchievement` prefix) to prevent test sessions from corrupting Steam profile integrity.
- **Cache Reflection Targets**: Always cache `MethodInfo`, `FieldInfo`, and `PropertyInfo` in static readonly fields (as seen with `SetShowCursor`, `SetBlockInput`, and `UpdateWeight`) to eliminate reflection overhead in per-frame update loops.
- **Respect Master Client Authority**: World progression, time of day, and weather states should only be modified when `PhotonNetwork.IsMasterClient` is true, preventing lobby desynchronization.

---
*Report compiled and verified against Summit.dll v1.1.1 decompiled C# sources.*
