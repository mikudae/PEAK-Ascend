using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Summit;

internal static class Patches
{
	[CompilerGenerated]
	private sealed class _003CKept_003Ed__17 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool ignoreActions;

		public Item item;

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
		public _003CKept_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
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
				if (!ignoreActions && item.OnConsumed != null)
				{
					item.OnConsumed();
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
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
	}

	private static readonly MethodInfo SetShowCursor = AccessTools.PropertySetter(typeof(GUIManager), "windowShowingCursor");

	private static readonly MethodInfo SetBlockInput = AccessTools.PropertySetter(typeof(GUIManager), "windowBlockingInput");

	private static readonly MethodInfo ToggleBody = AccessTools.Method(typeof(HideTheBody), "Toggle", (Type[])null, (Type[])null);

	private static readonly object[] True = new object[1] { true };

	internal static bool Granting;

	private static readonly HashSet<string> Weather = new HashSet<string> { "Hazard_Rain", "Hazard_Wind", "Hazard_Storm", "Hazard_Tornado", "Hazard_Heat", "ColdNight" };

	public static bool AddStatusPrefix(CharacterAfflictions __instance, CharacterAfflictions.STATUSTYPE statusType, ref bool __result)
	{
		if ((Object)(object)__instance.character == (Object)null || !__instance.character.IsLocal)
		{
			return true;
		}
		if (State.God)
		{
			__result = false;
			return false;
		}
		if (State.NoWeather && (statusType == CharacterAfflictions.STATUSTYPE.Cold || statusType == CharacterAfflictions.STATUSTYPE.Hot))
		{
			__result = false;
			return false;
		}
		if (State.NoHazards && (statusType == CharacterAfflictions.STATUSTYPE.Thorns ||
								statusType == CharacterAfflictions.STATUSTYPE.Spores ||
								statusType == CharacterAfflictions.STATUSTYPE.Web ||
								statusType == CharacterAfflictions.STATUSTYPE.Arrow ||
								statusType == CharacterAfflictions.STATUSTYPE.Petrify ||
								statusType == CharacterAfflictions.STATUSTYPE.FlyTrap ||
								statusType == CharacterAfflictions.STATUSTYPE.Poison))
		{
			__result = false;
			return false;
		}
		return true;
	}

	public static bool FallDamagePrefix(Character ___character)
	{
		if ((State.NoFall || State.Fly) && (Object)(object)___character != (Object)null)
		{
			return !___character.IsLocal;
		}
		return true;
	}

	public static bool FallPrefix(Character __instance)
	{
		if (State.NoFall || State.Fly)
		{
			return !__instance.IsLocal;
		}
		return true;
	}

	public static void MoveForcePostfix(Character ___character, ref float __result)
	{
		if (State.Speed != 1f && (Object)(object)___character != (Object)null && ___character.IsLocal)
		{
			__result *= State.Speed;
		}
	}

