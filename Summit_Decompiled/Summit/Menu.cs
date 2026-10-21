using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

public class Menu : MonoBehaviour
{
	internal class Control
	{
		public string Id;

		public string Label;

		public string Hint;

		public string Match;

		public Group In;

		public Func<bool> Hidden;

		public Func<bool> On;

		public Action<bool> Turn;

		public Func<float> Value;

		public Action<float> Move;

		public float Min;

		public float Max;

		public float Step;

		public Func<float, string> Shown;

		public string[] Buttons;

		public Func<int, string> ButtonNow;

		public Action<int> Press;

		public bool Strong;

		public bool Raw;

		public Func<int> Chosen;

		public Action<int> Choose;

		public Action<float> Custom;

		public Rect Where;

		public bool Pinnable => Custom == null;
	}

	internal class Group
	{
		public string Page;

		public string Title;

		public int Column;

		public Func<string> Summary;

		public readonly List<Control> Items = new List<Control>();
	}

	internal static Menu Instance;

	internal static bool IsOpen;

	internal static Func<bool> Ready = () => true;

	internal static string NotReady = "Load a save first: nothing here works from the title screen.";

	internal static bool OwnCursor = true;

	internal static readonly Dictionary<string, string> Names = new Dictionary<string, string>();

	internal readonly List<Group> groups = new List<Group>();

	internal readonly Dictionary<string, Control> controls = new Dictionary<string, Control>();

	internal readonly List<string> pinned = new List<string>();

	internal readonly List<Control> matches = new List<Control>();

	internal readonly Dictionary<string, Action<float>> whole = new Dictionary<string, Action<float>>();

	internal readonly Dictionary<string, string> wholeSearch = new Dictionary<string, string>();

	public static int LogoClicks = 0;
	public static bool LarpRevealed = false;
	public static bool IsLarpNetworkVisible => ModNet.Role == ModNet.ModRole.Master || LarpRevealed;

	private Rect itemWin = new Rect(100f, 100f, 760f, 540f);
	private bool itemWinInit = false;
	private int targetDeliverActor = 0;

	public static readonly string[] AllSections = new string[10]
	{
		"Home", "Scout", "Movement", "Items", "Teleport", "World", "Camera", "Help", "Troll", "About"
	};

	internal string[] tabs = new string[2] { "Home", "About" };

	internal readonly Dictionary<int, Rect> tabWhere = new Dictionary<int, Rect>();

	internal const int HomeTab = 0;

	internal int tab;

	internal string lastError;

	internal string find = "";

	internal string pageSearch = "";

	private string lastFind;

	private Group building;

	private Control hovered;

	private string hoveredNote;

	private bool built;

	private bool languageRead;

	private bool listening;

	private bool cursorTaken;

	private bool savedVisible;

	private CursorLockMode savedLock;

	private string keyFor;

	private GUIStyle activeStyle;

	private Rect win = new Rect(60f, 50f, 980f, 660f);

	private const float TallestH = 660f;

	private const float ChromeH = 116f;

	private float pageHeight = 400f;

	private float wantHeight = 660f;

	private string status = "";

	private float statusUntil;

	private Vector2 pageScroll;

	internal const float SideW = 136f;

	internal const float Gutter = 10f;

	internal const float RowH = 30f;

	internal const float SliderH = 46f;

	internal const float ButtonH = 32f;

	internal static float ForcedScale;

	private readonly Picker picker = new Picker();

	private bool itemsLoaded;

	private Character target;

	private Character friend;

	private float launchSpeed = 25f;

	private float foreverUntil;

	private string foreverAsk;

	private GUIStyle espStyle;

	private GUIStyle espShadow;

	internal int espDrawn;

	internal int AboutTab => (tabs != null) ? Array.IndexOf(tabs, "About") : -1;

	internal string HoveredId { get; private set; }

	internal bool open
	{
		get
		{
			return IsOpen;
		}
		set
		{
			SetOpen(value);
		}
	}

	internal Rect Window => win;

	internal string Status
	{
		get
		{
			if (!(Time.unscaledTime < statusUntil))
			{
				return "";
			}
			return status;
		}
	}

	internal static float Scale
	{
		get
		{
			if (ForcedScale > 0f)
			{
				return ForcedScale;
			}
			float num = Mathf.Min((float)Screen.width / 1100f, (float)Screen.height / 760f);
			if (!(num < 1f))
			{
				return Mathf.Max(1f, (float)Screen.height / 1080f);
			}
			return Mathf.Max(0.55f, num);
		}
	}

	internal bool Listening => listening;

	internal string KeyFor => keyFor;

	internal List<Picker.Entry> items
	{
		get
		{
			if (!itemsLoaded)
			{
				return null;
			}
			return picker.All;
		}
	}

	internal Picker.Entry picked
	{
		get
		{
			return picker.Picked;
		}
		set
		{
			picker.Picked = value;
		}
	}

