using UnityEditor;
using UnityEngine;

public class CritterCrab : CritterBase {

	[SerializeField] private Spline2DComponent path;
	[SerializeField] private float pathPosition;

	public float speed = 2;

	void OnEnable() {
		EditorApplication.update += UpdatePosition;
	}

	void UpdatePosition() {
		pathPosition += speed;
		Update();
	}

	void OnDisable() {
		EditorApplication.update -= UpdatePosition;
	}

	void Update() {
		pathPosition = Mathf.Repeat(pathPosition, path.Length);

		float t = path.DistanceToLinearT(pathPosition);
		transform.position = path.transform.TransformPoint(path.Interpolate(t));
		Vector3 derivative = path.transform.TransformDirection(path.Derivative(t));
		transform.rotation = Quaternion.LookRotation(Vector3.forward, new Vector3(derivative.y, -derivative.x));
	}
}
