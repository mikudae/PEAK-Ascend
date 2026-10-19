using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

internal static class Egg
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static GUI.WindowFunction _003C0_003E__Paint;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass48_0
	{
		public bool early;

		public bool wentOff;

		public string error;

		public bool ended;

		public bool byMouse;

		internal string _003COnly_003Eb__0()
		{
			if (early)
			{
				throw new Exception("it went off before the seventh click");
			}
			if (!wentOff)
			{
				throw new Exception("seven clicks on the logo did nothing");
			}
			if (error != null)
			{
				throw new Exception("it could not be drawn: " + error);
			}
			if (!ended)
			{
				throw new Exception("it was still there " + 7f + " seconds later");
			}
			return (byMouse ? "seven real clicks on the logo set it off, six did not" : "set off by seven taps asked for in code (the game was not the window in front)") + "; picture " + (HasPicture ? "shown" : "not shipped") + "; sound: " + Sound + "; gone by itself after " + 6f + " seconds";
		}
	}

	[CompilerGenerated]
	private sealed class _003COnly_003Ed__48 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Menu menu;

		public Menu _003C_003E3__menu;

		private _003C_003Ec__DisplayClass48_0 _003C_003E8__1;

		private Func<string, IEnumerable> shot;

		public Func<string, IEnumerable> _003C_003E3__shot;

		private Action<string, Func<string>> check;

		public Action<string, Func<string>> _003C_003E3__check;

		private int _003Cwent_003E5__2;

		private float _003Cwait_003E5__3;

		private int _003Ci_003E5__4;

		private int _003Ci_003E5__5;

		private IEnumerator _003C_003E7__wrap5;

		object IEnumerator<object>.Current
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
		public _003COnly_003Ed__48(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			switch (_003C_003E1__state)
			{
			case -3:
			case 4:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -4:
			case 5:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally2();
				}
				break;
			case -5:
			case 7:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally3();
				}
				break;
			}
			_003C_003E8__1 = null;
			_003C_003E7__wrap5 = null;
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
					_003C_003E8__1 = new _003C_003Ec__DisplayClass48_0();
					_003Ci_003E5__4 = 0;
					goto IL_0083;
				case 1:
					_003C_003E1__state = -1;
					_003Ci_003E5__4++;
					goto IL_0083;
				case 2:
					_003C_003E1__state = -1;
					_003Ci_003E5__5++;
					goto IL_00da;
				case 3:
					_003C_003E1__state = -1;
					_003Ci_003E5__4++;
					goto IL_017c;
				case 4:
					_003C_003E1__state = -3;
					goto IL_0203;
				case 5:
					_003C_003E1__state = -4;
					goto IL_02ae;
				case 6:
					_003C_003E1__state = -1;
					_003Ci_003E5__4++;
					goto IL_0380;
				case 7:
					_003C_003E1__state = -5;
					goto IL_03e0;
				case 8:
					_003C_003E1__state = -1;
					goto IL_044e;
				case 9:
					{
						_003C_003E1__state = -1;
						_003Ci_003E5__4++;
						break;
					}
					IL_0083:
					if (_003Ci_003E5__4 < 20)
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					Menu.ForcedScale = 1f;
					_003Ci_003E5__4 = 0;
					goto IL_00f4;
					IL_03fa:
					_003C_003E8__1.error = menu.lastError ?? Error;
					_003Cwait_003E5__3 = Time.unscaledTime + 6f + 1f;
					goto IL_044e;
					IL_00f4:
					if (_003Ci_003E5__4 < 5 && !InFront)
					{
						ToFront();
						_003Ci_003E5__5 = 0;
						goto IL_00da;
					}
					menu.open = true;
					menu.GoTo(menu.AboutTab);
					menu.Tidy(20f, 20f);
					menu.lastError = null;
					_003Ci_003E5__4 = 0;
					goto IL_017c;
					IL_02d8:
					if (((_003Ci_003E5__4 < 4) & _003C_003E8__1.byMouse) && Times == _003Cwent_003E5__2)
					{
						_003C_003E7__wrap5 = ClickLogo().GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_02ae;
					}
					if (Times == _003Cwent_003E5__2)
					{
						_003C_003E8__1.byMouse = false;
						for (int i = 0; i < 7; i++)
						{
							Tap();
						}
					}
					_003C_003E8__1.wentOff = Times == _003Cwent_003E5__2 + 1 && On;
					_003Ci_003E5__4 = 0;
					goto IL_0380;
					IL_00da:
					if (_003Ci_003E5__5 < 30)
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 2;
						return true;
					}
					_003Ci_003E5__4++;
					goto IL_00f4;
					IL_0380:
					if (_003Ci_003E5__4 < 50)
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 6;
						return true;
					}
					if (shot != null)
					{
						_003C_003E7__wrap5 = shot("Hidden").GetEnumerator();
						_003C_003E1__state = -5;
						goto IL_03e0;
					}
					goto IL_03fa;
					IL_02ae:
					if (_003C_003E7__wrap5.MoveNext())
					{
						_ = _003C_003E7__wrap5.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 5;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap5 = null;
					_003Ci_003E5__4++;
					goto IL_02d8;
					IL_044e:
					if (Time.unscaledTime < _003Cwait_003E5__3)
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 8;
						return true;
					}
					_003C_003E8__1.ended = !On;
					_003Ci_003E5__4 = 0;
					break;
					IL_017c:
					if (_003Ci_003E5__4 < 40)
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 3;
						return true;
					}
					_003Cwent_003E5__2 = Times;
					_003C_003E8__1.byMouse = InFront && LogoAt.width > 4f;
					_003Ci_003E5__4 = 0;
					goto IL_022d;
					IL_022d:
					if ((_003Ci_003E5__4 < 6) & _003C_003E8__1.byMouse)
					{
						_003C_003E7__wrap5 = ClickLogo().GetEnumerator();
						_003C_003E1__state = -3;
						goto IL_0203;
					}
					_003C_003E8__1.early = Times != _003Cwent_003E5__2;
					_003Ci_003E5__4 = 0;
					goto IL_02d8;
					IL_0203:
					if (_003C_003E7__wrap5.MoveNext())
					{
						_ = _003C_003E7__wrap5.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 4;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap5 = null;
					_003Ci_003E5__4++;
					goto IL_022d;
					IL_03e0:
					if (_003C_003E7__wrap5.MoveNext())
					{
						_ = _003C_003E7__wrap5.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 7;
						return true;
					}
					_003C_003Em__Finally3();
					_003C_003E7__wrap5 = null;
					goto IL_03fa;
				}
				if (_003Ci_003E5__4 < 10)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 9;
					return true;
				}
				check("something hidden on the About page", delegate
				{
					if (_003C_003E8__1.early)
					{
						throw new Exception("it went off before the seventh click");
					}
					if (!_003C_003E8__1.wentOff)
					{
						throw new Exception("seven clicks on the logo did nothing");
					}
					if (_003C_003E8__1.error != null)
					{
						throw new Exception("it could not be drawn: " + _003C_003E8__1.error);
					}
					if (!_003C_003E8__1.ended)
					{
						throw new Exception("it was still there " + 7f + " seconds later");
					}
					return (_003C_003E8__1.byMouse ? "seven real clicks on the logo set it off, six did not" : "set off by seven taps asked for in code (the game was not the window in front)") + "; picture " + (HasPicture ? "shown" : "not shipped") + "; sound: " + Sound + "; gone by itself after " + 6f + " seconds";
				});
				menu.open = false;
				Menu.ForcedScale = 0f;
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
			if (_003C_003E7__wrap5 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap5 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally3()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap5 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			_003COnly_003Ed__48 _003COnly_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003COnly_003Ed__ = this;
			}
			else
			{
				_003COnly_003Ed__ = new _003COnly_003Ed__48(0);
			}
			_003COnly_003Ed__.menu = _003C_003E3__menu;
			_003COnly_003Ed__.check = _003C_003E3__check;
			_003COnly_003Ed__.shot = _003C_003E3__shot;
			return _003COnly_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	private const int TapsNeeded = 7;

	private const int Bits = 110;

	private const float Lasts = 6f;

	internal const string Words = "duski was hereee";

	private static int taps;

	private static float lastTap;

	private static float until;

	private static float started;

	private static float lastFrame;

	private static bool looked;

	private static GUIStyle words;

	private static GUIStyle shadow;

	private static System.Random rng = new System.Random();

	private static readonly float[] x = new float[110];

	private static readonly float[] y = new float[110];

	private static readonly float[] vx = new float[110];

	private static readonly float[] vy = new float[110];

	private static readonly float[] size = new float[110];

	private static readonly int[] colour = new int[110];

	private static readonly Color[] Colours = (Color[])(object)new Color[6]
	{
		new Color(1f, 0.36f, 0.42f),
		new Color(1f, 0.78f, 0.25f),
		new Color(0.35f, 0.85f, 0.55f),
		new Color(0.35f, 0.65f, 1f),
		new Color(0.78f, 0.5f, 1f),
		new Color(1f, 1f, 1f)
	};

	internal static int Times;

	internal static string Sound = "not tried";

	internal static bool HasPicture;

	internal static string Error;

	internal static Rect LogoAt;

	private static GCHandle held;

	private static Texture2D picture;

	private const int WindowId = 1162299169;

	private static float scale = 1f;

	internal static bool On => Time.unscaledTime < until;

	private static bool InFront => KitTest.Focused;

	internal static void Tap()
	{
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime - lastTap > 1.5f)
		{
			taps = 0;
		}
		lastTap = unscaledTime;
		if (++taps >= 7 && !On)
		{
			taps = 0;
			Start();
		}
	}

	internal static void Stop()
	{
		until = 0f;
	}

	private static void Start()
	{
		started = (lastFrame = Time.unscaledTime);
		until = started + 6f;
		Times++;
		for (int i = 0; i < 110; i++)
		{
			x[i] = 0.5f + ((float)rng.NextDouble() - 0.5f) * 0.2f;
			y[i] = -0.05f - (float)rng.NextDouble() * 0.25f;
			vx[i] = ((float)rng.NextDouble() - 0.5f) * 0.9f;
			vy[i] = 0.05f + (float)rng.NextDouble() * 0.35f;
			size[i] = 9f + (float)rng.NextDouble() * 10f;
			colour[i] = rng.Next(Colours.Length);
		}
		Play();
	}

	private static byte[] Shipped(string ending)
	{
		Assembly assembly = typeof(Egg).Assembly;
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			if (!text.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			using Stream stream = assembly.GetManifestResourceStream(text);
			byte[] array = new byte[stream.Length];
			int num;
			for (int j = 0; j < array.Length; j += num)
			{
				num = stream.Read(array, j, array.Length - j);
				if (num <= 0)
				{
					break;
				}
			}
			return array;
		}
		return null;
	}

	[DllImport("winmm.dll", EntryPoint = "PlaySoundW")]
	private static extern bool PlaySound(IntPtr sound, IntPtr module, uint flags);

	private static void Play()
	{
		try
		{
			if (!held.IsAllocated)
			{
				byte[] array = Shipped("egg.wav");
				if (array == null)
				{
					Sound = "none shipped";
					return;
				}
				held = GCHandle.Alloc(array, GCHandleType.Pinned);
			}
			Sound = (PlaySound(held.AddrOfPinnedObject(), IntPtr.Zero, 7u) ? "played" : "Windows would not play it");
		}
		catch (Exception ex)
		{
			Sound = "it could not be played (" + ex.Message.Split('\n')[0] + ")";
			Kit.Log.LogInfo((object)("the hidden sound: " + Sound));
		}
	}

	private static void Look()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		looked = true;
		try
		{
			byte[] array = Shipped("egg.png");
			if (array != null)
			{
				picture = new Texture2D(2, 2, (TextureFormat)4, false)
				{
					hideFlags = (HideFlags)61
				};
				ImageConversion.LoadImage(picture, array);
				HasPicture = true;
			}
		}
		catch (Exception ex)
		{
			Kit.Log.LogInfo((object)("the hidden picture: " + ex.Message.Split('\n')[0]));
		}
	}

	private static void Fill(Rect r, int which)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Color color = GUI.color;
		GUI.color = Colours[which];
		GUI.DrawTexture(r, (Texture)(object)Texture2D.whiteTexture);
		GUI.color = color;
	}

	private static void Dim(Rect r)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Color color = GUI.color;
		GUI.color = new Color(0f, 0f, 0f, 0.6f);
		GUI.DrawTexture(r, (Texture)(object)Texture2D.whiteTexture);
		GUI.color = color;
	}

	private static void Picture(Rect r)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)picture != (Object)null)
		{
			GUI.DrawTexture(r, (Texture)(object)picture, (ScaleMode)2);
		}
	}

	internal static void OnScreen()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		if (On)
		{
			scale = Mathf.Max(0.5f, (float)Screen.height / 1080f);
			Matrix4x4 matrix = GUI.matrix;
			GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
			Rect val = new Rect(0f, 0f, (float)Screen.width / scale, (float)Screen.height / scale);
			object obj = _003C_003EO._003C0_003E__Paint;
			if (obj == null)
			{
				GUI.WindowFunction val2 = Paint;
				_003C_003EO._003C0_003E__Paint = val2;
				obj = (object)val2;
			}
			GUI.Window(1162299169, val, (GUI.WindowFunction)obj, GUIContent.none, GUIStyle.none);
			GUI.BringWindowToFront(1162299169);
			GUI.matrix = matrix;
		}
	}

	private static void Paint(int id)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Draw(new Rect(0f, 0f, (float)Screen.width / scale, (float)Screen.height / scale));
		}
		catch (Exception ex)
		{
			Error = ex.Message.Split('\n')[0];
			Stop();
		}
	}

	internal static void Logo(Rect r)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Event current = Event.current;
		if ((int)current.type == 7)
		{
			LogoAt = new Rect(GUIUtility.GUIToScreenPoint(r.position), r.size * Menu.Scale);
		}
		else if ((int)current.type == 0 && current.button == 0 && r.Contains(current.mousePosition))
		{
			Tap();
		}
	}

	private static void ToFront()
	{
		KitTest.Focus();
	}

	private static IEnumerable ClickLogo()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return KitTest.Click(LogoAt.center);
	}

	[IteratorStateMachine(typeof(_003COnly_003Ed__48))]
	internal static IEnumerable Only(Menu menu, Action<string, Func<string>> check, Func<string, IEnumerable> shot)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COnly_003Ed__48(-2)
		{
			_003C_003E3__menu = menu,
			_003C_003E3__check = check,
			_003C_003E3__shot = shot
		};
	}

	internal static void Draw(Rect area)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		if (!On || (int)Event.current.type != 7)
		{
			return;
		}
		if (!looked)
		{
			Look();
		}
		if (words == null)
		{
			words = Theme.Text_(44, Color.white, (FontStyle)1, (TextAnchor)4);
			shadow = Theme.Text_(44, new Color(0f, 0f, 0f, 0.85f), (FontStyle)1, (TextAnchor)4);
		}
		float unscaledTime = Time.unscaledTime;
		float num = Mathf.Min(0.05f, unscaledTime - lastFrame);
		float num2 = unscaledTime - started;
		lastFrame = unscaledTime;
		Dim(area);
		float num3 = 12f * Mathf.Max(0.25f, 1f - num2 / 6f);
		float num4 = ((float)rng.NextDouble() - 0.5f) * 2f * num3;
		float num5 = ((float)rng.NextDouble() - 0.5f) * 2f * num3;
		float num6 = Mathf.Min(420f, area.height * 0.4f);
		Rect r = default(Rect);
		r = new Rect(area.center.x - num6 / 2f + num4, area.center.y - num6 / 2f - 40f + num5, num6, num6);
		Picture(r);
		Rect val = default(Rect);
		val = new Rect(area.x + num4 * 0.6f, (HasPicture ? (r.yMax + 12f) : (area.center.y - 30f)) + num5 * 0.6f, area.width, 60f);
		GUI.Label(new Rect(val.x + 2f, val.y + 2f, val.width, val.height), "duski was hereee", shadow);
		GUI.Label(val, "duski was hereee", words);
		for (int i = 0; i < 110; i++)
		{
			vy[i] += 0.55f * num;
			x[i] += vx[i] * num;
			y[i] += vy[i] * num;
			vx[i] *= 1f - 0.9f * num;
			if (!(y[i] < 0f) && !(y[i] > 1f) && !(x[i] < 0f) && !(x[i] > 1f))
			{
				float num7 = size[i] * (0.35f + 0.65f * Mathf.Abs(Mathf.Sin(num2 * 7f + (float)i)));
				Fill(new Rect(area.x + x[i] * area.width - num7 / 2f, area.y + y[i] * area.height, num7, size[i] * 0.6f), colour[i]);
			}
		}
	}
}
