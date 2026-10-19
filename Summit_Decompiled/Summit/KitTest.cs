using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

internal static class KitTest
{
	private struct Point
	{
		public int X;

		public int Y;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass27_0
	{
		public Menu menu;

		public Menu.Control sw;

		internal string _003CRun_003Eb__1()
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<FieldInfo, object> item in Settings.Fields())
			{
				if (!menu.controls.ContainsKey(item.Key.Name) && !NoControl.Contains(item.Key.Name))
				{
					list.Add(item.Key.Name);
				}
			}
			if (list.Count > 0)
			{
				throw new Exception("no row for: " + string.Join(", ", list.ToArray()));
			}
			return Settings.Count + " settings, " + menu.controls.Count + " rows on " + menu.tabs.Length + " pages";
		}

		internal string _003CRun_003Eb__2()
		{
			_003C_003Ec__DisplayClass27_1 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass27_1
			{
				bad = new List<string>()
			};
			Action<string, string> action = delegate(string what, string text)
			{
				if (string.IsNullOrEmpty(text) || !Lang.Has(text))
				{
					CS_0024_003C_003E8__locals0.bad.Add(what + " \"" + text + "\"");
				}
			};
			string[] tabs = menu.tabs;
			foreach (string arg in tabs)
			{
				action("page", arg);
			}
			foreach (Menu.Group group in menu.groups)
			{
				action("card", group.Title);
				foreach (Menu.Control item in group.Items)
				{
					if (item.Id != "credits")
					{
						if (item.Buttons == null)
						{
							action("name of " + item.Id, item.Label);
						}
						else if (!item.Raw && item.ButtonNow == null)
						{
							tabs = item.Buttons;
							foreach (string arg2 in tabs)
							{
								action("button of " + item.Id, arg2);
							}
						}
					}
					action("explanation of " + item.Id, item.Hint);
				}
			}
			foreach (Keys.Bound item2 in Keys.All)
			{
				action("key", Keys.Label(item2));
			}
			if (CS_0024_003C_003E8__locals0.bad.Count > 0)
			{
				throw new Exception(CS_0024_003C_003E8__locals0.bad.Count + " without: " + string.Join("; ", CS_0024_003C_003E8__locals0.bad.Take(6).ToArray()));
			}
			return Lang.Count + " pieces of text";
		}

		internal bool _003CRun_003Eb__8(Keys.Bound k)
		{
			return k.Id == sw.Id;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass27_1
	{
		public List<string> bad;

		internal void _003CRun_003Eb__7(string what, string text)
		{
			if (string.IsNullOrEmpty(text) || !Lang.Has(text))
			{
				bad.Add(what + " \"" + text + "\"");
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003CClick_003Ed__18 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Vector2 p;

		public Vector2 _003C_003E3__p;

		private IEnumerator _003C_003E7__wrap1;

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
		public _003CClick_003Ed__18(int _003C_003E1__state)
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
			case 1:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -4:
			case 2:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally2();
				}
				break;
			case -5:
			case 3:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally3();
				}
				break;
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					MouseTo(p);
					_003C_003E7__wrap1 = Frames(5).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_007a;
				case 1:
					_003C_003E1__state = -3;
					goto IL_007a;
				case 2:
					_003C_003E1__state = -4;
					goto IL_00dd;
				case 3:
					{
						_003C_003E1__state = -5;
						break;
					}
					IL_007a:
					if (_003C_003E7__wrap1.MoveNext())
					{
						_ = _003C_003E7__wrap1.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					MouseDown();
					_003C_003E7__wrap1 = Frames(3).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00dd;
					IL_00dd:
					if (_003C_003E7__wrap1.MoveNext())
					{
						_ = _003C_003E7__wrap1.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap1 = null;
					MouseUp();
					_003C_003E7__wrap1 = Frames(5).GetEnumerator();
					_003C_003E1__state = -5;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					_ = _003C_003E7__wrap1.Current;
					_003C_003E2__current = null;
					_003C_003E1__state = 3;
					return true;
				}
				_003C_003Em__Finally3();
				_003C_003E7__wrap1 = null;
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
			if (_003C_003E7__wrap1 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally3()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 is IDisposable disposable)
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
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			_003CClick_003Ed__18 _003CClick_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CClick_003Ed__ = this;
			}
			else
			{
				_003CClick_003Ed__ = new _003CClick_003Ed__18(0);
			}
			_003CClick_003Ed__.p = _003C_003E3__p;
			return _003CClick_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CFrames_003Ed__17 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private int n;

		public int _003C_003E3__n;

		private int _003Ci_003E5__2;

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
		public _003CFrames_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				break;
			}
			if (_003Ci_003E5__2 < n)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			_003CFrames_003Ed__17 _003CFrames_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CFrames_003Ed__ = this;
			}
			else
			{
				_003CFrames_003Ed__ = new _003CFrames_003Ed__17(0);
			}
			_003CFrames_003Ed__.n = _003C_003E3__n;
			return _003CFrames_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CRun_003Ed__27 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Menu menu;

		public Menu _003C_003E3__menu;

		private Action<string, Func<string>> check;

		public Action<string, Func<string>> _003C_003E3__check;

		private _003C_003Ec__DisplayClass27_0 _003C_003E8__1;

		private Action<string> shot;

		public Action<string> _003C_003E3__shot;

		private object _003CpinsWere_003E5__2;

		private KeyCode _003CkeyWas_003E5__3;

		private KeyCode[] _003CkeysWere_003E5__4;

		private string _003Cabout_003E5__5;

		private Menu.Control _003Cslider_003E5__6;

		private string _003Cspanish_003E5__7;

		private string _003Cchinese_003E5__8;

		private int _003Clang_003E5__9;

		private int _003Ci_003E5__10;

		private IEnumerator _003C_003E7__wrap10;

		private Vector2 _003Cmid_003E5__12;

		private bool _003Cwas_003E5__13;

		private Vector2 _003Csquare_003E5__14;

		private KeyCode _003Cother_003E5__15;

		private Keys.Bound _003Cbound_003E5__16;

		private float _003Cbefore_003E5__17;

		private float _003Cy_003E5__18;

