import urllib.request
import json
import time

def call_api(url, method="GET", data=None):
    headers = {'User-Agent': 'DebugSuite'}
    body = None
    if data is not None:
        body = json.dumps(data).encode('utf-8')
        headers['Content-Type'] = 'application/json'
    
    req = urllib.request.Request(url, data=body, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=5) as resp:
            content = resp.read().decode('utf-8')
            try:
                return resp.status, json.loads(content)
            except:
                return resp.status, content
    except urllib.error.HTTPError as e:
        content = e.read().decode('utf-8')
        try:
            return e.code, json.loads(content)
        except:
            return e.code, content

results = []

def test(name, fn):
    try:
        ok, detail = fn()
        status = "PASS" if ok else "FAIL"
        results.append((status, name, detail))
        print(f"[{status}] {name}: {detail}")
    except Exception as ex:
        results.append(("ERROR", name, str(ex)))
        print(f"[ERROR] {name}: {ex}")

# 1. Telemetry verification
test("Master GET /status", lambda: (
    (code := call_api("http://127.0.0.1:18080/status"))[0] == 200 and code[1].get("role") == "Master",
    f"HTTP {code[0]}, role={code[1].get('role')}, health={code[1].get('health')}"
))

test("Slave GET /status", lambda: (
    (code := call_api("http://127.0.0.1:18081/status"))[0] == 200 and code[1].get("role") == "Slave",
    f"HTTP {code[0]}, role={code[1].get('role')}, health={code[1].get('health')}"
))

test("Master GET /scouts (Lobby detection)", lambda: (
    (code := call_api("http://127.0.0.1:18080/scouts"))[0] == 200 and len(code[1]) == 2,
    f"Found {len(code[1])} scouts: {[s.get('NickName') for s in code[1]]}"
))

test("Master GET /inventory (Live gear inspection)", lambda: (
    (code := call_api("http://127.0.0.1:18080/inventory"))[0] == 200 and len(code[1]) == 2,
    f"Inspected {len(code[1])} scout inventories"
))

# 2. In-Game Say Banners
test("Master POST /action say", lambda: (
    (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "say", "msg": "Debug Suite Test: Master"}))[0] == 200 and code[1].get("success") == True,
    f"Response: {code[1]}"
))

test("Slave POST /action say", lambda: (
    (code := call_api("http://127.0.0.1:18081/action", "POST", {"action": "say", "msg": "Debug Suite Test: Slave"}))[0] == 200 and code[1].get("success") == True,
    f"Response: {code[1]}"
))

# 3. Toggle Cheats
cheats_to_test = [
    ("god", "God mode"),
    ("inf_stamina", "Infinite stamina"),
    ("fly", "Fly mode"),
    ("no_fall", "No fall damage"),
    ("no_clip", "No clip"),
    ("no_weight", "No weight"),
    ("air_jump", "Air jump"),
    ("climb_anything", "Climb anything"),
    ("no_slip", "Never slip"),
    ("keep_items", "Keep items"),
    ("cosmetics", "Every cosmetic")
]

for cheat_key, cheat_name in cheats_to_test:
    # Toggle ON on Master
    test(f"Toggle {cheat_name} ON (Master)", lambda ck=cheat_key: (
        (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "toggle", "cheat": ck, "value": 1}))[0] == 200 and code[1].get("success") == True and code[1].get("enabled") == True,
        f"enabled={code[1].get('enabled')}"
    ))
    # Toggle OFF on Master
    test(f"Toggle {cheat_name} OFF (Master)", lambda ck=cheat_key: (
        (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "toggle", "cheat": ck, "value": 0}))[0] == 200 and code[1].get("success") == True and code[1].get("enabled") == False,
        f"enabled={code[1].get('enabled')}"
    ))

# 4. Movement Multipliers
multipliers = [
    ("speed", 2.5),
    ("jump", 2.0),
    ("climb", 1.5),
    ("fly_speed", 25.0)
]

