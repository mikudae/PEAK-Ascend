using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

internal static class Theme
{
	public static readonly Color Bg = Rgb(16, 22, 28);

	public static readonly Color Card = Rgb(25, 34, 43);

	public static readonly Color Raised = Rgb(36, 49, 61);

	public static readonly Color Hover = Rgb(49, 67, 83);

	public static readonly Color Text = Rgb(238, 242, 245);

	public static readonly Color Dim = Rgb(140, 156, 171);

	public static readonly Color Track = Rgb(58, 75, 91);

	public static readonly Color Blurple = Rgb(88, 101, 242);

	public static readonly Color BlurpleHi = Rgb(113, 124, 245);

	public static Color Accent = Rgb(47, 158, 143);

	public static Color AccentHi = Rgb(69, 191, 173);

	public static GUIStyle Window;

	public static GUIStyle Title;

	public static GUIStyle Hint;

	public static GUIStyle Button;

	public static GUIStyle Primary;

	public static GUIStyle Label;

	public static GUIStyle Small;

	public static GUIStyle Row;

	public static GUIStyle RowLabel;

	public static GUIStyle Tile;

	public static GUIStyle TileOn;

	public static GUIStyle TileName;

	public static GUIStyle Field;

	public static GUIStyle PillOn;

	public static GUIStyle PillOff;

	public static GUIStyle Knob;

	public static GUIStyle SliderBar;

	public static GUIStyle SliderFill;

	public static GUIStyle SliderHit;

	public static GUIStyle Chip;

	public static GUIStyle ChipOn;

	public static GUIStyle Big;

	public static GUIStyle Side;

	public static GUIStyle SideOn;

	public static GUIStyle Panel;

	public static GUIStyle CardTitle;

	public static GUIStyle PinOn;

	public static GUIStyle PinOff;

	public static GUIStyle HintBar;

	public static GUIStyle Placeholder;

	public static GUIStyle FieldBare;

	public static GUIStyle ClearX;

	public static GUIStyle Foot;

	public static GUIStyle Thumb;

	public static GUIStyle Tip;

	public static GUIStyle Discord;

	public static GUIStyle DiscordText;

	public static GUIStyle Centre;

	public static Texture2D GameLogo;

	public static Texture2D DiscordMark;

	private static Font wide;

	private static bool built;

	public struct ThemePresetItem
	{
		public string Name;
		public Color Primary;
		public Color Secondary;

		public ThemePresetItem(string name, Color primary, Color secondary)
		{
			Name = name;
			Primary = primary;
			Secondary = secondary;
		}
	}

	public static readonly ThemePresetItem[] Presets = new ThemePresetItem[]
	{
		new ThemePresetItem("Emerald Teal", Rgb(47, 158, 143), Rgb(69, 191, 173)),
		new ThemePresetItem("Royal Violet", Rgb(124, 58, 237), Rgb(167, 139, 250)),
		new ThemePresetItem("Crimson Ruby", Rgb(220, 38, 38), Rgb(248, 113, 113)),
		new ThemePresetItem("Sunset Amber", Rgb(217, 119, 6), Rgb(251, 191, 36)),
		new ThemePresetItem("Cyberpunk Neon", Rgb(6, 182, 212), Rgb(34, 211, 238)),
		new ThemePresetItem("Rose Quartz", Rgb(219, 39, 119), Rgb(244, 114, 182))
	};

	public static void Paint(Color accent, Color accentHi)
	{
		Accent = accent;
		AccentHi = accentHi;
		if (built)
		{
			RebuildStyles();
		}
	}

	public static void ApplyPreset(string name)
	{
		foreach (ThemePresetItem p in Presets)
		{
			if (p.Name == name)
			{
				Paint(p.Primary, p.Secondary);
				if (Kit.PrimaryR != null)
				{
					Kit.PrimaryR.Value = p.Primary.r;
					Kit.PrimaryG.Value = p.Primary.g;
					Kit.PrimaryB.Value = p.Primary.b;
					Kit.SecondaryR.Value = p.Secondary.r;
					Kit.SecondaryG.Value = p.Secondary.g;
					Kit.SecondaryB.Value = p.Secondary.b;
					Kit.ThemePreset.Value = p.Name;
				}
				return;
			}
		}
	}