	private void Pages()
	{
		tabs = (string[])AllSections.Clone();
		Whole("Items", DrawItems, "Search every item in the game");
		Card("Scout", "Survival", 0);
		Sw("God", "God mode", "No injury, hunger, cold, poison or any other status. Things that kill outright, like dropping off the bottom of the map, still do.");
		Sw("InfStamina", "Infinite stamina", "Your stamina bar stays full: run, climb and jump as long as you like.");
		Sw("NoFall", "No fall damage", "A long drop does not hurt you and does not knock you over.");
		Sw("NoWeight", "Nothing weighs anything", "What you carry adds no weight to your stamina bar.");
		Sw("KeepItems", "Items never run out", "Food, medicine, pitons, rope and anything else with a number of uses is not used up when you use it. Only for what you hold yourself.");
		Card("Scout", "Right now", 1);
		Bt("heal", "Clears every status and fills your stamina. Bonus stamina gives you the extra yellow bar.", delegate(int i)
		{
			if (i == 0)
			{
				Features.Heal();
			}
			else
			{
				Features.BonusStamina();
			}
		}, "Clear statuses", "Bonus stamina");
		Bt("revive", "Brings you back where you went down. The game drops what you were carrying when it revives a scout.", delegate
		{
			Features.Revive();
		}, "Revive me");
		Card("Scout", "Cosmetics", 1);
		Sw("Cosmetics", "Every cosmetic, for this session", "Open your passport and pick anything. Nothing is written to Steam, and the locks are back the next time you start the game.");
		Cu("forever", "Unlock every cosmetic for good", "The game decides what you own by asking Steam for your achievements. Keeping cosmetics means those achievements really go on your Steam profile, and the ascent levels they need are opened too. There is no taking it back.", DrawForever);
		Card("Scout", "Outfits", 2);
		Cu("outfits", "Saved outfits", "Four places to keep an outfit. Save what you are wearing now, and put it back on with one click. Works in the airport and on the mountain.", DrawOutfits);
		Card("Movement", "On foot", 0);
		Sl("Speed", "Walk and run speed", "How fast you move on the ground.", 1f, 5f, 0.5f, Times);
		Sl("Jump", "Jump height", "How high a jump takes you.", 1f, 5f, 0.5f, Times);
		Sw("AirJump", "Jump again in mid-air", "Jump as often as you like without touching the ground.");
		Card("Movement", "Climbing", 0);
		Sl("Climb", "Climbing speed", "How fast you move on a wall.", 1f, 5f, 0.5f, Times);
		Sw("ClimbAnything", "Grab any surface", "You can start climbing on overhangs and on ground the game calls too flat or too steep to hold.");
		Sw("NoSlip", "Never slip", "You do not slide down a wall, on ice, when out of stamina or after a fall onto it.");
		Sl("Reach", "Reach", "How far away a wall can be for you to grab it.", 1f, 4f, 0.5f, Times);
		Sl("RopeLength", "Rope length", "How long a rope from a spool can get. The spool is kept full enough for it.", 1f, 3f, 0.5f, Times);
		Card("Movement", "Flying", 1);
		Control control = Sw("Fly", "Fly", "Move keys fly where you look, jump goes up, crouch goes down, sprint goes faster. Let go and you hover. Fall damage is off while flying.");
		Action<bool> plainTurn = control.Turn;
		control.Turn = delegate(bool v)
		{
			if (v && Features.InGame)
			{
				Features.Me.refs.climbing.StopAnyClimbing();
			}
			plainTurn(v);
		};
		Sw("NoClip", "Pass through walls while flying", "While you fly, nothing stops you: not walls, not the ground.");
		Sl("FlySpeed", "Flying speed", "How fast you fly, in metres per second. Sprint is two and a half times this.", 5f, 60f, 5f, (float v) => v.ToString("0"));
		Card("Teleport", "Places", 0);
		Bt("warpAim", "Aim first, then open the menu and press the button. It also has a shortcut key on the About page.", delegate
		{
			open = false;
			Features.WarpToAim();
		}, "To where I am looking");
		Bt("warpPlaces", "The campfire and the scout statue of the part of the mountain you are on.", delegate(int i)
		{
			if (i == 0)
			{
				Features.WarpToCampfire();
			}
			else
			{
				Features.WarpToStatue();
			}
		}, "To the campfire", "To the scout statue");
		Card("Teleport", "My spots", 0);
		Cu("spots", "Three spots to remember", "Remember where you stand and come back to it later. Spots last until you close the game, and only make sense on the mountain they were set on.", DrawSpots);
		Card("Teleport", "Scouts", 1);
		Cu("toScouts", "To another scout", "Takes you next to another scout in the lobby.", DrawToScouts);
		Card("World", "See through the mountain", 0);
		Sw("EspScouts", "Other scouts", "Names and distances of the other scouts, drawn over the game, through walls.");
		Sw("EspLuggage", "Unopened luggage", "Luggage nobody has opened yet, within 250 m.");
		Sw("EspItems", "Items on the ground", "Loose items lying around, within 80 m.");
		Sw("EspCampfires", "Campfires", "Every campfire on the mountain.");
		Sw("EspStatues", "Scout statues", "The statues that bring fallen scouts back, within 600 m.");
		Sw("EspHazards", "Hazards", "Bees, scorpions, spiders, zombies, tornados and the other things that hurt, within 150 m.");
		Card("World", "The mountain", 1);
		Sw("StopFog", "The fog never comes", "The host's fog is the lobby's fog: this holds it for everyone when you host, and does nothing lasting when you do not.");
		Sw("StopLava", "The lava does not rise", "Holds the rising lava where it is. The host's lava is the lobby's lava.");
		Sw("NoHazards", "No hazards", "Switches off every hazard the game lets a run switch off: bees, urchins, scorpions, tornados, thorns and the rest. Switch it on before the run starts: what is already on the mountain may stay. The host decides for the lobby.");
		Sw("NoWeather", "No rain, wind, storm, heat or cold nights", "The weather stays out of your way. Switch it on before the run starts. The host decides for the lobby.");
		Put(new Control
		{
			Id = "ascent",
			Label = "Ascent level",
			Hint = "The difficulty level of the run: hunger, fall damage, weight and the rest follow it from the moment you move it. 0 is the standard climb.",
			Min = -1f,
			Max = 8f,
			Step = 1f,
			Shown = (float v) => v.ToString("0"),
			Value = () => Ascents.currentAscent,
			Move = delegate(float v)
			{
				Ascents.currentAscent = Mathf.RoundToInt(v);
			}
		});
		Card("World", "Time and route", 1);
		Bt("time", "Sets the time of day. Away from the host, the host's clock takes over again.", delegate(int i)
		{
			switch (i)
			{
			case 0:
				Features.SetTime(6f, "dawn");
				break;
			case 1:
				Features.SetTime(12f, "noon");
				break;
			case 2:
				Features.SetTime(19f, "dusk");
				break;
			default:
				Features.SetTime(0f, "midnight");
				break;
			}
		}, "Dawn", "Noon", "Dusk", "Midnight");
		Bt("nextSection", "Moves the whole lobby on to the next part of the mountain. Host only.", delegate
		{
			Features.NextSection();
		}, "Skip to the next section");
		Card("Camera", "View", 0);
		Sw("ThirdPerson", "Third person", "See your scout from behind. The camera stops short of walls.");
		Sl("CamDistance", "Distance", "How far behind your scout the third-person camera sits.", 2f, 10f, 0.5f, (float v) => v.ToString("0.0") + " m");
		Sl("Fov", "Field of view", "How wide the view is. All the way left leaves it to the game's own setting.", 50f, 130f, 5f, (float v) => (!(v <= 50f)) ? v.ToString("0") : Lang.T("the game's"));
		Sw("HideHud", "Hide the game's HUD", "Takes the stamina bar, the item slots and the prompts off the screen, for screenshots.");
		Card("Camera", "Free camera", 1);
		Sw("freeCam", "Free camera", "Your scout stays where it is and the view flies: move keys, jump for up, crouch for down, sprint for faster. Close the menu to steer. Good for screenshots and for looking ahead.", () => Live.FreeCam, Features.SetFreeCam);
		Card("Help", "Who", 2);
		Cu("friend", "The scout to help", "Pick the scout the buttons below are for.", delegate
		{
			DrawWho(ref friend, everyone: false);
		});
		Card("Help", "Back on their feet", 0);
		Bt("reviveOther", "Revives a scout who is down: where they fell, or right next to you.", delegate(int i)
		{
			Helping(delegate(Character c)
			{
				if (!Features.Down(c))
				{
					Say(Lang.T("That scout is not down"));
				}
				else
				{
					Features.ReviveOther(c, i == 1);
					Say(Lang.F("{0} is back up", NameOf(c)));
				}
			});
		}, "Revive where they fell", "Revive next to me");
		Bt("healOther", "Clears a scout's statuses. The game only takes this from the host.", delegate
		{
			Helping(delegate(Character c)
			{
				Say(Features.HealOther(c) ? Lang.F("{0}: statuses cleared", NameOf(c)) : Lang.T("Only the host can clear another scout's statuses"));
			});
		}, "Clear their statuses");
		Card("Help", "Bring and give", 1);
		Bt("bringOther", "Brings the scout to you, or takes you to them.", delegate(int i)
		{
			Helping(delegate(Character c)
			{
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				if (i == 0)
				{
					Features.Bring(c);
					Say(Lang.F("{0} brought over", NameOf(c)));
				}
				else
				{
					Features.Warp(c.Center + Vector3.up * 1.5f, NameOf(c));
				}
			});
		}, "Bring to me", "Go to them");
		Cu("handItem", "Hand them an item", "Puts the item picked on the Items page right in front of the scout.", DrawHand);
		Card("Help", "Scout Inventories & Gear", 1);
		Cu("scoutInventories", "Lobby Scout Inventories", "Live view of all scouts' held items, pockets, and backpacks with direct injection.", DrawScoutInventories);
		Card("Troll", "Who", 2);
		Cu("target", "Who to prank", "Pick one scout, or everyone.", delegate
		{
			DrawWho(ref target, everyone: true);
		});
		Card("Troll", "Shove", 0);
		Put(new Control
		{
			Id = "launchSpeed",
			Label = "How hard",
			Hint = "How fast a shoved scout leaves, in metres per second.",
			Min = 5f,
			Max = 60f,
			Step = 5f,
			Shown = (float v) => v.ToString("0"),
			Value = () => launchSpeed,
			Move = delegate(float v)
			{
				launchSpeed = v;
			}
		});
		Bt("shove", "Launches them up, throws them the way you look, or pulls them to you.", delegate(int i)
		{
			switch (i)
			{
			case 0:
				Each("Up they go", delegate(Character c)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					Features.Launch(c, Vector3.up, launchSpeed);
				});
				break;
			case 1:
				Each("Thrown", delegate(Character c)
				{
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					//IL_001a: Unknown result type (might be due to invalid IL or missing references)
					//IL_001f: Unknown result type (might be due to invalid IL or missing references)
					Features.Launch(c, Features.Me.data.lookDirection + Vector3.up * 0.4f, launchSpeed);
				});
				break;
			default:
				Each("Pulled", delegate(Character c)
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					//IL_0016: Unknown result type (might be due to invalid IL or missing references)
					//IL_0020: Unknown result type (might be due to invalid IL or missing references)
					//IL_0025: Unknown result type (might be due to invalid IL or missing references)
					Features.Launch(c, Features.Me.Center - c.Center + Vector3.up * 2f, launchSpeed);
				});
				break;
			}
		}, "Launch upward", "Throw where I look", "Pull toward me");
		Card("Troll", "Mess with", 1);
		Bt("trip", "Knocks them over for three seconds, or makes them hop.", delegate(int i)
		{
			if (i == 0)
			{
				Each("Tripped", delegate(Character c)
				{
					Features.Trip(c, 3f);
				});
			}
			else
			{
				Each("Hop", delegate(Character c)
				{
					Features.MakeJump(c);
				});
			}
		}, "Trip (3 seconds)", "Make them jump");
		Bt("swap", "Brings them to you, or swaps places with the one scout picked.", delegate(int i)
		{
			if (i == 0)
			{
				Each("Brought over", delegate(Character c)
				{
					Features.Bring(c);
				});
			}
			else if ((Object)(object)target == (Object)null)
			{
				Say(Lang.T("Swapping needs one scout picked"));
			}
			else
			{
				Features.Swap(target);
				Say(Lang.T("Swapped"));
			}
		}, "Bring to me", "Swap places");
		Cu("delivery", "Delivery", "Drops the item picked on the Items page on their head.", DrawDelivery);
		Card("Troll", "Fair warning", 2);
		Cu("trollNote", "About these", "These are pranks a scout walks away from: nothing here kills, kicks or takes anything. A hard launch near a ledge can still hurt, so mind where they are standing.", delegate
		{
			GUILayout.Label(Lang.T("These are pranks a scout walks away from: nothing here kills, kicks or takes anything. A hard launch near a ledge can still hurt, so mind where they are standing. Use them with friends who are in on the joke."), Theme.Small, Array.Empty<GUILayoutOption>());
		});
	}

	private void Tick()
	{
		Features.Say = Say;
		if (Features.InGame)
		{
			try
			{
				Features.Upkeep();
			}
			catch (Exception e)
			{
				Fail(e);
			}
		}
		else if (Live.FreeCam)
		{
			Live.FreeCam = false;
		}
		SelfTest.Tick(this);
	}

	private void Overlay()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type != 7 || !Features.InGame || (!State.EspScouts && !State.EspLuggage && !State.EspItems && !State.EspCampfires && !State.EspStatues && !State.EspHazards))
		{
			return;
		}
		Camera main = Camera.main;
		if ((Object)(object)main == (Object)null)
		{
			return;
		}
		Theme.Build();
		if (espStyle == null)
		{
			espStyle = new GUIStyle
			{
				fontSize = 12,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4
			};
			espShadow = new GUIStyle(espStyle);
			espShadow.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
		}
		GUIStyle obj = espStyle;
		Font font2 = (espShadow.font = Theme.Small.font);
		obj.font = font2;
		Vector3 position = ((Component)main).transform.position;
		int num = 0;
		if (State.EspScouts)
		{
			foreach (Character item in Features.Others())
			{
				num += Tag(main, position, item.Head + Vector3.up * 0.5f, NameOf(item), Theme.AccentHi, 100000f);
			}
		}
		if (State.EspLuggage)
		{
			string text = Lang.T("Luggage");
			foreach (Luggage item2 in Luggage.ALL_LUGGAGE)
			{
				if ((Object)(object)item2 != (Object)null && !item2.IsOpen && !(item2 is RespawnChest))
				{
					num += Tag(main, position, item2.Center(), text, new Color(1f, 0.82f, 0.3f), 250f);
				}
			}
		}
		if (State.EspItems)
		{
			foreach (Item aLL_ACTIVE_ITEM in Item.ALL_ACTIVE_ITEMS)
			{
				if ((Object)(object)aLL_ACTIVE_ITEM != (Object)null && (int)aLL_ACTIVE_ITEM.itemState == 0)
				{
					num += Tag(main, position, aLL_ACTIVE_ITEM.transform.position, aLL_ACTIVE_ITEM.GetName(), Theme.Text, 80f);
				}
			}
		}
		if (State.EspCampfires)
		{
			string text2 = Lang.T("Campfire");
			foreach (Campfire fire in Features.Fires)
			{
				if ((Object)(object)fire != (Object)null)
				{
					num += Tag(main, position, ((Component)fire).transform.position + Vector3.up, text2, new Color(1f, 0.6f, 0.25f), 100000f);
				}
			}
		}
		if (State.EspStatues)
		{
			string text3 = Lang.T("Scout statue");
			foreach (RespawnChest statue in Features.Statues)
			{
				if ((Object)(object)statue != (Object)null)
				{
					num += Tag(main, position, ((Component)statue).transform.position + Vector3.up * 1.5f, text3, new Color(0.7f, 0.85f, 1f), 600f);
				}
			}
		}
		if (State.EspHazards)
		{
			foreach (KeyValuePair<Transform, string> hazard in Features.Hazards)
			{
				if ((Object)(object)hazard.Key != (Object)null)
				{
					num += Tag(main, position, hazard.Key.position + Vector3.up * 0.5f, hazard.Value, new Color(1f, 0.4f, 0.4f), 150f);
				}
			}
		}
		espDrawn = num;
	}

	private void Normal()
	{
		if (Live.FreeCam)
		{
			Live.FreeCam = false;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		DebugBridge.Update();
		Settings.Tick();
		ModNet.Tick(Time.unscaledDeltaTime);
		if (!languageRead && Ready())
		{
			languageRead = true;
			Lang.Start();
		}
		try
		{
			Build();
			Tick();
		}
		catch (Exception e)
		{
			Fail(e);
		}
	}

	internal void Say(string msg)
	{
		status = msg;
		statusUntil = Time.unscaledTime + 4f;
	}

	internal void Fail(Exception e)
	{
		lastError = e.ToString();
		Say(Lang.F("error: {0}", e.Message));
		Kit.Log.LogError((object)e);
	}

	internal void Card(string page, string title, int column, Func<string> summary = null)
	{
		building = new Group
		{
			Page = page,
			Title = title,
			Column = column,
			Summary = summary
		};
		groups.Add(building);
	}

	internal Control Put(Control c)
	{
		c.In = building;
		string text = ((c.Buttons != null && !c.Raw) ? string.Join(" ", c.Buttons.Select(Lang.Every).ToArray()) : Lang.Every(c.Label));
		c.Match = (text + " " + Lang.Every(c.Hint) + " " + Lang.Every(building.Title) + " " + Lang.Every(building.Page)).ToLowerInvariant();
		building.Items.Add(c);
		controls[c.Id] = c;
		if (c.On != null || c.Value != null || c.Chosen != null)
		{
			Names[c.Id] = c.Label;
		}
		return c;
	}

	internal Control Sw(string field, string label, string hint)
	{
		FieldInfo f = typeof(State).GetField(field);
		if (f == null)
		{
			throw new MissingFieldException("State", field);
		}
		return Put(new Control
		{
			Id = field,
			Label = label,
			Hint = hint,
			On = () => (bool)f.GetValue(null),
			Turn = delegate(bool v)
			{
				f.SetValue(null, v);
			}
		});
	}

	internal Control Sw(string id, string label, string hint, Func<bool> on, Action<bool> turn)
	{
		return Put(new Control
		{
			Id = id,
			Label = label,
			Hint = hint,
			On = on,
			Turn = turn
		});
	}

	internal Control Sl(string field, string label, string hint, float min, float max, float step, Func<float, string> shown)
	{
		FieldInfo f = typeof(State).GetField(field);
		if (f == null)
		{
			throw new MissingFieldException("State", field);
		}
		bool whole = f.FieldType == typeof(int);
		return Put(new Control
		{
			Id = field,
			Label = label,
			Hint = hint,
			Min = min,
			Max = max,
			Step = step,
			Shown = shown,
			Value = () => Convert.ToSingle(f.GetValue(null)),
			Move = delegate(float v)
			{
				f.SetValue(null, whole ? ((object)Mathf.RoundToInt(v)) : ((object)v));
			}
		});
	}

	internal Control Bt(string id, string hint, params string[] labels)
	{
		return Put(new Control
		{
			Id = id,
			Label = string.Join(" / ", labels),
			Hint = hint,
			Buttons = labels
		});
	}

	internal Control Bt(string id, string hint, Action<int> press, params string[] labels)
	{
		Control control = Bt(id, hint, labels);
		control.Press = press;
		return control;
	}

	internal Control Pk(string field, string hint, int first, params string[] labels)
	{
		FieldInfo f = typeof(State).GetField(field);
		if (f == null)
		{
			throw new MissingFieldException("State", field);
		}
		return Put(new Control
		{
			Id = field,
			Label = string.Join(" / ", labels),
			Hint = hint,
			Buttons = labels,
			Chosen = () => (int)f.GetValue(null) - first,
			Choose = delegate(int i)
			{
				f.SetValue(null, i + first);
			}
		});
	}

	internal Control Cu(string id, string label, string hint, Action<float> draw)
	{
		return Put(new Control
		{
			Id = id,
			Label = label,
			Hint = hint,
			Custom = draw
		});
	}

	internal void Whole(string page, Action<float> draw, string searchHint = null)
	{
		whole[page] = draw;
		if (searchHint != null)
		{
			wholeSearch[page] = searchHint;
		}
	}

	internal static string Times(float v)
	{
		return "x" + v.ToString((v % 1f == 0f) ? "0" : "0.0");
	}

	internal void Build()
	{
		if (built)
		{
			return;
		}
		built = true;
		Lang.Start();
		Pages();
		AboutPage();
		string[] array = (Kit.Pins.Value ?? "").Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			if (controls.ContainsKey(text) && controls[text].Pinnable && !pinned.Contains(text))
			{
				pinned.Add(text);
			}
		}
		RefreshTabs();
	}

	public void RefreshTabs()
	{
		if (!built) return;

		string currentTab = (tabs != null && tab >= 0 && tab < tabs.Length) ? tabs[tab] : "Home";
		List<string> visible = new List<string>();
		foreach (string s in AllSections)
		{
			if (ModNet.IsSectionAllowed(s))
			{
				visible.Add(s);
			}
		}
		tabs = visible.ToArray();
		int newIndex = Array.IndexOf(tabs, currentTab);
		if (newIndex >= 0)
		{
			tab = newIndex;
		}
		else
		{
			tab = 0;
		}
		EnforceSectionPermissions();
	}

	private void EnforceSectionPermissions()
	{
		if (ModNet.Role == ModNet.ModRole.Master) return;

		if (!ModNet.IsSectionAllowed("Camera") && Live.FreeCam)
		{
			Features.SetFreeCam(false);
		}

		foreach (var kvp in controls)
		{
			Control c = kvp.Value;
			if (c.In != null && !ModNet.IsSectionAllowed(c.In.Page))
			{
				if (c.On != null && c.Turn != null)
				{
					try
					{
						if (c.On()) c.Turn(false);
					}
					catch { }
				}
			}
		}
	}

	private void AboutPage()
	{
		string page = "About";
		Lang.Know(Kit.Title);
		Card(page, "This menu", 2);
		Cu("credits", Kit.Title, Kit.Blurb, DrawCredits);
		Card(page, "Settings", 0);
		Put(new Control
		{
			Id = "language",
			Label = "Language",
			Hint = "The language of this menu.",
			Buttons = Lang.Names,
			Raw = true,
			Chosen = () => Lang.Now,
			Choose = Lang.Choose
		});
		Sw("remember", "Remember my switches and sliders", "On: every switch and slider is saved as it changes and comes back the next time you start the game. Off: they all start from normal each time.", () => Settings.Remember.Value, delegate(bool v)
		{
			Settings.Remember.Value = v;
			Settings.Save();
		});
		if (typeof(State).GetField("ShowActive") != null)
		{
			Sw("ShowActive", "Show what is switched on", "While the menu is closed, a line at the top of the screen lists everything that is switched on.");
		}
		if (Kit.BlockAchievements != null)
		{
			Sw("achievements", "Block achievements while this mod is loaded", "On by default, so nothing you do with the menu ends up on your profile. Your choice is saved.", () => Kit.BlockAchievements.Value, delegate(bool v)
			{
				Kit.BlockAchievements.Value = v;
			});
		}
		if (ModNet.Role == ModNet.ModRole.Master)
		{
			Card(page, "Ascend Network", 0);
			Cu("modNet", "Network Role & Lobby Scouts", "Shows network mod role and detected slave mod users in the lobby.", DrawNetStatus);
			Card(page, "Multiplayer Lobby", 0);
			Cu("joinSlave", "Join Slave Lobby", "Directly join a detected Slave scout's active lobby session without needing a room code.", DrawJoinSlave);
		}
		else
		{
			Card(page, "Multiplayer Lobby", 0);
			Cu("joinMaster", "Join Master Lobby", "Directly join the host's active lobby session without needing a Steam invite.", DrawJoinMaster);
		}
		Card(page, "Theme & Appearance", 1);
		Cu("themeSettings", "Menu Colors & Theme", "Customize the primary and secondary colors of the menu using presets or RGB sliders.", DrawThemeSettings);
		Card(page, "Keys and reset", 1);
		Cu("menuKey", "Menu key", "The key that opens and closes this menu. Press Change and then the key you want. It is saved.", DrawMenuKey);
		Bt("normal", "Switches everything off and puts every slider back, as when the game has just started.", delegate
		{
			Settings.Blank();
			Normal();
			Say(Lang.T("Everything is back to normal"));
		}, "Put everything back to normal");
		Card(page, "Shortcut keys", 2);
		Cu("shortcuts", "Shortcut keys", "Give a switch or an action a key and it works without opening the menu. Press its button and then the key. Backspace takes the key away, Escape leaves it as it was.", DrawShortcuts);
	}

	private static string NameOf(Control c, int button)
	{
		if (c.ButtonNow == null)
		{
			if (!c.Raw)
			{
				return Lang.T(c.Buttons[button]);
			}
			return c.Buttons[button];
		}
		return c.ButtonNow(button);
	}

	private string HintOf(Control c)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Keys.Bound bound = Keys.All.Find((Keys.Bound t) => t.Id == c.Id);
		return Lang.T(c.Hint) + ((bound != null && (int)bound.Key.Value != 0) ? ("   " + Lang.F("Its key: {0}.", bound.Key.Value)) : "");
	}

	private bool PinHit(Control c, Rect row)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!c.Pinnable)
		{
			return false;
		}
		Rect val = default(Rect);
		val = new Rect(row.x, row.y, 15f, row.height);
		if (val.Contains(Event.current.mousePosition))
		{
			hoveredNote = Lang.T(pinned.Contains(c.Id) ? "Click to take this off the home page." : "Click to pin this to the home page.");
		}
		if (!GUI.Button(val, GUIContent.none, GUIStyle.none))
		{
			return false;
		}
		TogglePin(c);
		return true;
	}

	private void PinDraw(Control c, Rect row)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (c.Pinnable && (int)Event.current.type == 7)
		{
			bool flag = pinned.Contains(c.Id);
			if (flag || row.Contains(Event.current.mousePosition))
			{
				(flag ? Theme.PinOn : Theme.PinOff).Draw(new Rect(row.x + 2f, row.center.y - 5f, 10f, 10f), false, false, false, false);
			}
		}
	}

	internal void TogglePin(Control c)
	{
		if (!pinned.Remove(c.Id))
		{
			pinned.Add(c.Id);
		}
		Kit.Pins.Value = string.Join(",", pinned.ToArray());
		Say(Lang.T(pinned.Contains(c.Id) ? "Pinned to Home" : "Taken off Home"));
	}

	internal void Tidy(float x, float y)
	{
		win.x = x;
		win.y = y;
		statusUntil = 0f;
	}

	private void DrawControl(Control c, float width)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Invalid comparison between Unknown and I4
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		if (c.Hidden != null && c.Hidden())
		{
			return;
		}
		Rect val;
		if (c.Custom != null)
		{
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			c.Custom(width);
			GUILayout.EndVertical();
			val = GUILayoutUtility.GetLastRect();
		}
		else if (c.On != null)
		{
			val = GUILayoutUtility.GetRect(10f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
			bool flag = PinHit(c, val);
			bool flag2 = c.On();
			bool flag3 = SwitchAt(val, flag2, Lang.T(c.Label));
			if (flag3 != flag2 && !flag)
			{
				c.Turn(flag3);
			}
			PinDraw(c, val);
		}
		else if (c.Value != null)
		{
			val = GUILayoutUtility.GetRect(10f, 46f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
			PinHit(c, val);
			float num = c.Value();
			float num2 = SliderAt(val, Lang.T(c.Label), num, c.Min, c.Max, c.Step, c.Shown(num));
			if (num2 != num)
			{
				c.Move(num2);
			}
			PinDraw(c, val);
		}
		else
		{
			val = GUILayoutUtility.GetRect(10f, 32f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
			bool flag4 = PinHit(c, val);
			int num3 = c.Buttons.Length;
			int num4 = ((c.Chosen != null) ? c.Chosen() : (-1));
			float num5 = val.x + 15f;
			float num6 = (val.xMax - num5 - 4f * (float)(num3 - 1)) / (float)num3;
			for (int i = 0; i < num3; i++)
			{
				Rect val2 = new Rect(num5 + (float)i * (num6 + 4f), val.y + 2f, num6, val.height - 4f);
				GUIStyle val3 = ((c.Chosen == null) ? (c.Strong ? Theme.Primary : Theme.Button) : ((i == num4) ? Theme.ChipOn : Theme.Chip));
				if (!(!GUI.Button(val2, NameOf(c, i), val3) || flag4))
				{
					if (c.Chosen != null)
					{
						c.Choose(i);
					}
					else if (c.Press != null)
					{
						c.Press(i);
					}
				}
			}
			PinDraw(c, val);
		}
		if ((int)Event.current.type == 7)
		{
			c.Where = new Rect(GUIUtility.GUIToScreenPoint(val.position) * Scale, val.size * Scale);
			if (val.Contains(Event.current.mousePosition))
			{
				hovered = c;
			}
		}
	}

	internal static bool SwitchAt(Rect r, bool value, string label)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		if (GUI.Button(r, GUIContent.none, Theme.Row))
		{
			value = !value;
		}
		GUI.Label(new Rect(r.x + 16f, r.y, r.width - 70f, r.height), label, Theme.RowLabel);
		if ((int)Event.current.type == 7)
		{
			Rect val = default(Rect);
			val = new Rect(r.xMax - 46f, r.y + (r.height - 18f) / 2f, 38f, 18f);
			(value ? Theme.PillOn : Theme.PillOff).Draw(val, false, false, false, false);
			Rect val2 = default(Rect);
			val2 = new Rect(value ? (val.xMax - 16f) : (val.x + 2f), val.y + 2f, 14f, 14f);
			Theme.Knob.Draw(val2, false, false, false, false);
		}
		return value;
	}

	internal static float SliderAt(Rect r, string label, float value, float min, float max, float step, string shown)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type == 7)
		{
			Theme.Row.Draw(r, false, false, false, false);
		}
		GUI.Label(new Rect(r.x + 16f, r.y + 3f, r.width - 130f, 20f), label, Theme.RowLabel);
		GUI.Label(new Rect(r.xMax - 118f, r.y + 3f, 110f, 20f), shown, Theme.Hint);
		Rect val = default(Rect);
		val = new Rect(r.x + 16f, r.y + 24f, r.width - 26f, 18f);
		if ((int)Event.current.type == 7)
		{
			Rect val2 = default(Rect);
			val2 = new Rect(val.x, val.y + 6f, val.width, 6f);
			Theme.SliderBar.Draw(val2, false, false, false, false);
			float num = Mathf.InverseLerp(min, max, value);
			if (num > 0.01f)
			{
				Theme.SliderFill.Draw(new Rect(val2.x, val2.y, val2.width * num, val2.height), false, false, false, false);
			}
			Theme.Knob.Draw(new Rect(val.x + (val.width - 14f) * num, val.y + 2f, 14f, 14f), false, false, false, false);
		}
		value = GUI.HorizontalSlider(val, value, min, max, Theme.SliderHit, GUIStyle.none);
		return Mathf.Clamp(Mathf.Round(value / step) * step, min, max);
	}

	internal static float SliderRow(string label, float value, float min, float max, float step, string shown)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return SliderAt(GUILayoutUtility.GetRect(10f, 46f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) }), label, value, min, max, step, shown);
	}

	internal void PanelBegin(string title, string summary, float width)
	{
		GUILayout.BeginVertical(Theme.Panel, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(width) });
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(Lang.T(title).ToUpperInvariant(), Theme.CardTitle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(18f) });
		GUILayout.FlexibleSpace();
		if (summary != null)
		{
			GUILayout.Label(summary, Theme.Hint, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(18f) });
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(2f);
	}

	internal void PanelEnd()
	{
		GUILayout.EndVertical();
	}

	private void DrawGroup(Group g, float width)
	{
		PanelBegin(g.Title, (g.Summary != null) ? g.Summary() : null, width);
		foreach (Control item in g.Items)
		{
			DrawControl(item, width - 20f);
		}
		GUILayout.EndVertical();
	}

	private void DrawPage(string page, float width)
	{
		float num = (width - 10f) / 2f;
		List<Group> list = groups.FindAll((Group g) =>
		{
			if (g.Page != page) return false;
			if ((g.Title == "Ascend Network" || g.Title == "Larp Network") && !IsLarpNetworkVisible) return false;
			return true;
		});
		int num2 = 0;
		while (num2 < list.Count)
		{
			if (list[num2].Column == 2)
			{
				DrawGroup(list[num2], width);
				num2++;
				continue;
			}
			int i;
			for (i = num2; i < list.Count && list[i].Column != 2; i++)
			{
			}
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			for (int j = 0; j < 2; j++)
			{
				if (j == 1)
				{
					GUILayout.Space(10f);
				}
				GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num) });
				for (int k = num2; k < i; k++)
				{
					if (list[k].Column == j)
					{
						DrawGroup(list[k], num);
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			num2 = i;
		}
	}

	private void DrawList(List<Control> list, float width, string title, string summary)
	{
		float num = (width - 10f) / 2f;
		int num2 = (list.Count + 1) / 2;
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		for (int i = 0; i < 2; i++)
		{
			if (i == 1)
			{
				GUILayout.Space(10f);
			}
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num) });
			int num3 = ((i != 0) ? num2 : 0);
			int num4 = ((i == 0) ? num2 : list.Count);
			if (num4 > num3)
			{
				PanelBegin((i == 0) ? title : " ", (i == 0) ? null : summary, num);
				Group group = null;
				for (int j = num3; j < num4; j++)
				{
					Control control = list[j];
					if (control.In != group)
					{
						group = control.In;
						GUILayout.Label(Lang.T(control.In.Page) + "  -  " + Lang.T(control.In.Title), Theme.Small, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(16f) });
					}
					if (control.Custom == null)
					{
						DrawControl(control, num - 20f);
					}
					else if (GUILayout.Button(Lang.F("Go to {0}: {1}", Lang.T(control.In.Page), Lang.T(control.Label)), Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(28f) }))
					{
						GoTo(Array.IndexOf(tabs, control.In.Page));
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
	}

	private void DrawHome(float width)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Invalid comparison between Unknown and I4
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		List<Control> list = new List<Control>();
		foreach (string item in pinned)
		{
			if (controls.TryGetValue(item, out var value))
			{
				if (value.In != null)
				{
					if ((value.In.Title == "Ascend Network" || value.In.Title == "Larp Network") && !IsLarpNetworkVisible) continue;
					if (!ModNet.IsSectionAllowed(value.In.Page)) continue;
				}
				list.Add(value);
			}
		}
		if (list.Count == 0)
		{
			PanelBegin("Pinned", null, width);
			GUILayout.Label(Lang.T("Nothing is pinned yet. The few things you use most can live here, one click away."), Theme.Label, Array.Empty<GUILayoutOption>());
			GUILayout.Space(6f);
			Rect rect = GUILayoutUtility.GetRect(10f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(Mathf.Min(420f, width - 20f)) });
			if ((int)Event.current.type == 7)
			{
				Theme.Foot.Draw(rect, false, false, false, false);
				Theme.Button.Draw(rect, false, false, false, false);
				Theme.PinOn.Draw(new Rect(rect.x + 6f, rect.center.y - 5f, 10f, 10f), false, false, false, false);
				GUI.Label(new Rect(rect.x + 24f, rect.y, rect.width - 80f, rect.height), Lang.T("Any switch, slider or row of buttons"), Theme.RowLabel);
				Rect val = default(Rect);
				val = new Rect(rect.xMax - 46f, rect.y + 6f, 38f, 18f);
				Theme.PillOff.Draw(val, false, false, false, false);
				Theme.Knob.Draw(new Rect(val.x + 2f, val.y + 2f, 14f, 14f), false, false, false, false);
			}
			GUILayout.Space(6f);
			GUILayout.Label(Lang.T("On any page, move the mouse over a row. A small square shows at its left edge, like the coloured one above. Click it and the row is pinned here. Click it again to take it off."), Theme.Small, Array.Empty<GUILayoutOption>());
			GUILayout.EndVertical();
		}
		else
		{
			DrawList(list, width, "Pinned", Lang.F("{0} pinned", list.Count));
		}
		PanelBegin("Switched on now", null, width);
		string text = Keys.Active();
		GUILayout.Label((text.Length > 0) ? text : Lang.T("Nothing is switched on."), Theme.Label, Array.Empty<GUILayoutOption>());
		GUILayout.EndVertical();
	}

	internal void Find(string text)
	{
		find = text ?? "";
		Refind();
	}

	private void Refind()
	{
		if (find == lastFind)
		{
			return;
		}
		lastFind = find;
		matches.Clear();
		string[] array = find.ToLowerInvariant().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return;
		}
		foreach (Group group in groups)
		{
			if ((group.Title == "Ascend Network" || group.Title == "Larp Network") && !IsLarpNetworkVisible) continue;
			if (!ModNet.IsSectionAllowed(group.Page)) continue;
			foreach (Control c in group.Items)
			{
				if (array.All((string w) => c.Match.Contains(w)))
				{
					matches.Add(c);
				}
			}
		}
	}

	private void DrawFound(float width)
	{
		Refind();
		if (matches.Count == 0)
		{
			PanelBegin("Search", null, width);
			GUILayout.Label(Lang.F("Nothing in the menu matches \"{0}\".", find.Trim()), Theme.Label, Array.Empty<GUILayoutOption>());
			GUILayout.EndVertical();
		}
		else
		{
			DrawList(matches, width, "Search", Lang.F("{0} found", matches.Count));
		}
	}

	internal void GoTo(int page)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (page >= 0 && page < tabs.Length)
		{
			tab = page;
			find = "";
			GUIUtility.keyboardControl = 0;
			listening = false;
			keyFor = null;
			pageScroll = Vector2.zero;
		}
	}

	internal void GoTo(string page)
	{
		GoTo(Array.IndexOf(tabs, page));
	}

	private void SetOpen(bool value)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (value == IsOpen)
		{
			return;
		}
		IsOpen = value;
		if (OwnCursor)
		{
			if (value)
			{
				savedLock = Cursor.lockState;
				savedVisible = Cursor.visible;
				cursorTaken = true;
			}
			else if (cursorTaken)
			{
				Cursor.lockState = savedLock;
				Cursor.visible = savedVisible;
				cursorTaken = false;
			}
		}
		if (!value)
		{
			listening = false;
			keyFor = null;
		}
		try
		{
		}
		catch (Exception e)
		{
			Fail(e);
		}
	}

	private void OnGUI()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Invalid comparison between Unknown and I4
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Invalid comparison between Unknown and I4
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Invalid comparison between Unknown and I4
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Expected O, but got Unknown
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		Event current = Event.current;
		if (keyFor != null && (int)current.type == 4 && (int)current.keyCode != 0)
		{
			SetShortcut(keyFor, current.keyCode);
			keyFor = null;
			current.Use();
		}
		else if (listening && (int)current.type == 4 && (int)current.keyCode != 0)
		{
			if ((int)current.keyCode == 27)
			{
				Say(Lang.F("Menu key is still {0}", Kit.MenuKey.Value));
			}
			else
			{
				SetKey(current.keyCode);
			}
			listening = false;
			current.Use();
		}
		else if ((int)current.type == 4 && current.keyCode == Kit.MenuKey.Value && GUIUtility.keyboardControl == 0)
		{
			SetOpen(!IsOpen);
			current.Use();
		}
		else if ((int)current.type == 5)
		{
			Keys.Event(current, null);
		}
		else if ((int)current.type == 4 && GUIUtility.keyboardControl == 0 && Ready())
		{
			Keys.Event(current, Say);
		}
		try
		{
			Overlay();
		}
		catch (Exception ex)
		{
			if (ex is ExitGUIException)
			{
				throw;
			}
			Fail(ex);
		}
		if (!IsOpen)
		{
			DrawActive();
			return;
		}
		Theme.Build();
		if (OwnCursor)
		{
			Cursor.visible = true;
			Cursor.lockState = (CursorLockMode)0;
		}
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.Scale(new Vector3(Scale, Scale, 1f));
		if ((int)current.type == 8)
		{
			win.height = Mathf.MoveTowards(win.height, wantHeight, 4000f * Time.unscaledDeltaTime);
			win.x = Mathf.Clamp(win.x, 40f - win.width, (float)Screen.width / Scale - 40f);
			win.y = Mathf.Clamp(win.y, 0f, (float)Screen.height / Scale - 40f);
		}
		win = GUI.Window(Kit.WindowId, win, new GUI.WindowFunction(Draw), GUIContent.none, Theme.Window);
		if (Kit.ItemsPoppedOut != null && Kit.ItemsPoppedOut.Value && ModNet.IsSectionAllowed("Items"))
		{
			if (!itemWinInit)
			{
				itemWin = new Rect(win.x + win.width + 12f, win.y, 760f, 540f);
				if (itemWin.x + itemWin.width > (float)Screen.width / Scale)
				{
					itemWin.x = Mathf.Max(10f, (float)Screen.width / Scale - itemWin.width - 20f);
				}
				itemWinInit = true;
			}
			itemWin = GUI.Window(Kit.WindowId + 1, itemWin, new GUI.WindowFunction(DrawPopoutItemWindow), GUIContent.none, Theme.Window);
		}
		Egg.OnScreen();
		GUI.matrix = matrix;
	}

	private void DrawActive()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type != 7 || !Keys.ShowActive || !Ready())
		{
			return;
		}
		string text = Keys.Active();
		if (text.Length != 0)
		{
			Theme.Build();
			if (activeStyle == null)
			{
				activeStyle = new GUIStyle(Theme.Small)
				{
					alignment = (TextAnchor)1,
					wordWrap = true,
					fontStyle = (FontStyle)1
				};
				activeStyle.normal.textColor = Color.white;
			}
			activeStyle.font = Theme.Small.font;
			Matrix4x4 matrix = GUI.matrix;
			Color color = GUI.color;
			GUI.matrix = Matrix4x4.Scale(new Vector3(Scale, Scale, 1f));
			float num = (float)Screen.width / Scale;
			Rect val = default(Rect);
			val = new Rect(num * 0.25f, 6f, num * 0.5f, 64f);
			GUI.color = new Color(0f, 0f, 0f, 0.85f);
			GUI.Label(new Rect(val.x + 1f, val.y + 1f, val.width, val.height), text, activeStyle);
			GUI.color = color;
			GUI.Label(val, text, activeStyle);
			GUI.matrix = matrix;
		}
	}

	internal void SetShortcut(string id, KeyCode key)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between Unknown and I4
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		Keys.Bound bound = Keys.All.Find((Keys.Bound x) => x.Id == id);
		if (bound == null)
		{
			return;
		}
		string text = Lang.T(Keys.Label(bound));
		if ((int)key == 27)
		{
			Say(Lang.T("No change"));
			return;
		}
		if ((int)key == 8 || (int)key == 127)
		{
			bound.Key.Value = (KeyCode)0;
			Say(Lang.F("{0}: no key", text));
			return;
		}
		if (key == Kit.MenuKey.Value)
		{
			Say(Lang.F("{0} already opens the menu", key));
			return;
		}
		foreach (Keys.Bound item in Keys.All)
		{
			if (item != bound && item.Key.Value == key)
			{
				item.Key.Value = (KeyCode)0;
			}
		}
		bound.Key.Value = key;
		Say(text + ": " + ((KeyCode)key).ToString());
	}

	internal void SetKey(KeyCode key)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		foreach (Keys.Bound item in Keys.All)
		{
			if (item.Key.Value == key)
			{
				item.Key.Value = (KeyCode)0;
			}
		}
		Kit.MenuKey.Value = key;
		Say(Lang.F("Menu key is now {0}", key));
	}

	private void Draw(int id)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Invalid comparison between Unknown and I4
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Invalid comparison between Unknown and I4
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Invalid comparison between Unknown and I4
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		Build();
		if ((int)Event.current.type == 7)
		{
			hovered = null;
			hoveredNote = null;
		}
		string text = (tabs != null && tabs.Length > 0) ? tabs[Mathf.Clamp(tab, 0, tabs.Length - 1)] : "";
		whole.TryGetValue(text, out var value);
		wholeSearch.TryGetValue(text, out var value2);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(Kit.Title.ToUpperInvariant(), Theme.Title, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Width(142f),
			GUILayout.Height(28f)
		});
		if (value2 != null)
		{
			pageSearch = SearchBox(pageSearch, Lang.T(value2));
		}
		else
		{
			find = SearchBox(find, Lang.T("Search every switch, slider and button"));
		}
		GUILayout.FlexibleSpace();
		GUILayout.Label((Time.unscaledTime < statusUntil) ? status : Lang.F("{0} to close", Kit.MenuKey.Value), Theme.Hint, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(28f) });
		GUILayout.EndHorizontal();
		GUILayout.Space(8f);
		bool flag = value2 == null && find.Trim().Length > 0;
		float num = win.width - 32f - 136f - 12f;
		float num2 = (((Object)(object)Theme.GameLogo != (Object)null) ? 54f : 0f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(136f) });
		for (int i = 0; i < tabs.Length; i++)
		{
			if (GUILayout.Button(Lang.T(tabs[i]), (i == tab && !flag) ? Theme.SideOn : Theme.Side, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(32f) }))
			{
				GoTo(i);
			}
			if ((int)Event.current.type == 7)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				tabWhere[i] = new Rect(GUIUtility.GUIToScreenPoint(lastRect.position) * Scale, lastRect.size * Scale);
			}
		}
		GUILayout.FlexibleSpace();
		if ((Object)(object)Theme.GameLogo != (Object)null)
		{
			Rect rect = GUILayoutUtility.GetRect(136f, num2 - 8f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(136f) });
			Color color = GUI.color;
			GUI.color = new Color(1f, 1f, 1f, 0.9f);
			Rect logoRect = new Rect(rect.x + 8f, rect.y, rect.width - 16f, rect.height);
			GUI.DrawTexture(logoRect, (Texture)(object)Theme.GameLogo, (ScaleMode)2);
			GUI.color = color;
			Event current = Event.current;
			if ((int)current.type == 0 && current.button == 0 && logoRect.Contains(current.mousePosition))
			{
				current.Use();
				if (ModNet.Role == ModNet.ModRole.Slave)
				{
					LogoClicks++;
					if (LogoClicks == 5)
					{
						LarpRevealed = true;
						Say(Lang.T("Ascend Network revealed"));
					}
				}
			}
			GUILayout.Space(8f);
		}
		GUILayout.EndVertical();
		GUILayout.Space(12f);
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num) });
		try
		{
			if (tabs == null || tabs.Length == 0)
			{
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.FlexibleSpace();
				GUILayout.Label(Lang.T("All menu sections have been disabled by Master."), Theme.Label, Array.Empty<GUILayoutOption>());
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				pageHeight = 0f;
			}
			else if (!Ready() && (flag || tab != AboutTab))
			{
				GUILayout.Label(Lang.T(NotReady), Theme.Label, Array.Empty<GUILayoutOption>());
				GUILayout.FlexibleSpace();
				pageHeight = 0f;
			}
			else if (!flag && value != null)
			{
				value(num);
				pageHeight = 660f;
			}
			else
			{
				pageScroll = GUILayout.BeginScrollView(pageScroll, GUIStyle.none, GUIStyle.none, Array.Empty<GUILayoutOption>());
				GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
				if (flag)
				{
					DrawFound(num);
				}
				else if (text == "Home")
				{
					DrawHome(num);
				}
				else
				{
					DrawPage(text, num);
				}
				GUILayout.EndVertical();
				if ((int)Event.current.type == 7)
				{
					Rect lastRect2 = GUILayoutUtility.GetLastRect();
					pageHeight = lastRect2.height;
				}
				GUILayout.EndScrollView();
			}
		}
		catch (Exception ex)
		{
			if (ex is ExitGUIException)
			{
				throw;
			}
			Fail(ex);
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		Rect rect2 = GUILayoutUtility.GetRect(10f, 48f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		if ((int)Event.current.type == 7)
		{
			Rect val = default(Rect);
			val = new Rect(rect2.x, rect2.y + 6f, rect2.width, 42f);
			Theme.Foot.Draw(val, false, false, false, false);
			bool flag2 = hovered != null && hovered.Pinnable && hoveredNote == null;
			string text2 = hoveredNote ?? ((hovered != null) ? HintOf(hovered) : Lang.T("Move the mouse over anything to read what it does."));
			GUI.Label(new Rect(val.x + 12f, val.y + 5f, val.width - 24f - (flag2 ? 190f : 0f), 34f), text2, Theme.HintBar);
			if (flag2)
			{
				GUI.Label(new Rect(val.xMax - 200f, val.y + 6f, 188f, 30f), Lang.T(pinned.Contains(hovered.Id) ? "On Home. The square at its\nleft edge takes it off." : "The square at its left edge\npins it to Home."), Theme.Tip);
			}
			HoveredId = ((hovered != null) ? hovered.Id : null);
			float num3 = (float)tabs.Length * 36f + num2;
			wantHeight = Mathf.Clamp(116f + Mathf.Max(num3, pageHeight), 116f + num3, 660f);
		}
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 40f));
	}

	internal void Explain(Rect r, string hint)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type == 7 && r.Contains(Event.current.mousePosition))
		{
			hoveredNote = Lang.T(hint);
		}
	}

	internal static string SearchBox(string text, string placeholder, float w = 320f)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Invalid comparison between Unknown and I4
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = GUILayoutUtility.GetRect(w, 28f, Theme.Field, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(w) });
		if ((int)Event.current.type == 7)
		{
			Theme.Field.Draw(rect, false, false, false, false);
		}
		bool flag = text.Length > 0;
		if (flag && GUI.Button(new Rect(rect.xMax - 26f, rect.y + 3f, 22f, 22f), "x", Theme.ClearX))
		{
			text = "";
			GUIUtility.keyboardControl = 0;
		}
		text = GUI.TextField(new Rect(rect.x, rect.y, rect.width - (flag ? 28f : 0f), rect.height), text, 40, Theme.FieldBare);
		if (text.Length == 0 && (int)Event.current.type == 7)
		{
			GUI.Label(new Rect(rect.x + 10f, rect.y, rect.width - 12f, rect.height), placeholder, Theme.Placeholder);
		}
		return text;
	}

	private void DrawCredits(float width)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Expected O, but got Unknown
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if ((Object)(object)Theme.GameLogo != (Object)null)
		{
			Rect rect = GUILayoutUtility.GetRect(150f, 64f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(150f) });
			GUI.DrawTexture(rect, (Texture)(object)Theme.GameLogo, (ScaleMode)2);
			Egg.Logo(rect);
			GUILayout.Space(14f);
		}
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Label(Kit.Title + " " + Kit.Version, Theme.Title, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(24f) });
		if ((Object)(object)Theme.GameLogo == (Object)null)
		{
			Egg.Logo(GUILayoutUtility.GetLastRect());
		}
		GUILayout.Label(Lang.T(Kit.Blurb) + "  " + Lang.F("Made by {0}.", Kit.Author), Theme.Label, Array.Empty<GUILayoutOption>());
		if (Kit.FailedPatches.Count > 0)
		{
			GUILayout.Label(Lang.F("Not working in this game version: {0}", string.Join(", ", Kit.FailedPatches.ToArray())), Theme.Small, Array.Empty<GUILayoutOption>());
		}
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Space(8f);
	}

	private int selectedSlaveActor = 0;

	private void DrawNetStatus(float width)
	{
		if (ModNet.Role != ModNet.ModRole.Master) return;

		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		string roleStr = (ModNet.Role == ModNet.ModRole.Master) ? "Master" : "Slave";
		GUILayout.Label("Mod Network Role: " + roleStr, Theme.RowLabel, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(24f) });

		if (ModNet.Role == ModNet.ModRole.Master)
		{
			if (Photon.Pun.PhotonNetwork.InRoom && Photon.Pun.PhotonNetwork.CurrentRoom != null)
			{
				GUILayout.Label("Active Room ID: " + Photon.Pun.PhotonNetwork.CurrentRoom.Name, Theme.Small, Array.Empty<GUILayoutOption>());
			}
			int count = ModNet.DetectedSlaves.Count;
			GUILayout.Label("Detected Slaves in Lobby: " + count, Theme.Label, Array.Empty<GUILayoutOption>());
			if (count > 0)
			{
				lock (ModNet.DetectedSlaves)
				{
					foreach (var s in ModNet.DetectedSlaves.Values)
					{
						string statusNote = ModNet.HasSlaveOverride(s.ActorNumber) ? " [Custom Overrides]" : " [Lobby Defaults]";
						GUILayout.Label("  [Slave v" + s.Version + "] " + s.NickName + " (Actor #" + s.ActorNumber + ")" + statusNote, Theme.Small, Array.Empty<GUILayoutOption>());
					}
				}
			}
			else
			{
				GUILayout.Label(Photon.Pun.PhotonNetwork.InRoom ? "No Slave mods detected in current lobby." : "Join a lobby to detect other scouts.", Theme.Small, Array.Empty<GUILayoutOption>());
			}

			GUILayout.Space(8f);
			GUILayout.Label("Slave Section Permissions Management:", Theme.RowLabel, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(24f) });

			// Target selector: [Global Defaults] and each detected slave
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("Global Lobby Defaults", (selectedSlaveActor == 0) ? Theme.ChipOn : Theme.Chip, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(26f) }))
			{
				selectedSlaveActor = 0;
			}
			lock (ModNet.DetectedSlaves)
			{
				foreach (var s in ModNet.DetectedSlaves.Values)
				{
					string slaveLabel = s.NickName + " (#" + s.ActorNumber + ")";
					if (GUILayout.Button(slaveLabel, (selectedSlaveActor == s.ActorNumber) ? Theme.ChipOn : Theme.Chip, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(26f) }))
					{
						selectedSlaveActor = s.ActorNumber;
					}
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(6f);

			if (selectedSlaveActor == 0)
			{
				// Global defaults view
				GUILayout.Label("Default sections visible to Slaves joining this lobby (without individual overrides):", Theme.Small, Array.Empty<GUILayoutOption>());
				for (int row = 0; row < 2; row++)
				{
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					for (int col = 0; col < 5; col++)
					{
						int idx = row * 5 + col;
						if (idx < ModNet.ManageableSections.Length)
						{
							string sec = ModNet.ManageableSections[idx];
							bool allowed = ModNet.IsGlobalSectionAllowed(sec);
							if (GUILayout.Button(Lang.T(sec), allowed ? Theme.ChipOn : Theme.Chip, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(26f) }))
							{
								ModNet.ToggleGlobalSection(sec);
								Say("Global default " + sec + (allowed ? " revoked" : " granted"));
							}
						}
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					if (row == 0) GUILayout.Space(3f);
				}

				GUILayout.Space(4f);
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				if (GUILayout.Button("Grant All Sections (Global)", Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[2] { GUILayout.Width(190f), GUILayout.Height(26f) }))
				{
					ModNet.SetAllGlobalSections(true);
					Say("All sections granted to Slaves by default");
				}
				GUILayout.Space(6f);
				if (GUILayout.Button("Revoke All Sections (Global)", Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[2] { GUILayout.Width(190f), GUILayout.Height(26f) }))
				{
					ModNet.SetAllGlobalSections(false);
					Say("All sections revoked from Slaves by default");
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			else
			{
				// Per-slave view
				ModNet.SlaveInfo targetSlave = null;
				lock (ModNet.DetectedSlaves)
				{
					ModNet.DetectedSlaves.TryGetValue(selectedSlaveActor, out targetSlave);
				}

				if (targetSlave != null)
				{
					bool hasOverride = ModNet.HasSlaveOverride(targetSlave.ActorNumber);
					GUILayout.Label("Permissions for " + targetSlave.NickName + " (Actor #" + targetSlave.ActorNumber + "):", Theme.Label, Array.Empty<GUILayoutOption>());
					GUILayout.Label("Status: " + (hasOverride ? "Custom Overrides Active (Independent of Global)" : "Inheriting Lobby Global Defaults"), Theme.Small, Array.Empty<GUILayoutOption>());

					for (int row = 0; row < 2; row++)
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						for (int col = 0; col < 5; col++)
						{
							int idx = row * 5 + col;
							if (idx < ModNet.ManageableSections.Length)
							{
								string sec = ModNet.ManageableSections[idx];
								bool allowed = ModNet.IsSlaveSectionAllowed(targetSlave.ActorNumber, sec);
								if (GUILayout.Button(Lang.T(sec), allowed ? Theme.ChipOn : Theme.Chip, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(26f) }))
								{
									ModNet.ToggleSlaveSection(targetSlave.ActorNumber, sec);
									Say(targetSlave.NickName + ": " + sec + (allowed ? " revoked" : " granted"));
								}
							}
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						if (row == 0) GUILayout.Space(3f);
					}

					GUILayout.Space(4f);
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					if (GUILayout.Button("Grant All for This Slave", Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[2] { GUILayout.Width(180f), GUILayout.Height(26f) }))
					{
						ModNet.SetAllSlaveSections(targetSlave.ActorNumber, true);
						Say("All sections granted to " + targetSlave.NickName);
					}
					GUILayout.Space(6f);
					if (GUILayout.Button("Revoke All for This Slave", Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[2] { GUILayout.Width(180f), GUILayout.Height(26f) }))
					{
						ModNet.SetAllSlaveSections(targetSlave.ActorNumber, false);
						Say("All sections revoked from " + targetSlave.NickName);
					}
					if (hasOverride)
					{
						GUILayout.Space(6f);
						if (GUILayout.Button("Reset to Global Defaults", Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[2] { GUILayout.Width(180f), GUILayout.Height(26f) }))
						{
							ModNet.ClearSlaveOverride(targetSlave.ActorNumber);
							Say("Cleared override for " + targetSlave.NickName + " (using Global)");
						}
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				else
				{
					GUILayout.Label("Selected scout is no longer in lobby.", Theme.Small, Array.Empty<GUILayoutOption>());
					selectedSlaveActor = 0;
				}
			}
		}

		GUILayout.EndVertical();
	}

	private void DrawJoinMaster(float width)
	{
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());

		if (Photon.Pun.PhotonNetwork.InRoom && Photon.Pun.PhotonNetwork.CurrentRoom != null)
		{
			GUILayout.Label("Connected to Room: " + Photon.Pun.PhotonNetwork.CurrentRoom.Name, Theme.Label, Array.Empty<GUILayoutOption>());
			GUILayout.Label("Connected with host and scouts in session.", Theme.Small, Array.Empty<GUILayoutOption>());
		}
		else
		{
			string detectedRoom = ModNet.GetActiveMasterRoomInfo();
			if (!string.IsNullOrEmpty(detectedRoom))
			{
				GUILayout.Label("Detected Master Lobby: " + detectedRoom, Theme.Label, Array.Empty<GUILayoutOption>());
				GUILayout.Space(4f);
				if (GUILayout.Button("Direct Join Master Lobby", Theme.Primary, (GUILayoutOption[])(object)new GUILayoutOption[2] { GUILayout.Width(220f), GUILayout.Height(32f) }))
				{
					if (ModNet.JoinMasterRoom())
					{
						Say("Connecting to Master lobby...");
					}
					else
					{
						Say("Could not connect to Master lobby");
					}
				}
				GUILayout.Space(4f);
			}
			else
			{
				GUILayout.Label("Waiting for Master to host a lobby...", Theme.Small, Array.Empty<GUILayoutOption>());
			}
		}

		GUILayout.EndVertical();
	}

	private void DrawJoinSlave(float width)
	{
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());

		if (Photon.Pun.PhotonNetwork.InRoom && Photon.Pun.PhotonNetwork.CurrentRoom != null)
		{
			GUILayout.Label("Connected to Room: " + Photon.Pun.PhotonNetwork.CurrentRoom.Name, Theme.Label, Array.Empty<GUILayoutOption>());
			GUILayout.Label("Connected with host and scouts in session.", Theme.Small, Array.Empty<GUILayoutOption>());
		}

		string detectedRoom = ModNet.GetActiveSlaveRoomInfo();
		if (!string.IsNullOrEmpty(detectedRoom))
		{
			GUILayout.Label("Detected Slave Lobby: " + detectedRoom, Theme.Label, Array.Empty<GUILayoutOption>());
			GUILayout.Space(4f);
			if (GUILayout.Button("Direct Join Slave Lobby", Theme.Primary, (GUILayoutOption[])(object)new GUILayoutOption[2] { GUILayout.Width(220f), GUILayout.Height(32f) }))
			{
				if (ModNet.JoinSlaveRoom())
				{
					Say("Connecting to Slave lobby...");
				}
				else
				{
					Say("Could not connect to Slave lobby");
				}
			}
			GUILayout.Space(4f);
		}
		else
		{
			GUILayout.Label("Waiting for Slave to host or join a lobby...", Theme.Small, Array.Empty<GUILayoutOption>());
		}

		GUILayout.EndVertical();
	}

	private void DrawMenuKey(float width)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(13f);
		GUILayout.Label(listening ? Lang.T("Press the key you want. Escape cancels.") : Lang.F("The menu opens with {0}", Kit.MenuKey.Value), Theme.RowLabel, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) });
		if (GUILayout.Button(Lang.T(listening ? "Cancel" : "Change"), listening ? Theme.Primary : Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Width(96f),
			GUILayout.Height(28f)
		}))
		{
			listening = !listening;
			keyFor = null;
		}
		GUILayout.EndHorizontal();
	}

	private void DrawShortcuts(float width)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		List<Keys.Bound> visibleBounds = new List<Keys.Bound>();
		foreach (Keys.Bound b in Keys.All)
		{
			string sec = Keys.GetSection(b);
			if (string.IsNullOrEmpty(sec) || ModNet.IsSectionAllowed(sec))
			{
				visibleBounds.Add(b);
			}
		}

		if (visibleBounds.Count == 0)
		{
			GUILayout.Label(Lang.T("No shortcuts available for permitted sections."), Theme.Small, Array.Empty<GUILayoutOption>());
			return;
		}

		float num = (width - 10f) / 2f;
		int num2 = (visibleBounds.Count + 1) / 2;
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		for (int i = 0; i < 2; i++)
		{
			if (i == 1)
			{
				GUILayout.Space(10f);
			}
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num) });
			for (int j = ((i != 0) ? num2 : 0); j < ((i == 0) ? num2 : visibleBounds.Count); j++)
			{
				Keys.Bound bound = visibleBounds[j];
				bool flag = keyFor == bound.Id;
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label(Lang.T(Keys.Label(bound)), Theme.RowLabel, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(26f) });
				string text;
				if (!flag)
				{
					if ((int)bound.Key.Value != 0)
					{
						KeyCode value = bound.Key.Value;
						text = ((KeyCode)value).ToString();
					}
					else
					{
						text = Lang.T("No key");
					}
				}
				else
				{
					text = Lang.T("Press a key");
				}
				if (GUILayout.Button(text, flag ? Theme.Primary : Theme.Chip, (GUILayoutOption[])(object)new GUILayoutOption[2]
				{
					GUILayout.Width(110f),
					GUILayout.Height(24f)
				}))
				{
					keyFor = (flag ? null : bound.Id);
					listening = false;
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
	}

	private void LateUpdate()
	{
		SelfTest.LateTick();
	}

	private static string NameOf(Character c)
	{
		if (!string.IsNullOrEmpty(c.characterName))
		{
			return c.characterName;
		}
		return Lang.T("Scout");
	}

	private void Each(string done, Action<Character> act)
	{
		int num = 0;
		foreach (Character item in Features.Others())
		{
			if (!((Object)(object)target != (Object)null) || !((Object)(object)item != (Object)(object)target))
			{
				act(item);
				num++;
			}
		}
		Say(Lang.T((num == 0) ? "Nobody to prank" : done));
	}

	private void Helping(Action<Character> act)
	{
		if ((Object)(object)friend == (Object)null || !Features.Others().Contains(friend))
		{
			Say(Lang.T("Pick a scout first"));
		}
		else
		{
			act(friend);
		}
	}

	private void DrawWho(ref Character who, bool everyone)
	{
		List<Character> list = Features.Others();
		if ((Object)(object)who != (Object)null && !list.Contains(who))
		{
			who = null;
		}
		if (!everyone && (Object)(object)who == (Object)null && list.Count > 0)
		{
			who = list[0];
		}
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (everyone && GUILayout.Button(Lang.T("Everyone"), ((Object)(object)who == (Object)null) ? Theme.ChipOn : Theme.Chip, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(28f) }))
		{
			who = null;
		}
		foreach (Character item in list)
		{
			string displayName = NameOf(item);
			if (ModNet.Role == ModNet.ModRole.Master && ModNet.IsSlave(item))
			{
				displayName += " [SLAVE]";
			}
			if (GUILayout.Button(displayName, ((Object)(object)who == (Object)(object)item) ? Theme.ChipOn : Theme.Chip, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(28f) }))
			{
				who = item;
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		if (list.Count == 0)
		{
			GUILayout.Label(Lang.T("Nobody else is here. This page needs a lobby with other scouts in it."), Theme.Small, Array.Empty<GUILayoutOption>());
		}
	}

	private void DrawForever(float width)
	{
		bool flag = Time.unscaledTime < foreverUntil;
		if (!GUILayout.Button(flag ? foreverAsk : Lang.T("Unlock every cosmetic for good"), flag ? Theme.Primary : Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(32f) }))
		{
			return;
		}
		if (flag)
		{
			foreverUntil = 0f;
			Features.UnlockCosmeticsForever();
			return;
		}
		int ascent;
		int locked;
		List<ACHIEVEMENTTYPE> list = Features.CosmeticRequirements(out ascent, out locked);
		if (locked == 0)
		{
			Say(Lang.T("Every cosmetic is already yours"));
			return;
		}
		foreverAsk = ((ascent > 0) ? Lang.F("Click again: {0} Steam achievements and ascent {1}, for good", list.Count, ascent) : Lang.F("Click again: {0} Steam achievements, for good", list.Count));
		foreverUntil = Time.unscaledTime + 6f;
	}

	private void DrawOutfits(float width)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Invalid comparison between Unknown and I4
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		float num = (width - 24f) / 4f;
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		Rect val = default(Rect);
		Rect val4 = default(Rect);
		for (int i = 0; i < 4; i++)
		{
			if (i > 0)
			{
				GUILayout.Space(8f);
			}
			GUILayout.BeginVertical(Theme.Foot, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num) });
			int[] array = Features.Outfit(i);
			GUILayout.Label(Lang.F("Outfit {0}", i + 1), Theme.CardTitle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(18f) });
			Rect rect = GUILayoutUtility.GetRect(num - 8f, 52f);
			if (array == null)
			{
				GUI.Label(rect, Lang.T("Empty"), Theme.Centre);
			}
			else if ((int)Event.current.type == 7)
			{
				float num2 = 44f;
				float num3 = rect.x + (rect.width - (num2 * 3f + 12f)) / 2f;
				float num4 = rect.y + (rect.height - num2) / 2f;
				val = new Rect(num3, num4, num2, num2);
				CustomizationOption val2 = Features.Part(0, array[0]);
				Color color = GUI.color;
				GUI.color = (((Object)(object)val2 != (Object)null) ? val2.color : Color.white);
				Theme.Knob.Draw(val, false, false, false, false);
				GUI.color = color;
				int[] array2 = new int[2] { 3, 1 };
				foreach (int num5 in array2)
				{
					CustomizationOption val3 = Features.Part(num5, array[num5]);
					if ((Object)(object)val3 != (Object)null && (Object)(object)val3.texture != (Object)null)
					{
						GUI.DrawTexture(new Rect(val.x + 3f, val.y + 3f, num2 - 6f, num2 - 6f), val3.texture, (ScaleMode)2);
					}
				}
				array2 = new int[2] { 4, 5 };
				foreach (int num6 in array2)
				{
					num3 += num2 + 6f;
					val4 = new Rect(num3, num4, num2, num2);
					Theme.Tile.Draw(val4, false, false, false, false);
					CustomizationOption val5 = Features.Part(num6, array[num6]);
					if ((Object)(object)val5 != (Object)null && (Object)(object)val5.texture != (Object)null)
					{
						GUI.DrawTexture(new Rect(val4.x + 3f, val4.y + 3f, num2 - 6f, num2 - 6f), val5.texture, (ScaleMode)2);
					}
				}
			}
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button(Lang.T("Wear"), (array != null) ? Theme.Primary : Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(28f) }))
			{
				Features.WearOutfit(i);
			}
			if (GUILayout.Button(Lang.T("Save"), Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(28f) }))
			{
				Features.SaveOutfit(i);
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
	}

	private void DrawSpots(float width)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Live.Spots.Length; i++)
		{
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Space(13f);
			GUILayout.Label(Live.Spots[i].HasValue ? Lang.F("Spot {0}: {1} m away", i + 1, Vector3.Distance(Features.Me.Center, Live.Spots[i].Value).ToString("0")) : Lang.F("Spot {0}: empty", i + 1), Theme.RowLabel, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) });
			if (GUILayout.Button(Lang.T("Remember"), Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.Width(110f),
				GUILayout.Height(28f)
			}))
			{
				Features.SaveSpot(i);
			}
			if (GUILayout.Button(Lang.T("Go"), Live.Spots[i].HasValue ? Theme.Primary : Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.Width(70f),
				GUILayout.Height(28f)
			}))
			{
				Features.GoSpot(i);
			}
			GUILayout.EndHorizontal();
		}
	}

	private void DrawToScouts(float width)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		List<Character> list = Features.Others();
		if (list.Count == 0)
		{
			GUILayout.Label(Lang.T("Nobody else is here."), Theme.Small, Array.Empty<GUILayoutOption>());
		}
		foreach (Character item in list)
		{
			string text = NameOf(item);
			if (GUILayout.Button(Lang.F("To {0}", text), Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
			{
				Features.Warp(item.Center + Vector3.up * 1.5f, text);
			}
		}
	}

	private void DrawHand(float width)
	{
		if (picked == null)
		{
			GUILayout.Label(Lang.T("Pick something on the Items page first."), Theme.Small, Array.Empty<GUILayoutOption>());
		}
		else if (GUILayout.Button(Lang.F("Hand them a {0}", picked.Name), Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
		{
			Helping(delegate(Character c)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Expected O, but got Unknown
				Features.HandItem(c, (Item)picked.What);
				Say(Lang.F("{0} for {1}", picked.Name, NameOf(c)));
			});
		}
	}

	private void DrawDelivery(float width)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		if (picked == null)
		{
			GUILayout.Label(Lang.T("Pick something on the Items page and it can be dropped on their head from here."), Theme.Small, Array.Empty<GUILayoutOption>());
		}
		else
		{
			if (!GUILayout.Button(Lang.F("Drop a {0} on their head", picked.Name), Theme.Button, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
			{
				return;
			}
			Item item = (Item)picked.What;
			int num = 0;
			foreach (Character item2 in Features.Others())
			{
				if (!((Object)(object)target != (Object)null) || !((Object)(object)item2 != (Object)(object)target))
				{
					Features.DropOn(item2, item);
					num++;
				}
			}
			Say((num == 0) ? Lang.T("Nobody to prank") : Lang.F("{0} incoming", picked.Name));
		}
	}

	internal void LoadItems()
	{
		picker.Clear();
		foreach (Item item in Features.ItemList())
		{
			if (!((Object)(object)item == (Object)null))
			{
				string text = null;
				try
				{
					text = item.GetName();
				}
				catch
				{
				}
				if (string.IsNullOrEmpty(text) || text.StartsWith("LOC:"))
				{
					text = ((Object)item).name;
				}
				if (!string.IsNullOrEmpty(text))
				{
					picker.Add(text, ((Object)item).name, item, (Texture)(object)((item.UIData != null) ? item.UIData.icon : null));
				}
			}
		}
		picker.Sort();
		itemsLoaded = true;
	}

	private void DrawItems(float width)
	{
		if (Kit.ItemsPoppedOut != null && Kit.ItemsPoppedOut.Value)
		{
			GUILayout.BeginVertical(Theme.Panel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			GUILayout.Space(30f);
			GUILayout.Label(Lang.T("Item Spawner Popped Out"), Theme.Big, Array.Empty<GUILayoutOption>());
			GUILayout.Space(8f);
			GUILayout.Label(Lang.T("The items catalog is currently open in its own floating window so you can navigate and spawn items freely without blocking the main menu."), Theme.Label, Array.Empty<GUILayoutOption>());
			GUILayout.Space(16f);
			if (GUILayout.Button(Lang.T("Dock Back to Main Menu"), Theme.Primary, GUILayout.Width(240f), GUILayout.Height(36f)))
			{
				Kit.ItemsPoppedOut.Value = false;
			}
			GUILayout.EndVertical();
			return;
		}

		DrawItemContent(width, isPoppedOut: false);
	}

	private void DrawPopoutItemWindow(int windowId)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Ascend - Item Spawner", Theme.Big, Array.Empty<GUILayoutOption>());
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(Lang.T("Dock"), Theme.Button, GUILayout.Width(80f), GUILayout.Height(24f)))
		{
			Kit.ItemsPoppedOut.Value = false;
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(4f);

		DrawItemContent(itemWin.width - 24f, isPoppedOut: true);

		GUI.DragWindow(new Rect(0f, 0f, 10000f, 30f));
	}

	private void DrawItemContent(float width, bool isPoppedOut)
	{
		if (!itemsLoaded)
		{
			LoadItems();
		}

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		picker.Draw(pageSearch);
		GUILayout.Space(8f);

		GUILayout.BeginVertical(Theme.Panel, GUILayout.Width(230f), GUILayout.ExpandHeight(true));

		if (!isPoppedOut)
		{
			if (GUILayout.Button(Lang.T("Pop Out Window \u2197"), Theme.Button, GUILayout.Height(26f)))
			{
				if (Kit.ItemsPoppedOut != null)
				{
					Kit.ItemsPoppedOut.Value = true;
				}
			}
			GUILayout.Space(4f);
		}

		if (picked == null)
		{
			GUILayout.Label(Lang.T("Pick an item"), Theme.Big, Array.Empty<GUILayoutOption>());
			GUILayout.Space(6f);
			GUILayout.Label(Lang.T("It goes into your hands when you host or play alone. In someone else's lobby, or with full pockets, it lands in front of you."), Theme.Small, Array.Empty<GUILayoutOption>());
		}
		else
		{
			Picker.Picture(GUILayoutUtility.GetRect(150f, 150f), picked);
			GUILayout.Label(picked.Name, Theme.Big, Array.Empty<GUILayoutOption>());
			GUILayout.Space(4f);

			if (GUILayout.Button(Lang.T("Spawn (Self)"), Theme.Primary, GUILayout.Height(34f)))
			{
				Features.Give((Item)picked.What, picked.Name);
			}

			if (ModNet.Role == ModNet.ModRole.Master)
			{
				GUILayout.Space(8f);
				GUILayout.Label(Lang.T("Deliver to Slave:"), Theme.Small, Array.Empty<GUILayoutOption>());

				List<Character> others = Features.InGame ? Features.Others() : new List<Character>();
				if (others == null || others.Count == 0)
				{
					GUILayout.Label(Lang.T("No other scouts in session"), Theme.Small, Array.Empty<GUILayoutOption>());
				}
				else
				{
					GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
					foreach (Character scout in others)
					{
						if (scout == null) continue;
						PhotonView pv = scout.GetComponent<PhotonView>();
						int actor = (pv != null && pv.Owner != null) ? pv.Owner.ActorNumber : -1;
						if (actor <= 0) continue;

						bool isSelected = (targetDeliverActor == actor);
						string sName = (pv.Owner != null && !string.IsNullOrEmpty(pv.Owner.NickName)) ? pv.Owner.NickName : NameOf(scout);
						if (GUILayout.Button((isSelected ? "\u25B6 " : "") + sName, isSelected ? Theme.ChipOn : Theme.Chip, GUILayout.Height(22f)))
						{
							targetDeliverActor = actor;
						}
					}
					GUILayout.EndVertical();

					if (targetDeliverActor > 0)
					{
						string realPrefab = (picked.What is Item it && (UnityEngine.Object)it != null) ? ((UnityEngine.Object)it).name : picked.Name;
						GUILayout.Space(4f);
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						if (GUILayout.Button(Lang.T("To Hands"), Theme.Button, GUILayout.Height(26f)))
						{
							ModNet.DeliverItemToScout(targetDeliverActor, "Hands", realPrefab);
							Say(Lang.F("Delivered {0} to hands", picked.Name));
						}
						if (GUILayout.Button(Lang.T("To Backpack"), Theme.Button, GUILayout.Height(26f)))
						{
							ModNet.DeliverItemToScout(targetDeliverActor, "Backpack", realPrefab);
							Say(Lang.F("Delivered {0} to backpack", picked.Name));
						}
						GUILayout.EndHorizontal();
					}
					else
					{
						GUILayout.Label(Lang.T("Select a scout above"), Theme.Small, Array.Empty<GUILayoutOption>());
					}
				}
			}
			else
			{
				GUILayout.Label(Lang.T("The Help and Troll pages can hand this item to another scout."), Theme.Small, Array.Empty<GUILayoutOption>());
			}
		}

		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	private void DrawThemeSettings(float width)
	{
		GUILayout.Label(Lang.T("Color Presets"), Theme.Big, Array.Empty<GUILayoutOption>());
		GUILayout.Space(4f);

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		int cols = 0;
		foreach (var preset in Theme.Presets)
		{
			bool isActive = Kit.ThemePreset != null && Kit.ThemePreset.Value == preset.Name;
			if (GUILayout.Button(preset.Name, isActive ? Theme.ChipOn : Theme.Chip, GUILayout.Height(26f)))
			{
				Theme.ApplyPreset(preset.Name);
				Say(Lang.F("Theme set to {0}", preset.Name));
			}
			cols++;
			if (cols % 3 == 0)
			{
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			}
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(12f);
		GUILayout.Label(Lang.T("Custom Colors"), Theme.Big, Array.Empty<GUILayoutOption>());
		GUILayout.Space(4f);

		Color primary = Theme.Accent;
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(Lang.T("Primary Accent:"), Theme.Label, GUILayout.Width(130f));
		Rect pSwatch = GUILayoutUtility.GetRect(40f, 20f, GUILayout.Width(40f), GUILayout.Height(20f));
		Color origColor = GUI.color;
		GUI.color = primary;
		GUI.DrawTexture(pSwatch, (Texture)(object)Texture2D.whiteTexture);
		GUI.color = origColor;
		GUILayout.EndHorizontal();

		float pr = DrawColorSlider("R", primary.r);
		float pg = DrawColorSlider("G", primary.g);
		float pb = DrawColorSlider("B", primary.b);

		Color secondary = Theme.AccentHi;
		GUILayout.Space(6f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(Lang.T("Secondary Highlight:"), Theme.Label, GUILayout.Width(130f));
		Rect sSwatch = GUILayoutUtility.GetRect(40f, 20f, GUILayout.Width(40f), GUILayout.Height(20f));
		GUI.color = secondary;
		GUI.DrawTexture(sSwatch, (Texture)(object)Texture2D.whiteTexture);
		GUI.color = origColor;
		GUILayout.EndHorizontal();

		float sr = DrawColorSlider("R", secondary.r);
		float sg = DrawColorSlider("G", secondary.g);
		float sb = DrawColorSlider("B", secondary.b);

		if (Mathf.Abs(pr - primary.r) > 0.005f || Mathf.Abs(pg - primary.g) > 0.005f || Mathf.Abs(pb - primary.b) > 0.005f ||
		    Mathf.Abs(sr - secondary.r) > 0.005f || Mathf.Abs(sg - secondary.g) > 0.005f || Mathf.Abs(sb - secondary.b) > 0.005f)
		{
			Theme.SetCustomColors(new Color(pr, pg, pb, 1f), new Color(sr, sg, sb, 1f));
		}

		GUILayout.Space(10f);
		if (GUILayout.Button(Lang.T("Reset to Default Emerald Teal"), Theme.Button, GUILayout.Height(28f)))
		{
			Theme.ApplyPreset("Emerald Teal");
			Say(Lang.T("Theme reset to Emerald Teal"));
		}
	}

	private float DrawColorSlider(string label, float val)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(label, Theme.Small, GUILayout.Width(20f));
		float newVal = GUILayout.HorizontalSlider(val, 0f, 1f, Array.Empty<GUILayoutOption>());
		GUILayout.Label(Mathf.RoundToInt(newVal * 255f).ToString(), Theme.Small, GUILayout.Width(35f));
		GUILayout.EndHorizontal();
		return newVal;
	}

	private void DrawScoutInventories(float width)
	{
		List<Character> all = new List<Character>();
		if (Features.Me != null) all.Add(Features.Me);
		if (Features.InGame)
		{
			all.AddRange(Features.Others());
		}

		if (all.Count == 0)
		{
			GUILayout.Label(Lang.T("No scouts detected in the lobby."), Theme.Label, Array.Empty<GUILayoutOption>());
			return;
		}

		GUILayout.Label(Lang.T("Live scout inventories, pockets, and backpacks:"), Theme.Label, Array.Empty<GUILayoutOption>());
		GUILayout.Space(8f);

		foreach (Character scout in all)
		{
			if (scout == null) continue;
			PhotonView pv = scout.GetComponent<PhotonView>();
			int actor = (pv != null && pv.Owner != null) ? pv.Owner.ActorNumber : -1;
			string sName = (pv != null && pv.Owner != null && !string.IsNullOrEmpty(pv.Owner.NickName)) ? pv.Owner.NickName : NameOf(scout);
			bool isMe = (scout == Features.Me);

			string title = (isMe ? "[You] " : "") + sName;
			if (actor > 0) title += " (Actor " + actor + ")";
			if (!isMe && ModNet.Role == ModNet.ModRole.Master && ModNet.IsSlave(scout)) title += " [SLAVE]";

			GUILayout.BeginVertical(Theme.Panel, GUILayout.ExpandWidth(true));
			GUILayout.Label(title, Theme.Big, GUILayout.ExpandWidth(true));

			if (!isMe && ModNet.Role == ModNet.ModRole.Master && picked != null && actor > 0)
			{
				string realPrefab = (picked.What is Item it && (UnityEngine.Object)it != null) ? ((UnityEngine.Object)it).name : picked.Name;
				GUILayout.Space(2f);
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				if (GUILayout.Button(Lang.F("Give to Hands ({0})", picked.Name), Theme.Button, GUILayout.Height(26f), GUILayout.ExpandWidth(true)))
				{
					ModNet.DeliverItemToScout(actor, "Hands", realPrefab);
					Say(Lang.F("Delivered {0} to {1}'s hands", picked.Name, sName));
				}
				if (GUILayout.Button(Lang.T("To Pack"), Theme.Button, GUILayout.Width(80f), GUILayout.Height(26f)))
				{
					ModNet.DeliverItemToScout(actor, "Backpack", realPrefab);
					Say(Lang.F("Delivered {0} to {1}'s pack", picked.Name, sName));
				}
				GUILayout.EndHorizontal();
			}

			ModNet.GetPlayerInventory(scout, out string currentItem, out List<string> pockets, out List<string> backpack);

			GUILayout.Space(4f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label("Holding: " + currentItem, Theme.Small, GUILayout.Width(220f));
			GUILayout.Label("Pockets: " + (pockets.Count > 0 ? string.Join(", ", pockets) : "Empty"), Theme.Small, Array.Empty<GUILayoutOption>());
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label("Backpack: " + (backpack.Count > 0 ? string.Join(", ", backpack) : "None"), Theme.Small, Array.Empty<GUILayoutOption>());
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			GUILayout.Space(6f);
		}
	}

	private int Tag(Camera cam, Vector3 eye, Vector3 world, string text, Color color, float range)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(eye, world);
		if (num > range)
		{
			return 0;
		}
		Vector3 val = cam.WorldToScreenPoint(world);
		if (val.z <= 0f)
		{
			return 0;
		}
		float num2 = val.x * (float)Screen.width / (float)cam.pixelWidth;
		float num3 = (float)Screen.height - val.y * (float)Screen.height / (float)cam.pixelHeight;
		Rect val2 = default(Rect);
		val2 = new Rect(num2 - 100f, num3 - 10f, 200f, 20f);
		string text2 = text + "  " + num.ToString("0") + " m";
		GUI.Label(new Rect(val2.x + 1f, val2.y + 1f, val2.width, val2.height), text2, espShadow);
		espStyle.normal.textColor = color;
		GUI.Label(val2, text2, espStyle);
		return 1;
	}
}
