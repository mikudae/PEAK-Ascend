using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Peak;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zorro.Core;
using Object = UnityEngine.Object;
using STATUSTYPE = CharacterAfflictions.STATUSTYPE;
using SETTINGTYPE = RunSettings.SETTINGTYPE;

namespace Summit;

internal static class SelfTest
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_0
	{
		public Menu menu;

		public float spent;

		public float y2;

		public Vector3 ground;

		public float y1;

		public Vector3 from;

		public Vector3 to;

		public bool airborne;

		public float vyAfter;

		public float vyBefore;

		public Vector3 arrived;

		public Vector3 spot;

		public Item give;

		public float fell;

		public float peak;

		public float launchFrom;

		public float topSpeed;

		public Vector3 bringFrom;

		public CharacterAfflictions aff;

		public float hurt;

		public float unhurt;

		public float tumble;

		public float heavy;

		public float light;

		public float heavyAgain;

		public int tags;

		public int fires;

		public int statues;

		public int hazards;

		public int more;

		public float fovWide;

		public float fovBack;

		public float fovGame;

		public float far;

		public bool headShown;

		public float nearAgain;

		public bool headHidden;

		public float near;

		public float camOff;

		public float scoutMoved;

		public float home;

		public bool hudOff;

		public bool hudOn;

		public int changed;

		public int[] wore;

		public int[] back;

		public bool sent;

		public float hungry;

		public float fed;

		public Campfire fire;

		public Vector3 atFire;

		public Vector3 beach;

		public float away;

		public RespawnChest statue;

		public Func<string> _003C_003E9__51;

		internal string _003CBody_003Eb__51()
		{
			Expect(menu.lastError == null, menu.lastError);
			return null;
		}

		internal string _003CBody_003Eb__3()
		{
			Expect(spent > 0.15f, "control failed: stamina did not drop without it (" + F(spent) + ")");
			float currentStamina = Me.data.currentStamina;
			Call(Me, "UseStamina", 0.2f, true, true);
			Expect(Me.data.currentStamina >= currentStamina, "stamina still dropped: " + F(currentStamina) + " -> " + F(Me.data.currentStamina));
			return "a 0.20 use cost " + F(spent) + " without it, 0.00 with it";
		}

		internal string _003CBody_003Eb__13()
		{
			Expect(y2 > ground.y + 8f, "the scout is back on the ground (height " + F(y2 - ground.y) + ")");
			Expect(Mathf.Abs(y2 - y1) < 0.5f, "drifted " + F(y2 - y1) + " m in 1.5 s");
			return "held " + F(y2 - ground.y) + " m up, drift " + F(y2 - y1) + " m";
		}

		internal string _003CBody_003Eb__14()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			float num = Vector3.Distance(from, to);
			Expect(num > 8f, "moved only " + F(num) + " m in 1 s at speed " + State.FlySpeed);
			Vector3 val = to - from;
			float num2 = Vector3.Dot(val.normalized, Me.data.lookDirection);
			Expect(num2 > 0.9f, "went off course (" + F(num2) + ")");
			return F(num) + " m in 1 s at speed " + State.FlySpeed.ToString("0");
		}

		internal string _003CBody_003Eb__17()
		{
			Expect(airborne, "the scout was already on the ground");
			Expect(Me.data.sinceJump < 0.5f, "the game refused the jump");
			Expect(vyAfter > vyBefore + 2f, "no upward kick: " + F(vyBefore) + " -> " + F(vyAfter) + " m/s");
			return "falling at " + F(vyBefore) + " m/s, then " + F(vyAfter) + " m/s";
		}

		internal string _003CBody_003Eb__18()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			float num = Vector2.Distance(new Vector2(arrived.x, arrived.z), new Vector2(spot.x, spot.z));
			Expect(num < 2f, "ended " + F(num) + " m away from the target");
			return "landed " + F(num) + " m from the target";
		}

		internal string _003CBody_003Eb__19()
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Expected O, but got Unknown
			menu.LoadItems();
			Expect(menu.items.Count > 50, "only " + menu.items.Count + " items");
			int num = 0;
			foreach (Picker.Entry item in menu.items)
			{
				Item val = (Item)item.What;
				if ((Object)(object)item.Icon != (Object)null)
				{
					num++;
				}
				if ((Object)(object)give == (Object)null && val.UIData.canPocket && ((Object)val).name == "Bandages")
				{
					give = val;
				}
			}
			foreach (Picker.Entry item2 in menu.items)
			{
				if ((Object)(object)give == (Object)null && ((Item)item2.What).UIData.canPocket && !(item2.What is Backpack))
				{
					give = (Item)item2.What;
				}
			}
			return menu.items.Count + " items, " + num + " with an icon";
		}

		internal string _003CBody_003Eb__20()
		{
			Expect(Player.localPlayer.HasInAnySlot(give.itemID), ((Object)give).name + " never reached the inventory");
			return ((Object)give).name;
		}

		internal string _003CBody_003Eb__21()
		{
			Expect(fell > 1f, "the scout did not go down (fall time " + F(fell) + ")");
			return "ragdolled for " + F(fell) + " s";
		}

		internal string _003CBody_003Eb__22()
		{
			Expect(peak - launchFrom > 5f, "rose only " + F(peak - launchFrom) + " m");
			return "asked for 25 m/s, left at " + F(topSpeed) + " m/s, rose " + F(peak - launchFrom) + " m";
		}

		internal string _003CBody_003Eb__24()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			float num = Vector3.Distance(bringFrom, Me.Center);
			Expect(num > 1f, "the scout did not move (" + F(num) + " m)");
			return "moved " + F(num) + " m";
		}

		internal string _003CBody_003Eb__25()
		{
			Expect(aff.AddStatus((STATUSTYPE)3, 0.1f, false, true, true, false, false) && aff.GetCurrentStatus((STATUSTYPE)3) > 0.05f, "the game refused a status, so the god mode check below proves nothing");
			return "poison " + F(aff.GetCurrentStatus((STATUSTYPE)3));
		}

		internal string _003CBody_003Eb__26()
		{
			State.God = true;
			float currentStatus = aff.GetCurrentStatus((STATUSTYPE)0);
			float currentStatus2 = aff.GetCurrentStatus((STATUSTYPE)2);
			bool num = aff.AddStatus((STATUSTYPE)0, 0.3f, false, true, true, false, false);
			bool flag = aff.AddStatus((STATUSTYPE)2, 0.3f, false, true, true, false, false);
			bool flag2 = aff.AddStatus((STATUSTYPE)1, 0.3f, false, true, true, false, false);
			State.God = false;
			Expect(!num && !flag && !flag2, "a status got through");
			Expect(aff.GetCurrentStatus((STATUSTYPE)0) == currentStatus && aff.GetCurrentStatus((STATUSTYPE)2) == currentStatus2, "a bar moved");
			return "injury, cold and hunger all refused";
		}

		internal string _003CBody_003Eb__27()
		{
			Me.data.currentStamina = 0.2f;
			Features.Heal();
			Expect(aff.GetCurrentStatus((STATUSTYPE)3) == 0f, "poison is still " + F(aff.GetCurrentStatus((STATUSTYPE)3)));
			Expect(Me.data.currentStamina > 0.9f, "stamina is " + F(Me.data.currentStamina));
			return null;
		}

		internal string _003CBody_003Eb__28()
		{
			Expect(hurt > 0f, "control failed: a " + 14f + " m drop did no damage without it, so this proves nothing");
			Expect(unhurt == 0f, "still took " + F(unhurt) + " injury");
			Expect(tumble <= 0f, "still ragdolled on landing");
			return "a " + 14f + " m drop: " + F(hurt) + " injury without it, 0.00 with it";
		}

		internal string _003CBody_003Eb__29()
		{
			Expect(heavy > 0f, "the scout was carrying nothing with weight, nothing to measure");
			Expect(light == 0f, "weight is still " + F(light));
			Expect(heavyAgain == heavy, "weight did not come back: " + F(heavyAgain) + " vs " + F(heavy));
			return "weight bar " + F(heavy) + " -> 0.00 -> " + F(heavyAgain);
		}

		internal string _003CBody_003Eb__32()
		{
			Expect(menu.lastError == null, menu.lastError);
			int num = 0;
			foreach (Luggage item in Luggage.ALL_LUGGAGE)
			{
				if ((Object)(object)item != (Object)null && !item.IsOpen)
				{
					num++;
				}
			}
			Expect(num > 0, "the map has no unopened luggage");
			return tags + " labels on screen, " + num + " unopened luggage on the map (other scouts: none in a solo session)";
		}

		internal string _003CBody_003Eb__33()
		{
			Expect(menu.lastError == null, menu.lastError);
			Expect(fires > 0, "no campfire was found on the mountain");
			return fires + " campfires, " + statues + " scout statues and " + hazards + " hazards found; " + more + " labels in range on screen";
		}

		internal string _003CBody_003Eb__35()
		{
			int currentAscent = Ascents.currentAscent;
			Menu.Control control = menu.controls["ascent"];
			control.Move(3f);
			int currentAscent2 = Ascents.currentAscent;
			control.Move(currentAscent);
			Expect(currentAscent2 == 3 && Ascents.currentAscent == currentAscent, "ascent read " + currentAscent2 + ", then " + Ascents.currentAscent);
			return currentAscent + " -> 3 -> " + currentAscent;
		}

		internal string _003CBody_003Eb__39()
		{
			Expect(Mathf.Abs(fovWide - 100f) < 0.5f, "asked for 100, the camera has " + F(fovWide));
			Expect(Mathf.Abs(fovBack - fovGame) < 2f, "it did not go back: " + F(fovBack));
			return F(fovGame) + " -> " + F(fovWide) + " -> " + F(fovBack);
		}

		internal string _003CBody_003Eb__40()
		{
			Expect(far > 1.2f && far <= State.CamDistance + 1f, "the camera is " + F(far) + " m from the head");
			Expect(headShown, "the scout's own head is still hidden");
			Expect((nearAgain < 1.2f) & headHidden, "first person did not come back: " + F(nearAgain) + " m");
			return "camera " + F(near) + " m -> " + F(far) + " m -> " + F(nearAgain) + " m from the head; the head is shown meanwhile";
		}

		internal string _003CBody_003Eb__41()
		{
			Expect(camOff < 0.1f, "the view is " + F(camOff) + " m from where it was put");
			Expect(scoutMoved < 1f, "the scout moved " + F(scoutMoved) + " m");
			Expect(home < 1.2f, "the view did not come back to the scout: " + F(home) + " m");
			return "the view went 12 m up while the scout stayed (" + F(scoutMoved) + " m), then came back";
		}

		internal string _003CBody_003Eb__42()
		{
			Expect(hudOff & hudOn, "hidden: " + hudOff + ", back: " + hudOn);
			return null;
		}

		internal string _003CBody_003Eb__43()
		{
			Expect(changed != wore[5], "the hat did not change, so this proves nothing");
			for (int i = 0; i < 8; i++)
			{
				Expect(back[i] == wore[i], "part " + i + " is " + back[i] + ", was " + wore[i]);
			}
			Expect((Object)(object)Features.Part(5, wore[5]) != (Object)null, "the hat has no entry in the game's list");
			return "hat " + wore[5] + " -> " + changed + " -> " + back[5] + "; all 8 parts as saved";
		}

		internal string _003CBody_003Eb__44()
		{
			Expect(sent, "the message was not sent: not the host");
			Expect(hungry > 0.2f && fed < 0.01f, "hunger " + F(hungry) + " -> " + F(fed));
			return "hunger " + F(hungry) + " -> " + F(fed);
		}

		internal string _003CBody_003Eb__45()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			Expect((Object)(object)fire != (Object)null, "this section has no campfire");
			float num = Vector3.Distance(atFire, ((Component)fire).transform.position);
			Expect(num < 6f, "ended " + F(num) + " m from the campfire");
			return F(num) + " m from the fire, " + F(Vector3.Distance(beach, ((Component)fire).transform.position)) + " m from where the run began";
		}

		internal string _003CBody_003Eb__46()
		{
			Expect(away > 1f, "the scout did not move");
			return "moved " + F(away) + " m";
		}

		internal string _003CBody_003Eb__47()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			float num = Vector3.Distance(atFire, Me.Center);
			Expect(num < 3f, "ended " + F(num) + " m from the remembered spot");
			Features.GoSpot(2);
			return "back within " + F(num) + " m; an empty spot says so and does nothing";
		}

		internal string _003CBody_003Eb__48()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			float num = Vector3.Distance(Me.Center, ((Component)statue).transform.position);
			Expect(num < 6f, "ended " + F(num) + " m from the statue");
			return F(num) + " m from the statue";
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_1
	{
		public int loose;

		public _003C_003Ec__DisplayClass34_0 CS_0024_003C_003E8__locals1;

		internal string _003CBody_003Eb__52()
		{
			Expect(Item.ALL_ITEMS.Count > loose, "no new item appeared");
			return ((Object)CS_0024_003C_003E8__locals1.give).name + " appeared above the scout";
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_2
	{
		public float wait2;

		public float wait1;

		public float size1;

		public float size0;

		public float wait0;

		internal string _003CBody_003Eb__55()
		{
			Expect(wait2 == wait1 && size1 == size0, "the fog clock kept running: " + F(wait1) + " -> " + F(wait2));
			if (!(wait1 > wait0))
			{
				return "its countdown was already paused by the game here, so only 'did not move' was shown";
			}
			return "its countdown ran " + F(wait1 - wait0) + " s in 2 s before, 0.00 after";
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_3
	{
		public bool inHand;

		public string pockets;

		public bool keptOn;

		public bool goneOff;

		public int usesBefore;

		public int usesKept;

		public int usesAfter;

		public string heldName;

		internal string _003CBody_003Eb__56()
		{
			Expect(inHand, "nothing came into the hand; pockets: " + pockets);
			Expect(keptOn, "with the switch on, using it up took it away");
			Expect(goneOff, "with the switch off it should have been used up, and it was not: the test proved nothing");
			if (usesBefore >= 0)
			{
				Expect(usesKept == usesBefore, "with the switch on a use was still counted: " + usesBefore + " -> " + usesKept);
			}
			if (usesAfter >= 0)
			{
				Expect(usesAfter == usesKept - 1, "with the switch off a use was not counted: " + usesKept + " -> " + usesAfter);
			}
			return heldName + ": kept with the switch on" + ((usesBefore >= 0) ? (" (uses stay at " + usesKept + ")") : "") + ", used up with it off";
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_4
	{
		public int lyingNow;

		public int lying;

		public float nearest;

		public _003C_003Ec__DisplayClass34_0 CS_0024_003C_003E8__locals2;

		internal string _003CBody_003Eb__57()
		{
			Expect(lyingNow == lying + 1, ((Object)CS_0024_003C_003E8__locals2.give).name + " on the ground: " + lying + " -> " + lyingNow);
			Expect(nearest < 4f, "it landed " + F(nearest) + " m away");
			return ((Object)CS_0024_003C_003E8__locals2.give).name + " appeared " + F(nearest) + " m from the scout";
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_5
	{
		public bool remembered;

		internal string _003CBody_003Eb__59()
		{
			Expect(remembered, "F10 did not remember the spot");
			return "F10 remembers the spot with the menu closed";
		}
	}

	[CompilerGenerated]
	private sealed class _003CBody_003Ed__34 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Menu menu;

		private _003C_003Ec__DisplayClass34_0 _003C_003E8__1;

		private _003C_003Ec__DisplayClass34_1 _003C_003E8__2;

		private _003C_003Ec__DisplayClass34_2 _003C_003E8__3;

		private _003C_003Ec__DisplayClass34_3 _003C_003E8__4;

		private _003C_003Ec__DisplayClass34_4 _003C_003E8__5;

		private _003C_003Ec__DisplayClass34_5 _003C_003E8__6;

		private bool _003ChadSave_003E5__2;

		private float _003ClaunchEnd_003E5__3;

		private OrbFogHandler _003Cfog_003E5__4;

		private HideTheBody _003Cbody_003E5__5;

		private FieldInfo _003Cshowing_003E5__6;

		private Vector3 _003Cstood_003E5__7;

		private Vector3 _003Cup_003E5__8;

		private string _003CslotWas_003E5__9;

		private string _003CfirstOutfit_003E5__10;

		private IEnumerator _003C_003E7__wrap10;

		private int _003Ci_003E5__12;

		private Item _003Cheld_003E5__13;

		private ushort _003CheldId_003E5__14;

		private byte _003Cs_003E5__15;

		private float _003Cuntil_003E5__16;

		private Action_ReduceUses _003Creduce_003E5__17;

		private Keys.Bound _003Cjob_003E5__18;

		private KeyCode _003CjobWas_003E5__19;

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
		public _003CBody_003Ed__34(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
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
			case 27:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally26();
				}
				break;
			case -29:
			case 28:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally27();
				}
				break;
			case -30:
			case 29:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally28();
				}
				break;
			case -31:
			case 30:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally29();
				}
				break;
			case -32:
			case 31:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally30();
				}
				break;
			case -33:
			case 32:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally31();
				}
				break;
			case -34:
			case 33:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally32();
				}
				break;
			case -35:
			case 34:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally33();
				}
				break;
			case -36:
			case 35:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally34();
				}
				break;
			case -37:
			case 36:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally35();
				}
				break;
			case -38:
			case 37:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally36();
				}
				break;
			case -39:
			case 38:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally37();
				}
				break;
			case -40:
			case 39:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally38();
				}
				break;
			case -41:
			case 40:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally39();
				}
				break;
			case -42:
			case 41:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally40();
				}
				break;
			case -43:
			case 42:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally41();
				}
				break;
			case -44:
			case 43:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally42();
				}
				break;
			case -45:
			case 44:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally43();
				}
				break;
			case -46:
			case 45:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally44();
				}
				break;
			case -47:
			case 46:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally45();
				}
				break;
			case -48:
			case 48:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally46();
				}
				break;
			case -49:
			case 49:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally47();
				}
				break;
			case -50:
			case 50:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally48();
				}
				break;
			case -51:
			case 51:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally49();
				}
				break;
			case -52:
			case 52:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally50();
				}
				break;
			case -53:
			case 53:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally51();
				}
				break;
			case -54:
			case 54:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally52();
				}
				break;
			case -55:
			case 55:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally53();
				}
				break;
			case -56:
			case 56:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally54();
				}
				break;
			case -57:
			case 57:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally55();
				}
				break;
			case -58:
			case 58:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally56();
				}
				break;
			case -59:
			case 59:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally57();
				}
				break;
			case -60:
			case 60:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally58();
				}
				break;
			case -61:
			case 61:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally59();
				}
				break;
			case -62:
			case 62:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally60();
				}
				break;
			case -63:
			case 63:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally61();
				}
				break;
			case -64:
			case 64:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally62();
				}
				break;
			case -65:
			case 65:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally63();
				}
				break;
			case -66:
			case 66:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally64();
				}
				break;
			case -67:
			case 67:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally65();
				}
				break;
			case -68:
			case 68:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally66();
				}
				break;
			case -69:
			case 69:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally67();
				}
				break;
			case -70:
			case 70:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally68();
				}
				break;
			case -71:
			case 71:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally69();
				}
				break;
			case -72:
			case 72:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally70();
				}
				break;
			case -73:
			case 73:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally71();
				}
				break;
			case -74:
			case 74:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally72();
				}
				break;
			case -75:
			case 75:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally73();
				}
				break;
			case -76:
			case 76:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally74();
				}
				break;
			case -77:
			case 77:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally75();
				}
				break;
			case -78:
			case 78:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally76();
				}
				break;
			case -79:
			case 79:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally77();
				}
				break;
			case -80:
			case 80:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally78();
				}
				break;
			case -81:
			case 81:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally79();
				}
				break;
			case -82:
			case 82:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally80();
				}
				break;
			}
			_003C_003E8__1 = null;
			_003C_003E8__2 = null;
			_003C_003E8__3 = null;
			_003C_003E8__4 = null;
			_003C_003E8__5 = null;
			_003C_003E8__6 = null;
			_003Cfog_003E5__4 = null;
			_003Cbody_003E5__5 = null;
			_003Cshowing_003E5__6 = null;
			_003CslotWas_003E5__9 = null;
			_003CfirstOutfit_003E5__10 = null;
			_003C_003E7__wrap10 = null;
			_003Cheld_003E5__13 = null;
			_003Creduce_003E5__17 = null;
			_003Cjob_003E5__18 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0284: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c06: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c0b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c1a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ca2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ca7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e91: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f3c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ff0: Unknown result type (might be due to invalid IL or missing references)
			//IL_1004: Unknown result type (might be due to invalid IL or missing references)
			//IL_1009: Unknown result type (might be due to invalid IL or missing references)
			//IL_100e: Unknown result type (might be due to invalid IL or missing references)
			//IL_1019: Unknown result type (might be due to invalid IL or missing references)
			//IL_1096: Unknown result type (might be due to invalid IL or missing references)
			//IL_109b: Unknown result type (might be due to invalid IL or missing references)
			//IL_132c: Unknown result type (might be due to invalid IL or missing references)
			//IL_13b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_157c: Unknown result type (might be due to invalid IL or missing references)
			//IL_1581: Unknown result type (might be due to invalid IL or missing references)
			//IL_1905: Unknown result type (might be due to invalid IL or missing references)
			//IL_190a: Unknown result type (might be due to invalid IL or missing references)
			//IL_199d: Unknown result type (might be due to invalid IL or missing references)
			//IL_19a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_19ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_19b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_19bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_19c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ac2: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ac7: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ad1: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ad6: Unknown result type (might be due to invalid IL or missing references)
			//IL_2aa8: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ab2: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b3a: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b44: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c72: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c7c: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d8e: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d94: Unknown result type (might be due to invalid IL or missing references)
			//IL_2dae: Unknown result type (might be due to invalid IL or missing references)
			//IL_2db4: Unknown result type (might be due to invalid IL or missing references)
			//IL_2eb0: Unknown result type (might be due to invalid IL or missing references)
			//IL_2eba: Unknown result type (might be due to invalid IL or missing references)
			//IL_35a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_35aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_36a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_36b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_3c0f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ac1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ac6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ad7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0adc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ae6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aeb: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ce1: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ce6: Unknown result type (might be due to invalid IL or missing references)
			//IL_2cf2: Unknown result type (might be due to invalid IL or missing references)
			//IL_2cf7: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d01: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d06: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d0b: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d11: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d16: Unknown result type (might be due to invalid IL or missing references)
			//IL_3b3f: Unknown result type (might be due to invalid IL or missing references)
			//IL_3b44: Unknown result type (might be due to invalid IL or missing references)
			//IL_3e02: Unknown result type (might be due to invalid IL or missing references)
			//IL_3dec: Unknown result type (might be due to invalid IL or missing references)
			//IL_3df1: Unknown result type (might be due to invalid IL or missing references)
			//IL_3470: Unknown result type (might be due to invalid IL or missing references)
			//IL_3357: Unknown result type (might be due to invalid IL or missing references)
			//IL_34ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_34b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_2278: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				AirportCheckInKiosk obj2;
				float num;
				string text;
				List<string> lines;
				Scene activeScene;
				int num2;
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E8__1 = new _003C_003Ec__DisplayClass34_0();
					_003C_003E8__1.menu = menu;
					_003C_003E7__wrap10 = WaitFor("the title screen", 180f, delegate
					{
						//IL_0000: Unknown result type (might be due to invalid IL or missing references)
						//IL_0005: Unknown result type (might be due to invalid IL or missing references)
						Scene activeScene4 = SceneManager.GetActiveScene();
						return activeScene4.name == "Title" && (Object)(object)Object.FindAnyObjectByType<MainMenu>() != (Object)null;
					}).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_01ef;
				case 1:
					_003C_003E1__state = -3;
					goto IL_01ef;
				case 2:
					_003C_003E1__state = -4;
					goto IL_0251;
				case 3:
					_003C_003E1__state = -5;
					goto IL_0303;
				case 4:
					_003C_003E1__state = -6;
					goto IL_03ef;
				case 5:
					_003C_003E1__state = -7;
					goto IL_0451;
				case 6:
					_003C_003E1__state = -8;
					goto IL_0510;
				case 7:
					_003C_003E1__state = -9;
					goto IL_063a;
				case 8:
					_003C_003E1__state = -10;
					goto IL_06e9;
				case 9:
					_003C_003E1__state = -11;
					goto IL_07b3;
				case 10:
					_003C_003E1__state = -12;
					goto IL_0837;
				case 11:
					_003C_003E1__state = -13;
					goto IL_09ae;
				case 12:
					_003C_003E1__state = -14;
					goto IL_0a73;
				case 13:
					_003C_003E1__state = -15;
					goto IL_0b43;
				case 14:
					_003C_003E1__state = -16;
					goto IL_0bb6;
				case 15:
					_003C_003E1__state = -17;
					goto IL_0c72;
				case 16:
					_003C_003E1__state = -18;
					goto IL_0d16;
				case 17:
					_003C_003E1__state = -19;
					goto IL_0da8;
				case 18:
					_003C_003E1__state = -20;
					goto IL_0e40;
				case 19:
					_003C_003E1__state = -21;
					goto IL_0f08;
				case 20:
					_003C_003E1__state = -22;
					goto IL_0fbb;
				case 21:
					_003C_003E1__state = -23;
					goto IL_1071;
				case 22:
					_003C_003E1__state = -24;
					goto IL_1104;
				case 23:
					_003C_003E1__state = -25;
					goto IL_11ce;
				case 24:
					_003C_003E1__state = -26;
					goto IL_1265;
				case 25:
					_003C_003E1__state = -27;
					goto IL_12fd;
				case 26:
					_003C_003E1__state = -1;
					goto IL_13ea;
				case 27:
					_003C_003E1__state = -28;
					goto IL_145e;
				case 28:
					_003C_003E1__state = -29;
					goto IL_14cb;
				case 29:
					_003C_003E1__state = -30;
					goto IL_1557;
				case 30:
					_003C_003E1__state = -31;
					goto IL_15d9;
				case 31:
					_003C_003E1__state = -32;
					goto IL_1657;
				case 32:
					_003C_003E1__state = -33;
					goto IL_171b;
				case 33:
					_003C_003E1__state = -34;
					goto IL_17e8;
				case 34:
					_003C_003E1__state = -35;
					goto IL_186f;
				case 35:
					_003C_003E1__state = -36;
					goto IL_18d2;
				case 36:
					_003C_003E1__state = -37;
					goto IL_1a19;
				case 37:
					_003C_003E1__state = -38;
					goto IL_1a98;
				case 38:
					_003C_003E1__state = -39;
					goto IL_1b2e;
				case 39:
					_003C_003E1__state = -40;
					goto IL_1be8;
				case 40:
					_003C_003E1__state = -41;
					goto IL_1c81;
				case 41:
					_003C_003E1__state = -42;
					goto IL_1d06;
				case 42:
					_003C_003E1__state = -43;
					goto IL_1d8b;
				case 43:
					_003C_003E1__state = -44;
					goto IL_1ec0;
				case 44:
					_003C_003E1__state = -45;
					goto IL_1f55;
				case 45:
					_003C_003E1__state = -46;
					goto IL_202f;
				case 46:
					_003C_003E1__state = -47;
					goto IL_20f2;
				case 47:
					_003C_003E1__state = -1;
					goto IL_22b2;
				case 48:
					_003C_003E1__state = -48;
					goto IL_231f;
				case 49:
					_003C_003E1__state = -49;
					goto IL_23fd;
				case 50:
					_003C_003E1__state = -50;
					goto IL_25aa;
				case 51:
					_003C_003E1__state = -51;
					goto IL_2641;
				case 52:
					_003C_003E1__state = -52;
					goto IL_2731;
				case 53:
					_003C_003E1__state = -53;
					goto IL_27c8;
				case 54:
					_003C_003E1__state = -54;
					goto IL_29a2;
				case 55:
					_003C_003E1__state = -55;
					goto IL_2a24;
				case 56:
					_003C_003E1__state = -56;
					goto IL_2b10;
				case 57:
					_003C_003E1__state = -57;
					goto IL_2bdf;
				case 58:
					_003C_003E1__state = -58;
					goto IL_2c48;
				case 59:
					_003C_003E1__state = -59;
					goto IL_2d64;
				case 60:
					_003C_003E1__state = -60;
					goto IL_2e1d;
				case 61:
					_003C_003E1__state = -61;
					goto IL_2e86;
				case 62:
					_003C_003E1__state = -62;
					goto IL_2f33;
				case 63:
					_003C_003E1__state = -63;
					goto IL_2fb9;
				case 64:
					_003C_003E1__state = -64;
					goto IL_30a6;
				case 65:
					_003C_003E1__state = -65;
					goto IL_3121;
				case 66:
					_003C_003E1__state = -66;
					goto IL_31eb;
				case 67:
					_003C_003E1__state = -67;
					goto IL_3283;
				case 68:
					_003C_003E1__state = -68;
					goto IL_33f6;
				case 69:
					_003C_003E1__state = -69;
					goto IL_3580;
				case 70:
					_003C_003E1__state = -70;
					goto IL_3613;
				case 71:
					_003C_003E1__state = -71;
					goto IL_3681;
				case 72:
					_003C_003E1__state = -72;
					goto IL_3724;
				case 73:
					_003C_003E1__state = -73;
					goto IL_378d;
				case 74:
					_003C_003E1__state = -74;
					goto IL_380b;
				case 75:
					_003C_003E1__state = -75;
					goto IL_38aa;
				case 76:
					_003C_003E1__state = -76;
					goto IL_3928;
				case 77:
					_003C_003E1__state = -77;
					goto IL_39ab;
				case 78:
					_003C_003E1__state = -78;
					goto IL_3ac3;
				case 79:
					_003C_003E1__state = -79;
					goto IL_3bce;
				case 80:
					_003C_003E1__state = -80;
					goto IL_3ca7;
				case 81:
					_003C_003E1__state = -81;
					goto IL_3d14;
				case 82:
					{
						_003C_003E1__state = -82;
						goto IL_3e52;
					}
					IL_15d9:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 30;
						return true;
					}
					_003C_003Em__Finally29();
					_003C_003E7__wrap10 = null;
					Check("bring to me (the same warp message)", delegate
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_000b: Unknown result type (might be due to invalid IL or missing references)
						float num19 = Vector3.Distance(_003C_003E8__1.bringFrom, Me.Center);
						Expect(num19 > 1f, "the scout did not move (" + F(num19) + " m)");
						return "moved " + F(num19) + " m";
					});
					_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
					_003C_003E1__state = -32;
					goto IL_1657;
					IL_2641:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 51;
						return true;
					}
					_003C_003Em__Finally49();
					_003C_003E7__wrap10 = null;
					_003C_003E8__4.keptOn = (Object)(object)_003Cheld_003E5__13 != (Object)null && (Object)(object)Me.data.currentItem == (Object)(object)_003Cheld_003E5__13 && ((Component)_003Cheld_003E5__13).gameObject.activeSelf && Player.localPlayer.HasInAnySlot(_003CheldId_003E5__14);
					State.KeepItems = false;
					if ((Object)(object)_003Creduce_003E5__17 != (Object)null && _003C_003E8__4.usesKept > 1)
					{
						((ItemActionBase)_003Creduce_003E5__17).RunAction();
						_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
						_003C_003E1__state = -52;
						goto IL_2731;
					}
					goto IL_2767;
					IL_01ef:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap10 = null;
					_003C_003E7__wrap10 = Wait(6f).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_0251;
					IL_380b:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 74;
						return true;
					}
					_003C_003Em__Finally72();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.statue = null;
					try
					{
						_003C_003E8__1.statue = MapHandler.CurrentScoutStatue;
					}
					catch
					{
					}
					if ((Object)(object)_003C_003E8__1.statue != (Object)null)
					{
						Features.WarpToStatue();
						_003C_003E7__wrap10 = Wait(1.2f).GetEnumerator();
						_003C_003E1__state = -75;
						goto IL_38aa;
					}
					SelfTest.lines.Add("      teleport to the scout statue: this part of the mountain has none");
					goto IL_3953;
					IL_0251:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap10 = null;
					screenW = Screen.width;
					screenH = Screen.height;
					screenMode = Screen.fullScreenMode;
					if (shots && (screenW != 1920 || screenH != 1080))
					{
						Screen.SetResolution(1920, 1080, (FullScreenMode)1);
						_003C_003E7__wrap10 = Wait(3f).GetEnumerator();
						_003C_003E1__state = -5;
						goto IL_0303;
					}
					goto IL_031d;
					IL_3724:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 72;
						return true;
					}
					_003C_003Em__Finally70();
					_003C_003E7__wrap10 = null;
					Features.GoSpot(0);
					_003C_003E7__wrap10 = Wait(1.2f).GetEnumerator();
					_003C_003E1__state = -73;
					goto IL_378d;
					IL_171b:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 32;
						return true;
					}
					_003C_003Em__Finally31();
					_003C_003E7__wrap10 = null;
					Check("drop an item on their head", delegate
					{
						Expect(Item.ALL_ITEMS.Count > _003C_003E8__2.loose, "no new item appeared");
						return ((Object)_003C_003E8__2.CS_0024_003C_003E8__locals1.give).name + " appeared above the scout";
					});
					_003C_003E8__2 = null;
					goto IL_1757;
					IL_38aa:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 75;
						return true;
					}
					_003C_003Em__Finally73();
					_003C_003E7__wrap10 = null;
					Check("teleport to the scout statue", delegate
					{
						//IL_0005: Unknown result type (might be due to invalid IL or missing references)
						//IL_0015: Unknown result type (might be due to invalid IL or missing references)
						float num18 = Vector3.Distance(Me.Center, ((Component)_003C_003E8__1.statue).transform.position);
						Expect(num18 < 6f, "ended " + F(num18) + " m from the statue");
						return F(num18) + " m from the statue";
					});
					_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
					_003C_003E1__state = -76;
					goto IL_3928;
					IL_3681:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 71;
						return true;
					}
					_003C_003Em__Finally69();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.away = Vector3.Distance(_003C_003E8__1.atFire, Me.Center);
					Check("teleport to where you look", delegate
					{
						Expect(_003C_003E8__1.away > 1f, "the scout did not move");
						return "moved " + F(_003C_003E8__1.away) + " m";
					});
					_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
					_003C_003E1__state = -72;
					goto IL_3724;
					IL_0303:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 3;
						return true;
					}
					_003C_003Em__Finally3();
					_003C_003E7__wrap10 = null;
					goto IL_031d;
					IL_1757:
					Settings.Blank();
					obj2 = Object.FindAnyObjectByType<AirportCheckInKiosk>();
					if ((Object)(object)obj2 == (Object)null)
					{
						throw new Exception("no check-in kiosk in the airport, could not start a run");
					}
					obj2.StartGame(0);
					_003C_003E7__wrap10 = WaitFor("the island", 300f, delegate
					{
						//IL_0000: Unknown result type (might be due to invalid IL or missing references)
						//IL_0005: Unknown result type (might be due to invalid IL or missing references)
						Scene activeScene3 = SceneManager.GetActiveScene();
						return activeScene3.name != "Airport" && Ready && MapHandler.ExistsAndInitialized && !Me.inAirport;
					}).GetEnumerator();
					_003C_003E1__state = -34;
					goto IL_17e8;
					IL_2731:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 52;
						return true;
					}
					_003C_003Em__Finally50();
					_003C_003E7__wrap10 = null;
					_003C_003E8__4.usesAfter = _003Cheld_003E5__13.GetData<OptionableIntItemData>((DataEntryKey)2).Value;
					goto IL_2767;
					IL_031d:
					_003ChadSave_003E5__2 = Quicksave.Exists;
					if (_003ChadSave_003E5__2)
					{
						throw new Exception("a saved run exists, and starting a new one would replace it - nothing was touched");
					}
					Section("Loading");
					Check("every hook was applied", delegate
					{
						Expect(Plugin.FailedPatches.Count == 0, "failed: " + string.Join(", ", Plugin.FailedPatches.ToArray()));
						return "25 hooks";
					});
					Call(Object.FindAnyObjectByType<MainMenu>(), "PlaySoloClicked");
					_003C_003E7__wrap10 = WaitFor("the airport", 180f, delegate
					{
						//IL_0000: Unknown result type (might be due to invalid IL or missing references)
						//IL_0005: Unknown result type (might be due to invalid IL or missing references)
						Scene activeScene2 = SceneManager.GetActiveScene();
						return activeScene2.name == "Airport" && Ready;
					}).GetEnumerator();
					_003C_003E1__state = -6;
					goto IL_03ef;
					IL_285d:
					Section("The mountain");
					Check("no hazards, no bad weather: the answers the game gets", delegate
					{
						int value = RunSettings.GetValue((SETTINGTYPE)1300, false);
						int value2 = RunSettings.GetValue((SETTINGTYPE)1250, false);
						Expect(value != 0 && value2 != 0, "bees or rain are already off in this run, so this proves nothing");
						State.NoWeather = true;
						int value3 = RunSettings.GetValue((SETTINGTYPE)1250, false);
						int value4 = RunSettings.GetValue((SETTINGTYPE)1300, false);
						State.NoWeather = false;
						State.NoHazards = true;
						int value5 = RunSettings.GetValue((SETTINGTYPE)1300, false);
						int value6 = RunSettings.GetValue((SETTINGTYPE)100, false);
						State.NoHazards = false;
						Expect(value3 == 0 && value4 == value, "no bad weather: rain " + value3 + ", bees " + value4);
						Expect(value5 == 0, "no hazards: bees " + value5);
						Expect(RunSettings.GetValue((SETTINGTYPE)1300, false) == value, "the answer did not come back");
						return "bees " + value + " -> 0, rain " + value2 + " -> 0, back when off; other settings untouched (fog " + value6 + ")";
					});
					Check("ascent level", delegate
					{
						int currentAscent = Ascents.currentAscent;
						Menu.Control control = _003C_003E8__1.menu.controls["ascent"];
						control.Move(3f);
						int currentAscent2 = Ascents.currentAscent;
						control.Move(currentAscent);
						Expect(currentAscent2 == 3 && Ascents.currentAscent == currentAscent, "ascent read " + currentAscent2 + ", then " + Ascents.currentAscent);
						return currentAscent + " -> 3 -> " + currentAscent;
					});
					Section("Climbing");
					Check("grab any surface", delegate
					{
						//IL_0005: Unknown result type (might be due to invalid IL or missing references)
						//IL_000a: Unknown result type (might be due to invalid IL or missing references)
						//IL_0016: Unknown result type (might be due to invalid IL or missing references)
						//IL_004c: Unknown result type (might be due to invalid IL or missing references)
						//IL_008e: Unknown result type (might be due to invalid IL or missing references)
						RaycastHit val3 = default(RaycastHit);
						Expect(Physics.Raycast(Me.Center, Vector3.down, out val3, 20f, (int)HelperFunctions.terrainMapMask, (QueryTriggerInteraction)1), "no ground under the scout");
						bool flag3 = (bool)Call(Me.refs.climbing, "AcceptableGrabAngle", Vector3.up, val3.collider);
						State.ClimbAnything = true;
						bool cond = (bool)Call(Me.refs.climbing, "AcceptableGrabAngle", Vector3.up, val3.collider);
						State.ClimbAnything = false;
						Expect(!flag3, "the game already lets you grab flat ground, so this proves nothing");
						Expect(cond, "flat ground still cannot be grabbed");
						return "flat ground: no without, yes with";
					});
					Check("never slip", delegate
					{
						//IL_001a: Unknown result type (might be due to invalid IL or missing references)
						//IL_0025: Unknown result type (might be due to invalid IL or missing references)
						//IL_0041: Unknown result type (might be due to invalid IL or missing references)
						//IL_004c: Unknown result type (might be due to invalid IL or missing references)
						//IL_005d: Unknown result type (might be due to invalid IL or missing references)
						CharacterClimbing climbing = Me.refs.climbing;
						climbing.playerSlide = new Vector2(0f, -50f);
						float y = climbing.playerSlide.y;
						State.NoSlip = true;
						climbing.playerSlide = new Vector2(0f, -50f);
						float y2 = climbing.playerSlide.y;
						State.NoSlip = false;
						climbing.playerSlide = Vector2.zero;
						Expect(y == -50f && y2 == 0f, "slide read " + F(y) + " without and " + F(y2) + " with");
						return "a slide of 50 becomes 0";
					});
					Check("rope length", delegate
					{
						int maxSegments = Rope.MaxSegments;
						State.RopeLength = 2f;
						int maxSegments2 = Rope.MaxSegments;
						State.RopeLength = 1f;
						Expect(maxSegments2 == maxSegments * 2 && Rope.MaxSegments == maxSegments, "segments " + maxSegments + " -> " + maxSegments2 + " -> " + Rope.MaxSegments);
						return maxSegments + " segments -> " + maxSegments2 + " -> " + maxSegments;
					});
					Section("Camera");
					_003C_003E8__1.fovGame = Camera.main.fieldOfView;
					State.Fov = 100f;
					_003C_003E7__wrap10 = Wait(1.5f).GetEnumerator();
					_003C_003E1__state = -54;
					goto IL_29a2;
					IL_2767:
					((MonoBehaviour)_003Cheld_003E5__13).StartCoroutine(_003Cheld_003E5__13.ConsumeDelayed(false));
					_003C_003E7__wrap10 = Wait(1.5f).GetEnumerator();
					_003C_003E1__state = -53;
					goto IL_27c8;
					IL_17e8:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 33;
						return true;
					}
					_003C_003Em__Finally32();
					_003C_003E7__wrap10 = null;
					_003C_003E7__wrap10 = WaitFor("the scout to wake up on the beach", 120f, () => !Me.data.passedOut && !Me.data.fullyPassedOut).GetEnumerator();
					_003C_003E1__state = -35;
					goto IL_186f;
					IL_3928:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 76;
						return true;
					}
					_003C_003Em__Finally74();
					_003C_003E7__wrap10 = null;
					goto IL_3953;
					IL_39ab:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 77;
						return true;
					}
					_003C_003Em__Finally75();
					_003C_003E7__wrap10 = null;
					foreach (Picker.Entry item in _003C_003E8__1.menu.items)
					{
						if (item.What == _003C_003E8__1.give)
						{
							_003C_003E8__1.menu.picked = item;
						}
					}
					_003CfirstOutfit_003E5__10 = Plugin.Outfits[0].Value;
					Features.SaveOutfit(0);
					_003C_003E7__wrap10 = KitTest.Run(_003C_003E8__1.menu, Check, Shot).GetEnumerator();
					_003C_003E1__state = -78;
					goto IL_3ac3;
					IL_27c8:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 53;
						return true;
					}
					_003C_003Em__Finally51();
					_003C_003E7__wrap10 = null;
					_003C_003E8__4.goneOff = (Object)(object)_003Cheld_003E5__13 == (Object)null || (Object)(object)Me.data.currentItem != (Object)(object)_003Cheld_003E5__13 || !((Component)_003Cheld_003E5__13).gameObject.activeSelf;
					_003Creduce_003E5__17 = null;
					goto IL_2834;
					IL_03ef:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 4;
						return true;
					}
					_003C_003Em__Finally4();
					_003C_003E7__wrap10 = null;
					_003C_003E7__wrap10 = Wait(6f).GetEnumerator();
					_003C_003E1__state = -7;
					goto IL_0451;
					IL_3953:
					Section("The menu");
					Features.WarpToCampfire();
					_003C_003E7__wrap10 = Wait(4f).GetEnumerator();
					_003C_003E1__state = -77;
					goto IL_39ab;
					IL_0451:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 5;
						return true;
					}
					_003C_003Em__Finally5();
					_003C_003E7__wrap10 = null;
					SelfTest.lines.Add("      reached the airport in a solo session");
					Section("Menu, in the airport");
					_003C_003E8__1.menu.open = true;
					_003Ci_003E5__12 = 0;
					goto IL_059b;
					IL_1657:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 31;
						return true;
					}
					_003C_003Em__Finally30();
					_003C_003E7__wrap10 = null;
					if ((Object)(object)_003C_003E8__1.give != (Object)null)
					{
						_003C_003E8__2 = new _003C_003Ec__DisplayClass34_1();
						_003C_003E8__2.CS_0024_003C_003E8__locals1 = _003C_003E8__1;
						_003C_003E8__2.loose = Item.ALL_ITEMS.Count;
						Features.DropOn(Me, _003C_003E8__2.CS_0024_003C_003E8__locals1.give);
						_003C_003E7__wrap10 = Wait(1f).GetEnumerator();
						_003C_003E1__state = -33;
						goto IL_171b;
					}
					goto IL_1757;
					IL_059b:
					if (_003Ci_003E5__12 < _003C_003E8__1.menu.tabs.Length)
					{
						_003C_003E8__1.menu.lastError = null;
						_003C_003E8__1.menu.GoTo(_003Ci_003E5__12);
						_003C_003E7__wrap10 = Wait(0.4f).GetEnumerator();
						_003C_003E1__state = -8;
						goto IL_0510;
					}
					Check("an open menu frees the mouse and holds the scout still", delegate
					{
						Expect(GUIManager.instance.windowBlockingInput, "the game still reads input");
						Expect(GUIManager.instance.windowShowingCursor, "the cursor is still locked");
						return (string)null;
					});
					_003C_003E8__1.menu.open = false;
					_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
					_003C_003E1__state = -9;
					goto IL_063a;
					IL_30a6:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 64;
						return true;
					}
					_003C_003Em__Finally62();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.changed = Features.Wearing()[5];
					Features.WearOutfit(3);
					_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
					_003C_003E1__state = -65;
					goto IL_3121;
					IL_1a98:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 37;
						return true;
					}
					_003C_003Em__Finally36();
					_003C_003E7__wrap10 = null;
					Features.Heal();
					State.NoFall = true;
					Features.Warp(Me.Center + Vector3.up * 14f, "a drop");
					_003C_003E7__wrap10 = Wait(6f).GetEnumerator();
					_003C_003E1__state = -39;
					goto IL_1b2e;
					IL_2834:
					Check("items never run out", delegate
					{
						Expect(_003C_003E8__4.inHand, "nothing came into the hand; pockets: " + _003C_003E8__4.pockets);
						Expect(_003C_003E8__4.keptOn, "with the switch on, using it up took it away");
						Expect(_003C_003E8__4.goneOff, "with the switch off it should have been used up, and it was not: the test proved nothing");
						if (_003C_003E8__4.usesBefore >= 0)
						{
							Expect(_003C_003E8__4.usesKept == _003C_003E8__4.usesBefore, "with the switch on a use was still counted: " + _003C_003E8__4.usesBefore + " -> " + _003C_003E8__4.usesKept);
						}
						if (_003C_003E8__4.usesAfter >= 0)
						{
							Expect(_003C_003E8__4.usesAfter == _003C_003E8__4.usesKept - 1, "with the switch off a use was not counted: " + _003C_003E8__4.usesKept + " -> " + _003C_003E8__4.usesAfter);
						}
						return _003C_003E8__4.heldName + ": kept with the switch on" + ((_003C_003E8__4.usesBefore >= 0) ? (" (uses stay at " + _003C_003E8__4.usesKept + ")") : "") + ", used up with it off";
					});
					_003C_003E8__4 = null;
					_003Cheld_003E5__13 = null;
					goto IL_285d;
					IL_063a:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 7;
						return true;
					}
					_003C_003Em__Finally7();
					_003C_003E7__wrap10 = null;
					Check("a closed menu gives control back", delegate
					{
						Expect(!GUIManager.instance.windowBlockingInput, "input is still blocked");
						return (string)null;
					});
					Section("Scout");
					num = 0f;
					Me.data.currentStamina = 0.6f;
					_003C_003E7__wrap10 = Wait(0.1f).GetEnumerator();
					_003C_003E1__state = -10;
					goto IL_06e9;
					IL_2f33:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 62;
						return true;
					}
					_003C_003Em__Finally60();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.hudOff = !((Behaviour)GUIManager.instance.hudCanvas).enabled;
					State.HideHud = false;
					_003C_003E7__wrap10 = Wait(0.3f).GetEnumerator();
					_003C_003E1__state = -63;
					goto IL_2fb9;
					IL_1be8:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 39;
						return true;
					}
					_003C_003Em__Finally38();
					_003C_003E7__wrap10 = null;
					if ((Object)(object)_003C_003E8__1.give != (Object)null)
					{
						Features.Give(_003C_003E8__1.give, ((Object)_003C_003E8__1.give).name);
						_003C_003E7__wrap10 = Wait(3f).GetEnumerator();
						_003C_003E1__state = -41;
						goto IL_1c81;
					}
					goto IL_1c9b;
					IL_3613:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 70;
						return true;
					}
					_003C_003Em__Finally68();
					_003C_003E7__wrap10 = null;
					Features.SaveSpot(0);
					Features.WarpToAim();
					_003C_003E7__wrap10 = Wait(1.2f).GetEnumerator();
					_003C_003E1__state = -71;
					goto IL_3681;
					IL_06e9:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 8;
						return true;
					}
					_003C_003Em__Finally8();
					_003C_003E7__wrap10 = null;
					num = Me.data.currentStamina;
					Call(Me, "UseStamina", 0.2f, true, true);
					_003C_003E8__1.spent = num - Me.data.currentStamina;
					State.InfStamina = true;
					_003C_003E7__wrap10 = Wait(0.3f).GetEnumerator();
					_003C_003E1__state = -11;
					goto IL_07b3;
					IL_2d64:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 59;
						return true;
					}
					_003C_003Em__Finally57();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.camOff = Vector3.Distance(((Component)Camera.main).transform.position, _003Cup_003E5__8);
					_003C_003E8__1.scoutMoved = Vector3.Distance(Me.Center, _003Cstood_003E5__7);
					if (shots)
					{
						Shot("FreeCamera");
					}
					_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
					_003C_003E1__state = -60;
					goto IL_2e1d;
					IL_07b3:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 9;
						return true;
					}
					_003C_003Em__Finally9();
					_003C_003E7__wrap10 = null;
					Check("infinite stamina", delegate
					{
						Expect(_003C_003E8__1.spent > 0.15f, "control failed: stamina did not drop without it (" + F(_003C_003E8__1.spent) + ")");
						float currentStamina = Me.data.currentStamina;
						Call(Me, "UseStamina", 0.2f, true, true);
						Expect(Me.data.currentStamina >= currentStamina, "stamina still dropped: " + F(currentStamina) + " -> " + F(Me.data.currentStamina));
						return "a 0.20 use cost " + F(_003C_003E8__1.spent) + " without it, 0.00 with it";
					});
					State.InfStamina = false;
					_003C_003E7__wrap10 = Wait(0.3f).GetEnumerator();
					_003C_003E1__state = -12;
					goto IL_0837;
					IL_1c81:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 40;
						return true;
					}
					_003C_003Em__Finally39();
					_003C_003E7__wrap10 = null;
					goto IL_1c9b;
					IL_0837:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 10;
						return true;
					}
					_003C_003Em__Finally10();
					_003C_003E7__wrap10 = null;
					Check("infinite stamina switches back off", delegate
					{
						Expect(!Me.infiniteStam, "the flag is still set");
						return (string)null;
					});
					Check("fill bonus stamina", delegate
					{
						Features.BonusStamina();
						Expect(Me.data.extraStamina > 0.9f, "extra stamina is " + F(Me.data.extraStamina));
						Me.SetExtraStamina(0f);
						return (string)null;
					});
					Check("achievements are blocked by default", delegate
					{
						Expect(Kit.BlockAchievements.Value, "the setting is off");
						Expect(!Patches.AchievementPrefix(), "the hook lets them through");
						return (string)null;
					});
					Check("every cosmetic unlocked for this session", delegate
					{
						CustomizationOption[] array = Resources.FindObjectsOfTypeAll<CustomizationOption>();
						int num15 = 0;
						int num16 = 0;
						int num17 = 0;
						CustomizationOption[] array2 = array;
						for (int j = 0; j < array2.Length; j++)
						{
							if (array2[j].IsLocked)
							{
								num15++;
							}
						}
						State.Cosmetics = true;
						array2 = array;
						for (int j = 0; j < array2.Length; j++)
						{
							if (array2[j].IsLocked)
							{
								num16++;
							}
						}
						State.Cosmetics = false;
						array2 = array;
						for (int j = 0; j < array2.Length; j++)
						{
							if (array2[j].IsLocked)
							{
								num17++;
							}
						}
						Expect(array.Length > 20, "found only " + array.Length + " cosmetics");
						Expect(num16 == 0, num16 + " are still locked");
						Expect(num17 == num15, "the locks did not come back: " + num15 + " before, " + num17 + " after");
						return array.Length + " cosmetics: " + num15 + " locked, 0 while on, " + num17 + " again when off";
					});
					Check("unlock for good: works out what it would grant (nothing is granted)", delegate
					{
						int ascent;
						int locked;
						List<ACHIEVEMENTTYPE> list3 = Features.CosmeticRequirements(out ascent, out locked);
						return locked + " locked cosmetics need " + list3.Count + " achievements and ascent " + ascent;
					});
					Section("Movement");
					Check("walk and run speed", delegate
					{
						CharacterMovement movement = Me.refs.movement;
						State.Speed = 1f;
						float num13 = (float)Call(movement, "GetMovementForce");
						State.Speed = 3f;
						float num14 = (float)Call(movement, "GetMovementForce");
						State.Speed = 1f;
						Expect(num13 > 0f, "the scout cannot move right now, nothing to measure");
						Expect(Mathf.Abs(num14 / num13 - 3f) < 0.01f, "x3 gave x" + F(num14 / num13));
						return "force " + F(num13) + " -> " + F(num14) + " at x3";
					});
					State.Jump = 2f;
					State.Climb = 3f;
					_003C_003E7__wrap10 = Wait(0.2f).GetEnumerator();
					_003C_003E1__state = -13;
					goto IL_09ae;
					IL_29a2:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 54;
						return true;
					}
					_003C_003Em__Finally52();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.fovWide = Camera.main.fieldOfView;
					State.Fov = 50f;
					_003C_003E7__wrap10 = Wait(2.5f).GetEnumerator();
					_003C_003E1__state = -55;
					goto IL_2a24;
					IL_22b2:
					if (Time.realtimeSinceStartup < _003Cuntil_003E5__16 && (Object)(object)Me.data.currentItem == (Object)null)
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 47;
						return true;
					}
					_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
					_003C_003E1__state = -48;
					goto IL_231f;
					IL_1d06:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 41;
						return true;
					}
					_003C_003Em__Finally40();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.light = _003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)7);
					State.NoWeight = false;
					_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
					_003C_003E1__state = -43;
					goto IL_1d8b;
					IL_3283:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 67;
						return true;
					}
					_003C_003Em__Finally65();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.fed = Me.refs.afflictions.GetCurrentStatus((STATUSTYPE)1);
					Check("clear a scout's statuses (as host)", delegate
					{
						Expect(_003C_003E8__1.sent, "the message was not sent: not the host");
						Expect(_003C_003E8__1.hungry > 0.2f && _003C_003E8__1.fed < 0.01f, "hunger " + F(_003C_003E8__1.hungry) + " -> " + F(_003C_003E8__1.fed));
						return "hunger " + F(_003C_003E8__1.hungry) + " -> " + F(_003C_003E8__1.fed);
					});
					if ((Object)(object)_003C_003E8__1.give != (Object)null)
					{
						_003C_003E8__5 = new _003C_003Ec__DisplayClass34_4();
						_003C_003E8__5.CS_0024_003C_003E8__locals2 = _003C_003E8__1;
						_003C_003E8__5.lying = 0;
						foreach (Item aLL_ACTIVE_ITEM in Item.ALL_ACTIVE_ITEMS)
						{
							if ((Object)(object)aLL_ACTIVE_ITEM != (Object)null && aLL_ACTIVE_ITEM.itemID == _003C_003E8__5.CS_0024_003C_003E8__locals2.give.itemID && (int)aLL_ACTIVE_ITEM.itemState == 0)
							{
								int lying = _003C_003E8__5.lying;
								_003C_003E8__5.lying = lying + 1;
							}
						}
						Features.HandItem(Me, _003C_003E8__5.CS_0024_003C_003E8__locals2.give);
						_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
						_003C_003E1__state = -68;
						goto IL_33f6;
					}
					goto IL_3507;
					IL_1d8b:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 42;
						return true;
					}
					_003C_003Em__Finally41();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.heavyAgain = _003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)7);
					Check("nothing you carry weighs anything", delegate
					{
						Expect(_003C_003E8__1.heavy > 0f, "the scout was carrying nothing with weight, nothing to measure");
						Expect(_003C_003E8__1.light == 0f, "weight is still " + F(_003C_003E8__1.light));
						Expect(_003C_003E8__1.heavyAgain == _003C_003E8__1.heavy, "weight did not come back: " + F(_003C_003E8__1.heavyAgain) + " vs " + F(_003C_003E8__1.heavy));
						return "weight bar " + F(_003C_003E8__1.heavy) + " -> 0.00 -> " + F(_003C_003E8__1.heavyAgain);
					});
					Section("World");
					Check("time of day", delegate
					{
						Features.SetTime(12f, "noon");
						float timeOfDay = DayNightManager.instance.timeOfDay;
						Features.SetTime(0f, "midnight");
						float timeOfDay2 = DayNightManager.instance.timeOfDay;
						Features.SetTime(9f, "morning");
						Expect(Mathf.Abs(timeOfDay - 12f) < 0.1f && timeOfDay2 < 0.1f, "clock read " + F(timeOfDay) + " and " + F(timeOfDay2));
						return (string)null;
					});
					_003Cfog_003E5__4 = Singleton<OrbFogHandler>.Instance;
					if ((Object)(object)_003Cfog_003E5__4 == (Object)null)
					{
						Check("the fog never comes", delegate
						{
							throw new Exception("no fog handler on this map");
						});
						goto IL_1fc3;
					}
					_003C_003E8__3 = new _003C_003Ec__DisplayClass34_2();
					_003C_003E8__3.wait0 = _003Cfog_003E5__4.currentWaitTime;
					_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
					_003C_003E1__state = -44;
					goto IL_1ec0;
					IL_234c:
					if (_003Cs_003E5__15 < 3 && (Object)(object)Me.data.currentItem == (Object)null)
					{
						Item prefab = Player.localPlayer.itemSlots[_003Cs_003E5__15].prefab;
						_003C_003E8__4.pockets = _003C_003E8__4.pockets + ((_003Cs_003E5__15 > 0) ? ", " : "") + (((Object)(object)prefab != (Object)null) ? ((Object)prefab).name : "empty");
						if (!((Object)(object)prefab == (Object)null))
						{
							Me.refs.items.EquipSlot(Optionable<byte>.Some(_003Cs_003E5__15));
							_003Cuntil_003E5__16 = Time.realtimeSinceStartup + 5f;
							goto IL_22b2;
						}
						goto IL_2339;
					}
					if ((Object)(object)Me.data.currentItem == (Object)null && _003Ci_003E5__12 == 0)
					{
						Features.Give(_003C_003E8__1.give, ((Object)_003C_003E8__1.give).name);
						_003C_003E7__wrap10 = Wait(4f).GetEnumerator();
						_003C_003E1__state = -49;
						goto IL_23fd;
					}
					goto IL_2417;
					IL_2c48:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 58;
						return true;
					}
					_003C_003Em__Finally56();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.nearAgain = Vector3.Distance(((Component)Camera.main).transform.position, Me.Head);
					_003C_003E8__1.headHidden = (Object)(object)_003Cbody_003E5__5 != (Object)null && !(bool)_003Cshowing_003E5__6.GetValue(_003Cbody_003E5__5);
					Check("third person", delegate
					{
						Expect(_003C_003E8__1.far > 1.2f && _003C_003E8__1.far <= State.CamDistance + 1f, "the camera is " + F(_003C_003E8__1.far) + " m from the head");
						Expect(_003C_003E8__1.headShown, "the scout's own head is still hidden");
						Expect((_003C_003E8__1.nearAgain < 1.2f) & _003C_003E8__1.headHidden, "first person did not come back: " + F(_003C_003E8__1.nearAgain) + " m");
						return "camera " + F(_003C_003E8__1.near) + " m -> " + F(_003C_003E8__1.far) + " m -> " + F(_003C_003E8__1.nearAgain) + " m from the head; the head is shown meanwhile";
					});
					_003Cstood_003E5__7 = Me.Center;
					Features.SetFreeCam(on: true);
					_003Cup_003E5__8 = Live.CamPos + Vector3.up * 12f;
					Live.CamPos = _003Cup_003E5__8;
					_003C_003E7__wrap10 = Wait(1f).GetEnumerator();
					_003C_003E1__state = -59;
					goto IL_2d64;
					IL_1c9b:
					_003C_003E8__1.heavy = _003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)7);
					State.NoWeight = true;
					_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
					_003C_003E1__state = -42;
					goto IL_1d06;
					IL_3ca7:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 80;
						return true;
					}
					_003C_003Em__Finally78();
					_003C_003E7__wrap10 = null;
					Shot("Overlay");
					_003C_003E7__wrap10 = Wait(0.8f).GetEnumerator();
					_003C_003E1__state = -81;
					goto IL_3d14;
					IL_3c42:
					if (shots)
					{
						State.EspLuggage = (State.EspCampfires = (State.EspItems = true));
						_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
						_003C_003E1__state = -80;
						goto IL_3ca7;
					}
					goto IL_3d40;
					IL_3d14:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 81;
						return true;
					}
					_003C_003Em__Finally79();
					_003C_003E7__wrap10 = null;
					State.EspLuggage = (State.EspCampfires = (State.EspItems = false));
					goto IL_3d40;
					IL_2bdf:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 57;
						return true;
					}
					_003C_003Em__Finally55();
					_003C_003E7__wrap10 = null;
					State.ThirdPerson = false;
					_003C_003E7__wrap10 = Wait(1.5f).GetEnumerator();
					_003C_003E1__state = -58;
					goto IL_2c48;
					IL_1ec0:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 43;
						return true;
					}
					_003C_003Em__Finally42();
					_003C_003E7__wrap10 = null;
					_003C_003E8__3.wait1 = _003Cfog_003E5__4.currentWaitTime;
					State.StopFog = true;
					_003C_003E8__3.size0 = _003Cfog_003E5__4.currentSize;
					_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
					_003C_003E1__state = -45;
					goto IL_1f55;
					IL_09ae:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 11;
						return true;
					}
					_003C_003Em__Finally11();
					_003C_003E7__wrap10 = null;
					Check("jump height", delegate
					{
						float jumpImpulse = Me.refs.movement.jumpImpulse;
						Expect(Features.JumpBase > 0f && Mathf.Abs(jumpImpulse / Features.JumpBase - 2f) < 0.01f, "x2 gave " + F(jumpImpulse) + " from " + F(Features.JumpBase));
						return "impulse " + F(Features.JumpBase) + " -> " + F(jumpImpulse) + " at x2";
					});
					Check("climbing speed", delegate
					{
						Expect(Mathf.Abs(Me.refs.climbing.climbSpeedMod - 3f) < 0.01f, "x3 gave " + F(Me.refs.climbing.climbSpeedMod));
						return (string)null;
					});
					State.Jump = (State.Climb = 1f);
					_003C_003E7__wrap10 = Wait(0.2f).GetEnumerator();
					_003C_003E1__state = -14;
					goto IL_0a73;
					IL_1f55:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 44;
						return true;
					}
					_003C_003Em__Finally43();
					_003C_003E7__wrap10 = null;
					_003C_003E8__3.wait2 = _003Cfog_003E5__4.currentWaitTime;
					_003C_003E8__3.size1 = _003Cfog_003E5__4.currentSize;
					State.StopFog = false;
					Check("the fog never comes", delegate
					{
						Expect(_003C_003E8__3.wait2 == _003C_003E8__3.wait1 && _003C_003E8__3.size1 == _003C_003E8__3.size0, "the fog clock kept running: " + F(_003C_003E8__3.wait1) + " -> " + F(_003C_003E8__3.wait2));
						return (!(_003C_003E8__3.wait1 > _003C_003E8__3.wait0)) ? "its countdown was already paused by the game here, so only 'did not move' was shown" : ("its countdown ran " + F(_003C_003E8__3.wait1 - _003C_003E8__3.wait0) + " s in 2 s before, 0.00 after");
					});
					_003C_003E8__3 = null;
					goto IL_1fc3;
					IL_202f:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 45;
						return true;
					}
					_003C_003Em__Finally44();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.tags = _003C_003E8__1.menu.espDrawn;
					State.EspScouts = (State.EspLuggage = (State.EspItems = false));
					Check("overlay: luggage and loose items", delegate
					{
						Expect(_003C_003E8__1.menu.lastError == null, _003C_003E8__1.menu.lastError);
						int num12 = 0;
						foreach (Luggage item2 in Luggage.ALL_LUGGAGE)
						{
							if ((Object)(object)item2 != (Object)null && !item2.IsOpen)
							{
								num12++;
							}
						}
						Expect(num12 > 0, "the map has no unopened luggage");
						return _003C_003E8__1.tags + " labels on screen, " + num12 + " unopened luggage on the map (other scouts: none in a solo session)";
					});
					State.EspCampfires = (State.EspStatues = (State.EspHazards = true));
					Features.LookAround(now: true);
					_003C_003E7__wrap10 = Wait(1.5f).GetEnumerator();
					_003C_003E1__state = -47;
					goto IL_20f2;
					IL_1fc3:
					_003C_003E8__1.menu.lastError = null;
					State.EspScouts = (State.EspLuggage = (State.EspItems = true));
					_003C_003E7__wrap10 = Wait(1.5f).GetEnumerator();
					_003C_003E1__state = -46;
					goto IL_202f;
					IL_20f2:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 46;
						return true;
					}
					_003C_003Em__Finally45();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.more = _003C_003E8__1.menu.espDrawn;
					_003C_003E8__1.fires = Features.Fires.Count;
					_003C_003E8__1.statues = Features.Statues.Count;
					_003C_003E8__1.hazards = Features.Hazards.Count;
					State.EspCampfires = (State.EspStatues = (State.EspHazards = false));
					Check("overlay: campfires, scout statues and hazards", delegate
					{
						Expect(_003C_003E8__1.menu.lastError == null, _003C_003E8__1.menu.lastError);
						Expect(_003C_003E8__1.fires > 0, "no campfire was found on the mountain");
						return _003C_003E8__1.fires + " campfires, " + _003C_003E8__1.statues + " scout statues and " + _003C_003E8__1.hazards + " hazards found; " + _003C_003E8__1.more + " labels in range on screen";
					});
					Section("Items, on the island");
					if ((Object)(object)_003C_003E8__1.give != (Object)null)
					{
						_003C_003E8__4 = new _003C_003Ec__DisplayClass34_3();
						_003C_003E8__4.pockets = "";
						_003Ci_003E5__12 = 0;
						goto IL_2429;
					}
					goto IL_285d;
					IL_1a19:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 36;
						return true;
					}
					_003C_003Em__Finally35();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.hurt = _003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)0);
					_003C_003E7__wrap10 = Wait(4f).GetEnumerator();
					_003C_003E1__state = -38;
					goto IL_1a98;
					IL_0a73:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 12;
						return true;
					}
					_003C_003Em__Finally12();
					_003C_003E7__wrap10 = null;
					Check("sliders go back to normal", delegate
					{
						Expect(Mathf.Abs(Me.refs.movement.jumpImpulse - Features.JumpBase) < 0.001f, "jump stayed changed");
						Expect(Mathf.Abs(Me.refs.climbing.climbSpeedMod - 1f) < 0.001f, "climb stayed changed");
						return (string)null;
					});
					_003C_003E8__1.ground = Me.Center;
					State.Fly = true;
					Features.Warp(_003C_003E8__1.ground + Vector3.up * 12f, "the air");
					_003C_003E7__wrap10 = Wait(3f).GetEnumerator();
					_003C_003E1__state = -15;
					goto IL_0b43;
					IL_0510:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 6;
						return true;
					}
					_003C_003Em__Finally6();
					_003C_003E7__wrap10 = null;
					text = _003C_003E8__1.menu.tabs[_003Ci_003E5__12];
					Check("the " + text + " page draws", delegate
					{
						Expect(_003C_003E8__1.menu.lastError == null, _003C_003E8__1.menu.lastError);
						return (string)null;
					});
					_003Ci_003E5__12++;
					goto IL_059b;
					IL_3e52:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 82;
						return true;
					}
					_003C_003Em__Finally80();
					_003C_003E7__wrap10 = null;
					break;
					IL_2429:
					if (_003Ci_003E5__12 < 2 && (Object)(object)Me.data.currentItem == (Object)null)
					{
						_003C_003E8__4.pockets = "";
						_003Cs_003E5__15 = 0;
						goto IL_234c;
					}
					_003Cheld_003E5__13 = Me.data.currentItem;
					_003CheldId_003E5__14 = (ushort)(((Object)(object)_003Cheld_003E5__13 != (Object)null) ? _003Cheld_003E5__13.itemID : 0);
					_003C_003E8__4.heldName = (((Object)(object)_003Cheld_003E5__13 != (Object)null) ? _003Cheld_003E5__13.GetName() : "nothing");
					_003C_003E8__4.inHand = (Object)(object)_003Cheld_003E5__13 != (Object)null;
					_003C_003E8__4.keptOn = false;
					_003C_003E8__4.goneOff = false;
					_003C_003E8__4.usesBefore = -1;
					_003C_003E8__4.usesKept = -1;
					_003C_003E8__4.usesAfter = -1;
					if ((Object)(object)_003Cheld_003E5__13 != (Object)null)
					{
						_003Creduce_003E5__17 = ((Component)_003Cheld_003E5__13).GetComponent<Action_ReduceUses>();
						State.KeepItems = true;
						if ((Object)(object)_003Creduce_003E5__17 != (Object)null)
						{
							_003C_003E8__4.usesBefore = _003Cheld_003E5__13.GetData<OptionableIntItemData>((DataEntryKey)2).Value;
							((ItemActionBase)_003Creduce_003E5__17).RunAction();
							_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
							_003C_003E1__state = -50;
							goto IL_25aa;
						}
						goto IL_25e0;
					}
					goto IL_2834;
					IL_0b43:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 13;
						return true;
					}
					_003C_003Em__Finally13();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.y1 = Hip;
					_003C_003E7__wrap10 = Wait(1.5f).GetEnumerator();
					_003C_003E1__state = -16;
					goto IL_0bb6;
					IL_2e1d:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 60;
						return true;
					}
					_003C_003Em__Finally58();
					_003C_003E7__wrap10 = null;
					Features.SetFreeCam(on: false);
					_003C_003E7__wrap10 = Wait(1.5f).GetEnumerator();
					_003C_003E1__state = -61;
					goto IL_2e86;
					IL_0bb6:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 14;
						return true;
					}
					_003C_003Em__Finally14();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.y2 = Hip;
					Check("fly: hovers when no key is held", delegate
					{
						Expect(_003C_003E8__1.y2 > _003C_003E8__1.ground.y + 8f, "the scout is back on the ground (height " + F(_003C_003E8__1.y2 - _003C_003E8__1.ground.y) + ")");
						Expect(Mathf.Abs(_003C_003E8__1.y2 - _003C_003E8__1.y1) < 0.5f, "drifted " + F(_003C_003E8__1.y2 - _003C_003E8__1.y1) + " m in 1.5 s");
						return "held " + F(_003C_003E8__1.y2 - _003C_003E8__1.ground.y) + " m up, drift " + F(_003C_003E8__1.y2 - _003C_003E8__1.y1) + " m";
					});
					_003C_003E8__1.from = Me.Center;
					heldMove = new Vector2(0f, 1f);
					_003C_003E7__wrap10 = Wait(1f).GetEnumerator();
					_003C_003E1__state = -17;
					goto IL_0c72;
					IL_3121:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 65;
						return true;
					}
					_003C_003Em__Finally63();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.back = Features.Wearing();
					Plugin.Outfits[3].Value = _003CslotWas_003E5__9;
					Check("save an outfit, change hat, wear the saved one", delegate
					{
						Expect(_003C_003E8__1.changed != _003C_003E8__1.wore[5], "the hat did not change, so this proves nothing");
						for (int i = 0; i < 8; i++)
						{
							Expect(_003C_003E8__1.back[i] == _003C_003E8__1.wore[i], "part " + i + " is " + _003C_003E8__1.back[i] + ", was " + _003C_003E8__1.wore[i]);
						}
						Expect((Object)(object)Features.Part(5, _003C_003E8__1.wore[5]) != (Object)null, "the hat has no entry in the game's list");
						return "hat " + _003C_003E8__1.wore[5] + " -> " + _003C_003E8__1.changed + " -> " + _003C_003E8__1.back[5] + "; all 8 parts as saved";
					});
					Section("Help (sent to your own scout)");
					Me.refs.afflictions.AddStatus((STATUSTYPE)1, 0.3f, false, true, true, false, false);
					_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
					_003C_003E1__state = -66;
					goto IL_31eb;
					IL_0c72:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 15;
						return true;
					}
					_003C_003Em__Finally15();
					_003C_003E7__wrap10 = null;
					heldMove = null;
					_003C_003E8__1.to = Me.Center;
					Check("fly: forward moves where you look", delegate
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						//IL_003f: Unknown result type (might be due to invalid IL or missing references)
						//IL_0045: Unknown result type (might be due to invalid IL or missing references)
						//IL_004a: Unknown result type (might be due to invalid IL or missing references)
						//IL_004f: Unknown result type (might be due to invalid IL or missing references)
						//IL_0052: Unknown result type (might be due to invalid IL or missing references)
						//IL_0061: Unknown result type (might be due to invalid IL or missing references)
						float num10 = Vector3.Distance(_003C_003E8__1.from, _003C_003E8__1.to);
						Expect(num10 > 8f, "moved only " + F(num10) + " m in 1 s at speed " + State.FlySpeed);
						Vector3 val2 = _003C_003E8__1.to - _003C_003E8__1.from;
						float num11 = Vector3.Dot(val2.normalized, Me.data.lookDirection);
						Expect(num11 > 0.9f, "went off course (" + F(num11) + ")");
						return F(num10) + " m in 1 s at speed " + State.FlySpeed.ToString("0");
					});
					State.NoClip = true;
					_003C_003E7__wrap10 = Wait(0.3f).GetEnumerator();
					_003C_003E1__state = -18;
					goto IL_0d16;
					IL_2a24:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 55;
						return true;
					}
					_003C_003Em__Finally53();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.fovBack = Camera.main.fieldOfView;
					Check("field of view", delegate
					{
						Expect(Mathf.Abs(_003C_003E8__1.fovWide - 100f) < 0.5f, "asked for 100, the camera has " + F(_003C_003E8__1.fovWide));
						Expect(Mathf.Abs(_003C_003E8__1.fovBack - _003C_003E8__1.fovGame) < 2f, "it did not go back: " + F(_003C_003E8__1.fovBack));
						return F(_003C_003E8__1.fovGame) + " -> " + F(_003C_003E8__1.fovWide) + " -> " + F(_003C_003E8__1.fovBack);
					});
					_003Cbody_003E5__5 = ((Component)Me).GetComponentInChildren<HideTheBody>();
					_003Cshowing_003E5__6 = AccessTools.Field(typeof(HideTheBody), "isShowing");
					_003C_003E8__1.near = Vector3.Distance(((Component)Camera.main).transform.position, Me.Head);
					State.ThirdPerson = true;
					_003C_003E7__wrap10 = Wait(1.5f).GetEnumerator();
					_003C_003E1__state = -56;
					goto IL_2b10;
					IL_0d16:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 16;
						return true;
					}
					_003C_003Em__Finally16();
					_003C_003E7__wrap10 = null;
					Check("fly: pass through walls switches collision off", delegate
					{
						List<Collider> list2 = BodyColliders();
						int num9 = 0;
						foreach (Collider item3 in list2)
						{
							if (item3.enabled)
							{
								num9++;
							}
						}
						Expect(list2.Count > 0, "found no body colliders");
						Expect(num9 == 0, num9 + " of " + list2.Count + " body colliders are still solid");
						return list2.Count + " body colliders off";
					});
					State.NoClip = false;
					_003C_003E7__wrap10 = Wait(0.3f).GetEnumerator();
					_003C_003E1__state = -19;
					goto IL_0da8;
					IL_31eb:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 66;
						return true;
					}
					_003C_003Em__Finally64();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.hungry = Me.refs.afflictions.GetCurrentStatus((STATUSTYPE)1);
					_003C_003E8__1.sent = Features.HealOther(Me);
					_003C_003E7__wrap10 = Wait(1f).GetEnumerator();
					_003C_003E1__state = -67;
					goto IL_3283;
					IL_3ac3:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 78;
						return true;
					}
					_003C_003Em__Finally76();
					_003C_003E7__wrap10 = null;
					Plugin.Outfits[0].Value = _003CfirstOutfit_003E5__10;
					if (KitTest.Focused)
					{
						_003C_003E8__6 = new _003C_003Ec__DisplayClass34_5();
						_003Cjob_003E5__18 = Keys.All.Find((Keys.Bound k) => k.Id == "SaveSpot");
						_003CjobWas_003E5__19 = _003Cjob_003E5__18.Key.Value;
						Live.Spots[1] = null;
						Live.Spots[0] = null;
						_003C_003E8__1.menu.SetShortcut("SaveSpot", (KeyCode)291);
						_003C_003E7__wrap10 = KitTest.Tap((KeyCode)291).GetEnumerator();
						_003C_003E1__state = -79;
						goto IL_3bce;
					}
					goto IL_3c42;
					IL_186f:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 34;
						return true;
					}
					_003C_003Em__Finally33();
					_003C_003E7__wrap10 = null;
					_003C_003E7__wrap10 = Wait(8f).GetEnumerator();
					_003C_003E1__state = -36;
					goto IL_18d2;
					IL_0da8:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 17;
						return true;
					}
					_003C_003Em__Finally17();
					_003C_003E7__wrap10 = null;
					Check("fly: collision comes back", delegate
					{
						List<Collider> list = BodyColliders();
						int num8 = 0;
						foreach (Collider item4 in list)
						{
							if (item4.enabled)
							{
								num8++;
							}
						}
						Expect(num8 == list.Count, "only " + num8 + " of " + list.Count + " are solid again");
						return (string)null;
					});
					State.Fly = false;
					State.AirJump = true;
					_003C_003E7__wrap10 = Wait(0.6f).GetEnumerator();
					_003C_003E1__state = -20;
					goto IL_0e40;
					IL_33f6:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 68;
						return true;
					}
					_003C_003Em__Finally66();
					_003C_003E7__wrap10 = null;
					_003C_003E8__5.lyingNow = 0;
					_003C_003E8__5.nearest = 999f;
					foreach (Item aLL_ACTIVE_ITEM2 in Item.ALL_ACTIVE_ITEMS)
					{
						if ((Object)(object)aLL_ACTIVE_ITEM2 != (Object)null && aLL_ACTIVE_ITEM2.itemID == _003C_003E8__5.CS_0024_003C_003E8__locals2.give.itemID && (int)aLL_ACTIVE_ITEM2.itemState == 0)
						{
							int lying = _003C_003E8__5.lyingNow;
							_003C_003E8__5.lyingNow = lying + 1;
							_003C_003E8__5.nearest = Mathf.Min(_003C_003E8__5.nearest, Vector3.Distance(aLL_ACTIVE_ITEM2.transform.position, Me.Center));
						}
					}
					Check("hand a scout an item", delegate
					{
						Expect(_003C_003E8__5.lyingNow == _003C_003E8__5.lying + 1, ((Object)_003C_003E8__5.CS_0024_003C_003E8__locals2.give).name + " on the ground: " + _003C_003E8__5.lying + " -> " + _003C_003E8__5.lyingNow);
						Expect(_003C_003E8__5.nearest < 4f, "it landed " + F(_003C_003E8__5.nearest) + " m away");
						return ((Object)_003C_003E8__5.CS_0024_003C_003E8__locals2.give).name + " appeared " + F(_003C_003E8__5.nearest) + " m from the scout";
					});
					_003C_003E8__5 = null;
					goto IL_3507;
					IL_3bce:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 79;
						return true;
					}
					_003C_003Em__Finally77();
					_003C_003E7__wrap10 = null;
					_003C_003E8__6.remembered = Live.Spots[0].HasValue;
					_003Cjob_003E5__18.Key.Value = _003CjobWas_003E5__19;
					Check("real keys: a shortcut key for an action", delegate
					{
						Expect(_003C_003E8__6.remembered, "F10 did not remember the spot");
						return "F10 remembers the spot with the menu closed";
					});
					_003C_003E8__6 = null;
					_003Cjob_003E5__18 = null;
					goto IL_3c42;
					IL_3507:
					Section("Teleport, on the island");
					_003C_003E8__1.fire = null;
					try
					{
						_003C_003E8__1.fire = MapHandler.CurrentCampfire;
					}
					catch
					{
					}
					Features.WarpToCampfire();
					_003C_003E7__wrap10 = Wait(1.2f).GetEnumerator();
					_003C_003E1__state = -69;
					goto IL_3580;
					IL_0e40:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 18;
						return true;
					}
					_003C_003Em__Finally18();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.airborne = !Me.data.isGrounded;
					_003C_003E8__1.vyBefore = Me.refs.hip.Rig.linearVelocity.y;
					Call(Me.refs.movement, "TryToJump");
					_003C_003E7__wrap10 = Wait(0.35f).GetEnumerator();
					_003C_003E1__state = -21;
					goto IL_0f08;
					IL_1b2e:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 38;
						return true;
					}
					_003C_003Em__Finally37();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.unhurt = _003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)0);
					_003C_003E8__1.tumble = Me.data.fallSeconds;
					State.NoFall = false;
					Check("no fall damage", delegate
					{
						Expect(_003C_003E8__1.hurt > 0f, "control failed: a " + 14f + " m drop did no damage without it, so this proves nothing");
						Expect(_003C_003E8__1.unhurt == 0f, "still took " + F(_003C_003E8__1.unhurt) + " injury");
						Expect(_003C_003E8__1.tumble <= 0f, "still ragdolled on landing");
						return "a " + 14f + " m drop: " + F(_003C_003E8__1.hurt) + " injury without it, 0.00 with it";
					});
					_003C_003E7__wrap10 = Wait(3f).GetEnumerator();
					_003C_003E1__state = -40;
					goto IL_1be8;
					IL_0f08:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 19;
						return true;
					}
					_003C_003Em__Finally19();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.vyAfter = Me.refs.hip.Rig.linearVelocity.y;
					Check("jump again in mid-air", delegate
					{
						Expect(_003C_003E8__1.airborne, "the scout was already on the ground");
						Expect(Me.data.sinceJump < 0.5f, "the game refused the jump");
						Expect(_003C_003E8__1.vyAfter > _003C_003E8__1.vyBefore + 2f, "no upward kick: " + F(_003C_003E8__1.vyBefore) + " -> " + F(_003C_003E8__1.vyAfter) + " m/s");
						return "falling at " + F(_003C_003E8__1.vyBefore) + " m/s, then " + F(_003C_003E8__1.vyAfter) + " m/s";
					});
					State.AirJump = false;
					State.NoFall = true;
					_003C_003E7__wrap10 = Wait(4f).GetEnumerator();
					_003C_003E1__state = -22;
					goto IL_0fbb;
					IL_3580:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 69;
						return true;
					}
					_003C_003Em__Finally67();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.atFire = Me.Center;
					Check("teleport to the campfire", delegate
					{
						//IL_0017: Unknown result type (might be due to invalid IL or missing references)
						//IL_0027: Unknown result type (might be due to invalid IL or missing references)
						//IL_0060: Unknown result type (might be due to invalid IL or missing references)
						//IL_0070: Unknown result type (might be due to invalid IL or missing references)
						Expect((Object)(object)_003C_003E8__1.fire != (Object)null, "this section has no campfire");
						float num7 = Vector3.Distance(_003C_003E8__1.atFire, ((Component)_003C_003E8__1.fire).transform.position);
						Expect(num7 < 6f, "ended " + F(num7) + " m from the campfire");
						return F(num7) + " m from the fire, " + F(Vector3.Distance(_003C_003E8__1.beach, ((Component)_003C_003E8__1.fire).transform.position)) + " m from where the run began";
					});
					_003C_003E7__wrap10 = Wait(3f).GetEnumerator();
					_003C_003E1__state = -70;
					goto IL_3613;
					IL_0fbb:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 20;
						return true;
					}
					_003C_003Em__Finally20();
					_003C_003E7__wrap10 = null;
					State.NoFall = false;
					Section("Teleport");
					_003C_003E8__1.spot = Me.Center + new Vector3(6f, 3f, 0f);
					Features.Warp(_003C_003E8__1.spot, "a test spot");
					_003C_003E7__wrap10 = Wait(1.2f).GetEnumerator();
					_003C_003E1__state = -23;
					goto IL_1071;
					IL_2339:
					_003Cs_003E5__15++;
					goto IL_234c;
					IL_1071:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 21;
						return true;
					}
					_003C_003Em__Finally21();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.arrived = Me.Center;
					Check("teleport", delegate
					{
						//IL_0016: Unknown result type (might be due to invalid IL or missing references)
						//IL_0031: Unknown result type (might be due to invalid IL or missing references)
						float num6 = Vector2.Distance(new Vector2(_003C_003E8__1.arrived.x, _003C_003E8__1.arrived.z), new Vector2(_003C_003E8__1.spot.x, _003C_003E8__1.spot.z));
						Expect(num6 < 2f, "ended " + F(num6) + " m away from the target");
						return "landed " + F(num6) + " m from the target";
					});
					_003C_003E7__wrap10 = Wait(3f).GetEnumerator();
					_003C_003E1__state = -24;
					goto IL_1104;
					IL_3d40:
					Section("Left alone");
					SelfTest.lines.Add("      unlock every cosmetic for good: it would put real achievements on this Steam account");
					SelfTest.lines.Add("      revive me, and revive another scout: both need a scout who is down, and dying alone ends the run");
					SelfTest.lines.Add("      skip to the next section: changes the map under the test");
					SelfTest.lines.Add("      the lava does not rise: there is no lava on the first part of the mountain");
					SelfTest.lines.Add("      reach: no wall at a known distance to try it on; the hook was applied");
					SelfTest.lines.Add("      rope length: the number of segments was read back, no rope was thrown");
					SelfTest.lines.Add("      no hazards and no bad weather: the answers the game gets were checked, not a whole run without them");
					SelfTest.lines.Add("      the Help and Troll pages against a second player: there was nobody else, so each message went to the tester's own scout");
					Settings.Blank();
					if (Live.FreeCam)
					{
						Features.SetFreeCam(on: false);
					}
					if (Screen.width == screenW && Screen.height == screenH && Screen.fullScreenMode == screenMode)
					{
						break;
					}
					Screen.SetResolution(screenW, screenH, screenMode);
					_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
					_003C_003E1__state = -82;
					goto IL_3e52;
					IL_1104:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 22;
						return true;
					}
					_003C_003Em__Finally22();
					_003C_003E7__wrap10 = null;
					Section("Items");
					_003C_003E8__1.give = null;
					Check("the item list loads", delegate
					{
						//IL_006d: Unknown result type (might be due to invalid IL or missing references)
						//IL_0073: Expected O, but got Unknown
						//IL_0102: Unknown result type (might be due to invalid IL or missing references)
						//IL_0129: Unknown result type (might be due to invalid IL or missing references)
						//IL_0133: Expected O, but got Unknown
						_003C_003E8__1.menu.LoadItems();
						Expect(_003C_003E8__1.menu.items.Count > 50, "only " + _003C_003E8__1.menu.items.Count + " items");
						int num5 = 0;
						foreach (Picker.Entry item5 in _003C_003E8__1.menu.items)
						{
							Item val = (Item)item5.What;
							if ((Object)(object)item5.Icon != (Object)null)
							{
								num5++;
							}
							if ((Object)(object)_003C_003E8__1.give == (Object)null && val.UIData.canPocket && ((Object)val).name == "Bandages")
							{
								_003C_003E8__1.give = val;
							}
						}
						foreach (Picker.Entry item6 in _003C_003E8__1.menu.items)
						{
							if ((Object)(object)_003C_003E8__1.give == (Object)null && ((Item)item6.What).UIData.canPocket && !(item6.What is Backpack))
							{
								_003C_003E8__1.give = (Item)item6.What;
							}
						}
						return _003C_003E8__1.menu.items.Count + " items, " + num5 + " with an icon";
					});
					if ((Object)(object)_003C_003E8__1.give != (Object)null)
					{
						Features.Give(_003C_003E8__1.give, ((Object)_003C_003E8__1.give).name);
						_003C_003E7__wrap10 = Wait(3f).GetEnumerator();
						_003C_003E1__state = -25;
						goto IL_11ce;
					}
					goto IL_1203;
					IL_23fd:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 49;
						return true;
					}
					_003C_003Em__Finally47();
					_003C_003E7__wrap10 = null;
					goto IL_2417;
					IL_11ce:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 23;
						return true;
					}
					_003C_003Em__Finally23();
					_003C_003E7__wrap10 = null;
					Check("spawn an item into your hands", delegate
					{
						Expect(Player.localPlayer.HasInAnySlot(_003C_003E8__1.give.itemID), ((Object)_003C_003E8__1.give).name + " never reached the inventory");
						return ((Object)_003C_003E8__1.give).name;
					});
					goto IL_1203;
					IL_2417:
					_003Ci_003E5__12++;
					goto IL_2429;
					IL_378d:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 73;
						return true;
					}
					_003C_003Em__Finally71();
					_003C_003E7__wrap10 = null;
					Check("remember a spot and go back to it", delegate
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_000b: Unknown result type (might be due to invalid IL or missing references)
						float num4 = Vector3.Distance(_003C_003E8__1.atFire, Me.Center);
						Expect(num4 < 3f, "ended " + F(num4) + " m from the remembered spot");
						Features.GoSpot(2);
						return "back within " + F(num4) + " m; an empty spot says so and does nothing";
					});
					_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
					_003C_003E1__state = -74;
					goto IL_380b;
					IL_1203:
					Section("Troll (each one sent to your own scout)");
					Features.Trip(Me, 2f);
					_003C_003E7__wrap10 = Wait(0.3f).GetEnumerator();
					_003C_003E1__state = -26;
					goto IL_1265;
					IL_1265:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 24;
						return true;
					}
					_003C_003Em__Finally24();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.fell = Me.data.fallSeconds;
					Check("trip", delegate
					{
						Expect(_003C_003E8__1.fell > 1f, "the scout did not go down (fall time " + F(_003C_003E8__1.fell) + ")");
						return "ragdolled for " + F(_003C_003E8__1.fell) + " s";
					});
					_003C_003E7__wrap10 = Wait(4f).GetEnumerator();
					_003C_003E1__state = -27;
					goto IL_12fd;
					IL_2e86:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 61;
						return true;
					}
					_003C_003Em__Finally59();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.home = Vector3.Distance(((Component)Camera.main).transform.position, Me.Head);
					Check("free camera", delegate
					{
						Expect(_003C_003E8__1.camOff < 0.1f, "the view is " + F(_003C_003E8__1.camOff) + " m from where it was put");
						Expect(_003C_003E8__1.scoutMoved < 1f, "the scout moved " + F(_003C_003E8__1.scoutMoved) + " m");
						Expect(_003C_003E8__1.home < 1.2f, "the view did not come back to the scout: " + F(_003C_003E8__1.home) + " m");
						return "the view went 12 m up while the scout stayed (" + F(_003C_003E8__1.scoutMoved) + " m), then came back";
					});
					State.HideHud = true;
					_003C_003E7__wrap10 = Wait(0.3f).GetEnumerator();
					_003C_003E1__state = -62;
					goto IL_2f33;
					IL_12fd:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 25;
						return true;
					}
					_003C_003Em__Finally25();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.launchFrom = Hip;
					Features.Launch(Me, Vector3.up, 25f);
					_003C_003E8__1.peak = _003C_003E8__1.launchFrom;
					_003ClaunchEnd_003E5__3 = Time.realtimeSinceStartup + 2.5f;
					_003C_003E8__1.topSpeed = 0f;
					goto IL_13ea;
					IL_18d2:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 35;
						return true;
					}
					_003C_003Em__Finally34();
					_003C_003E7__wrap10 = null;
					SelfTest.lines.Add("");
					lines = SelfTest.lines;
					activeScene = SceneManager.GetActiveScene();
					lines.Add("      started a run: " + activeScene.name);
					_003C_003E8__1.aff = Me.refs.afflictions;
					Section("Scout, on the island");
					Check("statuses land without god mode (control)", delegate
					{
						Expect(_003C_003E8__1.aff.AddStatus((STATUSTYPE)3, 0.1f, false, true, true, false, false) && _003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)3) > 0.05f, "the game refused a status, so the god mode check below proves nothing");
						return "poison " + F(_003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)3));
					});
					Check("god mode", delegate
					{
						State.God = true;
						float currentStatus = _003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)0);
						float currentStatus2 = _003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)2);
						bool num3 = _003C_003E8__1.aff.AddStatus((STATUSTYPE)0, 0.3f, false, true, true, false, false);
						bool flag = _003C_003E8__1.aff.AddStatus((STATUSTYPE)2, 0.3f, false, true, true, false, false);
						bool flag2 = _003C_003E8__1.aff.AddStatus((STATUSTYPE)1, 0.3f, false, true, true, false, false);
						State.God = false;
						Expect(!num3 && !flag && !flag2, "a status got through");
						Expect(_003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)0) == currentStatus && _003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)2) == currentStatus2, "a bar moved");
						return "injury, cold and hunger all refused";
					});
					Check("clear statuses + full stamina", delegate
					{
						Me.data.currentStamina = 0.2f;
						Features.Heal();
						Expect(_003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)3) == 0f, "poison is still " + F(_003C_003E8__1.aff.GetCurrentStatus((STATUSTYPE)3)));
						Expect(Me.data.currentStamina > 0.9f, "stamina is " + F(Me.data.currentStamina));
						return (string)null;
					});
					_003C_003E8__1.beach = Me.Center;
					Features.Warp(_003C_003E8__1.beach + Vector3.up * 14f, "a drop");
					_003C_003E7__wrap10 = Wait(6f).GetEnumerator();
					_003C_003E1__state = -37;
					goto IL_1a19;
					IL_13ea:
					if (Time.realtimeSinceStartup < _003ClaunchEnd_003E5__3)
					{
						_003C_003E8__1.peak = Mathf.Max(_003C_003E8__1.peak, Hip);
						_003C_003E8__1.topSpeed = Mathf.Max(_003C_003E8__1.topSpeed, Me.refs.hip.Rig.linearVelocity.y);
						_003C_003E2__current = null;
						_003C_003E1__state = 26;
						return true;
					}
					Check("launch upward", delegate
					{
						Expect(_003C_003E8__1.peak - _003C_003E8__1.launchFrom > 5f, "rose only " + F(_003C_003E8__1.peak - _003C_003E8__1.launchFrom) + " m");
						return "asked for 25 m/s, left at " + F(_003C_003E8__1.topSpeed) + " m/s, rose " + F(_003C_003E8__1.peak - _003C_003E8__1.launchFrom) + " m";
					});
					_003C_003E7__wrap10 = Wait(5f).GetEnumerator();
					_003C_003E1__state = -28;
					goto IL_145e;
					IL_2fb9:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 63;
						return true;
					}
					_003C_003Em__Finally61();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.hudOn = ((Behaviour)GUIManager.instance.hudCanvas).enabled;
					Check("hide the game's HUD", delegate
					{
						Expect(_003C_003E8__1.hudOff & _003C_003E8__1.hudOn, "hidden: " + _003C_003E8__1.hudOff + ", back: " + _003C_003E8__1.hudOn);
						return (string)null;
					});
					Section("Outfits");
					_003CslotWas_003E5__9 = Plugin.Outfits[3].Value;
					_003C_003E8__1.wore = Features.Wearing();
					Features.SaveOutfit(3);
					num2 = Singleton<Customization>.Instance.hats.Length;
					CharacterCustomization.SetCharacterHat((_003C_003E8__1.wore[5] + 1) % num2);
					_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
					_003C_003E1__state = -64;
					goto IL_30a6;
					IL_145e:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 27;
						return true;
					}
					_003C_003Em__Finally26();
					_003C_003E7__wrap10 = null;
					Features.MakeJump(Me);
					_003C_003E7__wrap10 = Wait(0.3f).GetEnumerator();
					_003C_003E1__state = -29;
					goto IL_14cb;
					IL_2b10:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 56;
						return true;
					}
					_003C_003Em__Finally54();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.far = Vector3.Distance(((Component)Camera.main).transform.position, Me.Head);
					_003C_003E8__1.headShown = (Object)(object)_003Cbody_003E5__5 != (Object)null && (bool)_003Cshowing_003E5__6.GetValue(_003Cbody_003E5__5);
					if (shots)
					{
						Shot("ThirdPerson");
					}
					_003C_003E7__wrap10 = Wait(0.5f).GetEnumerator();
					_003C_003E1__state = -57;
					goto IL_2bdf;
					IL_14cb:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 28;
						return true;
					}
					_003C_003Em__Finally27();
					_003C_003E7__wrap10 = null;
					Check("make them jump", delegate
					{
						Expect(Me.data.sinceJump < 0.5f, "no jump happened");
						return (string)null;
					});
					_003C_003E7__wrap10 = Wait(2f).GetEnumerator();
					_003C_003E1__state = -30;
					goto IL_1557;
					IL_25aa:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 50;
						return true;
					}
					_003C_003Em__Finally48();
					_003C_003E7__wrap10 = null;
					_003C_003E8__4.usesKept = _003Cheld_003E5__13.GetData<OptionableIntItemData>((DataEntryKey)2).Value;
					goto IL_25e0;
					IL_25e0:
					((MonoBehaviour)_003Cheld_003E5__13).StartCoroutine(_003Cheld_003E5__13.ConsumeDelayed(false));
					_003C_003E7__wrap10 = Wait(1.5f).GetEnumerator();
					_003C_003E1__state = -51;
					goto IL_2641;
					IL_231f:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 48;
						return true;
					}
					_003C_003Em__Finally46();
					_003C_003E7__wrap10 = null;
					goto IL_2339;
					IL_1557:
					if (_003C_003E7__wrap10.MoveNext())
					{
						_ = _003C_003E7__wrap10.Current;
						_003C_003E2__current = null;
						_003C_003E1__state = 29;
						return true;
					}
					_003C_003Em__Finally28();
					_003C_003E7__wrap10 = null;
					_003C_003E8__1.bringFrom = Me.Center;
					Features.Bring(Me);
					_003C_003E7__wrap10 = Wait(1.2f).GetEnumerator();
					_003C_003E1__state = -31;
					goto IL_15d9;
				}
				if (!_003ChadSave_003E5__2 && Quicksave.Exists)
				{
					Quicksave.DestroySaveData();
				}
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

		private void _003C_003Em__Finally35()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally36()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally37()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally38()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally39()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally40()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally41()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally42()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally43()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally44()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally45()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally46()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally47()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally48()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally49()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally50()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally51()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally52()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally53()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally54()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally55()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally56()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally57()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally58()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally59()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally60()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally61()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally62()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally63()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally64()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally65()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally66()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally67()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally68()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally69()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally70()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally71()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally72()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally73()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally74()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally75()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally76()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally77()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally78()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally79()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap10 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		private void _003C_003Em__Finally80()
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
	}

	[CompilerGenerated]
	private sealed class _003CEggOnly_003Ed__32 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Menu menu;

		private float _003Cuntil_003E5__2;

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
		public _003CEggOnly_003Ed__32(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap2 = null;
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
					_003Cuntil_003E5__2 = Time.realtimeSinceStartup + 25f;
					goto IL_0056;
				case 1:
					_003C_003E1__state = -1;
					goto IL_0056;
				case 2:
					{
						_003C_003E1__state = -3;
						break;
					}
					IL_0056:
					if (Time.realtimeSinceStartup < _003Cuntil_003E5__2)
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003E7__wrap2 = Egg.Only(menu, Check, EggShot).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap2.MoveNext())
				{
					_ = _003C_003E7__wrap2.Current;
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003Em__Finally1();
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

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CEggShot_003Ed__33 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string name;

		public string _003C_003E3__name;

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
		public _003CEggShot_003Ed__33(int _003C_003E1__state)
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
			{
				_003C_003E1__state = -1;
				string text = Path.Combine(Paths.BepInExRootPath, "summit_shots");
				Directory.CreateDirectory(text);
				ScreenCapture.CaptureScreenshot(Path.Combine(text, name + ".png"));
				_003Ci_003E5__2 = 0;
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				break;
			}
			if (_003Ci_003E5__2 < 15)
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
			_003CEggShot_003Ed__33 _003CEggShot_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEggShot_003Ed__ = this;
			}
			else
			{
				_003CEggShot_003Ed__ = new _003CEggShot_003Ed__33(0);
			}
			_003CEggShot_003Ed__.name = _003C_003E3__name;
			return _003CEggShot_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CWait_003Ed__21 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private float seconds;

		public float _003C_003E3__seconds;

		private float _003Cuntil_003E5__2;

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
		public _003CWait_003Ed__21(int _003C_003E1__state)
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
				_003Cuntil_003E5__2 = Time.realtimeSinceStartup + seconds;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.realtimeSinceStartup < _003Cuntil_003E5__2)
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
			_003CWait_003Ed__21 _003CWait_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CWait_003Ed__ = this;
			}
			else
			{
				_003CWait_003Ed__ = new _003CWait_003Ed__21(0);
			}
			_003CWait_003Ed__.seconds = _003C_003E3__seconds;
			return _003CWait_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CWaitFor_003Ed__22 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private float timeout;

		public float _003C_003E3__timeout;

		private string what;

		public string _003C_003E3__what;

		private Func<bool> cond;

		public Func<bool> _003C_003E3__cond;

		private float _003Cuntil_003E5__2;

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
		public _003CWaitFor_003Ed__22(int _003C_003E1__state)
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
				_003Cuntil_003E5__2 = Time.realtimeSinceStartup + timeout;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (!cond())
			{
				if (Time.realtimeSinceStartup > _003Cuntil_003E5__2)
				{
					throw new Exception("timed out waiting for " + what);
				}
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
			_003CWaitFor_003Ed__22 _003CWaitFor_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CWaitFor_003Ed__ = this;
			}
			else
			{
				_003CWaitFor_003Ed__ = new _003CWaitFor_003Ed__22(0);
			}
			_003CWaitFor_003Ed__.what = _003C_003E3__what;
			_003CWaitFor_003Ed__.timeout = _003C_003E3__timeout;
			_003CWaitFor_003Ed__.cond = _003C_003E3__cond;
			return _003CWaitFor_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	private static readonly string Flag = Path.Combine(Paths.BepInExRootPath, "summit_selftest.flag");

	private static readonly string Report = Path.Combine(Paths.BepInExRootPath, "summit_selftest_report.txt");

	private static IEnumerator run;

	private static bool done;

	private static bool shots;

	private static int passed;

	private static int failed;

	private static readonly List<string> lines = new List<string>();

	private static Vector2? heldMove;

	private static int screenW;

	private static int screenH;

	private static FullScreenMode screenMode;

	internal static bool Asked => File.Exists(Flag);

	private static Character Me => Features.Me;

	private static bool Ready
	{
		get
		{
			if (Features.InGame)
			{
				return !LoadingScreenHandler.loading;
			}
			return false;
		}
	}

	private static float Hip => Me.refs.hip.Rig.position.y;

	public static void Tick(Menu menu)
	{
		if (done)
		{
			return;
		}
		if (run == null)
		{
			if (!File.Exists(Flag))
			{
				done = true;
				return;
			}
			bool num = File.ReadAllText(Flag).Contains("egg");
			shots = File.ReadAllText(Flag).Contains("shots");
			File.Delete(Flag);
			Plugin.L.LogInfo((object)"self-test starting");
			run = (num ? EggOnly(menu) : Body(menu));
		}
		try
		{
			if (!run.MoveNext())
			{
				Finish(null);
			}
		}
		catch (Exception ex)
		{
			Plugin.L.LogError((object)ex);
			Finish("ABORTED: " + ex.Message);
		}
	}

	public static void LateTick()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (heldMove.HasValue && Features.InGame)
		{
			Features.Me.input.movementInput = heldMove.Value;
		}
	}

	private static void Finish(string note)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		done = true;
		if (screenW > 0 && (Screen.width != screenW || Screen.height != screenH || Screen.fullScreenMode != screenMode))
		{
			Screen.SetResolution(screenW, screenH, screenMode);
		}
		heldMove = null;
		List<string> list = new List<string> { string.Format("Summit {0} self-test: {1} passed, {2} failed", "1.1.1", passed, failed) };
		list.Add("Game " + Application.version + ", solo session, other plugins loaded: " + (Chainloader.PluginInfos.Count - 1));
		if (note != null)
		{
			list.Add(note);
		}
		list.Add("");
		list.AddRange(lines);
		File.WriteAllLines(Report, list.ToArray());
		Plugin.L.LogInfo((object)(list[0] + ((note != null) ? (" - " + note) : "")));
		Application.Quit();
	}

	private static void Section(string name)
	{
		lines.Add("");
		lines.Add("[" + name + "]");
	}

	private static void Check(string name, Func<string> fn)
	{
		string text2;
		try
		{
			string text = fn();
			passed++;
			text2 = "PASS  " + name + (string.IsNullOrEmpty(text) ? "" : ("  - " + text));
		}
		catch (Exception ex)
		{
			failed++;
			Exception ex2 = ((ex is TargetInvocationException && ex.InnerException != null) ? ex.InnerException : ex);
			text2 = "FAIL  " + name + "  - " + ex2.Message;
		}
		lines.Add(text2);
		Plugin.L.LogInfo((object)text2);
	}

	private static void Expect(bool cond, string msg)
	{
		if (!cond)
		{
			throw new Exception(msg);
		}
	}

	private static void Shot(string name)
	{
		if (shots && ((!name.StartsWith("es_") && !name.StartsWith("zh_")) || name.EndsWith("_Scout") || name.EndsWith("_World") || name.EndsWith("_About")))
		{
			string text = Path.Combine(Paths.BepInExRootPath, "summit_shots");
			Directory.CreateDirectory(text);
			ScreenCapture.CaptureScreenshot(Path.Combine(text, name + ".png"));
		}
	}

	[IteratorStateMachine(typeof(_003CWait_003Ed__21))]
	private static IEnumerable Wait(float seconds)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWait_003Ed__21(-2)
		{
			_003C_003E3__seconds = seconds
		};
	}

	[IteratorStateMachine(typeof(_003CWaitFor_003Ed__22))]
	private static IEnumerable WaitFor(string what, float timeout, Func<bool> cond)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWaitFor_003Ed__22(-2)
		{
			_003C_003E3__what = what,
			_003C_003E3__timeout = timeout,
			_003C_003E3__cond = cond
		};
	}

	private static object Call(object o, string method, params object[] args)
	{
		MethodInfo methodInfo = AccessTools.Method(o.GetType(), method, (Type[])null, (Type[])null);
		if (methodInfo == null)
		{
			throw new MissingMethodException(o.GetType().Name, method);
		}
		return methodInfo.Invoke(o, args);
	}

	private static string F(float v)
	{
		return v.ToString("0.00");
	}

	private static List<Collider> BodyColliders()
	{
		return (List<Collider>)AccessTools.Field(typeof(CharacterRagdoll), "colliderList").GetValue(Me.refs.ragdoll);
	}

	[IteratorStateMachine(typeof(_003CEggOnly_003Ed__32))]
	private static IEnumerator EggOnly(Menu menu)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEggOnly_003Ed__32(0)
		{
			menu = menu
		};
	}

	[IteratorStateMachine(typeof(_003CEggShot_003Ed__33))]
	private static IEnumerable EggShot(string name)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEggShot_003Ed__33(-2)
		{
			_003C_003E3__name = name
		};
	}

	[IteratorStateMachine(typeof(_003CBody_003Ed__34))]
	private static IEnumerator Body(Menu menu)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBody_003Ed__34(0)
		{
			menu = menu
		};
	}
}
