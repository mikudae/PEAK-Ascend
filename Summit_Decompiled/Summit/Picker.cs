using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

internal class Picker
{
	internal class Entry
	{
		public string Name;

		public string Match;

		public Texture Icon;

		public Sprite Sprite;

		public object What;

		public int Kind;
	}

	internal readonly List<Entry> All = new List<Entry>();

	internal readonly List<Entry> Shown = new List<Entry>();

	internal Entry Picked;

	internal int Kind;

	internal float TileW = 92f;

	internal float TileH = 100f;

	private string lastSearch;

	private int lastKind = -1;

	private int lastCount = -1;

	private Vector2 scroll;

	private const float Gap = 6f;

	internal void Add(string name, string match, object what, Texture icon = null, Sprite sprite = null, int kind = 0)
	{
		All.Add(new Entry
		{
			Name = name,
			Match = (name + " " + match).ToLowerInvariant(),
			What = what,
			Icon = icon,
			Sprite = sprite,
			Kind = kind
		});
	}

	internal void Sort()
	{
		All.Sort((Entry a, Entry b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
		lastSearch = null;
	}

	internal void Clear()
	{
		All.Clear();
		Shown.Clear();
		Picked = null;
		lastSearch = null;
	}

	internal void Filter(string search)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		search = search ?? "";
		if (search == lastSearch && Kind == lastKind && All.Count == lastCount)
		{
			return;
		}
		lastSearch = search;
		lastKind = Kind;
		lastCount = All.Count;
		Shown.Clear();
		string text = search.Trim().ToLowerInvariant();
		foreach (Entry item in All)
		{
			if ((Kind == 0 || item.Kind == Kind - 1) && (text.Length == 0 || item.Match.Contains(text)))
			{
				Shown.Add(item);
			}
		}
		scroll = Vector2.zero;
	}

	internal void Draw(string search)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Invalid comparison between Unknown and I4
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Invalid comparison between Unknown and I4
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Invalid comparison between Unknown and I4
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		Filter(search);
		Rect rect = GUILayoutUtility.GetRect(10f, 10f, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.ExpandWidth(true),
			GUILayout.ExpandHeight(true)
		});
		int num = Mathf.Max(1, (int)((rect.width - 16f + 6f) / (TileW + 6f)));
		int num2 = (Shown.Count + num - 1) / num;
		float num3 = Mathf.Floor((rect.width - 16f - 6f * (float)(num - 1)) / (float)num);
		Rect val = default(Rect);
		val = new Rect(0f, 0f, rect.width - 16f, (float)num2 * (TileH + 6f));
		float num4 = val.height - rect.height;
		Rect val2 = default(Rect);
		float num5;
		int controlID;
		Event current;
		if (num4 > 0f)
		{
			val2 = new Rect(rect.xMax - 8f, rect.y, 6f, rect.height);
			num5 = Mathf.Max(36f, rect.height * rect.height / val.height);
			controlID = GUIUtility.GetControlID((FocusType)2);
			current = Event.current;
			bool flag = GUIUtility.hotControl == controlID;
			if ((int)current.type == 0)
			{
				Rect val3 = new Rect(val2.x - 8f, val2.y, 22f, val2.height);
				if (val3.Contains(current.mousePosition))
				{
					goto IL_0187;
				}
			}
			if ((int)current.type == 3 && flag)
			{
				goto IL_0187;
			}
			if ((int)current.rawType == 1 && flag)
			{
				GUIUtility.hotControl = 0;
			}
			goto IL_01eb;
		}
		goto IL_024e;
		IL_01eb:
		if ((int)current.type == 7)
		{
			Theme.SliderBar.Draw(val2, false, false, false, false);
			Theme.Thumb.Draw(new Rect(val2.x, val2.y + (val2.height - num5) * Mathf.Clamp01(scroll.y / num4), val2.width, num5), false, false, false, false);
		}
		goto IL_024e;
		IL_0187:
		GUIUtility.hotControl = controlID;
		scroll.y = num4 * Mathf.InverseLerp(val2.y + num5 / 2f, val2.yMax - num5 / 2f, current.mousePosition.y);
		current.Use();
		goto IL_01eb;
		IL_024e:
		scroll = GUI.BeginScrollView(rect, scroll, val, false, false, GUIStyle.none, GUIStyle.none);
		int num6 = Mathf.Max(0, (int)(scroll.y / (TileH + 6f)));
		int num7 = Mathf.Min(num2 - 1, (int)((scroll.y + rect.height) / (TileH + 6f)));
		Rect val4 = default(Rect);
		Rect val5 = default(Rect);
		for (int i = num6; i <= num7; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num8 = i * num + j;
				if (num8 >= Shown.Count)
				{
					break;
				}
				Entry entry = Shown[num8];
				val4 = new Rect((float)j * (num3 + 6f), (float)i * (TileH + 6f), num3, TileH);
				if (GUI.Button(val4, GUIContent.none, (entry == Picked) ? Theme.TileOn : Theme.Tile))
				{
					Picked = entry;
					GUIUtility.keyboardControl = 0;
				}
				val5 = new Rect(val4.x + 14f, val4.y + 6f, num3 - 28f, TileH - 44f);
				if ((Object)(object)entry.Sprite != (Object)null)
				{
					Theme.DrawSprite(val5, entry.Sprite);
				}
				else if ((Object)(object)entry.Icon != (Object)null)
				{
					GUI.DrawTexture(val5, entry.Icon, (ScaleMode)2);
				}
				GUI.Label(new Rect(val4.x + 3f, val4.yMax - 36f, num3 - 6f, 34f), entry.Name, Theme.TileName);
			}
		}
		GUI.EndScrollView();
		if (Shown.Count == 0)
		{
			GUI.Label(rect, Lang.T("Nothing matches"), Theme.Centre);
		}
	}

	internal static void Picture(Rect r, Entry it)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (it != null)
		{
			if ((Object)(object)it.Sprite != (Object)null)
			{
				Theme.DrawSprite(r, it.Sprite);
			}
			else if ((Object)(object)it.Icon != (Object)null)
			{
				GUI.DrawTexture(r, it.Icon, (ScaleMode)2);
			}
		}
	}
}
