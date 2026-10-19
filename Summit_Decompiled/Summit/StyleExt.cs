using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Summit;

internal static class StyleExt
{
	private static readonly Dictionary<(GUIStyle, TextAnchor), GUIStyle> cache = new Dictionary<(GUIStyle, TextAnchor), GUIStyle>();

	internal static void Forget()
	{
		cache.Clear();
	}

	public static GUIStyle WithAnchor(this GUIStyle s, TextAnchor a = (TextAnchor)3)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!cache.TryGetValue((s, a), out var value))
		{
			value = new GUIStyle(s)
			{
				alignment = a
			};
			cache[(s, a)] = value;
		}
		return value;
	}
}
