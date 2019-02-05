using Cinemachine;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Rigidbody2D))]
public class SubController : MonoBehaviour {

	public enum VState { SINK, RISE, DIVE };
	public enum HState { DRIVE, REST };
	
	[SerializeField] private Rigidbody2D rb;
	[SerializeField] private SpriteRenderer mainSprite;
	[SerializeField] private CinemachineTargetGroup targetGroup;
	[Space]
	[SerializeField] private float riseSpeed = 0.3f;
	[SerializeField] private float riseAccel = 1.0f;
	[SerializeField] private float diveSpeed = 0.9f;
	[SerializeField] private float diveAccel = 1.0f;
	[SerializeField] private float sinkSpeed = 0.1f;
	[SerializeField] private float sinkAccel = 1.0f;
	[SerializeField] private float horizSpeed = 0.6f;
	[SerializeField] private float horizAccel = 1.0f;
	[SerializeField] private float horizRestAccel = 0.3f;
	[Space]
	[SerializeField] private float driveLean = 1.0f;
	[SerializeField] private float riseLean = -1.0f;
	[SerializeField] private float diveLean = 1.0f;
	[SerializeField] private float rightingSpeed = 0.1f;
	[SerializeField] private float rightingTime = 1.0f;

	public VState VerticalState { get; private set; }
	public HState HorizontalState { get; private set; }

	private Vector2 velocity;
	private float rotVelocity;

	void Start() {

	}

	void Update() {
	}
	
	void FixedUpdate() {

		Vector2 targetVel = Vector2.zero;
		float targetRot = 0.0f;

		float xMaxAccel, yMaxAccel;

		float vertInput = Input.GetAxis("Vertical");
		float horizInput = Input.GetAxis("Horizontal");
		
		if (vertInput < 0) {
			targetVel.y = vertInput * diveSpeed;
			targetRot += -vertInput * diveLean;
			yMaxAccel = diveAccel;
			VerticalState = VState.DIVE;
		} else if (vertInput > 0) {
			targetVel.y = vertInput * riseSpeed;
			targetRot += vertInput * riseLean;
			yMaxAccel = riseAccel;
			VerticalState = VState.RISE;
		} else {
			targetVel.y = -sinkSpeed;
			yMaxAccel = sinkAccel;
			VerticalState = VState.SINK;
		}

		if (horizInput != 0) {
			HorizontalState = HState.DRIVE;
			targetVel.x = horizInput * horizSpeed;
			xMaxAccel = horizAccel;
			targetRot += Mathf.Abs(horizInput) * driveLean;
		} else {
			xMaxAccel = horizRestAccel;
			HorizontalState = HState.REST;
		}

		if (targetVel.x != 0) {
			mainSprite.flipX = targetVel.x < 0;
		}

		if (mainSprite.flipX) {
			targetRot = -targetRot;
		}

		Vector2 acceleration = targetVel - velocity;
		Utils.ClampSymmetric(ref acceleration.x, xMaxAccel * Time.fixedDeltaTime);
		Utils.ClampSymmetric(ref acceleration.y, yMaxAccel * Time.fixedDeltaTime);

		velocity += acceleration;
		
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

		// Rotation
		
		rb.MoveRotation(
			Mathf.SmoothDampAngle(rb.rotation, targetRot, ref rotVelocity, rightingTime, rightingSpeed));
	}
}