	public static void FlyPostfix(Character ___character)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if (!State.Fly || (Object)(object)___character == (Object)null || !___character.IsLocal || ___character.data.dead)
		{
			return;
		}
		CharacterInput input = ___character.input;
		Vector3 val = ___character.data.lookDirection * input.movementInput.y + ___character.data.lookDirection_Right * input.movementInput.x;
		if (input.jumpIsPressed)
		{
			val += Vector3.up;
		}
		if (input.crouchIsPressed)
		{
			val += Vector3.down;
		}
		Vector3 linearVelocity = Vector3.ClampMagnitude(val, 1f) * State.FlySpeed * (input.sprintIsPressed ? 2.5f : 1f);
		foreach (Bodypart part in ___character.refs.ragdoll.partList)
		{
			Rigidbody rig = part.Rig;
			if ((Object)(object)rig != (Object)null)
			{
				rig.linearVelocity = linearVelocity;
			}
		}
		___character.data.sinceGrounded = 0f;
	}

	public static void CarryWeightPostfix(ref int __result)
	{
		if (State.NoWeight)
		{
			__result = 0;
		}
	}

	public static bool FogPrefix()
	{
		return !State.StopFog;
	}

	public static bool FogUpdatePrefix(Fog __instance)
	{
		if (State.StopFog)
		{
			if ((Object)(object)__instance != (Object)null)
			{
				__instance.fogHeight = -9999f;
				__instance.transform.position = new Vector3(0f, -9999f, 0f);
				Shader.SetGlobalFloat("FogHeight", -9999f);
			}
			Character me = Character.localCharacter;
			if ((Object)(object)me != (Object)null && me.data != null)
			{
				me.data.isInFog = false;
			}
			return false;
		}
		return true;
	}

	public static void FogEnabledPostfix(ref bool __result)
	{
		if (State.StopFog)
		{
			__result = false;
		}
	}

	public static void NightColdPostfix(ref bool __result)
	{
		if (State.NoWeather)
		{
			__result = false;
		}
	}

	public static bool WeatherPrefix()
	{
		return !State.NoWeather;
	}

	public static bool HazardPrefix()
	{
		return !State.NoHazards;
	}

	public static bool ThornPrefix(CharacterAfflictions __instance)
	{
		if (State.NoHazards && (Object)(object)__instance.character != (Object)null && __instance.character.IsLocal)
		{
			return false;
		}
		return true;
	}

	public static void WindowStatusPostfix(GUIManager __instance)
	{
		if (Menu.IsOpen && !(SetShowCursor == null) && !(SetBlockInput == null))
		{
			SetShowCursor.Invoke(__instance, True);
			SetBlockInput.Invoke(__instance, True);
		}
	}

	public static void CosmeticLockPostfix(ref bool __result)
	{
		if (State.Cosmetics)
		{
			__result = false;
		}
	}

	public static bool AchievementPrefix()
	{
		if (!Granting)
		{
			return !Kit.BlockAchievements.Value;
		}
		return true;
	}

	private static bool Mine(Item item)
	{
		if ((Object)(object)item != (Object)null && (Object)(object)item.holderCharacter != (Object)null)
		{
			return item.holderCharacter.IsLocal;
		}
		return false;
	}

	public static bool ConsumePrefix(Item __instance, bool ignoreActions, ref IEnumerator __result)
	{
		if (!State.KeepItems || !Mine(__instance))
		{
			return true;
		}
		__result = Kept(__instance, ignoreActions);
		return false;
	}

	[IteratorStateMachine(typeof(_003CKept_003Ed__17))]
	private static IEnumerator Kept(Item item, bool ignoreActions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CKept_003Ed__17(0)
		{
			item = item,
			ignoreActions = ignoreActions
		};
	}

	public static bool ReduceUsesPrefix(Action_ReduceUses __instance)
	{
		Item component = ((Component)__instance).GetComponent<Item>();
		if (State.KeepItems)
		{
			return !Mine(component);
		}
		return true;
	}

	public static bool RopeFuelPrefix(RopeSpool __instance, float value, float ___ropeFuel)
	{
		if (!State.KeepItems || value >= ___ropeFuel)
		{
			return true;
		}
		return !Mine(((Component)__instance).GetComponent<Item>());
	}

	public static void MaxSegmentsPostfix(ref int __result)
	{
		if (State.RopeLength > 1f)
		{
			__result = Mathf.RoundToInt((float)__result * State.RopeLength);
		}
	}

	public static void GrabAnglePostfix(Character ___character, ref bool __result)
	{
		if (State.ClimbAnything && (Object)(object)___character != (Object)null && ___character.IsLocal)
		{
			__result = true;
		}
	}

	public static void SlidePrefix(Character ___character, ref Vector2 value)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (State.NoSlip && (Object)(object)___character != (Object)null && ___character.IsLocal)
		{
			value = Vector2.zero;
		}
	}

	public static void ReachPrefix(Character ___character, ref float raycastDistance)
	{
		if (State.Reach > 1f && (Object)(object)___character != (Object)null && ___character.IsLocal)
		{
			raycastDistance *= State.Reach;
		}
	}

	public static void RunValuePostfix(RunSettings.SETTINGTYPE setting, ref int __result)
	{
		if (__result == 0) return;
		string key = setting.ToString();
		if (State.StopFog && key == "Fog")
		{
			__result = 0;
			return;
		}
		if (State.StopLava && (key == "Hazard_TheLavaRises" || key == "Hazard_TheGloomRises"))
		{
			__result = 0;
			return;
		}
		if (State.NoWeather && (Weather.Contains(key) || key == "Hazard_Heat" || key == "ColdNight" || key == "Hazard_Rain" || key == "Hazard_Wind" || key == "Hazard_Storm" || key == "Hazard_Tornado"))
		{
			__result = 0;
			return;
		}
		if (State.NoHazards && key.StartsWith("Hazard_") && !Weather.Contains(key) && key != "Hazard_TheLavaRises" && key != "Hazard_TheGloomRises")
		{
			__result = 0;
			return;
		}
	}

	public static void LavaRisingStartPostfix(LavaRising __instance)
	{
		if ((Object)(object)__instance != (Object)null && State.LavaHeightOffset != 0f)
		{
			try
			{
				FieldInfo shField = typeof(LavaRising).GetField("startHeight", BindingFlags.Instance | BindingFlags.NonPublic);
				if (shField != null)
				{
					float cur = (float)shField.GetValue(__instance);
					shField.SetValue(__instance, cur + State.LavaHeightOffset);
				}
				if ((Object)(object)__instance.topTransform != (Object)null)
				{
					__instance.topTransform.position += Vector3.up * State.LavaHeightOffset;
				}
				if ((Object)(object)__instance.heightMinimumTransform != (Object)null)
				{
					__instance.heightMinimumTransform.position += Vector3.up * State.LavaHeightOffset;
				}
				if ((Object)(object)__instance.lava != (Object)null)
				{
					__instance.lava.transform.position += Vector3.up * State.LavaHeightOffset;
					__instance.lava.position += Vector3.up * State.LavaHeightOffset;
				}
			}
			catch {}
		}
	}

	public static bool LavaPrefix(LavaRising __instance)
	{
		if (State.StopLava || State.LavaSpeed <= 0f)
		{
			return false;
		}
		if (State.LavaSpeed != 1f && (Object)(object)__instance != (Object)null)
		{
			__instance.timeTraveled += Time.deltaTime * (State.LavaSpeed - 1f);
		}
		return true;
	}

	public static bool MovingLavaUpdatePrefix(MovingLava __instance)
	{
		if (State.StopLava || State.LavaSpeed <= 0f)
		{
			return false;
		}
		if (State.LavaSpeed != 1f && (Object)(object)__instance != (Object)null)
		{
			__instance.transform.position += __instance.transform.up * (__instance.speed * (State.LavaSpeed - 1f)) * Time.deltaTime;
		}
		return true;
	}

	public static bool FogMovePrefix(Fog __instance)
	{
		if (State.StopFog || State.FogSpeed <= 0f)
		{
			return false;
		}
		if (State.FogSpeed != 1f && (Object)(object)__instance != (Object)null)
		{
			__instance.fogHeight += Time.deltaTime * __instance.fogSpeed * (State.FogSpeed - 1f);
		}
		return true;
	}

	public static bool OrbFogPrefix(OrbFogHandler __instance)
	{
		if (State.StopFog || State.FogSpeed <= 0f)
		{
			return false;
		}
		if (State.FogSpeed != 1f && (Object)(object)__instance != (Object)null)
		{
			__instance.currentSize -= __instance.speed * Time.deltaTime * (State.FogSpeed - 1f);
		}
		return true;
	}

	public static void FovPostfix(ref float __result)
	{
		if (State.Fov > 50f)
		{
			__result = State.Fov;
		}
	}

	public static void ThirdPersonPostfix(MainCameraMovement __instance, Vector3 ___targetPlayerPovPosition)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (State.ThirdPerson && !Live.FreeCam && !((Object)(object)Character.localCharacter == (Object)null))
		{
			Transform transform = ((Component)__instance).transform;
			Vector3 val = ___targetPlayerPovPosition + Vector3.up * 0.35f;
			Vector3 val2 = val - transform.forward * State.CamDistance;
			Vector3 val3 = val2 - val;
			RaycastHit val4 = default(RaycastHit);
			if (Physics.SphereCast(val, 0.25f, val3.normalized, out val4, State.CamDistance, (int)HelperFunctions.terrainMapMask, (QueryTriggerInteraction)1))
			{
				val2 = val - transform.forward * Mathf.Max(0.4f, val4.distance - 0.1f);
			}
			transform.position = val2;
		}
	}

	public static void FreeCamPostfix(MainCameraMovement __instance)
	{
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		if (!Live.FreeCam)
		{
			return;
		}
		if ((Object)(object)Character.localCharacter == (Object)null)
		{
			Live.FreeCam = false;
			return;
		}
		Keyboard current = Keyboard.current;
		Mouse current2 = Mouse.current;
		if (!Menu.IsOpen && current != null && current2 != null)
		{
			Vector2 val = ((InputControl<Vector2>)(object)((UnityEngine.InputSystem.Pointer)current2).delta).ReadValue() * 0.08f;
			Live.CamYaw += val.x;
			Live.CamPitch = Mathf.Clamp(Live.CamPitch - val.y, -89f, 89f);
			Quaternion val2 = Quaternion.Euler(Live.CamPitch, Live.CamYaw, 0f);
			Vector3 val3 = Vector3.zero;
			if (((ButtonControl)current.wKey).isPressed)
			{
				val3 += Vector3.forward;
			}
			if (((ButtonControl)current.sKey).isPressed)
			{
				val3 += Vector3.back;
			}
			if (((ButtonControl)current.dKey).isPressed)
			{
				val3 += Vector3.right;
			}
			if (((ButtonControl)current.aKey).isPressed)
			{
				val3 += Vector3.left;
			}
			Vector3 val4 = val2 * val3;
			if (((ButtonControl)current.spaceKey).isPressed)
			{
				val4 += Vector3.up;
			}
			if (((ButtonControl)current.leftCtrlKey).isPressed)
			{
				val4 += Vector3.down;
			}
			Live.CamPos += val4 * (((ButtonControl)current.leftShiftKey).isPressed ? 30f : 10f) * Time.unscaledDeltaTime;
		}
		((Component)__instance).transform.SetPositionAndRotation(Live.CamPos, Quaternion.Euler(Live.CamPitch, Live.CamYaw, 0f));
	}

	public static bool ShowBodyPrefix(HideTheBody __instance, Character ___character, bool ___isShowing)
	{
		if ((!State.ThirdPerson && !Live.FreeCam) || (Object)(object)___character == (Object)null || !___character.IsLocal || ToggleBody == null)
		{
			return true;
		}
		if (!___isShowing)
		{
			ToggleBody.Invoke(__instance, True);
		}
		return false;
	}

	public static void SamplePrefix(CharacterInput __instance, ref bool playerMovementActive)
	{
		if (Live.FreeCam && (Object)(object)Character.localCharacter != (Object)null && (Object)(object)__instance == (Object)(object)Character.localCharacter.input)
		{
			playerMovementActive = false;
		}
	}

	public static bool GetBestUserIDPrefix(ref string __result)
	{
		if (ModNet.Role == ModNet.ModRole.Slave)
		{
			__result = "Slave_" + UnityEngine.Random.Range(100000, 999999);
			return false;
		}
		return true;
	}

	public static bool GetUsernamePrefix(ref string __result)
	{
		if (ModNet.Role == ModNet.ModRole.Slave)
		{
			__result = "Slave Scout #" + UnityEngine.Random.Range(100, 999);
			return false;
		}
		return true;
	}

	public static bool RestartAppIfNecessaryPrefix(ref bool __result)
	{
		__result = false;
		return false;
	}
}