		private float _003Cfrom_003E5__19;

		private float _003Cto_003E5__20;

		private KeyCode _003Cshortcut_003E5__21;

		private bool _003Cbefore_003E5__22;

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
		public _003CRun_003Ed__27(int _003C_003E1__state)
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
			case 1:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -4:
			case 2:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally2();
				}
				break;
			case -5:
			case 3:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally3();
				}
				break;
			case -6:
			case 4:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally4();
				}
				break;
			case -7:
			case 5:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally5();
				}
				break;
			case -8:
			case 6:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally6();
				}
				break;
			case -9:
			case 7:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally7();
				}
				break;
			case -10:
			case 8:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally8();
				}
				break;
			case -11:
			case 9:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally9();
				}
				break;
			case -12:
			case 10:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally10();
				}
				break;
			case -13:
			case 11:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally11();
				}
				break;
			case -14:
			case 12:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally12();
				}
				break;
			case -15:
			case 13:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally13();
				}
				break;
			case -16:
			case 14:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally14();
				}
				break;
			case -17:
			case 15:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally15();
				}
				break;
			case -18:
			case 16:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally16();
				}
				break;
			case -19:
			case 17:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally17();
				}
				break;
			case -20:
			case 18:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally18();
				}
				break;
			case -21:
			case 19:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally19();
				}
				break;
			case -22:
			case 20:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally20();
				}
				break;
			case -23:
			case 21:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally21();
				}
				break;
			case -24:
			case 22:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally22();
				}
				break;
			case -25:
			case 23:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally23();
				}
				break;
			case -26:
			case 24:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally24();
				}
				break;
			case -27:
			case 25:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally25();
				}
				break;
			case -28:
			case 26:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally26();
				}
				break;
			case -29:
			case 27:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally27();
				}
				break;
			case -30:
			case 28:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally28();
				}
				break;
			case -31:
			case 29:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally29();
				}
				break;
			case -32:
			case 30:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally30();
				}
				break;
			case -33:
			case 31:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally31();
				}
				break;
			case -34:
			case 32:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally32();
				}
				break;
			case -35:
			case 33:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally33();
				}
				break;
			case -36:
			case 34:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally34();
				}
				break;
			}
			_003C_003E8__1 = null;
			_003CpinsWere_003E5__2 = null;
			_003CkeysWere_003E5__4 = null;
			_003Cabout_003E5__5 = null;
			_003Cslider_003E5__6 = null;
			_003Cspanish_003E5__7 = null;
			_003Cchinese_003E5__8 = null;
			_003C_003E7__wrap10 = null;
			_003Cbound_003E5__16 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cf3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cf8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cfe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e66: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ef4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0faa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0faf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fb6: Unknown result type (might be due to invalid IL or missing references)
			//IL_1070: Unknown result type (might be due to invalid IL or missing references)
			//IL_137c: Unknown result type (might be due to invalid IL or missing references)
			//IL_13f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_1770: Unknown result type (might be due to invalid IL or missing references)
			//IL_1804: Unknown result type (might be due to invalid IL or missing references)
			//IL_189b: Unknown result type (might be due to invalid IL or missing references)
			//IL_18a5: Invalid comparison between Unknown and I4
			//IL_1969: Unknown result type (might be due to invalid IL or missing references)
			//IL_19e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_1a88: Unknown result type (might be due to invalid IL or missing references)
			//IL_1c2d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1cf8: Unknown result type (might be due to invalid IL or missing references)
			//IL_1cfe: Invalid comparison between Unknown and I4
			//IL_1d26: Unknown result type (might be due to invalid IL or missing references)
			//IL_1d3b: Unknown result type (might be due to invalid IL or missing references)
			//IL_1d41: Invalid comparison between Unknown and I4
			//IL_1b0a: Unknown result type (might be due to invalid IL or missing references)
			//IL_1b2b: Unknown result type (might be due to invalid IL or missing references)
			//IL_1b40: Unknown result type (might be due to invalid IL or missing references)
			//IL_1b46: Unknown result type (might be due to invalid IL or missing references)
			//IL_1b74: Unknown result type (might be due to invalid IL or missing references)
			//IL_1296: Unknown result type (might be due to invalid IL or missing references)
			//IL_162b: Unknown result type (might be due to invalid IL or missing references)
			//IL_18b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_18c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_18d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_2200: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				string text2;
				float num;
				float num2;
				float num3;
				float num4;
				Rect value;
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E8__1 = new _003C_003Ec__DisplayClass27_0();
					_003C_003E8__1.menu = menu;
					_003C_003E8__1.menu.Build();
					Theme.Build();
					_003CpinsWere_003E5__2 = Kit.Pins.Value;
					_003CkeyWas_003E5__3 = Kit.MenuKey.Value;
					_003CkeysWere_003E5__4 = Keys.All.Select((Keys.Bound k) => k.Key.Value).ToArray();
					_003Cabout_003E5__5 = _003C_003E8__1.menu.tabs[_003C_003E8__1.menu.AboutTab];
					check("menu: every setting has a row on a page", delegate
					{
						List<string> list = new List<string>();
						foreach (KeyValuePair<FieldInfo, object> item in Settings.Fields())
						{
							if (!_003C_003E8__1.menu.controls.ContainsKey(item.Key.Name) && !NoControl.Contains(item.Key.Name))
							{
								list.Add(item.Key.Name);
							}
						}
						if (list.Count > 0)
						{
							throw new Exception("no row for: " + string.Join(", ", list.ToArray()));
						}
						return Settings.Count + " settings, " + _003C_003E8__1.menu.controls.Count + " rows on " + _003C_003E8__1.menu.tabs.Length + " pages";
					});
					check("menu: every row has a name and an explanation, in English, Spanish and Chinese", delegate
					{
						_003C_003Ec__DisplayClass27_1 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass27_1
						{
							bad = new List<string>()
						};
						Action<string, string> action = delegate(string what, string text)
						{
							if (string.IsNullOrEmpty(text) || !Lang.Has(text))
							{
								CS_0024_003C_003E8__locals0.bad.Add(what + " \"" + text + "\"");
							}
						};
						string[] tabs = _003C_003E8__1.menu.tabs;
						foreach (string arg in tabs)
						{
							action("page", arg);
						}
						foreach (Menu.Group group in _003C_003E8__1.menu.groups)
						{
							action("card", group.Title);
							foreach (Menu.Control item2 in group.Items)
							{
								if (item2.Id != "credits")
								{
									if (item2.Buttons == null)
									{
										action("name of " + item2.Id, item2.Label);
									}
									else if (!item2.Raw && item2.ButtonNow == null)
									{
										tabs = item2.Buttons;
										foreach (string arg2 in tabs)
										{
											action("button of " + item2.Id, arg2);
										}
									}
								}
								action("explanation of " + item2.Id, item2.Hint);
							}
						}
						foreach (Keys.Bound item3 in Keys.All)
						{
							action("key", Keys.Label(item3));
						}
						if (CS_0024_003C_003E8__locals0.bad.Count > 0)
						{
							throw new Exception(CS_0024_003C_003E8__locals0.bad.Count + " without: " + string.Join("; ", CS_0024_003C_003E8__locals0.bad.Take(6).ToArray()));
						}
						return Lang.Count + " pieces of text";
					});
					_003Clang_003E5__9 = 0;
					goto IL_0498;
				case 1:
					_003C_003E1__state = -3;
					goto IL_023e;
				case 2:
					_003C_003E1__state = -4;
					goto IL_0330;
				case 3:
					_003C_003E1__state = -5;
					goto IL_05d7;
				case 4:
					_003C_003E1__state = -6;
					goto IL_0776;
				case 5:
					_003C_003E1__state = -7;
					goto IL_0853;
				case 6:
					_003C_003E1__state = -8;
					goto IL_08c1;
				case 7:
					_003C_003E1__state = -9;
					goto IL_0a4b;
				case 8:
					_003C_003E1__state = -10;
					goto IL_0aeb;
				case 9:
					_003C_003E1__state = -11;
					goto IL_0bce;
				case 10:
					_003C_003E1__state = -12;
					goto IL_0ca2;
				case 11:
					_003C_003E1__state = -13;
					goto IL_0d4d;
				case 12:
					_003C_003E1__state = -14;
					goto IL_0e2f;
				case 13:
					_003C_003E1__state = -15;
					goto IL_0eae;
				case 14:
					_003C_003E1__state = -16;
					goto IL_0f3c;
				case 15:
					_003C_003E1__state = -17;
					goto IL_0ffe;
				case 16:
					_003C_003E1__state = -18;
					goto IL_10b8;
				case 17:
					_003C_003E1__state = -19;
					goto IL_1198;
				case 18:
					_003C_003E1__state = -20;
					goto IL_12e5;
				case 19:
					_003C_003E1__state = -21;
					goto IL_1349;
				case 20:
					_003C_003E1__state = -22;
					goto IL_13cb;
				case 21:
					_003C_003E1__state = -23;
					goto IL_1440;
				case 22:
					_003C_003E1__state = -24;
					goto IL_14a4;
				case 23:
					_003C_003E1__state = -25;
					goto IL_15d4;
				case 24:
					_003C_003E1__state = -26;
					goto IL_1673;
				case 25:
					_003C_003E1__state = -27;
					goto IL_1754;
				case 26:
					_003C_003E1__state = -28;
					goto IL_17b8;
				case 27:
					_003C_003E1__state = -29;
					goto IL_184c;
				case 28:
					_003C_003E1__state = -30;
					goto IL_1918;
				case 29:
					_003C_003E1__state = -31;
					goto IL_19b1;
				case 30:
					_003C_003E1__state = -32;
					goto IL_1a2f;
				case 31:
					_003C_003E1__state = -33;
					goto IL_1bbc;
				case 32:
					_003C_003E1__state = -34;
					goto IL_1c75;
				case 33:
					_003C_003E1__state = -35;
					goto IL_1e0a;
				case 34:
					{
						_003C_003E1__state = -36;
						break;
					}
					IL_035c:
					if (_003Ci_003E5__10 < _003C_003E8__1.menu.tabs.Length)
					{
						_003C_003E8__1.menu.GoTo(_003Ci_003E5__10);
						_003C_003E8__1.menu.lastError = null;
						_003C_003E7__wrap10 = Frames(5).GetEnumerator();
						_003C_003E1__state = -3;
						goto IL_023e;
					}
					text2 = Lang.T("Put everything back to normal");
					if (_003Clang_003E5__9 > 0)
					{
						Expect(Lang.Missing.Count == 0, Lang.Missing.Count + " not translated: " + string.Join(" | ", Lang.Missing.Take(6).ToArray()));
						Expect(text2 != "Put everything back to normal", "the text did not change");
					}
					detail = _003C_003E8__1.menu.tabs.Length + " pages" + ((_003Clang_003E5__9 > 0) ? ("; \"Put everything back to normal\" is \"" + text2 + "\"") : "");
					Report(check, "menu: every page draws in " + ((_003Clang_003E5__9 == 0) ? "English" : ((_003Clang_003E5__9 == 1) ? "Spanish" : "Chinese")) + ((_003Clang_003E5__9 > 0) ? ", nothing left in English" : ""));
					_003Clang_003E5__9++;
					goto IL_0498;
					IL_1d92:
					_003Cbound_003E5__16 = null;
					goto IL_1d99;
					IL_0498:
					if (_003Clang_003E5__9 < Lang.Names.Length)
					{
						Lang.Choose(_003Clang_003E5__9);
						Lang.Missing.Clear();
						_003C_003E8__1.menu.open = true;
						_003Ci_003E5__10 = 0;
						goto IL_035c;
					}
					Lang.Choose(0);
					_003C_003E8__1.sw = First(_003C_003E8__1.menu, (Menu.Control c) => c.On != null && typeof(State).GetField(c.Id) != null);
					_003Cslider_003E5__6 = First(_003C_003E8__1.menu, (Menu.Control c) => c.Value != null);
					if (_003C_003E8__1.sw == null)
					{
						check("menu: a switch to test with", delegate
						{
							throw new Exception("the menu has no switch");
						});
						return false;
					}
					_003C_003E8__1.menu.open = true;
					_003C_003E8__1.menu.Find(_003C_003E8__1.sw.Label);
					_003C_003E7__wrap10 = Frames(4).GetEnumerator();
					_003C_003E1__state = -5;
					goto IL_05d7;
					IL_023e:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.lastError == null, _003C_003E8__1.menu.tabs[_003Ci_003E5__10] + ": " + _003C_003E8__1.menu.lastError);
					shot(((_003Clang_003E5__9 == 0) ? "" : ((_003Clang_003E5__9 == 1) ? "es_" : "zh_")) + _003C_003E8__1.menu.tabs[_003Ci_003E5__10]);
					_003C_003E7__wrap10 = Frames(2).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_0330;
					IL_0ca2:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 10;
						return true;
					}
					_003C_003Em__Finally10();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.sw.Where.width > 10f, "the switch was not drawn");
					_003Cmid_003E5__12 = _003C_003E8__1.sw.Where.center;
					MouseTo(_003Cmid_003E5__12);
					_003C_003E7__wrap10 = Frames(6).GetEnumerator();
					_003C_003E1__state = -13;
					goto IL_0d4d;
					IL_0f3c:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 14;
						return true;
					}
					_003C_003Em__Finally14();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.sw.On() == _003Cwas_003E5__13, "a second click did not flip it back");
					_003Csquare_003E5__14 = new Vector2(_003C_003E8__1.sw.Where.x + 7f * Menu.Scale, _003Cmid_003E5__12.y);
					_003C_003E7__wrap10 = Click(_003Csquare_003E5__14).GetEnumerator();
					_003C_003E1__state = -17;
					goto IL_0ffe;
					IL_0d4d:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 11;
						return true;
					}
					_003C_003Em__Finally11();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.HoveredId == _003C_003E8__1.sw.Id, "the mouse is over \"" + _003C_003E8__1.sw.Id + "\" and the menu thinks it is over \"" + _003C_003E8__1.menu.HoveredId + "\"");
					shot("Hover");
					_003C_003E7__wrap10 = Frames(2).GetEnumerator();
					_003C_003E1__state = -14;
					goto IL_0e2f;
					IL_1e0a:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 33;
						return true;
					}
					_003C_003Em__Finally33();
					_003C_003E7__wrap10 = null;
					Expect(Keys.Active().Contains(_003C_003E8__1.sw.Label), "the line says \"" + Keys.Active() + "\"");
					shot("Active");
					_003C_003E7__wrap10 = Frames(2).GetEnumerator();
					_003C_003E1__state = -36;
					break;
					IL_13cb:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 20;
						return true;
					}
					_003C_003Em__Finally20();
					_003C_003E7__wrap10 = null;
					MouseTo(new Vector2(_003Cto_003E5__20, _003Cy_003E5__18));
					_003C_003E7__wrap10 = Frames(4).GetEnumerator();
					_003C_003E1__state = -23;
					goto IL_1440;
					IL_0eae:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 13;
						return true;
					}
					_003C_003Em__Finally13();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.sw.On() != _003Cwas_003E5__13, "a click on the switch did not flip it");
					_003C_003E7__wrap10 = Click(_003Cmid_003E5__12).GetEnumerator();
					_003C_003E1__state = -16;
					goto IL_0f3c;
					IL_1673:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 24;
						return true;
					}
					_003C_003Em__Finally24();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.tab == _003Clang_003E5__9, "a click on \"" + _003Cabout_003E5__5 + "\" opened page " + _003C_003E8__1.menu.tab);
					detail = "a click on \"" + _003Cabout_003E5__5 + "\" opens it";
					Report(check, "real mouse: the list of pages");
					_003C_003E8__1.menu.open = false;
					_003C_003E7__wrap10 = Frames(4).GetEnumerator();
					_003C_003E1__state = -27;
					goto IL_1754;
					IL_0e2f:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 12;
						return true;
					}
					_003C_003Em__Finally12();
					_003C_003E7__wrap10 = null;
					_003Cwas_003E5__13 = _003C_003E8__1.sw.On();
					_003C_003E7__wrap10 = Click(_003Cmid_003E5__12).GetEnumerator();
					_003C_003E1__state = -15;
					goto IL_0eae;
					IL_05d7:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 3;
						return true;
					}
					_003C_003Em__Finally3();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.matches.Contains(_003C_003E8__1.sw), "\"" + _003C_003E8__1.sw.Label + "\" did not find its own row");
					Lang.Choose(1);
					_003Cspanish_003E5__7 = Lang.T(_003C_003E8__1.sw.Label);
					Lang.Choose(2);
					_003Cchinese_003E5__8 = Lang.T(_003C_003E8__1.sw.Label);
					Lang.Choose(0);
					_003C_003E8__1.menu.Find(_003Cspanish_003E5__7);
					Expect(_003C_003E8__1.menu.matches.Contains(_003C_003E8__1.sw), "the Spanish name \"" + _003Cspanish_003E5__7 + "\" did not find it");
					_003C_003E8__1.menu.Find(_003Cchinese_003E5__8);
					Expect(_003C_003E8__1.menu.matches.Contains(_003C_003E8__1.sw), "the Chinese name \"" + _003Cchinese_003E5__8 + "\" did not find it");
					_003C_003E8__1.menu.Find("zzqqxx");
					_003C_003E7__wrap10 = Frames(3).GetEnumerator();
					_003C_003E1__state = -6;
					goto IL_0776;
					IL_1754:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 25;
						return true;
					}
					_003C_003Em__Finally25();
					_003C_003E7__wrap10 = null;
					_003C_003E7__wrap10 = Tap(_003CkeyWas_003E5__3).GetEnumerator();
					_003C_003E1__state = -28;
					goto IL_17b8;
					IL_0776:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 4;
						return true;
					}
					_003C_003Em__Finally4();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.matches.Count == 0 && _003C_003E8__1.menu.lastError == null, "a word that is nowhere found " + _003C_003E8__1.menu.matches.Count + " rows");
					_003C_003E8__1.menu.Find(_003C_003E8__1.sw.Label);
					_003C_003E7__wrap10 = Frames(3).GetEnumerator();
					_003C_003E1__state = -7;
					goto IL_0853;
					IL_17b8:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 26;
						return true;
					}
					_003C_003Em__Finally26();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.open, ((KeyCode)_003CkeyWas_003E5__3).ToString() + " did not open the menu");
					_003C_003E7__wrap10 = Tap(_003CkeyWas_003E5__3).GetEnumerator();
					_003C_003E1__state = -29;
					goto IL_184c;
					IL_10b8:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 16;
						return true;
					}
					_003C_003Em__Finally16();
					_003C_003E7__wrap10 = null;
					Expect(!_003C_003E8__1.menu.pinned.Contains(_003C_003E8__1.sw.Id), "a second click on the square did not take the pin off");
					detail = "hover names \"" + _003C_003E8__1.sw.Id + "\"; two clicks flip it and flip it back; the square pins and unpins";
					Report(check, "real mouse: hover, a switch, the pin square");
					if (_003Cslider_003E5__6 != null)
					{
						_003C_003E7__wrap10 = Show(_003C_003E8__1.menu, _003Cslider_003E5__6).GetEnumerator();
						_003C_003E1__state = -19;
						goto IL_1198;
					}
					goto IL_1579;
					IL_184c:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 27;
						return true;
					}
					_003C_003Em__Finally27();
					_003C_003E7__wrap10 = null;
					Expect(!_003C_003E8__1.menu.open, ((KeyCode)_003CkeyWas_003E5__3).ToString() + " did not close the menu");
					_003Cother_003E5__15 = (KeyCode)(((int)_003CkeyWas_003E5__3 == 288) ? 289 : 288);
					_003C_003E8__1.menu.SetKey(_003Cother_003E5__15);
					_003C_003E7__wrap10 = Tap(_003Cother_003E5__15).GetEnumerator();
					_003C_003E1__state = -30;
					goto IL_1918;
					IL_0853:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 5;
						return true;
					}
					_003C_003Em__Finally5();
					_003C_003E7__wrap10 = null;
					shot("Search");
					_003C_003E7__wrap10 = Frames(2).GetEnumerator();
					_003C_003E1__state = -8;
					goto IL_08c1;
					IL_1198:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 17;
						return true;
					}
					_003C_003Em__Finally17();
					_003C_003E7__wrap10 = null;
					_003Cbefore_003E5__17 = _003Cslider_003E5__6.Value();
					_003Cy_003E5__18 = _003Cslider_003E5__6.Where.y + 33f * Menu.Scale;
					num = _003Cslider_003E5__6.Where.x + 16f * Menu.Scale;
					num2 = _003Cslider_003E5__6.Where.xMax - 10f * Menu.Scale;
					num3 = Mathf.InverseLerp(_003Cslider_003E5__6.Min, _003Cslider_003E5__6.Max, _003Cbefore_003E5__17);
					_003Cfrom_003E5__19 = Mathf.Lerp(num + 7f, num2 - 7f, num3);
					_003Cto_003E5__20 = Mathf.Lerp(num, num2, (num3 < 0.5f) ? 0.8f : 0.2f);
					MouseTo(new Vector2(_003Cfrom_003E5__19, _003Cy_003E5__18));
					_003C_003E7__wrap10 = Frames(5).GetEnumerator();
					_003C_003E1__state = -20;
					goto IL_12e5;
					IL_08c1:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 6;
						return true;
					}
					_003C_003Em__Finally6();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.menu.Find("");
					detail = "\"" + _003C_003E8__1.sw.Label + "\", \"" + _003Cspanish_003E5__7 + "\" and \"" + _003Cchinese_003E5__8 + "\" all find the same row";
					Report(check, "menu: the search finds a row by its name in any of the three languages");
					_003C_003E8__1.menu.pinned.Clear();
					_003C_003E8__1.menu.TogglePin(_003C_003E8__1.sw);
					if (_003Cslider_003E5__6 != null)
					{
						_003C_003E8__1.menu.TogglePin(_003Cslider_003E5__6);
					}
					Expect(_003C_003E8__1.menu.pinned.Contains(_003C_003E8__1.sw.Id) && Kit.Pins.Value.Contains(_003C_003E8__1.sw.Id), "the pin was not kept");
					_003C_003E8__1.menu.GoTo(0);
					_003C_003E7__wrap10 = Frames(5).GetEnumerator();
					_003C_003E1__state = -9;
					goto IL_0a4b;
					IL_1918:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 28;
						return true;
					}
					_003C_003Em__Finally28();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.open, "after the change, " + ((KeyCode)_003Cother_003E5__15).ToString() + " did not open the menu");
					_003C_003E7__wrap10 = Tap(_003CkeyWas_003E5__3).GetEnumerator();
					_003C_003E1__state = -31;
					goto IL_19b1;
					IL_19b1:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 29;
						return true;
					}
					_003C_003Em__Finally29();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.open, "the old key still works");
					_003C_003E7__wrap10 = Tap(_003Cother_003E5__15).GetEnumerator();
					_003C_003E1__state = -32;
					goto IL_1a2f;
					IL_1a2f:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 30;
						return true;
					}
					_003C_003Em__Finally30();
					_003C_003E7__wrap10 = null;
					Expect(!_003C_003E8__1.menu.open, ((KeyCode)_003Cother_003E5__15).ToString() + " did not close the menu");
					_003C_003E8__1.menu.SetKey(_003CkeyWas_003E5__3);
					detail = ((KeyCode)_003CkeyWas_003E5__3).ToString() + " opens and closes; changed to " + ((KeyCode)_003Cother_003E5__15).ToString() + " and back";
					Report(check, "real keys: the menu key, and changing it");
					_003Cbound_003E5__16 = Keys.All.Find((Keys.Bound k) => k.Id == _003C_003E8__1.sw.Id);
					if (_003Cbound_003E5__16 != null)
					{
						_003Cshortcut_003E5__21 = (KeyCode)290;
						_003C_003E8__1.menu.SetShortcut(_003C_003E8__1.sw.Id, _003Cshortcut_003E5__21);
						Expect(_003Cbound_003E5__16.Key.Value == _003Cshortcut_003E5__21, "the key was not taken");
						_003Cbefore_003E5__22 = _003C_003E8__1.sw.On();
						_003C_003E7__wrap10 = Tap(_003Cshortcut_003E5__21).GetEnumerator();
						_003C_003E1__state = -33;
						goto IL_1bbc;
					}
					goto IL_1d92;
					IL_1440:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 21;
						return true;
					}
					_003C_003Em__Finally21();
					_003C_003E7__wrap10 = null;
					MouseUp();
					_003C_003E7__wrap10 = Frames(5).GetEnumerator();
					_003C_003E1__state = -24;
					goto IL_14a4;
					IL_1349:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 19;
						return true;
					}
					_003C_003Em__Finally19();
					_003C_003E7__wrap10 = null;
					MouseTo(new Vector2((_003Cfrom_003E5__19 + _003Cto_003E5__20) / 2f, _003Cy_003E5__18));
					_003C_003E7__wrap10 = Frames(3).GetEnumerator();
					_003C_003E1__state = -22;
					goto IL_13cb;
					IL_12e5:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 18;
						return true;
					}
					_003C_003Em__Finally18();
					_003C_003E7__wrap10 = null;
					MouseDown();
					_003C_003E7__wrap10 = Frames(3).GetEnumerator();
					_003C_003E1__state = -21;
					goto IL_1349;
					IL_0a4b:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 7;
						return true;
					}
					_003C_003Em__Finally7();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.lastError == null, "Home: " + _003C_003E8__1.menu.lastError);
					shot("Home_pinned");
					_003C_003E7__wrap10 = Frames(2).GetEnumerator();
					_003C_003E1__state = -10;
					goto IL_0aeb;
					IL_1bbc:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 31;
						return true;
					}
					_003C_003Em__Finally31();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.sw.On() != _003Cbefore_003E5__22, ((KeyCode)_003Cshortcut_003E5__21).ToString() + " did not flip \"" + _003C_003E8__1.sw.Label + "\" with the menu closed");
					_003C_003E7__wrap10 = Tap(_003Cshortcut_003E5__21).GetEnumerator();
					_003C_003E1__state = -34;
					goto IL_1c75;
					IL_0aeb:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 8;
						return true;
					}
					_003C_003Em__Finally8();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.menu.TogglePin(_003C_003E8__1.sw);
					if (_003Cslider_003E5__6 != null)
					{
						_003C_003E8__1.menu.TogglePin(_003Cslider_003E5__6);
					}
					Expect(_003C_003E8__1.menu.pinned.Count == 0, "the pin could not be taken off");
					detail = "pinned, shown on Home, taken off again";
					Report(check, "menu: a row can be pinned to Home and taken off");
					_003Clang_003E5__9 = 0;
					goto IL_0bfa;
					IL_1c75:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 32;
						return true;
					}
					_003C_003Em__Finally32();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.sw.On() == _003Cbefore_003E5__22, ((KeyCode)_003Cshortcut_003E5__21).ToString() + " did not flip it back");
					_003C_003E8__1.menu.SetShortcut(_003C_003E8__1.sw.Id, (KeyCode)8);
					Expect((int)_003Cbound_003E5__16.Key.Value == 0, "Backspace did not take the key away");
					_003C_003E8__1.menu.SetShortcut(_003C_003E8__1.sw.Id, _003CkeyWas_003E5__3);
					Expect((int)_003Cbound_003E5__16.Key.Value == 0, "the menu key could be given a second job");
					detail = ((KeyCode)_003Cshortcut_003E5__21).ToString() + " flips \"" + _003C_003E8__1.sw.Label + "\" and flips it back; Backspace removes the key";
					Report(check, "real keys: a shortcut key for a switch");
					goto IL_1d92;
					IL_14a4:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 22;
						return true;
					}
					_003C_003Em__Finally22();
					_003C_003E7__wrap10 = null;
					num4 = _003Cslider_003E5__6.Value();
					Expect(num4 != _003Cbefore_003E5__17, "dragging the slider left it at " + _003Cbefore_003E5__17);
					_003Cslider_003E5__6.Move(_003Cbefore_003E5__17);
					detail = _003Cslider_003E5__6.Id + ": " + _003Cslider_003E5__6.Shown(_003Cbefore_003E5__17) + " dragged to " + _003Cslider_003E5__6.Shown(num4);
					Report(check, "real mouse: a slider is dragged");
					goto IL_1579;
					IL_0330:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap10 = null;
					_003Ci_003E5__10++;
					goto IL_035c;
					IL_0bfa:
					if (_003Clang_003E5__9 < 5 && !Focused)
					{
						Focus();
						_003C_003E7__wrap10 = Frames(30).GetEnumerator();
						_003C_003E1__state = -11;
						goto IL_0bce;
					}
					if (!Focused)
					{
						check("menu: the real mouse and keyboard", delegate
						{
							throw new Exception("the game is not the window in front, so real clicks and keys cannot reach it");
						});
						goto IL_1d99;
					}
					_003C_003E7__wrap10 = Show(_003C_003E8__1.menu, _003C_003E8__1.sw).GetEnumerator();
					_003C_003E1__state = -12;
					goto IL_0ca2;
					IL_1d99:
					_003C_003E8__1.menu.open = false;
					Settings.Blank();
					_003C_003E8__1.sw.Turn(obj: true);
					_003C_003E7__wrap10 = Frames(4).GetEnumerator();
					_003C_003E1__state = -35;
					goto IL_1e0a;
					IL_0ffe:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 15;
						return true;
					}
					_003C_003Em__Finally15();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.pinned.Contains(_003C_003E8__1.sw.Id), "a click on the square did not pin the row");
					Expect(_003C_003E8__1.sw.On() == _003Cwas_003E5__13, "the click on the square also flipped the switch");
					_003C_003E7__wrap10 = Click(_003Csquare_003E5__14).GetEnumerator();
					_003C_003E1__state = -18;
					goto IL_10b8;
					IL_0bce:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 9;
						return true;
					}
					_003C_003Em__Finally9();
					_003C_003E7__wrap10 = null;
					_003Clang_003E5__9++;
					goto IL_0bfa;
					IL_1579:
					_003Clang_003E5__9 = _003C_003E8__1.menu.AboutTab;
					_003C_003E7__wrap10 = Frames(3).GetEnumerator();
					_003C_003E1__state = -25;
					goto IL_15d4;
					IL_15d4:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 23;
						return true;
					}
					_003C_003Em__Finally23();
					_003C_003E7__wrap10 = null;
					Expect(_003C_003E8__1.menu.tabWhere.TryGetValue(_003Clang_003E5__9, out value) && value.width > 10f, "the list of pages was not drawn");
					_003C_003E7__wrap10 = Click(value.center).GetEnumerator();
					_003C_003E1__state = -26;
					goto IL_1673;
				}
				if (_003C_003E7__wrap10.MoveNext())
				{
					_ = _003C_003E7__wrap10.Current;
					_003C_003E2__current = null;
					_003C_003E1__state = 34;
					return true;
				}
				_003C_003Em__Finally34();
				_003C_003E7__wrap10 = null;
				_003C_003E8__1.sw.Turn(obj: false);
				Expect(Keys.Active().Length == 0, "with everything off the line says \"" + Keys.Active() + "\"");
				detail = "\"" + _003C_003E8__1.sw.Label + "\" is listed while on, and nothing while off";
				Report(check, "menu: the line that says what is switched on");
				object[] values = Settings.Snapshot();
				Settings.Paused = false;
				try
				{
					Settings.Remember.Value = true;
					_003C_003E8__1.sw.Turn(obj: true);
					float num5 = 0f;
					if (_003Cslider_003E5__6 != null)
					{
						num5 = ((_003Cslider_003E5__6.Value() == _003Cslider_003E5__6.Max) ? _003Cslider_003E5__6.Min : _003Cslider_003E5__6.Max);
						_003Cslider_003E5__6.Move(num5);
					}
					Settings.Save();
					Settings.Blank();
					Expect(!_003C_003E8__1.sw.On(), "the switch could not be put back to normal");
					Settings.Reload();
					Expect(_003C_003E8__1.sw.On(), "with remembering on, the switch did not come back");
					if (_003Cslider_003E5__6 != null)
					{
						Expect(Mathf.Approximately(_003Cslider_003E5__6.Value(), num5), "with remembering on, the slider came back as " + _003Cslider_003E5__6.Value());
					}
					Settings.Remember.Value = false;
					Settings.Save();
					Settings.Blank();
					Settings.Reload();
					Expect(!_003C_003E8__1.sw.On(), "with remembering off, the switch still came back");
					detail = "on: a switch" + ((_003Cslider_003E5__6 != null) ? " and a slider" : "") + " come back after a restart; off: they start from normal";
				}
				finally
				{
					Settings.Restore(values);
					Settings.Paused = true;
					Settings.Blank();
				}
				Report(check, "menu: remember my switches and sliders");
				_003C_003E8__1.sw.Turn(obj: true);
				if (_003Cslider_003E5__6 != null)
				{
					_003Cslider_003E5__6.Move((_003Cslider_003E5__6.Value() == _003Cslider_003E5__6.Max) ? _003Cslider_003E5__6.Min : _003Cslider_003E5__6.Max);
				}
				_003C_003E8__1.menu.controls["normal"].Press(0);
				int num6 = 0;
				foreach (KeyValuePair<FieldInfo, object> item4 in Settings.Fields())
				{
					if (!object.Equals(item4.Key.GetValue(null), item4.Value))
					{
						num6++;
					}
				}
				Expect(num6 == 0, num6 + " settings were not put back");
				detail = "every switch and slider is back where it starts";
				Report(check, "menu: put everything back to normal");
				Kit.Pins.Value = (string)_003CpinsWere_003E5__2;
				Kit.MenuKey.Value = _003CkeyWas_003E5__3;
				for (int i = 0; i < _003CkeysWere_003E5__4.Length; i++)
				{
					Keys.All[i].Key.Value = _003CkeysWere_003E5__4[i];
				}
				_003C_003E8__1.menu.pinned.Clear();
				_003C_003E8__1.menu.open = false;
				Lang.Choose(0);
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
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally3()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally4()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally5()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally6()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally7()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally8()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally9()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally10()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally11()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally12()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally13()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally14()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally15()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally16()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally17()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally18()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally19()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally20()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally21()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally22()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally23()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally24()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally25()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally26()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally27()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally28()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally29()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally30()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally31()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally32()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally33()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally34()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
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
			_003CRun_003Ed__27 _003CRun_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CRun_003Ed__ = this;
			}
			else
			{
				_003CRun_003Ed__ = new _003CRun_003Ed__27(0);
			}
			_003CRun_003Ed__.menu = _003C_003E3__menu;
			_003CRun_003Ed__.check = _003C_003E3__check;
			_003CRun_003Ed__.shot = _003C_003E3__shot;
			return _003CRun_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CShow_003Ed__26 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Menu menu;

		public Menu _003C_003E3__menu;

		private Menu.Control c;

		public Menu.Control _003C_003E3__c;

		private IEnumerator _003C_003E7__wrap1;

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
		public _003CShow_003Ed__26(int _003C_003E1__state)
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
			_003C_003E7__wrap1 = null;
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
					menu.open = true;
					menu.GoTo(c.In.Page);
					_003C_003E7__wrap1 = Frames(6).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					_ = _003C_003E7__wrap1.Current;
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
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
			if (_003C_003E7__wrap1 is IDisposable disposable)
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
			_003CShow_003Ed__26 _003CShow_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CShow_003Ed__ = this;
			}
			else
			{
				_003CShow_003Ed__ = new _003CShow_003Ed__26(0);
			}
			_003CShow_003Ed__.menu = _003C_003E3__menu;
			_003CShow_003Ed__.c = _003C_003E3__c;
			return _003CShow_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CTap_003Ed__20 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private KeyCode key;

		public KeyCode _003C_003E3__key;

		private byte _003Cvk_003E5__2;

		private IEnumerator _003C_003E7__wrap2;

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
		public _003CTap_003Ed__20(int _003C_003E1__state)
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
			case 1:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -4:
			case 2:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally2();
				}
				break;
			}
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003Cvk_003E5__2 = Vk(key);
					keybd_event(_003Cvk_003E5__2, 0, 0u, UIntPtr.Zero);
					_003C_003E7__wrap2 = Frames(4).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_008e;
				case 1:
					_003C_003E1__state = -3;
					goto IL_008e;
				case 2:
					{
						_003C_003E1__state = -4;
						break;
					}
					IL_008e:
					if (_003C_003E7__wrap2.MoveNext())
					{
						_ = _003C_003E7__wrap2.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap2 = null;
					keybd_event(_003Cvk_003E5__2, 0, 2u, UIntPtr.Zero);
					_003C_003E7__wrap2 = Frames(5).GetEnumerator();
					_003C_003E1__state = -4;
					break;
				}
				if (_003C_003E7__wrap2.MoveNext())
				{
					_ = _003C_003E7__wrap2.Current;
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003Em__Finally2();
				_003C_003E7__wrap2 = null;
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
			if (_003C_003E7__wrap2 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 is IDisposable disposable)
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
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			_003CTap_003Ed__20 _003CTap_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CTap_003Ed__ = this;
			}
			else
			{
				_003CTap_003Ed__ = new _003CTap_003Ed__20(0);
			}
			_003CTap_003Ed__.key = _003C_003E3__key;
			return _003CTap_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	internal static readonly HashSet<string> NoControl = new HashSet<string>();

	private static IntPtr window;

	private static string problem;

	private static string detail;

	internal static bool Focused
	{
		get
		{
			GetWindowThreadProcessId(GetForegroundWindow(), out var process);
			return process == (uint)Process.GetCurrentProcess().Id;
		}
	}

	[DllImport("user32.dll")]
	private static extern bool SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);

	[DllImport("user32.dll")]
	private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

	[DllImport("user32.dll")]
	private static extern bool ClientToScreen(IntPtr window, ref Point p);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr window);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr window, out uint process);

	private static IntPtr Window()
	{
		if (window == IntPtr.Zero)
		{
			window = Process.GetCurrentProcess().MainWindowHandle;
		}
		return window;
	}

	internal static void Focus()
	{
		if (!Focused)
		{
			keybd_event(18, 0, 0u, UIntPtr.Zero);
			SetForegroundWindow(Window());
			keybd_event(18, 0, 2u, UIntPtr.Zero);
		}
	}

	internal static void MouseTo(Vector2 p)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Point point = default(Point);
		point.X = Mathf.RoundToInt(p.x);
		point.Y = Mathf.RoundToInt(p.y);
		Point p2 = point;
		ClientToScreen(Window(), ref p2);
		SetCursorPos(p2.X, p2.Y);
	}

	internal static void MouseDown()
	{
		mouse_event(2u, 0u, 0u, 0u, UIntPtr.Zero);
	}

	internal static void MouseUp()
	{
		mouse_event(4u, 0u, 0u, 0u, UIntPtr.Zero);
	}

	[IteratorStateMachine(typeof(_003CFrames_003Ed__17))]
	internal static IEnumerable Frames(int n)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFrames_003Ed__17(-2)
		{
			_003C_003E3__n = n
		};
	}

	[IteratorStateMachine(typeof(_003CClick_003Ed__18))]
	internal static IEnumerable Click(Vector2 p)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CClick_003Ed__18(-2)
		{
			_003C_003E3__p = p
		};
	}

	private static byte Vk(KeyCode key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected I4, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		if ((int)key >= 282 && (int)key <= 293)
		{
			return (byte)(112 + (key - 282));
		}
		if ((int)key >= 97 && (int)key <= 122)
		{
			return (byte)(65 + (key - 97));
		}
		if ((int)key >= 48 && (int)key <= 57)
		{
			return (byte)(48 + (key - 48));
		}
		if ((int)key <= 27)
		{
			if ((int)key == 8)
			{
				return (byte)8;
			}
			if ((int)key == 27)
			{
				return (byte)27;
			}
		}
		else
		{
			if ((int)key == 127)
			{
				return (byte)46;
			}
			switch ((int)key - 277)
			{
			case 0:
				return (byte)45;
			case 1:
				return (byte)36;
			case 2:
				return (byte)35;
			case 3:
				return (byte)33;
			case 4:
				return (byte)34;
			}
		}
		throw new Exception("the test cannot press " + ((KeyCode)key).ToString());
	}

	[IteratorStateMachine(typeof(_003CTap_003Ed__20))]
	internal static IEnumerable Tap(KeyCode key)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTap_003Ed__20(-2)
		{
			_003C_003E3__key = key
		};
	}

	private static void Expect(bool cond, string msg)
	{
		if (!cond && problem == null)
		{
			problem = msg;
		}
	}

	private static void Report(Action<string, Func<string>> check, string name)
	{
		string p = problem;
		string d = detail;
		problem = (detail = null);
		check(name, delegate
		{
			if (p != null)
			{
				throw new Exception(p);
			}
			return d;
		});
	}

	private static Menu.Control First(Menu menu, Func<Menu.Control, bool> want)
	{
		foreach (Menu.Group group in menu.groups)
		{
			if (!(group.Page != menu.tabs[menu.AboutTab]))
			{
				continue;
			}
			foreach (Menu.Control item in group.Items)
			{
				if ((item.Hidden == null || !item.Hidden()) && want(item))
				{
					return item;
				}
			}
		}
		return null;
	}

	[IteratorStateMachine(typeof(_003CShow_003Ed__26))]
	private static IEnumerable Show(Menu menu, Menu.Control c)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CShow_003Ed__26(-2)
		{
			_003C_003E3__menu = menu,
			_003C_003E3__c = c
		};
	}

	[IteratorStateMachine(typeof(_003CRun_003Ed__27))]
	internal static IEnumerable Run(Menu menu, Action<string, Func<string>> check, Action<string> shot)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRun_003Ed__27(-2)
		{
			_003C_003E3__menu = menu,
			_003C_003E3__check = check,
			_003C_003E3__shot = shot
		};
	}
}