	public static void SetCustomColors(Color primary, Color secondary)
	{
		Paint(primary, secondary);
		if (Kit.PrimaryR != null)
		{
			Kit.PrimaryR.Value = primary.r;
			Kit.PrimaryG.Value = primary.g;
			Kit.PrimaryB.Value = primary.b;
			Kit.SecondaryR.Value = secondary.r;
			Kit.SecondaryG.Value = secondary.g;
			Kit.SecondaryB.Value = secondary.b;
			Kit.ThemePreset.Value = "Custom";
		}
	}

	public static void RebuildStyles()
	{
		if (!built) return;
		Button = Clicky(Raised, Hover, Accent);
		Primary = Clicky(Accent, AccentHi, Accent, 14);
		Primary.fontStyle = (FontStyle)1;
		Chip = Clicky(Raised, Hover, Accent, 12, 6);
		Chip.padding = new RectOffset(6, 6, 5, 5);
		ChipOn = Clicky(Accent, AccentHi, Accent, 12, 6);
		ChipOn.padding = new RectOffset(6, 6, 5, 5);
		SideOn = Clicky(Accent, AccentHi, Accent);
		SideOn.fontStyle = (FontStyle)1;
		SideOn.alignment = (TextAnchor)3;
		SideOn.margin = new RectOffset(0, 0, 2, 2);
		Color normal = Color.Lerp(Card, Accent, 0.32f);
		Color hover = Color.Lerp(Card, Accent, 0.42f);
		TileOn = Clicky(normal, hover, Accent, 11, 8);
		TileOn.margin = new RectOffset(0, 0, 0, 0);
		PillOn = Box(Accent, 9);
		SliderFill = Box(Accent, 3);
		PinOn = Box(Accent, 3);
		StyleExt.Forget();
		Refont();
	}

