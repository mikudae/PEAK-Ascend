using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

internal static class Settings
{
	private class Kept
	{
		public FieldInfo Field;

		public ConfigEntryBase Entry;

		public object Normal;
	}

	[CompilerGenerated]
	private sealed class _003CFields_003Ed__18 : IEnumerable<KeyValuePair<FieldInfo, object>>, IEnumerable, IEnumerator<KeyValuePair<FieldInfo, object>>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private KeyValuePair<FieldInfo, object> _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private List<Kept>.Enumerator _003C_003E7__wrap1;

		KeyValuePair<FieldInfo, object> IEnumerator<KeyValuePair<FieldInfo, object>>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CFields_003Ed__18(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(List<Kept>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = kept.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					Kept current = _003C_003E7__wrap1.Current;
					_003C_003E2__current = new KeyValuePair<FieldInfo, object>(current.Field, current.Normal);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(List<Kept>.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<KeyValuePair<FieldInfo, object>> IEnumerable<KeyValuePair<FieldInfo, object>>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CFields_003Ed__18(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<FieldInfo, object>>)this).GetEnumerator();
		}
	}

	internal static ConfigEntry<bool> Remember;

	internal static bool Paused;

	private static ConfigFile file;

	private static readonly List<Kept> kept = new List<Kept>();

	private static float nextLook;

	internal static string Path => file.ConfigFilePath;

	internal static int Count => kept.Count;

	public static void Bind(ConfigFile config)
	{
		file = config;
		Remember = config.Bind<bool>("General", "RememberSettings", false, "While true, every switch and slider of the menu is saved here as it changes and comes back when the game starts. While false, they all start from normal each time. Can also be switched on the menu's About page.");
		FieldInfo[] fields = typeof(State).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsLiteral)
			{
				continue;
			}
			object value = fieldInfo.GetValue(null);
			ConfigEntryBase entry;
			if (fieldInfo.FieldType == typeof(bool))
			{
				entry = (ConfigEntryBase)(object)config.Bind<bool>("Remembered", fieldInfo.Name, (bool)value, "Kept by the menu while RememberSettings is true. Change it in the menu, not here.");
			}
			else if (fieldInfo.FieldType == typeof(int))
			{
				entry = (ConfigEntryBase)(object)config.Bind<int>("Remembered", fieldInfo.Name, (int)value, "Kept by the menu while RememberSettings is true. Change it in the menu, not here.");
			}
			else
			{
				if (!(fieldInfo.FieldType == typeof(float)))
				{
					continue;
				}
				entry = (ConfigEntryBase)(object)config.Bind<float>("Remembered", fieldInfo.Name, (float)value, "Kept by the menu while RememberSettings is true. Change it in the menu, not here.");
			}
			kept.Add(new Kept
			{
				Field = fieldInfo,
				Entry = entry,
				Normal = value
			});
		}
		Application.quitting += Save;
	}

	public static void Load()
	{
		if (Paused || !Remember.Value)
		{
			return;
		}
		int num = 0;
		foreach (Kept item in kept)
		{
			object obj = item.Entry.BoxedValue;
			if (obj is float && (float.IsNaN((float)obj) || float.IsInfinity((float)obj) || (float)obj < 0f))
			{
				obj = item.Normal;
			}
			if (obj is int && (int)obj < 0)
			{
				obj = item.Normal;
			}
			if (!object.Equals(obj, item.Normal))
			{
				num++;
			}
			item.Field.SetValue(null, obj);
		}
		if (num > 0)
		{
			Kit.Log.LogInfo((object)(num + " remembered settings were put back."));
		}
	}

	public static void Save()
	{
		if (Paused || file == null)
		{
			return;
		}
		bool flag = false;
		bool saveOnConfigSet = file.SaveOnConfigSet;
		file.SaveOnConfigSet = false;
		try
		{
			foreach (Kept item in kept)
			{
				object obj = (Remember.Value ? item.Field.GetValue(null) : item.Normal);
				if (!object.Equals(item.Entry.BoxedValue, obj))
				{
					item.Entry.BoxedValue = obj;
					flag = true;
				}
			}
		}
		finally
		{
			file.SaveOnConfigSet = saveOnConfigSet;
		}
		if (flag)
		{
			file.Save();
		}
	}

	public static void Tick()
	{
		if (!Paused && !(Time.unscaledTime < nextLook))
		{
			nextLook = Time.unscaledTime + 1f;
			Save();
		}
	}

	internal static void Blank()
	{
		foreach (Kept item in kept)
		{
			item.Field.SetValue(null, item.Normal);
		}
	}

	internal static void Reload()
	{
		file.Reload();
		Load();
	}

	internal static object[] Snapshot()
	{
		object[] array = new object[kept.Count + 1];
		array[0] = Remember.Value;
		for (int i = 0; i < kept.Count; i++)
		{
			array[i + 1] = kept[i].Entry.BoxedValue;
		}
		return array;
	}

	internal static void Restore(object[] values)
	{
		bool saveOnConfigSet = file.SaveOnConfigSet;
		file.SaveOnConfigSet = false;
		try
		{
			Remember.Value = (bool)values[0];
			for (int i = 0; i < kept.Count; i++)
			{
				kept[i].Entry.BoxedValue = values[i + 1];
			}
		}
		finally
		{
			file.SaveOnConfigSet = saveOnConfigSet;
		}
		file.Save();
	}

	[IteratorStateMachine(typeof(_003CFields_003Ed__18))]
	internal static IEnumerable<KeyValuePair<FieldInfo, object>> Fields()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFields_003Ed__18(-2);
	}
}
