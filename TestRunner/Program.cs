using System;
using System.IO;
using System.Reflection;
using System.Linq;

namespace TestRunner;

internal class Program
{
    private static int assertions = 0;

    private static void AssertEqual<T>(T actual, T expected, string testName)
    {
        assertions++;
        if (!object.Equals(actual, expected))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FAIL] {testName} - Expected: '{expected}', Got: '{actual}'");
            Console.ResetColor();
            Environment.Exit(1);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [PASS] {testName} ({actual})");
            Console.ResetColor();
        }
    }

    private static void AssertTrue(bool condition, string testName)
    {
        assertions++;
        if (!condition)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FAIL] {testName}");
            Console.ResetColor();
            Environment.Exit(1);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [PASS] {testName}");
            Console.ResetColor();
        }
    }

    public static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==========================================================");
        Console.WriteLine("       Executing Summit Mod Automated Unit Tests");
        Console.WriteLine("==========================================================");
        Console.ResetColor();

        string currentDir = AppContext.BaseDirectory;
        string workspaceRoot = Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\.."));
        string slaveDllPath = Path.Combine(workspaceRoot, "dist", "Summit_Slave.dll");
        string masterDllPath = Path.Combine(workspaceRoot, "dist", "Summit_Master.dll");

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var name = new AssemblyName(args.Name).Name;
            string gameManagedDir = @"C:\Users\feldring\Downloads\PEAK\PEAK.v2.6.b-OFME\PEAK\PEAK_Data\Managed";
            string p = Path.Combine(gameManagedDir, name + ".dll");
            if (File.Exists(p)) return Assembly.LoadFrom(p);
            string lp = Path.Combine(workspaceRoot, "lib", name + ".dll");
            if (File.Exists(lp)) return Assembly.LoadFrom(lp);
            return null;
        };

        if (!File.Exists(slaveDllPath))
        {
            Console.WriteLine("Error: dist/Summit_Slave.dll not found at: " + slaveDllPath);
            Environment.Exit(1);
        }

        Assembly slaveAsm = Assembly.LoadFrom(slaveDllPath);
        Type modNetType = slaveAsm.GetType("Summit.ModNet");
        Type keysType = slaveAsm.GetType("Summit.Keys");

        AssertTrue(modNetType != null, "ModNet type found in Summit_Slave.dll");
        AssertTrue(keysType != null, "Keys type found in Summit_Slave.dll");

        // 1. ManageableSections length and contents
        FieldInfo sectionsField = modNetType.GetField("ManageableSections", BindingFlags.Public | BindingFlags.Static);
        string[] sections = (string[])sectionsField.GetValue(null);
        AssertEqual(sections.Length, 10, "ManageableSections count is exactly 10");

        string[] expectedSections = new string[]
        {
            "Home", "Scout", "Movement", "Items", "Teleport", "World", "Camera", "Help", "Troll", "About"
        };
        for (int i = 0; i < 10; i++)
        {
            AssertEqual(sections[i], expectedSections[i], $"ManageableSection[{i}] == '{expectedSections[i]}'");
        }

        // 2. ModRole check on Slave
        FieldInfo roleField = modNetType.GetField("Role", BindingFlags.Public | BindingFlags.Static);
        AssertEqual(roleField.GetValue(null).ToString(), "Slave", "Slave binary Role is Slave");

        // 3. Default Slave Permissions (Home, Scout, About granted; all other 7 locked)
        MethodInfo isAllowedMethod = modNetType.GetMethod("IsSectionAllowed", BindingFlags.Public | BindingFlags.Static);
        AssertTrue((bool)isAllowedMethod.Invoke(null, new object[] { "Home" }), "Default Slave: Home is GRANTED");
        AssertTrue((bool)isAllowedMethod.Invoke(null, new object[] { "Scout" }), "Default Slave: Scout is GRANTED");
        AssertTrue((bool)isAllowedMethod.Invoke(null, new object[] { "About" }), "Default Slave: About is GRANTED");

        string[] lockedDefaults = new string[] { "Movement", "Items", "Teleport", "World", "Camera", "Help", "Troll" };
        foreach (string sec in lockedDefaults)
        {
            AssertEqual((bool)isAllowedMethod.Invoke(null, new object[] { sec }), false, $"Default Slave: {sec} is LOCKED");
        }

        // 4. Test Master revoking Home, Scout, About globally and granting Movement, Items
        MethodInfo updateGlobalMethod = modNetType.GetMethod("UpdateLocalGlobalPermissions", BindingFlags.Public | BindingFlags.Static);
        updateGlobalMethod.Invoke(null, new object[] { "Movement,Items" });

        AssertEqual((bool)isAllowedMethod.Invoke(null, new object[] { "Home" }), false, "Revoke test: Home is LOCKED");
        AssertEqual((bool)isAllowedMethod.Invoke(null, new object[] { "Scout" }), false, "Revoke test: Scout is LOCKED");
        AssertEqual((bool)isAllowedMethod.Invoke(null, new object[] { "About" }), false, "Revoke test: About is LOCKED");
        AssertTrue((bool)isAllowedMethod.Invoke(null, new object[] { "Movement" }), "Grant test: Movement is GRANTED");
        AssertTrue((bool)isAllowedMethod.Invoke(null, new object[] { "Items" }), "Grant test: Items is GRANTED");
        AssertEqual((bool)isAllowedMethod.Invoke(null, new object[] { "Teleport" }), false, "Grant test: Teleport remains LOCKED");

        // 5. Test Per-Slave Custom Override (overrides lobby global)
        MethodInfo updateOverrideMethod = modNetType.GetMethod("UpdateLocalSlaveOverride", BindingFlags.Public | BindingFlags.Static);
        updateOverrideMethod.Invoke(null, new object[] { "Teleport,Camera" });

        AssertEqual((bool)isAllowedMethod.Invoke(null, new object[] { "Movement" }), false, "Override: Movement ignored");
        AssertTrue((bool)isAllowedMethod.Invoke(null, new object[] { "Teleport" }), "Override: Teleport is GRANTED");
        AssertTrue((bool)isAllowedMethod.Invoke(null, new object[] { "Camera" }), "Override: Camera is GRANTED");

        // 6. Test Clear Override (restores lobby global defaults)
        MethodInfo clearOverrideMethod = modNetType.GetMethod("ClearLocalSlaveOverride", BindingFlags.Public | BindingFlags.Static);
        clearOverrideMethod.Invoke(null, null);

        AssertTrue((bool)isAllowedMethod.Invoke(null, new object[] { "Movement" }), "Clear override: Movement restored");
        AssertTrue((bool)isAllowedMethod.Invoke(null, new object[] { "Items" }), "Clear override: Items restored");
        AssertEqual((bool)isAllowedMethod.Invoke(null, new object[] { "Teleport" }), false, "Clear override: Teleport revoked");

        // 7. Check Keys.GetSection mapping for standalone Jobs
        Type boundType = keysType.GetNestedType("Bound", BindingFlags.NonPublic | BindingFlags.Public);
        MethodInfo getSectionMethod = keysType.GetMethod("GetSection", BindingFlags.Public | BindingFlags.Static);

        void CheckBound(string id, string expectedSection)
        {
            object b = Activator.CreateInstance(boundType);
            boundType.GetField("Id").SetValue(b, id);
            string sec = (string)getSectionMethod.Invoke(null, new object[] { b });
            AssertEqual(sec, expectedSection, $"Keys.GetSection('{id}') -> '{expectedSection}'");
        }

        CheckBound("WarpAim", "Teleport");
        CheckBound("WarpFire", "Teleport");
        CheckBound("SaveSpot", "Teleport");
        CheckBound("GoSpot", "Teleport");
        CheckBound("Heal", "Scout");
        CheckBound("FreeCam", "Camera");

        // 8. Test Master Binary
        var masterContext = new System.Runtime.Loader.AssemblyLoadContext("MasterContext");
        masterContext.Resolving += (ctx, asmName) =>
        {
            string gameManagedDir = @"C:\Users\feldring\Downloads\PEAK\PEAK.v2.6.b-OFME\PEAK\PEAK_Data\Managed";
            string p = Path.Combine(gameManagedDir, asmName.Name + ".dll");
            if (File.Exists(p)) return ctx.LoadFromAssemblyPath(p);
            string lp = Path.Combine(workspaceRoot, "lib", asmName.Name + ".dll");
            if (File.Exists(lp)) return ctx.LoadFromAssemblyPath(lp);
            return null;
        };
        Assembly masterAsm = masterContext.LoadFromAssemblyPath(masterDllPath);
        Type masterModNet = masterAsm.GetType("Summit.ModNet");
        FieldInfo masterRoleField = masterModNet.GetField("Role", BindingFlags.Public | BindingFlags.Static);
        AssertEqual(masterRoleField.GetValue(null).ToString(), "Master", "Master binary Role is Master");

        MethodInfo masterIsAllowedMethod = masterModNet.GetMethod("IsSectionAllowed", BindingFlags.Public | BindingFlags.Static);
        foreach (string sec in expectedSections)
        {
            AssertTrue((bool)masterIsAllowedMethod.Invoke(null, new object[] { sec }), $"Master has full access to {sec}");
        }

        // 9. Ascend 2.0 Rebrand & Version Verification
        Type kitType = masterAsm.GetType("Summit.Kit");
        FieldInfo versionField = kitType.GetField("Version", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        AssertEqual(versionField?.GetValue(null)?.ToString(), "2.0", "Kit.Version is '2.0'");

        Type pluginType = masterAsm.GetType("Summit.Plugin");
        var bepInAttr = pluginType.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "BepInPlugin");
        AssertTrue(bepInAttr != null, "BepInPlugin attribute exists on Plugin class");
        var nameProp = bepInAttr.GetType().GetProperty("Name");
        var verProp = bepInAttr.GetType().GetProperty("Version");
        AssertEqual(nameProp?.GetValue(bepInAttr)?.ToString(), "Ascend", "Plugin display name is 'Ascend'");
        AssertEqual(verProp?.GetValue(bepInAttr)?.ToString(), "2.0", "Plugin version is '2.0'");

        // 10. Theme Presets & Dynamic Palette Verification
        Type themeType = masterAsm.GetType("Summit.Theme");
        FieldInfo presetsField = themeType.GetField("Presets", BindingFlags.Public | BindingFlags.Static);
        Array presetsArr = (Array)presetsField.GetValue(null);
        AssertEqual(presetsArr.Length, 6, "Theme presets count is exactly 6");

        string[] expectedPresetNames = new string[]
        {
            "Emerald Teal", "Royal Violet", "Crimson Ruby", "Sunset Amber", "Cyberpunk Neon", "Rose Quartz"
        };
        for (int i = 0; i < presetsArr.Length; i++)
        {
            object item = presetsArr.GetValue(i);
            string name = item.GetType().GetField("Name").GetValue(item)?.ToString();
            AssertEqual(name, expectedPresetNames[i], $"Theme Preset[{i}] == '{expectedPresetNames[i]}'");
        }

        // 11. Inventory Management & Event Code Verification
        FieldInfo injectCodeField = masterModNet.GetField("InjectItemEventCode", BindingFlags.Public | BindingFlags.Static);
        AssertEqual((byte)injectCodeField.GetValue(null), (byte)144, "InjectItemEventCode is 144");

        MethodInfo deliverMethod = masterModNet.GetMethod("DeliverItemToScout", BindingFlags.Public | BindingFlags.Static);
        AssertTrue(deliverMethod != null, "DeliverItemToScout method exists on ModNet");

        MethodInfo handleInjectMethod = masterModNet.GetMethod("HandleInjectedItem", BindingFlags.Public | BindingFlags.Static);
        AssertTrue(handleInjectMethod != null, "HandleInjectedItem method exists on ModNet");

        MethodInfo getInvMethod = masterModNet.GetMethod("GetPlayerInventory", BindingFlags.Public | BindingFlags.Static);
        AssertTrue(getInvMethod != null, "GetPlayerInventory method exists on ModNet");

        // 12. DebugBridge Type Verification
        Type masterBridgeType = masterAsm.GetType("Summit.DebugBridge");
        Type slaveBridgeType = slaveAsm.GetType("Summit.DebugBridge");
        AssertTrue(masterBridgeType != null, "DebugBridge type exists in Summit_Master.dll");
        AssertTrue(slaveBridgeType != null, "DebugBridge type exists in Summit_Slave.dll");

        MethodInfo startMethod = masterBridgeType.GetMethod("Start", BindingFlags.Public | BindingFlags.Static);
        MethodInfo stopMethod = masterBridgeType.GetMethod("Stop", BindingFlags.Public | BindingFlags.Static);
        MethodInfo updateMethod = masterBridgeType.GetMethod("Update", BindingFlags.Public | BindingFlags.Static);
        MethodInfo logMethod = masterBridgeType.GetMethod("Log", BindingFlags.Public | BindingFlags.Static);
        MethodInfo logPhotonMethod = masterBridgeType.GetMethod("LogPhoton", BindingFlags.Public | BindingFlags.Static);

        AssertTrue(startMethod != null, "DebugBridge.Start method exists");
        AssertTrue(stopMethod != null, "DebugBridge.Stop method exists");
        AssertTrue(updateMethod != null, "DebugBridge.Update method exists");
        AssertTrue(logMethod != null, "DebugBridge.Log method exists");
        AssertTrue(logPhotonMethod != null, "DebugBridge.LogPhoton method exists");

        // 13. RingBuffer Functionality
        Type ringBufferType = masterBridgeType.GetNestedType("RingBuffer", BindingFlags.Public);
        AssertTrue(ringBufferType != null, "DebugBridge.RingBuffer type exists");
        object ringBuffer = Activator.CreateInstance(ringBufferType, new object[] { 5 });
        MethodInfo addMethod = ringBufferType.GetMethod("Add");
        MethodInfo getLastMethod = ringBufferType.GetMethod("GetLast");

        for (int i = 0; i < 7; i++)
        {
            addMethod.Invoke(ringBuffer, new object[] { $"Log_{i}" });
        }

        var last3 = (System.Collections.Generic.List<string>)getLastMethod.Invoke(ringBuffer, new object[] { 3 });
        AssertEqual(last3.Count, 3, "RingBuffer GetLast(3) count is 3");
        AssertEqual(last3[0], "Log_4", "RingBuffer oldest of last 3 is Log_4");
        AssertEqual(last3[1], "Log_5", "RingBuffer middle of last 3 is Log_5");
        AssertEqual(last3[2], "Log_6", "RingBuffer latest of last 3 is Log_6");

        var lastAll = (System.Collections.Generic.List<string>)getLastMethod.Invoke(ringBuffer, new object[] { 10 });
        AssertEqual(lastAll.Count, 5, "RingBuffer GetLast(10) capped at capacity 5");
        AssertEqual(lastAll[0], "Log_2", "RingBuffer oldest capped is Log_2");

        MethodInfo clearMethod = ringBufferType.GetMethod("Clear");
        clearMethod.Invoke(ringBuffer, null);
        var afterClear = (System.Collections.Generic.List<string>)getLastMethod.Invoke(ringBuffer, new object[] { 10 });
        AssertEqual(afterClear.Count, 0, "RingBuffer Clear() empties buffer");

        // Test RingBuffer handles capacity 0 safely
        object zeroBuffer = Activator.CreateInstance(ringBufferType, new object[] { 0 });
        addMethod.Invoke(zeroBuffer, new object[] { "SafeEntry" });
        var zeroLast = (System.Collections.Generic.List<string>)getLastMethod.Invoke(zeroBuffer, new object[] { 1 });
        AssertEqual(zeroLast.Count, 1, "RingBuffer(0) clamps capacity to >= 1 without DivideByZeroException");

        // 14. Live Server Start & Port Allocation (Master)
        startMethod.Invoke(null, null);
        PropertyInfo isRunningProp = masterBridgeType.GetProperty("IsRunning", BindingFlags.Public | BindingFlags.Static);
        PropertyInfo activePortProp = masterBridgeType.GetProperty("ActivePort", BindingFlags.Public | BindingFlags.Static);
        PropertyInfo activeUrlProp = masterBridgeType.GetProperty("ActiveUrl", BindingFlags.Public | BindingFlags.Static);

        AssertTrue((bool)isRunningProp.GetValue(null), "DebugBridge.IsRunning is true after Start()");
        int masterPort = (int)activePortProp.GetValue(null);
        string masterUrl = (string)activeUrlProp.GetValue(null);
        AssertTrue(masterPort >= 18080, $"DebugBridge allocated master port >= 18080 (got {masterPort})");
        AssertEqual(masterUrl, $"http://127.0.0.1:{masterPort}", "DebugBridge.ActiveUrl matches format");

        string masterTempFile = Path.Combine(Path.GetTempPath(), "peak_debug_master.txt");
        AssertTrue(File.Exists(masterTempFile), "peak_debug_master.txt was created in %TEMP%");
        string recordedUrl = File.ReadAllText(masterTempFile).Trim();
        AssertEqual(recordedUrl, masterUrl, "peak_debug_master.txt records exact active URL");

        // Background dispatcher to drain main-thread action queue during test
        bool dispatching = true;
        Thread dispatcherThread = new Thread(() =>
        {
            while (dispatching)
            {
                try
                {
                    updateMethod.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DISPATCHER THREAD ERROR: " + ex.InnerException ?? ex.Message);
                }
                Thread.Sleep(5);
            }
        });
        dispatcherThread.IsBackground = true;
        dispatcherThread.Start();

        // 15. Live HTTP Requests using HttpClient
        using (var http = new System.Net.Http.HttpClient())
        {
            http.Timeout = TimeSpan.FromSeconds(10);

            // Test GET /
            var rootResp = http.GetAsync(masterUrl + "/").GetAwaiter().GetResult();
            AssertEqual((int)rootResp.StatusCode, 200, "GET / returns HTTP 200");
            string rootBody = rootResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(rootBody.Contains("Ascend Live Debug Bridge"), "GET / response contains mod title");

            // Test GET /status
            var statusResp = http.GetAsync(masterUrl + "/status").GetAwaiter().GetResult();
            string statusBody = statusResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if ((int)statusResp.StatusCode != 200) Console.WriteLine("DEBUG /status Error: " + statusBody);
            AssertEqual((int)statusResp.StatusCode, 200, "GET /status returns HTTP 200");
            AssertTrue(statusBody.Contains("\"scene\""), "GET /status JSON contains 'scene'");
            AssertTrue(statusBody.Contains("\"inGame\""), "GET /status JSON contains 'inGame'");
            AssertTrue(statusBody.Contains("\"role\""), "GET /status JSON contains 'role'");
            AssertTrue(statusBody.Contains("\"coords\""), "GET /status JSON contains 'coords'");
            AssertTrue(statusBody.Contains("\"cheats\""), "GET /status JSON contains 'cheats'");
            AssertTrue(statusBody.Contains("\"health\""), "GET /status JSON contains 'health'");
            AssertTrue(statusBody.Contains("\"stamina\""), "GET /status JSON contains 'stamina'");
            AssertTrue(statusBody.Contains("\"inRoom\""), "GET /status JSON contains 'inRoom'");

            // Test HEAD /status returns HTTP 200 without body
            var headReq = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Head, masterUrl + "/status");
            var headResp = http.SendAsync(headReq).GetAwaiter().GetResult();
            AssertEqual((int)headResp.StatusCode, 200, "HEAD /status returns HTTP 200");

            // Test POST /status returns HTTP 405 Method Not Allowed
            var postStatusResp = http.PostAsync(masterUrl + "/status", new System.Net.Http.StringContent("")).GetAwaiter().GetResult();
            AssertEqual((int)postStatusResp.StatusCode, 405, "POST /status returns HTTP 405 Method Not Allowed");

            // Test OPTIONS /action returns HTTP 204 No Content
            var optionsReq = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("OPTIONS"), masterUrl + "/action");
            var optionsResp = http.SendAsync(optionsReq).GetAwaiter().GetResult();
            AssertEqual((int)optionsResp.StatusCode, 204, "OPTIONS /action returns HTTP 204 No Content");

            // Test GET /scouts
            var scoutsResp = http.GetAsync(masterUrl + "/scouts").GetAwaiter().GetResult();
            AssertEqual((int)scoutsResp.StatusCode, 200, "GET /scouts returns HTTP 200");
            string scoutsBody = scoutsResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(scoutsBody.StartsWith("[") && scoutsBody.EndsWith("]"), "GET /scouts returns JSON array");

            // Test GET /inventory
            var invResp = http.GetAsync(masterUrl + "/inventory").GetAwaiter().GetResult();
            AssertEqual((int)invResp.StatusCode, 200, "GET /inventory returns HTTP 200");
            string invBody = invResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(invBody.StartsWith("[") && invBody.EndsWith("]"), "GET /inventory returns JSON array");

            // Test GET /logs
            logMethod.Invoke(null, new object[] { "TestRunner log entry 1" });
            logPhotonMethod.Invoke(null, new object[] { "TestRunner photon trace 1" });

            var logsResp = http.GetAsync(masterUrl + "/logs").GetAwaiter().GetResult();
            AssertEqual((int)logsResp.StatusCode, 200, "GET /logs returns HTTP 200");
            string logsBody = logsResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(logsBody.Contains("TestRunner log entry 1"), "GET /logs contains logged entry");
            AssertTrue(logsBody.Contains("TestRunner photon trace 1"), "GET /logs contains photon trace");

            var logsCountResp = http.GetAsync(masterUrl + "/logs?count=1").GetAwaiter().GetResult();
            AssertEqual((int)logsCountResp.StatusCode, 200, "GET /logs?count=1 returns HTTP 200");

            var logsZeroResp = http.GetAsync(masterUrl + "/logs?count=0").GetAwaiter().GetResult();
            AssertEqual((int)logsZeroResp.StatusCode, 200, "GET /logs?count=0 returns HTTP 200");
            string logsZeroBody = logsZeroResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertEqual(logsZeroBody.Trim(), "[]", "GET /logs?count=0 returns empty JSON array []");

            // Test POST /action say
            var sayContent = new System.Net.Http.StringContent("action=say&msg=Hello", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var sayResp = http.PostAsync(masterUrl + "/action", sayContent).GetAwaiter().GetResult();
            AssertEqual((int)sayResp.StatusCode, 200, "POST /action (say) returns HTTP 200");
            string sayBody = sayResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(sayBody.Contains("\"success\":true"), "POST /action (say) success is true");
            AssertTrue(sayBody.Contains("Hello"), "POST /action (say) echoed message");

            // Test POST /action toggle cheat=God
            Type stateType = masterAsm.GetType("Summit.State");
            FieldInfo godField = stateType.GetField("God", BindingFlags.Public | BindingFlags.Static);
            bool initialGod = (bool)godField.GetValue(null);

            var toggleContent = new System.Net.Http.StringContent("action=toggle&cheat=God", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var toggleResp = http.PostAsync(masterUrl + "/action", toggleContent).GetAwaiter().GetResult();
            AssertEqual((int)toggleResp.StatusCode, 200, "POST /action (toggle God) returns HTTP 200");
            string toggleBody = toggleResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(toggleBody.Contains("\"success\":true"), "POST /action (toggle God) success is true");

            bool toggledGod = (bool)godField.GetValue(null);
            AssertEqual(toggledGod, !initialGod, "State.God was toggled by POST /action");

            // Check GET /status reflects toggled God mode
            var statusAfterResp = http.GetAsync(masterUrl + "/status").GetAwaiter().GetResult();
            string statusAfterBody = statusAfterResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(statusAfterBody.Contains($"\"god\":{toggledGod.ToString().ToLowerInvariant()}"), "GET /status reflects updated God mode");

            // Toggle back to initial state
            http.PostAsync(masterUrl + "/action", toggleContent).GetAwaiter().GetResult();
            AssertEqual((bool)godField.GetValue(null), initialGod, "State.God restored to original state");

            // Test POST /action toggle with JSON body
            var jsonToggleContent = new System.Net.Http.StringContent("{\"action\":\"toggle\",\"cheat\":\"Fly\"}", System.Text.Encoding.UTF8, "application/json");
            var jsonToggleResp = http.PostAsync(masterUrl + "/action", jsonToggleContent).GetAwaiter().GetResult();
            AssertEqual((int)jsonToggleResp.StatusCode, 200, "POST /action with JSON payload returns HTTP 200");

            // Test POST /action JSON with escaped quotes and backslashes
            var jsonSayContent = new System.Net.Http.StringContent("{\"action\":\"say\",\"msg\":\"Hello \\\"world\\\"!\"}", System.Text.Encoding.UTF8, "application/json");
            var jsonSayResp = http.PostAsync(masterUrl + "/action", jsonSayContent).GetAwaiter().GetResult();
            AssertEqual((int)jsonSayResp.StatusCode, 200, "POST /action with JSON escaped quotes returns HTTP 200");
            string jsonSayBody = jsonSayResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(jsonSayBody.Contains("\"success\":true"), "POST /action with escaped quotes success is true");

            // Test POST /action JSON with backslash paths
            var jsonPathContent = new System.Net.Http.StringContent("{\"action\":\"say\",\"msg\":\"C:\\\\path\\\\to\\\\game\"}", System.Text.Encoding.UTF8, "application/json");
            var jsonPathResp = http.PostAsync(masterUrl + "/action", jsonPathContent).GetAwaiter().GetResult();
            AssertEqual((int)jsonPathResp.StatusCode, 200, "POST /action with JSON path backslash returns HTTP 200");

            // Test POST /action warp outside game (must report failure instead of false success)
            var warpContent = new System.Net.Http.StringContent("action=warp&x=10&y=20&z=30", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var warpResp = http.PostAsync(masterUrl + "/action", warpContent).GetAwaiter().GetResult();
            string warpBody = warpResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(warpBody.Contains("\"success\":false"), "POST /action (warp) reports success:false when not in-game");

            // Test POST /action warp_to outside game (must report failure)
            var warpToContent = new System.Net.Http.StringContent("action=warp_to&actor=999", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var warpToResp = http.PostAsync(masterUrl + "/action", warpToContent).GetAwaiter().GetResult();
            string warpToBody = warpToResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(warpToBody.Contains("\"success\":false"), "POST /action (warp_to) reports success:false when not in-game");

            // Test POST /action spawn outside game / empty DB (must report failure)
            var spawnContent = new System.Net.Http.StringContent("action=spawn&item=Piton", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var spawnResp = http.PostAsync(masterUrl + "/action", spawnContent).GetAwaiter().GetResult();
            string spawnBody = spawnResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(spawnBody.Contains("\"success\":false"), "POST /action (spawn) reports success:false when item DB empty/offline");

            // Test POST /action deliver outside game (must report failure)
            var deliverContent = new System.Net.Http.StringContent("action=deliver&actor=1&dest=Hands&item=Rope", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var deliverResp = http.PostAsync(masterUrl + "/action", deliverContent).GetAwaiter().GetResult();
            string deliverBody = deliverResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(deliverBody.Contains("\"success\":false"), "POST /action (deliver) reports success:false when offline/empty DB");

            // Test POST /action toggle with snake_case and value=1
            var toggleSnakeContent = new System.Net.Http.StringContent("action=toggle&cheat=inf_stamina&value=1", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var toggleSnakeResp = http.PostAsync(masterUrl + "/action", toggleSnakeContent).GetAwaiter().GetResult();
            AssertEqual((int)toggleSnakeResp.StatusCode, 200, "POST /action (toggle snake_case with value=1) returns HTTP 200");
            string toggleSnakeBody = toggleSnakeResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(toggleSnakeBody.Contains("\"enabled\":true"), "POST /action (toggle snake_case) enabled is true");

            // Test Concurrent POST requests to thread-safe action queue
            var concurrentTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>>();
            for (int i = 0; i < 10; i++)
            {
                var content = new System.Net.Http.StringContent($"action=say&msg=Concurrent_{i}", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                concurrentTasks.Add(http.PostAsync(masterUrl + "/action", content));
            }
            System.Threading.Tasks.Task.WaitAll(concurrentTasks.ToArray());
            for (int i = 0; i < 10; i++)
            {
                AssertEqual((int)concurrentTasks[i].Result.StatusCode, 200, $"Concurrent action {i} returns HTTP 200");
            }

            // Test POST /action perm global
            var permContent = new System.Net.Http.StringContent("action=perm&section=Movement&allow=true", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var permResp = http.PostAsync(masterUrl + "/action", permContent).GetAwaiter().GetResult();
            AssertEqual((int)permResp.StatusCode, 200, "POST /action (perm global allow=true) returns HTTP 200");
            string permBody = permResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(permBody.Contains("\"allow\":true"), "POST /action perm global allow is true");

            // Test POST /action perm global allow=0 (numeric bool)
            var perm0Content = new System.Net.Http.StringContent("action=perm&section=Movement&allow=0", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var perm0Resp = http.PostAsync(masterUrl + "/action", perm0Content).GetAwaiter().GetResult();
            AssertEqual((int)perm0Resp.StatusCode, 200, "POST /action (perm global allow=0) returns HTTP 200");
            string perm0Body = perm0Resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(perm0Body.Contains("\"allow\":false"), "POST /action perm global allow=0 sets allow to false");

            // Test POST /action perm per-slave override
            var permSlaveContent = new System.Net.Http.StringContent("action=perm&actor=2&section=Teleport&allow=1", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var permSlaveResp = http.PostAsync(masterUrl + "/action", permSlaveContent).GetAwaiter().GetResult();
            AssertEqual((int)permSlaveResp.StatusCode, 200, "POST /action (perm slave allow=1) returns HTTP 200");
            string permSlaveBody = permSlaveResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(permSlaveBody.Contains("\"allow\":true"), "POST /action perm slave allow=1 sets allow to true");

            // Test POST /action perm clear override
            var permClearContent = new System.Net.Http.StringContent("action=perm&actor=2&section=Teleport&allow=clear", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var permClearResp = http.PostAsync(masterUrl + "/action", permClearContent).GetAwaiter().GetResult();
            AssertEqual((int)permClearResp.StatusCode, 200, "POST /action (perm slave allow=clear) returns HTTP 200");

            // Test POST /action perm invalid section returns 400
            var permInvalidContent = new System.Net.Http.StringContent("action=perm&section=BogusSection", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var permInvalidResp = http.PostAsync(masterUrl + "/action", permInvalidContent).GetAwaiter().GetResult();
            AssertEqual((int)permInvalidResp.StatusCode, 400, "POST /action (perm invalid section) returns HTTP 400");

            // Test POST /action with malformed percent encoding does not throw
            var malformedPercentContent = new System.Net.Http.StringContent("action=say&msg=100% discount", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var malformedResp = http.PostAsync(masterUrl + "/action", malformedPercentContent).GetAwaiter().GetResult();
            AssertEqual((int)malformedResp.StatusCode, 200, "POST /action with raw % does not throw and returns HTTP 200");

            // Test POST /action with reflection snake_case cheat air_jump
            FieldInfo airJumpField = stateType.GetField("AirJump", BindingFlags.Public | BindingFlags.Static);
            bool initialAirJump = (bool)airJumpField.GetValue(null);
            var toggleAirJumpContent = new System.Net.Http.StringContent("action=toggle&cheat=air_jump&value=1", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var toggleAirJumpResp = http.PostAsync(masterUrl + "/action", toggleAirJumpContent).GetAwaiter().GetResult();
            AssertEqual((int)toggleAirJumpResp.StatusCode, 200, "POST /action (toggle air_jump) returns HTTP 200");
            AssertTrue((bool)airJumpField.GetValue(null), "State.AirJump toggled to true via reflection fallback");
            airJumpField.SetValue(null, initialAirJump);

            // Test POST /action with nested JSON coords
            var nestedWarpContent = new System.Net.Http.StringContent("{\"action\":\"warp\",\"coords\":{\"x\":15.5,\"y\":25.5,\"z\":35.5}}", System.Text.Encoding.UTF8, "application/json");
            var nestedWarpResp = http.PostAsync(masterUrl + "/action", nestedWarpContent).GetAwaiter().GetResult();
            AssertEqual((int)nestedWarpResp.StatusCode, 200, "POST /action with nested coords returns HTTP 200");
            string nestedWarpBody = nestedWarpResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(nestedWarpBody.Contains("\"success\":false"), "POST /action nested coords reports success:false when offline");

            // Test POST /action with NaN coords returns 400
            var nanWarpContent = new System.Net.Http.StringContent("action=warp&x=NaN&y=10&z=20", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var nanWarpResp = http.PostAsync(masterUrl + "/action", nanWarpContent).GetAwaiter().GetResult();
            AssertEqual((int)nanWarpResp.StatusCode, 400, "POST /action (warp with NaN coordinate) returns HTTP 400");

            // Test POST /action missing required param on say returns 400
            var missingMsgContent = new System.Net.Http.StringContent("action=say", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var missingMsgResp = http.PostAsync(masterUrl + "/action", missingMsgContent).GetAwaiter().GetResult();
            AssertEqual((int)missingMsgResp.StatusCode, 400, "POST /action (say without msg) returns HTTP 400");

            // Test POST /action deliver to non-existent remote actor returns false
            var deliverRemoteContent = new System.Net.Http.StringContent("action=deliver&actor=999&item=Piton", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var deliverRemoteResp = http.PostAsync(masterUrl + "/action", deliverRemoteContent).GetAwaiter().GetResult();
            AssertEqual((int)deliverRemoteResp.StatusCode, 200, "POST /action (deliver to remote actor) returns HTTP 200");
            string deliverRemoteBody = deliverRemoteResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(deliverRemoteBody.Contains("\"success\":false"), "POST /action (deliver to remote actor 999) reports success:false");

            // Test POST /action say with whitespace-only msg returns 400
            var whitespaceMsgContent = new System.Net.Http.StringContent("action=say&msg=   ", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var whitespaceMsgResp = http.PostAsync(masterUrl + "/action", whitespaceMsgContent).GetAwaiter().GetResult();
            AssertEqual((int)whitespaceMsgResp.StatusCode, 400, "POST /action (say with whitespace-only msg) returns HTTP 400");

            // Test POST /action say with 'text' alias returns HTTP 200
            var textAliasContent = new System.Net.Http.StringContent("action=say&text=CustomNotification", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var textAliasResp = http.PostAsync(masterUrl + "/action", textAliasContent).GetAwaiter().GetResult();
            AssertEqual((int)textAliasResp.StatusCode, 200, "POST /action (say with text alias) returns HTTP 200");
            string textAliasBody = textAliasResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(textAliasBody.Contains("CustomNotification") && textAliasBody.Contains("\"msg\":"), "POST /action (say text alias) echoes msg field");

            // Test POST /action from query string only (no entity body)
            var queryOnlyResp = http.PostAsync(masterUrl + "/action?action=say&msg=QueryOnly", new System.Net.Http.StringContent("")).GetAwaiter().GetResult();
            AssertEqual((int)queryOnlyResp.StatusCode, 200, "POST /action (query string only) returns HTTP 200");
            string queryOnlyBody = queryOnlyResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(queryOnlyBody.Contains("QueryOnly"), "POST /action (query string only) echoed message");

            // Test POST /action toggle with invalid boolean value returns 400
            var badBoolToggle = new System.Net.Http.StringContent("action=toggle&cheat=God&value=flase", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var badBoolResp = http.PostAsync(masterUrl + "/action", badBoolToggle).GetAwaiter().GetResult();
            AssertEqual((int)badBoolResp.StatusCode, 400, "POST /action (toggle with invalid bool) returns HTTP 400");

            // Test POST /action toggle cheat name with space (no fall)
            var spaceToggleContent = new System.Net.Http.StringContent("action=toggle&cheat=no fall", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var spaceToggleResp = http.PostAsync(masterUrl + "/action", spaceToggleContent).GetAwaiter().GetResult();
            AssertEqual((int)spaceToggleResp.StatusCode, 200, "POST /action (toggle cheat with space 'no fall') returns HTTP 200");
            FieldInfo noFallField = stateType.GetField("NoFall", BindingFlags.Public | BindingFlags.Static);
            AssertTrue((bool)noFallField.GetValue(null), "State.NoFall toggled to true");
            http.PostAsync(masterUrl + "/action", spaceToggleContent).GetAwaiter().GetResult(); // restore

            // Test POST /action toggle reflection cheat name with space (air jump)
            var spaceAirJumpContent = new System.Net.Http.StringContent("action=toggle&cheat=air jump", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var spaceAirJumpResp = http.PostAsync(masterUrl + "/action", spaceAirJumpContent).GetAwaiter().GetResult();
            AssertEqual((int)spaceAirJumpResp.StatusCode, 200, "POST /action (toggle cheat with space 'air jump') returns HTTP 200");
            AssertTrue((bool)airJumpField.GetValue(null), "State.AirJump toggled to true via space normalization");
            http.PostAsync(masterUrl + "/action", spaceAirJumpContent).GetAwaiter().GetResult(); // restore

            // Test POST /action perm with invalid actor returns 400
            var permInvalidActorContent = new System.Net.Http.StringContent("action=perm&actor=invalid_player&section=Movement&allow=true", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var permInvalidActorResp = http.PostAsync(masterUrl + "/action", permInvalidActorContent).GetAwaiter().GetResult();
            AssertEqual((int)permInvalidActorResp.StatusCode, 400, "POST /action (perm invalid actor) returns HTTP 400");

            // Test POST /action perm with invalid bool returns 400
            var permInvalidBoolContent = new System.Net.Http.StringContent("action=perm&section=Movement&allow=bad_bool", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var permInvalidBoolResp = http.PostAsync(masterUrl + "/action", permInvalidBoolContent).GetAwaiter().GetResult();
            AssertEqual((int)permInvalidBoolResp.StatusCode, 400, "POST /action (perm invalid bool) returns HTTP 400");

            // Test POST /action deliver with invalid actor returns 400
            var deliverInvalidActorContent = new System.Net.Http.StringContent("action=deliver&actor=invalid_player&item=Piton", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var deliverInvalidActorResp = http.PostAsync(masterUrl + "/action", deliverInvalidActorContent).GetAwaiter().GetResult();
            AssertEqual((int)deliverInvalidActorResp.StatusCode, 400, "POST /action (deliver invalid actor) returns HTTP 400");

            // Test POST /action warp with coords=x,y,z comma-separated
            var warpCoordsContent = new System.Net.Http.StringContent("action=warp&coords=12.5,22.5,32.5", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var warpCoordsResp = http.PostAsync(masterUrl + "/action", warpCoordsContent).GetAwaiter().GetResult();
            AssertEqual((int)warpCoordsResp.StatusCode, 200, "POST /action (warp coords=x,y,z) returns HTTP 200");
            string warpCoordsBody = warpCoordsResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(warpCoordsBody.Contains("\"success\":false"), "POST /action warp coords reports success:false when offline");

            // Test POST /action warp_to with invalid actor returns 400
            var warpToInvalidContent = new System.Net.Http.StringContent("action=warp_to&target=invalid_player", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var warpToInvalidResp = http.PostAsync(masterUrl + "/action", warpToInvalidContent).GetAwaiter().GetResult();
            AssertEqual((int)warpToInvalidResp.StatusCode, 400, "POST /action (warp_to invalid target) returns HTTP 400");

            // Test POST /action with UTF-8 BOM prefix
            var bomContent = new System.Net.Http.StringContent("\uFEFF{\"action\":\"say\",\"msg\":\"BOM test\"}", System.Text.Encoding.UTF8, "application/json");
            var bomResp = http.PostAsync(masterUrl + "/action", bomContent).GetAwaiter().GetResult();
            AssertEqual((int)bomResp.StatusCode, 200, "POST /action with UTF-8 BOM returns HTTP 200");

            // Test GET /action returns 405 Method Not Allowed
            var getActionResp = http.GetAsync(masterUrl + "/action").GetAwaiter().GetResult();
            AssertEqual((int)getActionResp.StatusCode, 405, "GET /action returns HTTP 405 Method Not Allowed");

            // Test GET /unknown_path returns 404 Not Found
            var notFoundResp = http.GetAsync(masterUrl + "/nonexistent").GetAwaiter().GetResult();
            AssertEqual((int)notFoundResp.StatusCode, 404, "GET /nonexistent returns HTTP 404");

            // Test error handling: missing action parameter
            var badContent = new System.Net.Http.StringContent("foo=bar", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var badResp = http.PostAsync(masterUrl + "/action", badContent).GetAwaiter().GetResult();
            AssertEqual((int)badResp.StatusCode, 400, "POST /action without 'action' returns HTTP 400");
        }

        // Test ToJson handles float.NaN and float.PositiveInfinity cleanly
        MethodInfo toJsonMethod = masterBridgeType.GetMethod("ToJson", BindingFlags.Public | BindingFlags.Static);
        string nanJson = (string)toJsonMethod.Invoke(null, new object[] { float.NaN });
        AssertEqual(nanJson, "0.0", "ToJson(float.NaN) returns valid JSON '0.0'");
        string infJson = (string)toJsonMethod.Invoke(null, new object[] { float.PositiveInfinity });
        AssertEqual(infJson, "0.0", "ToJson(float.PositiveInfinity) returns valid JSON '0.0'");

        // Test ToJson handles UnityEngine.Vector3 cleanly
        Type v3Type = typeof(UnityEngine.Vector3);
        object v3Obj = Activator.CreateInstance(v3Type, new object[] { 1.5f, 2.5f, 3.5f });
        string v3Json = (string)toJsonMethod.Invoke(null, new object[] { v3Obj });
        AssertTrue(v3Json.Contains("\"x\":1.5") && v3Json.Contains("\"y\":2.5") && v3Json.Contains("\"z\":3.5"), "ToJson(Vector3) serializes to proper JSON object");

        // Stop dispatcher and bridge
        dispatching = false;
        dispatcherThread.Join(500);
        stopMethod.Invoke(null, null);
        AssertEqual((bool)isRunningProp.GetValue(null), false, "DebugBridge.IsRunning is false after Stop()");
        AssertEqual(File.Exists(masterTempFile), false, "peak_debug_master.txt was deleted on Stop()");

        // 16. Port Collision & Auto-Increment Test
        var dummyListener = new System.Net.HttpListener();
        dummyListener.Prefixes.Add("http://127.0.0.1:18080/");
        dummyListener.Start();

        startMethod.Invoke(null, null);
        int incrementedPort = (int)activePortProp.GetValue(null);
        AssertEqual(incrementedPort, 18081, "Port auto-incremented from 18080 to 18081 when 18080 is occupied");
        stopMethod.Invoke(null, null);

        dummyListener.Stop();
        dummyListener.Close();

        // 17. Slave DebugBridge Default Port & Temp File Test
        MethodInfo slaveStartMethod = slaveBridgeType.GetMethod("Start", BindingFlags.Public | BindingFlags.Static);
        MethodInfo slaveStopMethod = slaveBridgeType.GetMethod("Stop", BindingFlags.Public | BindingFlags.Static);
        PropertyInfo slavePortProp = slaveBridgeType.GetProperty("ActivePort", BindingFlags.Public | BindingFlags.Static);
        PropertyInfo slaveUrlProp = slaveBridgeType.GetProperty("ActiveUrl", BindingFlags.Public | BindingFlags.Static);

        slaveStartMethod.Invoke(null, null);
        int slavePort = (int)slavePortProp.GetValue(null);
        AssertEqual(slavePort, 18081, "Slave binary defaults to port 18081");
        string slaveTempFile = Path.Combine(Path.GetTempPath(), "peak_debug_slave.txt");
        AssertTrue(File.Exists(slaveTempFile), "peak_debug_slave.txt was created in %TEMP%");
        string recordedSlaveUrl = File.ReadAllText(slaveTempFile).Trim();
        AssertEqual(recordedSlaveUrl, $"http://127.0.0.1:{slavePort}", "peak_debug_slave.txt records exact active URL");

        // Test Slave rejects POST /action perm
        MethodInfo slaveUpdateMethod = slaveBridgeType.GetMethod("Update", BindingFlags.Public | BindingFlags.Static);
        bool slaveDispatching = true;
        Thread slaveDispatcher = new Thread(() =>
        {
            while (slaveDispatching)
            {
                try { slaveUpdateMethod.Invoke(null, null); } catch {}
                Thread.Sleep(5);
            }
        });
        slaveDispatcher.IsBackground = true;
        slaveDispatcher.Start();

        using (var slaveHttp = new System.Net.Http.HttpClient())
        {
            var slavePermContent = new System.Net.Http.StringContent("action=perm&section=Movement&allow=true", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var slavePermResp = slaveHttp.PostAsync($"http://127.0.0.1:{slavePort}/action", slavePermContent).GetAwaiter().GetResult();
            AssertEqual((int)slavePermResp.StatusCode, 200, "POST /action on Slave returns HTTP 200");
            string slavePermBody = slavePermResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(slavePermBody.Contains("\"success\":false"), "POST /action (perm on Slave) rejects with success:false");

            // Test Slave rejects POST /action deliver to remote actor
            var slaveDeliverContent = new System.Net.Http.StringContent("action=deliver&actor=2&item=Piton", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            var slaveDeliverResp = slaveHttp.PostAsync($"http://127.0.0.1:{slavePort}/action", slaveDeliverContent).GetAwaiter().GetResult();
            AssertEqual((int)slaveDeliverResp.StatusCode, 200, "POST /action (deliver on Slave) returns HTTP 200");
            string slaveDeliverBody = slaveDeliverResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AssertTrue(slaveDeliverBody.Contains("\"success\":false"), "POST /action (deliver on Slave) rejects with success:false");
            AssertTrue(slaveDeliverBody.Contains("Only Master instance can deliver items to remote scouts"), "POST /action deliver on Slave specifies Master authority error");
        }

        slaveDispatching = false;
        slaveDispatcher.Join(500);

        slaveStopMethod.Invoke(null, null);
        AssertEqual(File.Exists(slaveTempFile), false, "peak_debug_slave.txt was deleted on Stop()");

        // 18. Multi-Instance Temp File Preservation Test
        startMethod.Invoke(null, null);
        string mUrl = (string)activeUrlProp.GetValue(null);
        AssertTrue(File.Exists(masterTempFile), "Master temp file created");
        // Simulate a second master instance starting and overwriting the temp file with another URL
        string simulatedSecondUrl = "http://127.0.0.1:18082";
        File.WriteAllText(masterTempFile, simulatedSecondUrl);
        // Stop first master instance
        stopMethod.Invoke(null, null);
        // Verify file was NOT deleted because it belongs to the second instance
        AssertTrue(File.Exists(masterTempFile), "Multi-instance: temp file preserved when overwritten by another instance");
        string remainingUrl = File.ReadAllText(masterTempFile).Trim();
        AssertEqual(remainingUrl, simulatedSecondUrl, "Preserved temp file retains second instance URL");
        // Clean up simulated file
        try { File.Delete(masterTempFile); } catch {}

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"==========================================================");
        Console.WriteLine($" [SUCCESS] All {assertions} assertions PASSED cleanly!");
        Console.WriteLine($"==========================================================");
        Console.ResetColor();
    }
}
