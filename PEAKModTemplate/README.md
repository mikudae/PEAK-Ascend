# PEAK Mod Template

Ready-to-build BepInEx 5 C# plugin starter template for **PEAK**.

## Features

- **Target Framework**: .NET Framework 4.7.2 (`net472`), fully compatible with Unity 6 Mono runtime and BepInEx 5.4.
- **Reference Assemblies**: Pre-configured references to game logic (`Assembly-CSharp.dll`), Unity engine modules, BepInEx core, and Harmony.
- **Harmony Patching**: Initialized in `Awake()` via `Harmony.CreateAndPatchAll()`, with a sample postfix patch targeting `Character.Start`.
- **BepInEx Configuration**: Typed configuration binding (`Config.Bind`) for mod toggles and custom settings.
- **Auto-Deploy Target**: Automatically copies compiled `.dll` and `.pdb` directly to the active PEAK `BepInEx/plugins/` directory upon build.

## Building

Ensure `.NET SDK >= 8.0` is available in your PATH:

```powershell
dotnet build
```

## Customizing

1. Edit `PEAKModTemplate.csproj` to rename `<AssemblyName>` and `<RootNamespace>`.
2. Update plugin metadata in `Plugin.cs`:
   - Plugin GUID (e.g. `com.yourname.yourmod`)
   - Plugin Name
   - Version
3. Add new Harmony patches under the `Patches/` folder.
