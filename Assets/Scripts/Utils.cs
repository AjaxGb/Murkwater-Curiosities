using UnityEngine;

public static class Utils {
	public static void ClampSymmetric(ref float val, float radius) {
		val = Mathf.Clamp(val, -radius, radius);
	}
}