for m_key, m_val in multipliers:
    test(f"Set multiplier {m_key} to {m_val}", lambda k=m_key, v=m_val: (
        (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "toggle", "cheat": k, "value": v}))[0] == 200 and code[1].get("success") == True,
        f"value={code[1].get('value')}"
    ))
    # Reset back to 1.0 or 15.0
    reset_val = 15.0 if m_key == "fly_speed" else 1.0
    call_api("http://127.0.0.1:18080/action", "POST", {"action": "toggle", "cheat": m_key, "value": reset_val})

# 5. Permission Control Matrix
sections = ["Movement", "Items", "Teleport", "World", "Camera", "Help", "Troll"]
for sec in sections:
    # Grant globally
    test(f"Perm Grant Global '{sec}'", lambda s=sec: (
        (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "perm", "section": s, "allow": True}))[0] == 200 and code[1].get("allow") == True,
        f"allow={code[1].get('allow')}"
    ))
    # Revoke globally
    test(f"Perm Revoke Global '{sec}'", lambda s=sec: (
        (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "perm", "section": s, "allow": False}))[0] == 200 and code[1].get("allow") == False,
        f"allow={code[1].get('allow')}"
    ))

# Per-slave override
test("Perm Custom Override for Slave (Actor 2: Movement,Teleport)", lambda: (
    (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "perm", "actor": 2, "section": "Movement,Teleport", "allow": True}))[0] == 200 and code[1].get("allow") == True,
    f"Override applied: {code[1]}"
))

test("Perm Clear Override for Slave (Actor 2)", lambda: (
    (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "perm", "actor": 2, "allow": "clear"}))[0] == 200 and code[1].get("success") == True,
    f"Override cleared: {code[1]}"
))

# 6. Item Spawning & Remote Delivery
test("Master Item Spawner (Compass)", lambda: (
    (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "spawn", "item": "Compass", "count": 1}))[0] == 200 and code[1].get("success") == True,
    f"Spawned: {code[1].get('prefab')}"
))

test("Remote Delivery: Master -> Slave Hands (Compass)", lambda: (
    (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "deliver", "actor": 2, "dest": "Hands", "item": "Compass"}))[0] == 200 and code[1].get("success") == True,
    f"Delivered to Hands: {code[1]}"
))

test("Remote Delivery: Master -> Slave Backpack (Anti-Rope Spool)", lambda: (
    (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "deliver", "actor": 2, "dest": "Backpack", "item": "Anti-Rope Spool"}))[0] == 200 and code[1].get("success") == True,
    f"Delivered to Pack: {code[1]}"
))

# Verify Slave Inventory updated
time.sleep(0.5)
test("Verify Slave Inventory after Delivery", lambda: (
    (code := call_api("http://127.0.0.1:18080/inventory"))[0] == 200 and any(s.get("actorNumber") == 2 and (s.get("heldItem") != "None" or len(s.get("backpack", [])) > 0 or any(p != "Empty" for p in s.get("pockets", []))) for s in code[1]),
    f"Slave items present: {[s for s in code[1] if s.get('actorNumber') == 2]}"
))

# 7. Warp & Coordinates
m_pos = call_api("http://127.0.0.1:18080/status")[1].get("coords", {})
orig_x, orig_y, orig_z = m_pos.get("x", 16.0), m_pos.get("y", 1.8), m_pos.get("z", -373.0)

test("Master Warp Coordinates (+2m Y)", lambda: (
    (code := call_api("http://127.0.0.1:18080/action", "POST", {"action": "warp", "x": orig_x, "y": orig_y + 2.0, "z": orig_z}))[0] == 200 and code[1].get("success") == True,
    f"Warp response: {code[1]}"
))

test("Slave Warp To Master (warp_to)", lambda: (
    (code := call_api("http://127.0.0.1:18081/action", "POST", {"action": "warp_to", "actor": 1}))[0] == 200 and code[1].get("success") == True,
    f"Warp to Master response: {code[1]}"
))

# Summary
passed = sum(1 for s, _, _ in results if s == "PASS")
failed = sum(1 for s, _, _ in results if s == "FAIL")
errors = sum(1 for s, _, _ in results if s == "ERROR")
print(f"\n==========================================")
print(f"LIVE TEST SUITE RESULTS: {passed} PASSED, {failed} FAILED, {errors} ERRORS (Total: {len(results)})")
print(f"==========================================")
