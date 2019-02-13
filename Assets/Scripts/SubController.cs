using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[SelectionBase]
[RequireComponent(typeof(Rigidbody2D))]
public class SubController : MonoBehaviour {

	public static SubController Main;

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
	[SerializeField] private float speedBoost = 2.0f;
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

	public float laserCurrTimeout = 0;
	public float laserMaxTimeout = 2;
	public float sonicCurrTimeout = 0;
	public float sonicMaxTimeout = 10;
	public float maxLaserDist = 100;
	public LayerMask laserLayers;

	public RectTransform laserTimeoutOverlay;
	public RectTransform sonicTimeoutOverlay;

	public SpriteRenderer laserPrefab;
	public float laserRenderDuration = 0.2f;
	public float laserDamage = 5;

	public ParticleSystem sonicParticles;
	public float sonicRadius = 5;

	public RectTransform healthDisplay;
	public TextMeshProUGUI moneyDisplay;

	public ParticleSystem bubbleParticles;
	public float bubbleSpeedScale = 5;

	[SerializeField] private float maxHealth = 100;
	private float _currHealth;
	public float CurrHealth {
		get => _currHealth;
		set {
			_currHealth = Mathf.Clamp(value, 0, maxHealth);
			healthDisplay.localScale = new Vector2(1, _currHealth / maxHealth);

			if (_currHealth <= 0) {
				Time.timeScale = 0;
				SceneManager.LoadSceneAsync(gameObject.scene.buildIndex, LoadSceneMode.Single);
			}
		}
	}

	private int _currMoney;
	public int CurrMoney {
		get => _currMoney;
		set {
			_currMoney = value;

			moneyDisplay.text = _currMoney.ToString("$00000");
		}
	}

	void Start() {
		if (!Main) Main = this;
		lastPosition = rb.position;

		CurrHealth = maxHealth;
		CurrMoney = 0;
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}

		if (Time.timeScale == 0) return;

		if (laserCurrTimeout > Time.deltaTime) {
			laserCurrTimeout -= Time.deltaTime;
		} else {
			laserCurrTimeout = 0;
		}

		if (sonicCurrTimeout > Time.deltaTime) {
			sonicCurrTimeout -= Time.deltaTime;
		} else {
			sonicCurrTimeout = 0;
		}

		Vector2 currPos = transform.position;

		if (laserCurrTimeout <= 0 && Input.GetMouseButtonDown(0)) {
			Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast(transform.position, target - currPos, maxLaserDist, laserLayers);
			Vector2 endPos = hit ? hit.point : currPos + (target - currPos).normalized * maxLaserDist;

			Vector2 length = endPos - currPos;
			SpriteRenderer laser = Instantiate(laserPrefab, currPos, Quaternion.FromToRotation(Vector2.right, length));
			Vector2 size = laser.size;
			size.x = length.magnitude;
			laser.size = size;
			Destroy(laser.gameObject, laserRenderDuration);

			if (hit) {
				CritterBase critter = hit.transform.GetComponentInParent<CritterBase>();
				if (critter) {
					critter.Hurt(laserDamage);
				}
			}

			laserCurrTimeout = laserMaxTimeout;
		}

		if (sonicCurrTimeout <= 0 && Input.GetMouseButtonDown(1)) {

			sonicParticles.Play();

			foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, sonicRadius)) {
				CritterBase critter = collider.GetComponentInParent<CritterBase>();
				if (!critter) continue;
				critter.Stun();
			}

			laserCurrTimeout = sonicMaxTimeout;
			sonicCurrTimeout = sonicMaxTimeout;
		}

		laserTimeoutOverlay.localScale = new Vector2(1, Mathf.Clamp01(laserCurrTimeout / laserMaxTimeout));
		sonicTimeoutOverlay.localScale = new Vector2(1, Mathf.Clamp01(sonicCurrTimeout / sonicMaxTimeout));
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

		float modifier = Input.GetKey(KeyCode.LeftShift) ? speedBoost : 1;

		float xMaxAccel, yMaxAccel;

		float vertInput = Input.GetAxis("Vertical");
		float horizInput = Input.GetAxis("Horizontal");
		
		if (vertInput < 0) {
			targetVel.y = vertInput * diveSpeed * modifier;
			targetRot += -vertInput * diveLean;
			yMaxAccel = diveAccel * modifier;
			VerticalState = VState.DIVE;
		} else if (vertInput > 0) {
			targetVel.y = vertInput * riseSpeed * modifier;
			targetRot += vertInput * riseLean;
			yMaxAccel = riseAccel * modifier;
			VerticalState = VState.RISE;
		} else {
			targetVel.y = -sinkSpeed;
			yMaxAccel = sinkAccel;
			VerticalState = VState.SINK;
		}

		if (horizInput != 0) {
			HorizontalState = HState.DRIVE;
			targetVel.x = horizInput * horizSpeed * modifier;
			xMaxAccel = horizAccel * modifier;
			targetRot += Mathf.Abs(horizInput) * driveLean;
		} else {
			xMaxAccel = horizRestAccel;
			HorizontalState = HState.REST;
		}

		if (targetVel.x != 0) {
			mainSprite.transform.localScale = new Vector3(targetVel.x < 0 ? -1 : 1, 1, 1);
		}

		if (mainSprite.transform.localScale.x < 0) {
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

		float bubbleMultiplier = Mathf.Max(1, velocity.sqrMagnitude / bubbleSpeedScale);

		var bubbleEmission = bubbleParticles.emission;
		bubbleEmission.rateOverTimeMultiplier = bubbleMultiplier;
		bubbleEmission.rateOverDistanceMultiplier = bubbleMultiplier;

		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

		// Rotation
		
		rb.MoveRotation(
			Mathf.SmoothDampAngle(rb.rotation, targetRot, ref rotVelocity, rightingTime, rightingSpeed));

		lastPosition = currPosition;
	}
}
