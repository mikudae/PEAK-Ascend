using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

internal static class Keys
{
	internal class Bound
	{
		public string Id;

		public string Name;

		public FieldInfo Info;

		public Action Run;

		public ConfigEntry<KeyCode> Key;
	}

	internal static readonly List<Bound> All = new List<Bound>();

	internal static readonly HashSet<string> Quiet = new HashSet<string> { "ShowActive" };

	private static readonly List<Bound> actions = new List<Bound>();

	private static FieldInfo showActive;

	private static readonly HashSet<KeyCode> held = new HashSet<KeyCode>();

	internal static bool ShowActive
	{
		get
		{
			if (!(showActive == null))
			{
				return (bool)showActive.GetValue(null);
			}
			return true;
		}
	}

	internal static void Job(string id, string name, Action run)
	{
		actions.Add(new Bound
		{
			Id = id,
			Name = name,
			Run = run
		});
	}

	internal static string Label(Bound b)
	{
		if (b.Name != null)
		{
			return b.Name;
		}
		if (!Menu.Names.TryGetValue(b.Id, out var value))
		{
			return b.Id;
		}
		return value;
	}

	public static void Bind(ConfigFile config)
	{
		showActive = typeof(State).GetField("ShowActive");
		FieldInfo[] fields = typeof(State).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsLiteral && !(fieldInfo.FieldType != typeof(bool)) && !(fieldInfo.Name == "ShowActive"))
			{
				All.Add(new Bound
				{
					Id = fieldInfo.Name,
					Info = fieldInfo,
					Key = config.Bind<KeyCode>("Keys", fieldInfo.Name + "Key", (KeyCode)0, "Key that switches " + fieldInfo.Name + " on and off without opening the menu. None for no key. Can be set on the menu's About page.")
				});
			}
		}
		foreach (Bound action in actions)
		{
			action.Key = config.Bind<KeyCode>("Keys", action.Id + "Key", (KeyCode)0, "Key for \"" + action.Name + "\" without opening the menu. None for no key. Can be set on the menu's About page.");
			All.Add(action);
		}
	}

	public static bool Event(Event e, Action<string> say)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((int)e.type == 5)
		{
			held.Remove(e.keyCode);
			return false;
		}
		if ((int)e.type != 4 || (int)e.keyCode == 0)
		{
			return false;
		}
		if (!held.Add(e.keyCode))
		{
			return false;
		}
		return Press(e.keyCode, say);
	}

	public static string GetSection(Bound b)
	{
		if (b == null) return null;
		if (b.Id == "WarpAim" || b.Id == "WarpFire" || b.Id == "SaveSpot" || b.Id == "GoSpot")
		{
			return "Teleport";
		}
		if (b.Id == "Heal" || b.Id == "KillSelf")
		{
			return "Scout";
		}
		if (b.Id == "FreeCam")
		{
			return "Camera";
		}
		if (Menu.Instance != null && Menu.Instance.controls != null && Menu.Instance.controls.TryGetValue(b.Id, out var ctrl))
		{
			if (ctrl.In != null && !string.IsNullOrEmpty(ctrl.In.Page))
			{
				return ctrl.In.Page;
			}
		}
		return null;
	}

	public static bool Press(KeyCode key, Action<string> say)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		foreach (Bound item in All)
		{
			if (item.Key.Value != key)
			{
				continue;
			}
			string section = GetSection(item);
			if (!string.IsNullOrEmpty(section) && !ModNet.IsSectionAllowed(section))
			{
				say?.Invoke(Lang.F("{0} is locked by Master", section));
				return true;
			}
			result = true;
			if (item.Run != null)
			{
				try
				{
					item.Run();
				}
				catch (Exception e)
				{
					if ((Object)(object)Menu.Instance != (Object)null)
					{
						Menu.Instance.Fail(e);
					}
				}
			}
			else
			{
				bool flag = !(bool)item.Info.GetValue(null);
				item.Info.SetValue(null, flag);
				say?.Invoke(Lang.T(Label(item)) + ": " + Lang.T(flag ? "on" : "off"));
			}
		}
		return result;
	}

	public static string Active()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<FieldInfo, object> item in Settings.Fields())
		{
			string name = item.Key.Name;
			if (Quiet.Contains(name))
			{
				continue;
			}
			object value = item.Key.GetValue(null);
			if (object.Equals(value, item.Value))
			{
				continue;
			}
			string value2 = Lang.T(Menu.Names.TryGetValue(name, out value2) ? value2 : name);
			if (value is bool)
			{
				list.Add(value2);
				continue;
			}
			Menu.Control value3 = null;
			if ((Object)(object)Menu.Instance != (Object)null)
			{
				Menu.Instance.controls.TryGetValue(name, out value3);
			}
			if (value3 != null && value3.Shown != null)
			{
				list.Add(value2 + " " + value3.Shown(Convert.ToSingle(value)));
			}
			else if (value3 != null && value3.Chosen != null && value3.Chosen() >= 0 && value3.Chosen() < value3.Buttons.Length)
			{
				list.Add(value3.Raw ? value3.Buttons[value3.Chosen()] : Lang.T(value3.Buttons[value3.Chosen()]));
			}
			else
			{
				list.Add(value2 + " " + value);
			}
		}
		return string.Join("  ·  ", list.ToArray());
	}
}
