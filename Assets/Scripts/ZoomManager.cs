using Cinemachine;
using UnityEngine;

using static Cinemachine.CinemachineTargetGroup;

[ExecuteInEditMode]
public class ZoomManager : MonoBehaviour {

	[SerializeField] private CinemachineTargetGroup targetGroup;

	[SerializeField] private float surfaceRadius = 10;
	[SerializeField] private float bottomRadius = 50;
	[SerializeField] private float surfaceHeight = 0;
	[SerializeField] private float bottomHeight = -100;

	void Start() {
		if (targetGroup.IsEmpty) {
			targetGroup.AddMember(transform, 1, surfaceRadius);
		}

		Debug.Assert(targetGroup.m_Targets.Length == 1 && targetGroup.m_Targets[0].target == transform);
	}

	void Update() {
		Target me = targetGroup.m_Targets[0];
		me.radius = Mathf.Lerp(surfaceRadius, bottomRadius,
			Mathf.InverseLerp(surfaceHeight, bottomHeight, transform.position.y));
		targetGroup.m_Targets[0] = me;
	}
}