	public static Color Rgb(int r, int g, int b)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, 1f);
	}

	public static void Refont()
	{
		if (!built)
		{
			return;
		}
		Font font = null;
		if (Lang.Now == 2)
		{
			if ((Object)(object)wide == (Object)null)
			{
				wide = Font.CreateDynamicFontFromOSFont(new string[7] { "Microsoft YaHei UI", "Microsoft YaHei", "DengXian", "SimHei", "SimSun", "Noto Sans CJK SC", "Arial Unicode MS" }, 13);
			}
			font = wide;
		}
		FieldInfo[] fields = typeof(Theme).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			object value = fields[i].GetValue(null);
			GUIStyle val = (GUIStyle)((value is GUIStyle) ? value : null);
			if (val != null)
			{
				val.font = font;
			}
		}
		StyleExt.Forget();
	}

	private static Texture2D Round(int size, int radius, Color c)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(size, size, (TextureFormat)4, false)
		{
			hideFlags = (HideFlags)61,
			filterMode = (FilterMode)1,
			wrapMode = (TextureWrapMode)1
		};
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num = Mathf.Clamp((float)j + 0.5f, (float)radius, (float)(size - radius));
				float num2 = Mathf.Clamp((float)i + 0.5f, (float)radius, (float)(size - radius));
				float num3 = Vector2.Distance(new Vector2((float)j + 0.5f, (float)i + 0.5f), new Vector2(num, num2));
				float num4 = Mathf.Clamp01((float)radius - num3 + 0.5f);
				val.SetPixel(j, i, new Color(c.r, c.g, c.b, c.a * num4));
			}
		}
		val.Apply();
		return val;
	}

	public static GUIStyle Box(Color c, int radius = 7)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		int size = radius * 2 + 4;
		GUIStyle val = new GUIStyle
		{
			border = new RectOffset(radius + 1, radius + 1, radius + 1, radius + 1)
		};
		val.normal.background = Round(size, radius, c);
		val.normal.textColor = Text;
		return val;
	}

	public static GUIStyle Text_(int size, Color c, FontStyle fs = (FontStyle)0, TextAnchor a = (TextAnchor)3)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		GUIStyle val = new GUIStyle
		{
			fontSize = size,
			fontStyle = fs,
			alignment = a,
			wordWrap = false,
			richText = true
		};
		val.normal.textColor = c;
		return val;
	}

	public static GUIStyle Clicky(Color normal, Color hover, Color active, int size = 13, int radius = 7)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		GUIStyle val = Box(normal, radius);
		val.hover.background = Round(radius * 2 + 4, radius, hover);
		val.active.background = Round(radius * 2 + 4, radius, active);
		GUIStyleState hover2 = val.hover;
		Color textColor = (val.active.textColor = Text);
		hover2.textColor = textColor;
		val.fontSize = size;
		val.alignment = (TextAnchor)4;
		val.padding = new RectOffset(12, 12, 7, 7);
		val.margin = new RectOffset(3, 3, 3, 3);
		val.clipping = (TextClipping)1;
		return val;
	}

	private static Texture2D Picture(string ending)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_006c: Expected O, but got Unknown
		try
		{
			Assembly assembly = typeof(Theme).Assembly;
			string[] manifestResourceNames = assembly.GetManifestResourceNames();
			foreach (string text in manifestResourceNames)
			{
				if (text.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
				{
					using (Stream stream = assembly.GetManifestResourceStream(text))
					{
						byte[] array = new byte[stream.Length];
						stream.Read(array, 0, array.Length);
						Texture2D val = new Texture2D(2, 2, (TextureFormat)4, false)
						{
							hideFlags = (HideFlags)61
						};
						ImageConversion.LoadImage(val, array);
						return val;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Kit.Log.LogWarning((object)("could not load " + ending + ": " + ex.Message));
		}
		return null;
	}

	public static void Build()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Expected O, but got Unknown
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Expected O, but got Unknown
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Expected O, but got Unknown
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Expected O, but got Unknown
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Expected O, but got Unknown
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Expected O, but got Unknown
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Expected O, but got Unknown
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Expected O, but got Unknown
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Expected O, but got Unknown
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Expected O, but got Unknown
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Expected O, but got Unknown
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e4: Expected O, but got Unknown
		//IL_05e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0602: Unknown result type (might be due to invalid IL or missing references)
		if (!built)
		{
			built = true;
			GameLogo = Picture("logo.png");
			DiscordMark = Picture("discord_mark.png");
			Window = Box(Bg, 12);
			Window.padding = new RectOffset(16, 16, 12, 12);
			Title = Text_(18, Text, (FontStyle)1, (TextAnchor)3);
			Hint = Text_(12, Dim, (FontStyle)0, (TextAnchor)5);
			Label = Text_(13, Text, (FontStyle)0, (TextAnchor)3);
			Label.wordWrap = true;
			Centre = Text_(13, Text, (FontStyle)0, (TextAnchor)4);
			Centre.wordWrap = true;
			Small = Text_(11, Dim, (FontStyle)0, (TextAnchor)0);
			Small.wordWrap = true;
			Big = Text_(16, Text, (FontStyle)1, (TextAnchor)1);
			Big.wordWrap = true;
			Button = Clicky(Raised, Hover, Accent);
			Primary = Clicky(Accent, AccentHi, Accent, 14);
			Primary.fontStyle = (FontStyle)1;
			Chip = Clicky(Raised, Hover, Accent, 12, 6);
			Chip.padding = new RectOffset(6, 6, 5, 5);
			ChipOn = Clicky(Accent, AccentHi, Accent, 12, 6);
			ChipOn.padding = new RectOffset(6, 6, 5, 5);
			Row = Clicky(Card, Raised, Hover);
			Row.margin = new RectOffset(3, 3, 2, 2);
			RowLabel = Text_(13, Text, (FontStyle)0, (TextAnchor)3);
			RowLabel.clipping = (TextClipping)1;
			Tile = Clicky(Card, Hover, Accent, 11, 8);
			Tile.margin = new RectOffset(0, 0, 0, 0);
			Color normal = Color.Lerp(Card, Accent, 0.32f);
			Color hover = Color.Lerp(Card, Accent, 0.42f);
			TileOn = Clicky(normal, hover, Accent, 11, 8);
			TileOn.margin = new RectOffset(0, 0, 0, 0);
			TileName = Text_(10, Text, (FontStyle)0, (TextAnchor)1);
			TileName.wordWrap = true;
			TileName.clipping = (TextClipping)1;
			Field = Box(Rgb(10, 14, 18), 6);
			Field.padding = new RectOffset(9, 9, 6, 6);
			Field.fontSize = 13;
			Field.margin = new RectOffset(3, 3, 3, 3);
			Field.focused.background = Field.normal.background;
			GUIStyleState focused = Field.focused;
			Color textColor = (Field.hover.textColor = Text);
			focused.textColor = textColor;
			Field.hover.background = Field.normal.background;
			Field.clipping = (TextClipping)1;
			PillOn = Box(Accent, 9);
			PillOff = Box(Track, 9);
			Knob = Box(Text);
			SliderBar = Box(Track, 3);
			SliderFill = Box(Accent, 3);
			SliderHit = new GUIStyle
			{
				fixedHeight = 18f
			};
			Side = Clicky(new Color(0f, 0f, 0f, 0f), Raised, Hover);
			Side.normal.textColor = Dim;
			Side.fontStyle = (FontStyle)1;
			Side.alignment = (TextAnchor)3;
			Side.margin = new RectOffset(0, 0, 2, 2);
			SideOn = Clicky(Accent, AccentHi, Accent);
			SideOn.fontStyle = (FontStyle)1;
			SideOn.alignment = (TextAnchor)3;
			SideOn.margin = new RectOffset(0, 0, 2, 2);
			Panel = Box(Card, 10);
			Panel.padding = new RectOffset(10, 10, 8, 10);
			Panel.margin = new RectOffset(0, 0, 0, 8);
			CardTitle = Text_(11, Dim, (FontStyle)1, (TextAnchor)3);
			PinOn = Box(Accent, 3);
			PinOff = Box(Dim, 3);
			HintBar = Text_(13, Rgb(200, 210, 219), (FontStyle)0, (TextAnchor)0);
			HintBar.wordWrap = true;
			Tip = Text_(11, Dim, (FontStyle)0, (TextAnchor)2);
			Foot = Box(Card, 8);
			Thumb = Box(Hover, 3);
			Placeholder = Text_(13, Dim, (FontStyle)0, (TextAnchor)3);
			FieldBare = new GUIStyle(Field);
			GUIStyleState normal2 = FieldBare.normal;
			GUIStyleState hover2 = FieldBare.hover;
			Texture2D val2 = (FieldBare.focused.background = null);
			Texture2D background = (hover2.background = val2);
			normal2.background = background;
			ClearX = Clicky(new Color(0f, 0f, 0f, 0f), Raised, Hover, 12, 6);
			ClearX.normal.textColor = Dim;
			ClearX.padding = new RectOffset(0, 0, 0, 2);
			ClearX.margin = new RectOffset(0, 0, 0, 0);
			Discord = Clicky(Blurple, BlurpleHi, Blurple, 14);
			DiscordText = Text_(14, Color.white, (FontStyle)1, (TextAnchor)3);
			Refont();
		}
	}

	public static void DrawSprite(Rect r, Sprite s)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)s == (Object)null) && !((Object)(object)s.texture == (Object)null))
		{
			Rect val;
			try
			{
				val = s.textureRect;
			}
			catch
			{
				val = s.rect;
			}
			Texture2D texture = s.texture;
			float num = val.width / Mathf.Max(1f, val.height);
			float num2 = r.width;
			float num3 = r.height;
			if (num > num2 / num3)
			{
				num3 = num2 / num;
			}
			else
			{
				num2 = num3 * num;
			}
			GUI.DrawTextureWithTexCoords(new Rect(r.x + (r.width - num2) / 2f, r.y + (r.height - num3) / 2f, num2, num3), (Texture)(object)texture, new Rect(val.x / (float)((Texture)texture).width, val.y / (float)((Texture)texture).height, val.width / (float)((Texture)texture).width, val.height / (float)((Texture)texture).height));
		}
	}
}
