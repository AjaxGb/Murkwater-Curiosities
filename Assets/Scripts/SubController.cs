using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Rigidbody2D))]
public class SubController : MonoBehaviour {

	public enum VState { SINK, RISE, DIVE };
	public enum HState { DRIVE, REST };
	
	[SerializeField] private Rigidbody2D rb;
	[SerializeField] private SpriteRenderer mainSprite;
	[Space]
	public float waterLevel = 0.5f;
	[SerializeField] private float floatMidpoint = 0.5f;
	[SerializeField] private float floatRadius = 1.5f;
	[SerializeField] private float bouyancyRadiusBonus = 1.0f;
	[SerializeField] private float maxBouyancy = 1.5f;
	[SerializeField] private float outOfWaterGravity = 9.8f;
	[SerializeField] private AnimationCurve gravityStrengthCurve;
	[SerializeField] private AnimationCurve steeringStrengthCurve;
	[SerializeField] private AnimationCurve bouyancyStrengthCurve;
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

	private Vector2 lastPosition;
	private Vector2 velocity;
	private float rotVelocity;

	void Start() {
		lastPosition = rb.position;
	}

	void Update() {
	}
	
	void FixedUpdate() {

		Vector2 currPosition = rb.position;
		Vector2 realVelocity = (currPosition - lastPosition) / Time.fixedDeltaTime;
		Utils.ClampSymmetric(ref velocity.x, Mathf.Abs(realVelocity.x * 1.1f));
		Utils.ClampSymmetric(ref velocity.y, Mathf.Abs(realVelocity.y * 1.1f));

		Vector2 targetVel = Vector2.zero;
		float targetRot = 0.0f;

		float inWaterPercent;
		float outOfWaterPercent;
		float atSurfacePercent;
		{
			float floatMidpoint = currPosition.y + this.floatMidpoint;
			float floatBottom = floatMidpoint - floatRadius;
			float floatTop = floatMidpoint + floatRadius;
			inWaterPercent = Mathf.InverseLerp(floatBottom, floatTop, waterLevel);
			outOfWaterPercent = 1 - inWaterPercent;

			if (inWaterPercent < 0.5f) {
				atSurfacePercent = 1 - (0.5f - inWaterPercent) * 2;
			} else {
				atSurfacePercent = Mathf.InverseLerp(floatTop + bouyancyRadiusBonus, floatMidpoint, waterLevel);
			}

#if UNITY_EDITOR
			Debug.DrawRay(new Vector2(currPosition.x - 2, waterLevel), Vector2.right * 4, Color.blue);
			Debug.DrawLine(new Vector2(currPosition.x, floatBottom), new Vector2(currPosition.x, floatTop), Color.red);
			Debug.DrawRay(new Vector2(currPosition.x, floatTop), Vector2.up * bouyancyRadiusBonus, Color.green);
			Debug.DrawRay(new Vector2(currPosition.x - 0.1f, floatMidpoint), Vector2.right * 0.2f, Color.red);
#endif
		}

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
		velocity += acceleration * steeringStrengthCurve.Evaluate(inWaterPercent);

		if (VerticalState != VState.DIVE) {
			Vector2 bouyancy = Vector2.up * maxBouyancy * Time.fixedDeltaTime;
			velocity += bouyancy * bouyancyStrengthCurve.Evaluate(atSurfacePercent);
		}

		Vector2 gravity = Vector2.down * outOfWaterGravity * Time.fixedDeltaTime;
		velocity += gravity * gravityStrengthCurve.Evaluate(outOfWaterPercent);

		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

		// Rotation
		
		rb.MoveRotation(
			Mathf.SmoothDampAngle(rb.rotation, targetRot, ref rotVelocity, rightingTime, rightingSpeed));

		lastPosition = currPosition;
	}
}
