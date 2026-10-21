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
        string slaveDllPath = Path.Combine(workspaceRoot, "dist", "AscentS.dll");
        if (!File.Exists(slaveDllPath)) slaveDllPath = Path.Combine(workspaceRoot, "dist", "Summit_Slave.dll");
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

        // 9. Ascend 2.5 Rebrand & Version Verification
        Type kitType = masterAsm.GetType("Summit.Kit");
        FieldInfo versionField = kitType.GetField("Version", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        AssertEqual(versionField?.GetValue(null)?.ToString(), "2.5", "Kit.Version is '2.5'");

        Type pluginType = masterAsm.GetType("Summit.Plugin");
        var bepInAttr = pluginType.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "BepInPlugin");
        AssertTrue(bepInAttr != null, "BepInPlugin attribute exists on Plugin class");
        var nameProp = bepInAttr.GetType().GetProperty("Name");
        var verProp = bepInAttr.GetType().GetProperty("Version");
        AssertEqual(nameProp?.GetValue(bepInAttr)?.ToString(), "Ascend", "Plugin display name is 'Ascend'");
        AssertEqual(verProp?.GetValue(bepInAttr)?.ToString(), "2.5", "Plugin version is '2.5'");

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

        // 11b. Direct Join Master/Slave Methods & Shared File Verification
        FieldInfo sharedMasterFileField = masterModNet.GetField("SharedRoomFile", BindingFlags.Public | BindingFlags.Static);
        AssertTrue(sharedMasterFileField != null, "SharedRoomFile exists on ModNet");
        FieldInfo sharedSlaveFileField = masterModNet.GetField("SharedSlaveRoomFile", BindingFlags.Public | BindingFlags.Static);
        AssertTrue(sharedSlaveFileField != null, "SharedSlaveRoomFile exists on ModNet");

        MethodInfo getActiveMasterMethod = masterModNet.GetMethod("GetActiveMasterRoomInfo", BindingFlags.Public | BindingFlags.Static);
        AssertTrue(getActiveMasterMethod != null, "GetActiveMasterRoomInfo exists on ModNet");
        MethodInfo getActiveSlaveMethod = masterModNet.GetMethod("GetActiveSlaveRoomInfo", BindingFlags.Public | BindingFlags.Static);
        AssertTrue(getActiveSlaveMethod != null, "GetActiveSlaveRoomInfo exists on ModNet");

        MethodInfo joinMasterMethod = masterModNet.GetMethod("JoinMasterRoom", BindingFlags.Public | BindingFlags.Static);
        AssertTrue(joinMasterMethod != null, "JoinMasterRoom exists on ModNet");
        MethodInfo joinSlaveMethod = masterModNet.GetMethod("JoinSlaveRoom", BindingFlags.Public | BindingFlags.Static);
        AssertTrue(joinSlaveMethod != null, "JoinSlaveRoom exists on ModNet");

        string tempSlaveFile = (string)sharedSlaveFileField.GetValue(null);
        try
        {
            File.WriteAllText(tempSlaveFile, "SLAVE_ROOM_999\neu\nAirport\nFriendScout");
            string parsedInfo = (string)getActiveSlaveMethod.Invoke(null, null);
            AssertEqual(parsedInfo, "SLAVE_ROOM_999 (FriendScout)", "GetActiveSlaveRoomInfo formats room and scout name");
        }
        finally
        {
            if (File.Exists(tempSlaveFile)) File.Delete(tempSlaveFile);
        }
        AssertEqual((string)getActiveSlaveMethod.Invoke(null, null), null, "GetActiveSlaveRoomInfo returns null when file is absent");

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
        startMethod.Invoke(null, null);
        int portBeforeCollision = (int)activePortProp.GetValue(null);
        stopMethod.Invoke(null, null);

        var dummyListener = new System.Net.HttpListener();
        dummyListener.Prefixes.Add($"http://127.0.0.1:{portBeforeCollision}/");
        dummyListener.Start();

        startMethod.Invoke(null, null);
        int incrementedPort = (int)activePortProp.GetValue(null);
        AssertEqual(incrementedPort, portBeforeCollision + 1, $"Port auto-incremented from {portBeforeCollision} to {portBeforeCollision + 1} when {portBeforeCollision} is occupied");
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
        AssertTrue(slavePort >= 18081, $"Slave binary allocates port >= 18081 (got {slavePort})");
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
        string simulatedSecondUrl = "http://127.0.0.1:19999";
        File.WriteAllText(masterTempFile, simulatedSecondUrl);
        // Stop first master instance
        stopMethod.Invoke(null, null);
        // Verify file was NOT deleted because it belongs to the second instance
        AssertTrue(File.Exists(masterTempFile), "Multi-instance: temp file preserved when overwritten by another instance");
        string remainingUrl = File.ReadAllText(masterTempFile).Trim();
        AssertEqual(remainingUrl, simulatedSecondUrl, "Preserved temp file retains second instance URL");
        // Clean up simulated file
        try { File.Delete(masterTempFile); } catch {}

        // ==============================================================================
        // 19. Auto-Updater Verification (Summit.Updater)
        // ==============================================================================
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n>>> Testing Auto-Updater (Summit.Updater)...");
        Console.ResetColor();

        Type slaveUpdaterType = slaveAsm.GetType("Summit.Updater");
        Type masterUpdaterType = masterAsm.GetType("Summit.Updater");
        AssertTrue(slaveUpdaterType != null, "Updater type exists in Summit_Slave.dll");
        AssertTrue(masterUpdaterType != null, "Updater type exists in Summit_Master.dll");

        // Role check
        MethodInfo slaveIsSlaveMethod = slaveUpdaterType.GetMethod("IsSlaveRole", BindingFlags.Public | BindingFlags.Static);
        MethodInfo masterIsSlaveMethod = masterUpdaterType.GetMethod("IsSlaveRole", BindingFlags.Public | BindingFlags.Static);
        AssertTrue((bool)slaveIsSlaveMethod.Invoke(null, null), "Updater.IsSlaveRole() returns true on Slave mod");
        AssertEqual((bool)masterIsSlaveMethod.Invoke(null, null), false, "Updater.IsSlaveRole() returns false on Master mod");

        // Tag & Version Comparison Tests
        MethodInfo compareVersionsMethod = slaveUpdaterType.GetMethod("CompareVersions", BindingFlags.Public | BindingFlags.Static);
        MethodInfo isNewerMethod = slaveUpdaterType.GetMethod("IsNewerVersion", BindingFlags.Public | BindingFlags.Static);

        AssertTrue((int)compareVersionsMethod.Invoke(null, new object[] { "v1.2.0", "v1.1.1" }) > 0, "CompareVersions: v1.2.0 > v1.1.1");
        AssertTrue((int)compareVersionsMethod.Invoke(null, new object[] { "v1.1.1", "v1.2.0" }) < 0, "CompareVersions: v1.1.1 < v1.2.0");
        AssertEqual((int)compareVersionsMethod.Invoke(null, new object[] { "v2.0", "2.0" }), 0, "CompareVersions: v2.0 == 2.0");
        AssertEqual((int)compareVersionsMethod.Invoke(null, new object[] { "v2.0.0", "2.0" }), 0, "CompareVersions: v2.0.0 == 2.0");
        AssertTrue((int)compareVersionsMethod.Invoke(null, new object[] { "1.10.0", "1.9.0" }) > 0, "CompareVersions: 1.10.0 > 1.9.0 (semver numeric)");
        AssertTrue((int)compareVersionsMethod.Invoke(null, new object[] { "v2.1", "2.0" }) > 0, "CompareVersions: v2.1 > 2.0");
        AssertTrue((int)compareVersionsMethod.Invoke(null, new object[] { "v2.0.1", "2.0" }) > 0, "CompareVersions: v2.0.1 > 2.0");
        AssertEqual((int)compareVersionsMethod.Invoke(null, new object[] { "v1.2.0-beta.1", "1.2.0" }), 0, "CompareVersions: v1.2.0-beta.1 == 1.2.0 (ignores prerelease suffix)");
        AssertTrue((int)compareVersionsMethod.Invoke(null, new object[] { "", "2.0" }) < 0, "CompareVersions: empty < 2.0");
        AssertEqual((int)compareVersionsMethod.Invoke(null, new object[] { null, "" }), 0, "CompareVersions: null == empty");

        AssertTrue((bool)isNewerMethod.Invoke(null, new object[] { "v1.1.1", "v1.2.0" }), "IsNewerVersion: v1.2.0 is newer than v1.1.1");
        AssertEqual((bool)isNewerMethod.Invoke(null, new object[] { "v1.2.0", "v1.1.1" }), false, "IsNewerVersion: v1.1.1 is NOT newer than v1.2.0");
        AssertEqual((bool)isNewerMethod.Invoke(null, new object[] { "2.0", "v2.0" }), false, "IsNewerVersion: v2.0 is NOT newer than 2.0 (matching release)");
        AssertEqual((bool)isNewerMethod.Invoke(null, new object[] { "2.0", "v2.0.0" }), false, "IsNewerVersion: v2.0.0 is NOT newer than 2.0 (matching release)");
        AssertTrue((bool)isNewerMethod.Invoke(null, new object[] { "2.0", "v2.0.1" }), "IsNewerVersion: v2.0.1 is newer than 2.0 (semver patch)");
        AssertTrue((bool)isNewerMethod.Invoke(null, new object[] { "2.0", "v2.1.0" }), "IsNewerVersion: v2.1.0 is newer than 2.0 (semver minor)");
        AssertTrue((bool)isNewerMethod.Invoke(null, new object[] { "2.0", "v3.0.0" }), "IsNewerVersion: v3.0.0 is newer than 2.0 (semver major)");
        AssertTrue((bool)isNewerMethod.Invoke(null, new object[] { "1.9.0", "v1.10.0" }), "IsNewerVersion: v1.10.0 is newer than 1.9.0 (semver numeric 10 > 9)");
        AssertEqual((bool)isNewerMethod.Invoke(null, new object[] { "2.0", "" }), false, "IsNewerVersion: empty string returns false");
        AssertEqual((bool)isNewerMethod.Invoke(null, new object[] { "2.0", null }), false, "IsNewerVersion: null returns false");
        AssertEqual((bool)isNewerMethod.Invoke(null, new object[] { "2.0", "invalid-tag" }), false, "IsNewerVersion: non-numeric string returns false");

        // ParseReleaseTag Tests
        MethodInfo parseTagMethod = slaveUpdaterType.GetMethod("ParseReleaseTag", BindingFlags.Public | BindingFlags.Static);
        string sampleJson = "{\"tag_name\":\"v2.2.0\",\"name\":\"Ascend v2.2.0\",\"assets\":[]}";
        AssertEqual((string)parseTagMethod.Invoke(null, new object[] { sampleJson }), "v2.2.0", "ParseReleaseTag extracts tag name cleanly");
        string sampleJsonEscaped = "{\"tag_name\":\"v2.2.0\\/hotfix\",\"name\":\"Hotfix\"}";
        AssertEqual((string)parseTagMethod.Invoke(null, new object[] { sampleJsonEscaped }), "v2.2.0/hotfix", "ParseReleaseTag unescapes forward slashes");
        AssertEqual((string)parseTagMethod.Invoke(null, new object[] { "{\"name\":\"No Tag\"}" }), null, "ParseReleaseTag returns null if tag_name missing");
        AssertEqual((string)parseTagMethod.Invoke(null, new object[] { "" }), null, "ParseReleaseTag returns null on empty json");

        // Asset Matcher Tests
        MethodInfo resolveUrlMethod = slaveUpdaterType.GetMethod("ResolveDownloadUrl", BindingFlags.Public | BindingFlags.Static);
        
        // Asset Matcher Case 0: AscentS.dll present -> prioritizes AscentS.dll
        string jsonAscentS = "{\"tag_name\":\"v2.5.0\",\"assets\":[{\"name\":\"Summit_Master.dll\",\"browser_download_url\":\"https://github.com/releases/Summit_Master.dll\"},{\"name\":\"AscentS.dll\",\"browser_download_url\":\"https://github.com/releases/AscentS.dll\"}]}";
        string ascentSResolved = (string)resolveUrlMethod.Invoke(null, new object[] { jsonAscentS, null });
        AssertEqual(ascentSResolved, "https://github.com/releases/AscentS.dll", "Asset Matcher: Prioritizes AscentS.dll when present");

        // Asset Matcher Case 1: Both Summit_Master.dll and Summit_Slave.dll present -> resolves to Summit_Slave.dll
        string jsonDual = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"Summit_Master.dll\",\"browser_download_url\":\"https://github.com/releases/Summit_Master.dll\"},{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"https://github.com/releases/Summit_Slave.dll\"}]}";
        string dualResolved = (string)resolveUrlMethod.Invoke(null, new object[] { jsonDual, null });
        AssertEqual(dualResolved, "https://github.com/releases/Summit_Slave.dll", "Asset Matcher: Filters for Summit_Slave.dll when both Master and Slave present");

        // Asset Matcher Case 2: Only Summit.dll present -> resolves to Summit.dll
        string jsonSingle = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"Summit.dll\",\"browser_download_url\":\"https://github.com/releases/Summit.dll\"}]}";
        string singleResolved = (string)resolveUrlMethod.Invoke(null, new object[] { jsonSingle, null });
        AssertEqual(singleResolved, "https://github.com/releases/Summit.dll", "Asset Matcher: Falls back to Summit.dll when only Summit.dll present");

        // Asset Matcher Case 3: Both Summit.dll and Summit_Slave.dll present -> prioritizes Summit_Slave.dll
        string jsonBoth = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"Summit.dll\",\"browser_download_url\":\"https://github.com/releases/Summit.dll\"},{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"https://github.com/releases/Summit_Slave.dll\"}]}";
        string bothResolved = (string)resolveUrlMethod.Invoke(null, new object[] { jsonBoth, null });
        AssertEqual(bothResolved, "https://github.com/releases/Summit_Slave.dll", "Asset Matcher: Prioritizes Summit_Slave.dll over Summit.dll");

        // Asset Matcher Case 4: Only Summit_Master.dll present -> returns null (never installs master onto slave)
        string jsonMasterOnly = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"Summit_Master.dll\",\"browser_download_url\":\"https://github.com/releases/Summit_Master.dll\"}]}";
        string masterOnlyResolved = (string)resolveUrlMethod.Invoke(null, new object[] { jsonMasterOnly, null });
        AssertEqual(masterOnlyResolved, null, "Asset Matcher: Returns null when only Summit_Master.dll is present");

        // Asset Matcher Case 5: Escaped slashes in browser_download_url
        string jsonEscapedUrl = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"https:\\/\\/github.com\\/download\\/Summit_Slave.dll\"}]}";
        string escapedResolved = (string)resolveUrlMethod.Invoke(null, new object[] { jsonEscapedUrl, null });
        AssertEqual(escapedResolved, "https://github.com/download/Summit_Slave.dll", "Asset Matcher: Unescapes forward slashes in download URL");

        // Asset Matcher Case 6: Empty assets array -> returns null
        string jsonEmptyAssets = "{\"tag_name\":\"v2.1.0\",\"assets\":[]}";
        string emptyAssetsResolved = (string)resolveUrlMethod.Invoke(null, new object[] { jsonEmptyAssets, null });
        AssertEqual(emptyAssetsResolved, null, "Asset Matcher: Returns null for empty assets array");

        // Asset Matcher Case 7: Custom candidate names
        string jsonCustom = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"CustomMod.dll\",\"browser_download_url\":\"https://github.com/download/CustomMod.dll\"}]}";
        string customResolved = (string)resolveUrlMethod.Invoke(null, new object[] { jsonCustom, new string[] { "CustomMod.dll" } });
        AssertEqual(customResolved, "https://github.com/download/CustomMod.dll", "Asset Matcher: Supports custom candidate names");

        // File Replacement Simulation & Startup Cleanup Tests
        MethodInfo swapMethod = slaveUpdaterType.GetMethod("PerformFileSwap", BindingFlags.Public | BindingFlags.Static);
        MethodInfo cleanupMethod = slaveUpdaterType.GetMethod("CleanupOldFiles", BindingFlags.Public | BindingFlags.Static);

        string tempTestDir = Path.Combine(Path.GetTempPath(), "summit_updater_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempTestDir);

        string simActiveDll = Path.Combine(tempTestDir, "Summit.dll");
        string simDownload = Path.Combine(tempTestDir, "Summit.dll.download");
        string simOld = Path.Combine(tempTestDir, "Summit.dll.old");

        File.WriteAllText(simActiveDll, "ACTIVE_MOD_V2.0");
        File.WriteAllText(simDownload, "DOWNLOADED_MOD_V2.1");

        // Execute 3-step swap simulation
        bool swapResult = (bool)swapMethod.Invoke(null, new object[] { simActiveDll, simDownload });
        AssertTrue(swapResult, "PerformFileSwap returns true on successful swap");
        AssertEqual(File.Exists(simDownload), false, "PerformFileSwap: .download file no longer exists");
        AssertTrue(File.Exists(simOld), "PerformFileSwap: active was renamed to .old");
        AssertEqual(File.ReadAllText(simOld), "ACTIVE_MOD_V2.0", "PerformFileSwap: .old contains previous active content");
        AssertTrue(File.Exists(simActiveDll), "PerformFileSwap: new binary moved to active target Summit.dll");
        AssertEqual(File.ReadAllText(simActiveDll), "DOWNLOADED_MOD_V2.1", "PerformFileSwap: active Summit.dll contains new binary");

        // Startup Cleanup Simulation
        File.WriteAllText(simOld, "ORPHANED_OLD_FILE");
        File.WriteAllText(simDownload, "ORPHANED_DOWNLOAD_FILE");
        string simExtraOld = Path.Combine(tempTestDir, "Summit.20261019_120000.old");
        File.WriteAllText(simExtraOld, "EXTRA_OLD_FILE");

        cleanupMethod.Invoke(null, new object[] { simActiveDll });

        AssertEqual(File.Exists(simOld), false, "CleanupOldFiles: orphaned Summit.dll.old was removed");
        AssertEqual(File.Exists(simDownload), false, "CleanupOldFiles: orphaned Summit.dll.download was removed");
        AssertEqual(File.Exists(simExtraOld), false, "CleanupOldFiles: wildcard Summit*.old was removed");
        AssertTrue(File.Exists(simActiveDll), "CleanupOldFiles: active Summit.dll is preserved untouched");
        AssertEqual(File.ReadAllText(simActiveDll), "DOWNLOADED_MOD_V2.1", "CleanupOldFiles: active Summit.dll content unchanged");

        // Configuration Tests
        MethodInfo bindConfigMethod = slaveUpdaterType.GetMethod("Bind", BindingFlags.Public | BindingFlags.Static);
        PropertyInfo repoProp = slaveUpdaterType.GetProperty("GitHubRepo", BindingFlags.Public | BindingFlags.Static);
        MethodInfo getRepoMethod = slaveUpdaterType.GetMethod("GetRepoTarget", BindingFlags.Public | BindingFlags.Static);

        string testCfgPath = Path.Combine(tempTestDir, "test_config.cfg");
        var cfg = new BepInEx.Configuration.ConfigFile(testCfgPath, true);
        bindConfigMethod.Invoke(null, new object[] { cfg });

        var repoEntry = (BepInEx.Configuration.ConfigEntry<string>)repoProp.GetValue(null);
        AssertTrue(repoEntry != null, "GitHubRepo ConfigEntry exists after Bind");
        AssertEqual(repoEntry.Value, "mikudae/PEAK-Ascend", "GitHubRepo default value is 'mikudae/PEAK-Ascend'");
        AssertEqual((string)getRepoMethod.Invoke(null, null), "mikudae/PEAK-Ascend", "GetRepoTarget() returns default repo");

        // Test override
        repoEntry.Value = "testowner/testmod";
        cfg.Save();
        AssertEqual(repoEntry.Value, "testowner/testmod", "GitHubRepo can be overridden in-memory");
        AssertEqual((string)getRepoMethod.Invoke(null, null), "testowner/testmod", "GetRepoTarget() returns overridden repo");

        // Reload from disk to verify persistence
        var reloadedCfg = new BepInEx.Configuration.ConfigFile(testCfgPath, true);
        bindConfigMethod.Invoke(null, new object[] { reloadedCfg });
        var reloadedRepoEntry = (BepInEx.Configuration.ConfigEntry<string>)repoProp.GetValue(null);
        AssertEqual(reloadedRepoEntry.Value, "testowner/testmod", "GitHubRepo persists across ConfigFile reloads");

        // End-to-End Update Simulation & Silent Process Termination Test
        MethodInfo checkApplyMethod = slaveUpdaterType.GetMethod("CheckAndApplyUpdate", BindingFlags.Public | BindingFlags.Static);
        PropertyInfo termProp = slaveUpdaterType.GetProperty("TerminateProcess", BindingFlags.Public | BindingFlags.Static);

        Action originalTerminate = (Action)termProp.GetValue(null);
        bool processTerminationTriggered = false;
        termProp.SetValue(null, (Action)(() =>
        {
            processTerminationTriggered = true;
        }));

        int mockPort = 18096;
        var mockServer = new System.Net.HttpListener();
        mockServer.Prefixes.Add($"http://127.0.0.1:{mockPort}/");
        mockServer.Start();

        bool mockServerRunning = true;
        Thread mockServerThread = new Thread(() =>
        {
            while (mockServerRunning)
            {
                try
                {
                    var ctx = mockServer.GetContext();
                    string path = ctx.Request.Url.AbsolutePath;
                    if (path.Contains("/releases/latest"))
                    {
                        string respJson = "{\"tag_name\":\"v9.9.9\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"http://127.0.0.1:" + mockPort + "/download/Summit_Slave.dll\"}]}";
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(respJson);
                        ctx.Response.ContentType = "application/json";
                        ctx.Response.ContentLength64 = buf.Length;
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/download/Summit_Slave.dll"))
                    {
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes("NEW_BINARY_V9.9.9_CONTENT");
                        ctx.Response.ContentType = "application/octet-stream";
                        ctx.Response.ContentLength64 = buf.Length;
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                        ctx.Response.Close();
                    }
                }
                catch
                {
                    break;
                }
            }
        });
        mockServerThread.IsBackground = true;
        mockServerThread.Start();

        string e2eActiveDll = Path.Combine(tempTestDir, "E2E_Summit.dll");
        File.WriteAllText(e2eActiveDll, "OLD_BINARY_V2.0");

        bool updateApplied = (bool)checkApplyMethod.Invoke(null, new object[] { $"http://127.0.0.1:{mockPort}/releases/latest", e2eActiveDll, null });

        mockServerRunning = false;
        mockServer.Stop();
        mockServer.Close();
        mockServerThread.Join(500);

        AssertTrue(updateApplied, "CheckAndApplyUpdate applied new release v9.9.9");
        AssertTrue(processTerminationTriggered, "CheckAndApplyUpdate triggered silent process termination");
        AssertTrue(File.Exists(e2eActiveDll), "E2E target active DLL exists");
        AssertEqual(File.ReadAllText(e2eActiveDll), "NEW_BINARY_V9.9.9_CONTENT", "E2E target active DLL replaced with downloaded binary");
        AssertTrue(File.Exists(e2eActiveDll + ".old"), "E2E active DLL was renamed to .old");
        AssertEqual(File.ReadAllText(e2eActiveDll + ".old"), "OLD_BINARY_V2.0", "E2E .old contains previous binary");
        AssertEqual(File.Exists(e2eActiveDll + ".download"), false, "E2E .download temporary file was moved");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: Repository Normalization
        // ------------------------------------------------------------------------------
        MethodInfo normalizeRepoMethod = slaveUpdaterType.GetMethod("NormalizeRepository", BindingFlags.Public | BindingFlags.Static);
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "https://github.com/mikudae/PEAK-Ascend" }), "mikudae/PEAK-Ascend", "NormalizeRepository: https://github.com/owner/repo");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "https://github.com/mikudae/PEAK-Ascend/" }), "mikudae/PEAK-Ascend", "NormalizeRepository: trailing slash stripped");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "https://github.com/mikudae/PEAK-Ascend.git" }), "mikudae/PEAK-Ascend", "NormalizeRepository: .git stripped");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "github.com/mikudae/PEAK-Ascend" }), "mikudae/PEAK-Ascend", "NormalizeRepository: github.com prefix stripped");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "  mikudae/PEAK-Ascend  " }), "mikudae/PEAK-Ascend", "NormalizeRepository: whitespace trimmed");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "http://127.0.0.1:18096/releases/latest" }), "http://127.0.0.1:18096/releases/latest", "NormalizeRepository: custom mock URL preserved");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: Tag Prefixes (release-v2.1.0, Summit-2.1.0)
        // ------------------------------------------------------------------------------
        AssertTrue((int)compareVersionsMethod.Invoke(null, new object[] { "release-v2.1.0", "2.0" }) > 0, "CompareVersions: release-v2.1.0 > 2.0");
        AssertTrue((int)compareVersionsMethod.Invoke(null, new object[] { "Summit-2.1.0", "2.0" }) > 0, "CompareVersions: Summit-2.1.0 > 2.0");
        AssertTrue((bool)isNewerMethod.Invoke(null, new object[] { "2.0", "release-v2.1.0" }), "IsNewerVersion: release-v2.1.0 is newer than 2.0");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: Release Body containing 'assets' keyword & Malformed JSON
        // ------------------------------------------------------------------------------
        string jsonWithBodyAssets = "{\"tag_name\":\"v2.2.0\",\"body\":\"Download the assets here: [link](https://foo.bar)\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"https://github.com/download/Summit_Slave.dll\"}]}";
        string resolvedWithBodyAssets = (string)resolveUrlMethod.Invoke(null, new object[] { jsonWithBodyAssets, null });
        AssertEqual(resolvedWithBodyAssets, "https://github.com/download/Summit_Slave.dll", "Asset Matcher: Correctly extracts asset when body text contains 'assets'");

        MethodInfo parseAssetsMethod = slaveUpdaterType.GetMethod("ParseAssets", BindingFlags.Public | BindingFlags.Static);
        var malformedAssets = parseAssetsMethod.Invoke(null, new object[] { "{\"assets\": [{\"name\": \"" });
        AssertTrue(malformedAssets != null, "ParseAssets: handles malformed JSON without throwing ArgumentOutOfRangeException");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: Binary Validation (IsValidDownloadedBinary)
        // ------------------------------------------------------------------------------
        MethodInfo isValidBinaryMethod = slaveUpdaterType.GetMethod("IsValidDownloadedBinary", BindingFlags.Public | BindingFlags.Static);
        string emptyFilePath = Path.Combine(tempTestDir, "empty.dll");
        File.WriteAllBytes(emptyFilePath, new byte[0]);
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { emptyFilePath }), false, "IsValidDownloadedBinary: 0-byte file rejected");

        string htmlFilePath = Path.Combine(tempTestDir, "error.html");
        File.WriteAllText(htmlFilePath, "<!DOCTYPE html><html><body>Error 404</body></html>");
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { htmlFilePath }), false, "IsValidDownloadedBinary: HTML error page rejected");

        string jsonErrPath = Path.Combine(tempTestDir, "error.json");
        File.WriteAllText(jsonErrPath, "{\"message\": \"Not Found\", \"documentation_url\": \"...\"}");
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { jsonErrPath }), false, "IsValidDownloadedBinary: JSON error payload rejected");

        string validBinPath = Path.Combine(tempTestDir, "valid.dll");
        File.WriteAllText(validBinPath, "NEW_VALID_BINARY_BYTES");
        AssertTrue((bool)isValidBinaryMethod.Invoke(null, new object[] { validBinPath }), "IsValidDownloadedBinary: Valid content accepted");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: 0-byte Download Swap Rejection
        // ------------------------------------------------------------------------------
        string activeCorruptTest = Path.Combine(tempTestDir, "CorruptTest_Summit.dll");
        File.WriteAllText(activeCorruptTest, "ORIGINAL_ACTIVE_BINARY");
        bool corruptSwapThrew = false;
        try
        {
            swapMethod.Invoke(null, new object[] { activeCorruptTest, emptyFilePath });
        }
        catch (TargetInvocationException tie) when (tie.InnerException is InvalidDataException)
        {
            corruptSwapThrew = true;
        }
        AssertTrue(corruptSwapThrew, "PerformFileSwap: Throws InvalidDataException on 0-byte download");
        AssertEqual(File.ReadAllText(activeCorruptTest), "ORIGINAL_ACTIVE_BINARY", "PerformFileSwap: Active DLL untouched when download is 0-byte");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: Windows NTFS Open-File Handle Simulation
        // ------------------------------------------------------------------------------
        string lockedActiveDll = Path.Combine(tempTestDir, "Locked_Summit.dll");
        string lockedDownload = Path.Combine(tempTestDir, "Locked_Summit.dll.download");
        File.WriteAllText(lockedActiveDll, "ORIGINAL_LOCKED_CONTENT");
        File.WriteAllText(lockedDownload, "NEW_LOCKED_CONTENT");

        // Hold file handle open with FileShare.Read | FileShare.Delete (mimicking Windows PE loader)
        using (FileStream lockedFs = new FileStream(lockedActiveDll, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
        {
            bool lockedSwapResult = (bool)swapMethod.Invoke(null, new object[] { lockedActiveDll, lockedDownload });
            AssertTrue(lockedSwapResult, "PerformFileSwap: Swap succeeded while active DLL has open FileShare.Delete handle");
            AssertTrue(File.Exists(lockedActiveDll), "PerformFileSwap: Active DLL exists after locked swap");
            AssertEqual(File.ReadAllText(lockedActiveDll), "NEW_LOCKED_CONTENT", "PerformFileSwap: Active DLL replaced with new content while old handle was open");
            AssertTrue(File.Exists(lockedActiveDll + ".old"), "PerformFileSwap: .old exists while file handle was open");
        }

        // Now file handle is closed, verify CleanupOldFiles removes the .old file
        cleanupMethod.Invoke(null, new object[] { lockedActiveDll });
        AssertEqual(File.Exists(lockedActiveDll + ".old"), false, "CleanupOldFiles: Removes .old file after handle closed");
        AssertEqual(File.ReadAllText(lockedActiveDll), "NEW_LOCKED_CONTENT", "CleanupOldFiles: Active DLL content preserved after cleanup");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: Collision Handling on .old file
        // ------------------------------------------------------------------------------
        string collideActiveDll = Path.Combine(tempTestDir, "Collide_Summit.dll");
        string collideDownload = Path.Combine(tempTestDir, "Collide_Summit.dll.download");
        string collideOld = collideActiveDll + ".old";
        File.WriteAllText(collideActiveDll, "ACTIVE_COLLIDE");
        File.WriteAllText(collideDownload, "DOWNLOAD_COLLIDE");
        File.WriteAllText(collideOld, "EXISTING_LOCKED_OLD");

        using (FileStream lockedOldFs = new FileStream(collideOld, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
        {
            bool collideSwapResult = (bool)swapMethod.Invoke(null, new object[] { collideActiveDll, collideDownload });
            AssertTrue(collideSwapResult, "PerformFileSwap: Succeeded even when existing .old cannot be deleted");
            AssertEqual(File.ReadAllText(collideActiveDll), "DOWNLOAD_COLLIDE", "PerformFileSwap: Active DLL updated under .old collision");
        }
        cleanupMethod.Invoke(null, new object[] { collideActiveDll });

        // ------------------------------------------------------------------------------
        // Adversarial Tests: HTTP 403, 404, and HTML download response handling
        // ------------------------------------------------------------------------------
        int errPort = 18097;
        var errServer = new System.Net.HttpListener();
        errServer.Prefixes.Add($"http://127.0.0.1:{errPort}/");
        errServer.Start();

        bool errServerRunning = true;
        Thread errServerThread = new Thread(() =>
        {
            while (errServerRunning)
            {
                try
                {
                    var ctx = errServer.GetContext();
                    string path = ctx.Request.Url.AbsolutePath;
                    if (path.Contains("/403_rate_limit"))
                    {
                        ctx.Response.StatusCode = 403;
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes("{\"message\":\"API rate limit exceeded\"}");
                        ctx.Response.ContentType = "application/json";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/404_not_found"))
                    {
                        ctx.Response.StatusCode = 404;
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes("{\"message\":\"Not Found\"}");
                        ctx.Response.ContentType = "application/json";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/html_release"))
                    {
                        string respJson = "{\"tag_name\":\"v9.9.9\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"http://127.0.0.1:" + errPort + "/download/html_error\"}]}";
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(respJson);
                        ctx.Response.ContentType = "application/json";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/download/html_error"))
                    {
                        ctx.Response.StatusCode = 200;
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes("<!DOCTYPE html><html><body>Error</body></html>");
                        ctx.Response.ContentType = "text/html";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                        ctx.Response.Close();
                    }
                }
                catch
                {
                    break;
                }
            }
        });
        errServerThread.IsBackground = true;
        errServerThread.Start();

        string errActiveDll = Path.Combine(tempTestDir, "Err_Summit.dll");
        File.WriteAllText(errActiveDll, "ORIGINAL_ERR_ACTIVE");

        bool termCalled = false;
        termProp.SetValue(null, (Action)(() => { termCalled = true; }));

        // Test 403 Forbidden
        bool res403 = (bool)checkApplyMethod.Invoke(null, new object[] { $"http://127.0.0.1:{errPort}/403_rate_limit", errActiveDll, null });
        AssertEqual(res403, false, "CheckAndApplyUpdate: Returns false on HTTP 403 Rate Limit");
        AssertEqual(termCalled, false, "CheckAndApplyUpdate: Terminate not called on HTTP 403");
        AssertEqual(File.ReadAllText(errActiveDll), "ORIGINAL_ERR_ACTIVE", "CheckAndApplyUpdate: Active DLL preserved on HTTP 403");

        // Test 404 Not Found
        bool res404 = (bool)checkApplyMethod.Invoke(null, new object[] { $"http://127.0.0.1:{errPort}/404_not_found", errActiveDll, null });
        AssertEqual(res404, false, "CheckAndApplyUpdate: Returns false on HTTP 404 Not Found");
        AssertEqual(termCalled, false, "CheckAndApplyUpdate: Terminate not called on HTTP 404");
        AssertEqual(File.ReadAllText(errActiveDll), "ORIGINAL_ERR_ACTIVE", "CheckAndApplyUpdate: Active DLL preserved on HTTP 404");

        // Test HTML payload on download endpoint
        bool resHtml = (bool)checkApplyMethod.Invoke(null, new object[] { $"http://127.0.0.1:{errPort}/html_release", errActiveDll, null });
        AssertEqual(resHtml, false, "CheckAndApplyUpdate: Returns false when download payload is HTML error");
        AssertEqual(termCalled, false, "CheckAndApplyUpdate: Terminate not called on HTML download error");
        AssertEqual(File.ReadAllText(errActiveDll), "ORIGINAL_ERR_ACTIVE", "CheckAndApplyUpdate: Active DLL preserved when download payload is HTML");

        errServerRunning = false;
        errServer.Stop();
        errServer.Close();
        errServerThread.Join(500);

        // ------------------------------------------------------------------------------
        // Adversarial Tests: False Tag Matching from Properties Preceding tag_name
        // ------------------------------------------------------------------------------
        string jsonPrecedingTag = "{\"name\":\"Release with \\\"tag_name\\\": \\\"v0.0.1\\\" in title\",\"tag_name\":\"v3.5.0\",\"assets\":[]}";
        AssertEqual((string)parseTagMethod.Invoke(null, new object[] { jsonPrecedingTag }), "v3.5.0", "ParseReleaseTag ignores embedded 'tag_name' in preceding 'name' field");

        string jsonBodyBlock = "{\"body\":\"```json\\n{\\\"tag_name\\\": \\\"v0.1.0\\\"}\\n```\",\"tag_name\":\"v4.0.0\"}";
        AssertEqual((string)parseTagMethod.Invoke(null, new object[] { jsonBodyBlock }), "v4.0.0", "ParseReleaseTag ignores fake tag_name inside markdown code block");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: S3 XML and Plain-text Error Payloads (IsValidDownloadedBinary)
        // ------------------------------------------------------------------------------
        string s3XmlPath1 = Path.Combine(tempTestDir, "s3_err1.xml");
        File.WriteAllText(s3XmlPath1, "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Error><Code>NoSuchKey</Code></Error>");
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { s3XmlPath1 }), false, "IsValidDownloadedBinary: S3 <?xml error rejected");

        string s3XmlPath2 = Path.Combine(tempTestDir, "s3_err2.xml");
        File.WriteAllText(s3XmlPath2, "<Error><Code>AccessDenied</Code><Message>Access Denied</Message></Error>");
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { s3XmlPath2 }), false, "IsValidDownloadedBinary: S3 <Error> payload rejected");

        string plainTextErrPath = Path.Combine(tempTestDir, "gateway_error.txt");
        File.WriteAllText(plainTextErrPath, "502 Bad Gateway: Host unreachable");
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { plainTextErrPath }), false, "IsValidDownloadedBinary: 502 Bad Gateway text rejected");

        string peBinaryPath = Path.Combine(tempTestDir, "real_pe.dll");
        byte[] peBytes = new byte[512];
        peBytes[0] = 0x4D; // 'M'
        peBytes[1] = 0x5A; // 'Z'
        File.WriteAllBytes(peBinaryPath, peBytes);
        AssertTrue((bool)isValidBinaryMethod.Invoke(null, new object[] { peBinaryPath }), "IsValidDownloadedBinary: Real PE binary with MZ header accepted");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: SSH URL in NormalizeRepository
        // ------------------------------------------------------------------------------
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "git@github.com:owner/repo.git" }), "owner/repo", "NormalizeRepository: git@github.com: SSH URL normalized");

        // ------------------------------------------------------------------------------
        // Adversarial Tests: GitHub HTTP 302 and 307 Download Redirections (S3 / CDN Simulation)
        // ------------------------------------------------------------------------------
        int redirPort = 18098;
        var redirServer = new System.Net.HttpListener();
        redirServer.Prefixes.Add($"http://127.0.0.1:{redirPort}/");
        redirServer.Start();

        bool redirServerRunning = true;
        Thread redirServerThread = new Thread(() =>
        {
            while (redirServerRunning)
            {
                try
                {
                    var ctx = redirServer.GetContext();
                    string path = ctx.Request.Url.AbsolutePath;

                    if (path.Contains("/releases/latest_redirect_abs"))
                    {
                        string respJson = "{\"tag_name\":\"v10.0.0\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"http://127.0.0.1:" + redirPort + "/redirect_step1\"}]}";
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(respJson);
                        ctx.Response.ContentType = "application/json";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/redirect_step1"))
                    {
                        // Simulate GitHub 302 Found redirect to S3 / CDN with absolute URL
                        ctx.Response.StatusCode = 302;
                        ctx.Response.Headers["Location"] = "http://127.0.0.1:" + redirPort + "/redirect_final_s3";
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/redirect_final_s3"))
                    {
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes("NEW_BINARY_VIA_302_REDIRECT");
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "application/octet-stream";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/releases/latest_redirect_rel"))
                    {
                        string respJson = "{\"tag_name\":\"v10.0.1\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"http://127.0.0.1:" + redirPort + "/redirect_rel_step\"}]}";
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(respJson);
                        ctx.Response.ContentType = "application/json";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/redirect_rel_step"))
                    {
                        // Simulate 307 Temporary Redirect with relative location
                        ctx.Response.StatusCode = 307;
                        ctx.Response.Headers["Location"] = "/redirect_rel_target";
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/redirect_rel_target"))
                    {
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes("NEW_BINARY_VIA_307_RELATIVE_REDIRECT");
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "application/octet-stream";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                        ctx.Response.Close();
                    }
                }
                catch
                {
                    break;
                }
            }
        });
        redirServerThread.IsBackground = true;
        redirServerThread.Start();

        string redirActiveDll = Path.Combine(tempTestDir, "Redir_Summit.dll");
        File.WriteAllText(redirActiveDll, "ACTIVE_PRE_REDIR_V2.0");

        bool redirTermCalled = false;
        termProp.SetValue(null, (Action)(() => { redirTermCalled = true; }));

        // Test 302 Absolute Redirect
        bool redirApplied1 = (bool)checkApplyMethod.Invoke(null, new object[] { $"http://127.0.0.1:{redirPort}/releases/latest_redirect_abs", redirActiveDll, null });
        AssertTrue(redirApplied1, "CheckAndApplyUpdate successfully follows HTTP 302 redirect to final binary");
        AssertTrue(redirTermCalled, "CheckAndApplyUpdate terminates process on 302 redirect update");
        AssertEqual(File.ReadAllText(redirActiveDll), "NEW_BINARY_VIA_302_REDIRECT", "Active DLL updated with content from 302 redirect target");

        // Test 307 Relative Redirect
        redirTermCalled = false;
        bool redirApplied2 = (bool)checkApplyMethod.Invoke(null, new object[] { $"http://127.0.0.1:{redirPort}/releases/latest_redirect_rel", redirActiveDll, null });
        AssertTrue(redirApplied2, "CheckAndApplyUpdate successfully follows HTTP 307 relative redirect");
        AssertTrue(redirTermCalled, "CheckAndApplyUpdate terminates process on 307 redirect update");
        AssertEqual(File.ReadAllText(redirActiveDll), "NEW_BINARY_VIA_307_RELATIVE_REDIRECT", "Active DLL updated with content from 307 relative redirect target");

        redirServerRunning = false;
        redirServer.Stop();
        redirServer.Close();
        redirServerThread.Join(500);

        // ------------------------------------------------------------------------------
        // Adversarial Tests: Concurrency Reentrancy Guard Stress Test
        // ------------------------------------------------------------------------------
        int concurrentPort = 18099;
        var concurrentServer = new System.Net.HttpListener();
        concurrentServer.Prefixes.Add($"http://127.0.0.1:{concurrentPort}/");
        concurrentServer.Start();

        bool concurrentServerRunning = true;
        Thread concurrentServerThread = new Thread(() =>
        {
            while (concurrentServerRunning)
            {
                try
                {
                    var ctx = concurrentServer.GetContext();
                    string path = ctx.Request.Url.AbsolutePath;
                    if (path.Contains("/releases/latest"))
                    {
                        string respJson = "{\"tag_name\":\"v11.0.0\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"http://127.0.0.1:" + concurrentPort + "/download/binary\"}]}";
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(respJson);
                        ctx.Response.ContentType = "application/json";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/download/binary"))
                    {
                        Thread.Sleep(50);
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes("CONCURRENT_TEST_BINARY");
                        ctx.Response.ContentType = "application/octet-stream";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                        ctx.Response.Close();
                    }
                }
                catch { break; }
            }
        });
        concurrentServerThread.IsBackground = true;
        concurrentServerThread.Start();

        string concurrentActiveDll = Path.Combine(tempTestDir, "Concurrent_Summit.dll");
        File.WriteAllText(concurrentActiveDll, "ORIGINAL_CONCURRENT_ACTIVE");

        termProp.SetValue(null, (Action)(() => { }));

        int completedSuccessCount = 0;
        int threadsCount = 5;
        Thread[] workers = new Thread[threadsCount];
        for (int i = 0; i < threadsCount; i++)
        {
            workers[i] = new Thread(() =>
            {
                try
                {
                    bool ok = (bool)checkApplyMethod.Invoke(null, new object[] { $"http://127.0.0.1:{concurrentPort}/releases/latest", concurrentActiveDll, null });
                    if (ok)
                    {
                        Interlocked.Increment(ref completedSuccessCount);
                    }
                }
                catch { }
            });
        }

        for (int i = 0; i < threadsCount; i++) workers[i].Start();
        for (int i = 0; i < threadsCount; i++) workers[i].Join();

        AssertTrue(completedSuccessCount >= 1, "Concurrency test: At least one thread completed the update successfully");
        AssertTrue(completedSuccessCount < threadsCount, "Concurrency test: Reentrancy guard prevented concurrent race collisions");
        AssertEqual(File.ReadAllText(concurrentActiveDll), "CONCURRENT_TEST_BINARY", "Concurrency test: Active DLL consistently updated without corruption");

        concurrentServerRunning = false;
        concurrentServer.Stop();
        concurrentServer.Close();
        concurrentServerThread.Join(500);

        // ------------------------------------------------------------------------------
        // Adversarial Tests: Cross-Name Startup Cleanup (Summit_Slave.dll cleans Summit.dll.old)
        // ------------------------------------------------------------------------------
        string crossActiveDll = Path.Combine(tempTestDir, "Summit_Slave.dll");
        File.WriteAllText(crossActiveDll, "ACTIVE_SLAVE_DLL");
        string orphanedSummitOld = Path.Combine(tempTestDir, "Summit.dll.old");
        string orphanedSummitDownload = Path.Combine(tempTestDir, "Summit.dll.download");
        File.WriteAllText(orphanedSummitOld, "ORPHANED_SUMMIT_OLD");
        File.WriteAllText(orphanedSummitDownload, "ORPHANED_SUMMIT_DOWNLOAD");

        cleanupMethod.Invoke(null, new object[] { crossActiveDll });
        AssertEqual(File.Exists(orphanedSummitOld), false, "Cross-Name Cleanup: Summit.dll.old removed when active is Summit_Slave.dll");
        AssertEqual(File.Exists(orphanedSummitDownload), false, "Cross-Name Cleanup: Summit.dll.download removed when active is Summit_Slave.dll");
        AssertTrue(File.Exists(crossActiveDll), "Cross-Name Cleanup: Active Summit_Slave.dll preserved");

        // ------------------------------------------------------------------------------
        // Adversarial Tests (Round 3): Nested Property Rejection
        // ------------------------------------------------------------------------------
        string jsonNestedObjectTag = "{\"name\":\"Release\",\"author\":{\"login\":\"octocat\",\"tag_name\":\"v9.9.9\"}}";
        AssertEqual((string)parseTagMethod.Invoke(null, new object[] { jsonNestedObjectTag }), null, "ParseReleaseTag ignores tag_name inside nested object");

        string jsonBodyMentionOnly = "{\"name\":\"Release Notice\",\"body\":\"Changelog for tag_name: v8.8.8\"}";
        AssertEqual((string)parseTagMethod.Invoke(null, new object[] { jsonBodyMentionOnly }), null, "ParseReleaseTag ignores tag_name in body when no root tag exists");

        string jsonPrecedingPrimitives = "{\"id\":456,\"prerelease\":false,\"tag_name\":\"v2.5.0\"}";
        AssertEqual((string)parseTagMethod.Invoke(null, new object[] { jsonPrecedingPrimitives }), "v2.5.0", "ParseReleaseTag parses tag_name following numeric and boolean primitives");

        // ------------------------------------------------------------------------------
        // Adversarial Tests (Round 3): Path Traversal in Asset Name and Malicious URL Schemes
        // ------------------------------------------------------------------------------
        string jsonPathTraversal1 = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"../../Summit_Slave.dll\",\"browser_download_url\":\"https://github.com/releases/Summit_Slave.dll\"}]}";
        AssertEqual((string)resolveUrlMethod.Invoke(null, new object[] { jsonPathTraversal1, null }), null, "Asset Matcher: Rejects forward-slash path traversal in asset name");

        string jsonPathTraversal2 = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"..\\\\..\\\\Summit_Slave.dll\",\"browser_download_url\":\"https://github.com/releases/Summit_Slave.dll\"}]}";
        AssertEqual((string)resolveUrlMethod.Invoke(null, new object[] { jsonPathTraversal2, null }), null, "Asset Matcher: Rejects back-slash path traversal in asset name");

        string jsonMaliciousUrlFile = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"file:///C:/Windows/win.ini\"}]}";
        AssertEqual((string)resolveUrlMethod.Invoke(null, new object[] { jsonMaliciousUrlFile, null }), null, "Asset Matcher: Rejects non-HTTP file:// URL scheme");

        string jsonMaliciousUrlJs = "{\"tag_name\":\"v2.1.0\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"javascript:alert(1)\"}]}";
        AssertEqual((string)resolveUrlMethod.Invoke(null, new object[] { jsonMaliciousUrlJs, null }), null, "Asset Matcher: Rejects javascript: URL scheme");

        // ------------------------------------------------------------------------------
        // Adversarial Tests (Round 3): NormalizeRepository Quotes, Spaces, Fragments, and Unicode
        // ------------------------------------------------------------------------------
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "\"mikudae/PEAK-Ascend\"" }), "mikudae/PEAK-Ascend", "NormalizeRepository: Strips surrounding double quotes");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "'mikudae/PEAK-Ascend'" }), "mikudae/PEAK-Ascend", "NormalizeRepository: Strips surrounding single quotes");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { " mikudae / PEAK-Ascend " }), "mikudae/PEAK-Ascend", "NormalizeRepository: Trims spaces around repository slashes");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "mikudae/PEAK-Ascend#readme" }), "mikudae/PEAK-Ascend", "NormalizeRepository: Strips URL fragment hash");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "https://github.com/mikudae/PEAK-Ascend/releases/latest" }), "mikudae/PEAK-Ascend", "NormalizeRepository: Extracts owner/repo from releases/latest path");
        AssertEqual((string)normalizeRepoMethod.Invoke(null, new object[] { "test-owner/모드-repo" }), "test-owner/모드-repo", "NormalizeRepository: Preserves Unicode characters in repo name");

        // ------------------------------------------------------------------------------
        // Adversarial Tests (Round 3): Binary Validation against ZIP/GZIP and Proxy Errors
        // ------------------------------------------------------------------------------
        string zipFilePath = Path.Combine(tempTestDir, "test.zip");
        File.WriteAllBytes(zipFilePath, new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00 });
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { zipFilePath }), false, "IsValidDownloadedBinary: PK ZIP archive rejected");

        string gzipFilePath = Path.Combine(tempTestDir, "test.gz");
        File.WriteAllBytes(gzipFilePath, new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 });
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { gzipFilePath }), false, "IsValidDownloadedBinary: GZIP compressed archive rejected");

        string svcUnavailPath = Path.Combine(tempTestDir, "503.txt");
        File.WriteAllText(svcUnavailPath, "Service Unavailable: Server overloaded");
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { svcUnavailPath }), false, "IsValidDownloadedBinary: Service Unavailable text rejected");

        string upstreamErrPath = Path.Combine(tempTestDir, "upstream.txt");
        File.WriteAllText(upstreamErrPath, "upstream connect error or disconnect/reset before headers");
        AssertEqual((bool)isValidBinaryMethod.Invoke(null, new object[] { upstreamErrPath }), false, "IsValidDownloadedBinary: Upstream proxy connect error rejected");

        // ------------------------------------------------------------------------------
        // Adversarial Tests (Round 3): Ascend Startup Cleanup
        // ------------------------------------------------------------------------------
        string orphanedAscendOld = Path.Combine(tempTestDir, "Ascend.dll.old");
        string orphanedAscendDownload = Path.Combine(tempTestDir, "Ascend.dll.download");
        File.WriteAllText(orphanedAscendOld, "ORPHANED_ASCEND_OLD");
        File.WriteAllText(orphanedAscendDownload, "ORPHANED_ASCEND_DOWNLOAD");

        cleanupMethod.Invoke(null, new object[] { crossActiveDll });
        AssertEqual(File.Exists(orphanedAscendOld), false, "Startup Cleanup: Ascend.dll.old removed");
        AssertEqual(File.Exists(orphanedAscendDownload), false, "Startup Cleanup: Ascend.dll.download removed");

        // ------------------------------------------------------------------------------
        // Adversarial Tests (Round 3): Absolute Path Resolution
        // ------------------------------------------------------------------------------
        MethodInfo getActiveDllMethod = slaveUpdaterType.GetMethod("GetActiveDllPath", BindingFlags.Public | BindingFlags.Static);
        string activeDllResolved = (string)getActiveDllMethod.Invoke(null, null);
        AssertTrue(!string.IsNullOrEmpty(activeDllResolved), "GetActiveDllPath returns non-empty path");
        AssertTrue(Path.IsPathRooted(activeDllResolved), "GetActiveDllPath returns fully rooted absolute path");

        // ------------------------------------------------------------------------------
        // Adversarial Tests (Round 3): Non-HTTP Redirect Scheme Protection
        // ------------------------------------------------------------------------------
        int nonHttpRedirPort = 18101;
        var nonHttpServer = new System.Net.HttpListener();
        nonHttpServer.Prefixes.Add($"http://127.0.0.1:{nonHttpRedirPort}/");
        nonHttpServer.Start();
        bool nonHttpRunning = true;
        Thread nonHttpThread = new Thread(() =>
        {
            while (nonHttpRunning)
            {
                try
                {
                    var ctx = nonHttpServer.GetContext();
                    string path = ctx.Request.Url.AbsolutePath;
                    if (path.Contains("/releases/latest"))
                    {
                        string respJson = "{\"tag_name\":\"v12.0.0\",\"assets\":[{\"name\":\"Summit_Slave.dll\",\"browser_download_url\":\"http://127.0.0.1:" + nonHttpRedirPort + "/download/malicious_redirect\"}]}";
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(respJson);
                        ctx.Response.ContentType = "application/json";
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        ctx.Response.Close();
                    }
                    else if (path.Contains("/download/malicious_redirect"))
                    {
                        // Redirect to non-HTTP URI scheme (e.g. file://)
                        ctx.Response.StatusCode = 302;
                        ctx.Response.Headers["Location"] = "file:///C:/Windows/win.ini";
                        ctx.Response.Close();
                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                        ctx.Response.Close();
                    }
                }
                catch { break; }
            }
        });
        nonHttpThread.IsBackground = true;
        nonHttpThread.Start();

        string maliciousRedirDll = Path.Combine(tempTestDir, "Malicious_Summit.dll");
        File.WriteAllText(maliciousRedirDll, "ACTIVE_MOD_BEFORE_MALICIOUS_REDIRECT");

        bool nonHttpTermCalled = false;
        termProp.SetValue(null, (Action)(() => { nonHttpTermCalled = true; }));

        bool maliciousApplied = (bool)checkApplyMethod.Invoke(null, new object[] { $"http://127.0.0.1:{nonHttpRedirPort}/releases/latest", maliciousRedirDll, null });
        AssertEqual(maliciousApplied, false, "CheckAndApplyUpdate rejects redirect to non-HTTP scheme");
        AssertEqual(nonHttpTermCalled, false, "TerminateProcess not called on non-HTTP redirect rejection");
        AssertEqual(File.ReadAllText(maliciousRedirDll), "ACTIVE_MOD_BEFORE_MALICIOUS_REDIRECT", "Active DLL preserved when redirect points to non-HTTP scheme");

        nonHttpRunning = false;
        nonHttpServer.Stop();
        nonHttpServer.Close();
        nonHttpThread.Join(500);

        // Clean up mock state
        termProp.SetValue(null, originalTerminate);
        try { Directory.Delete(tempTestDir, true); } catch {}

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"==========================================================");
        Console.WriteLine($" [SUCCESS] All {assertions} assertions PASSED cleanly!");
        Console.WriteLine($"==========================================================");
        Console.ResetColor();
    }
}
