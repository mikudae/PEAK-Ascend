using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

internal static class Live
{
	public static bool FreeCam;

	public static Vector3 CamPos;

	public static float CamYaw;

	public static float CamPitch;

	public static readonly Vector3?[] Spots = new Vector3?[3];
}
