using UnityEditor;
using UnityEngine;

public class CritterCrab : CritterBase {

	[SerializeField] private Spline2DComponent path;
	[SerializeField] private float pathPosition;
	[SerializeField] private Transform bossDoor;
	[SerializeField] private AudioSource yellSound;

	public float speed = 2;

	public bool awake = false;

	public void Enrage() {
		gameObject.SetActive(true);
		awake = true;
		bossDoor.gameObject.SetActive(true);
		yellSound.Play();
	}

	protected override void Update() {
		if (!IsAlive) {
			bossDoor.gameObject.SetActive(false);
		}

		base.Update();

		if (awake) {
			pathPosition += speed * Time.deltaTime;
		}

		pathPosition = Mathf.Repeat(pathPosition, path.Length);

		float t = path.DistanceToLinearT(pathPosition);
		transform.position = path.transform.TransformPoint(path.Interpolate(t));
		Vector3 derivative = path.transform.TransformDirection(path.Derivative(t));
		transform.rotation = Quaternion.LookRotation(Vector3.forward, new Vector3(derivative.y, -derivative.x));
	}

	protected override void OnTriggerEnter2D(Collider2D collider) {}

	public override void Stun() {
		// Nope
	}
}
