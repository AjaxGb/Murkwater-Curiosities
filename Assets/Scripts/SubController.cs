using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SubController : MonoBehaviour {
	
	[SerializeField] private Rigidbody2D rb;
	[SerializeField] private CinemachineTargetGroup targetGroup;

	[SerializeField] private float riseSpeed = 0.3f;
	[SerializeField] private float diveSpeed = 0.9f;
	[SerializeField] private float sinkSpeed = 0.1f;
	[SerializeField] private float horizSpeed = 0.6f;

	private Vector2 velocity;

	void Start() {
		
	}

	void Update() {
	}
	
	void FixedUpdate() {
		velocity = Vector2.zero;

		float vertInput = Input.GetAxis("Vertical");
		float horizInput = Input.GetAxis("Horizontal");

		if (vertInput < 0) {
			velocity.y = vertInput * diveSpeed;
		} else if (vertInput > 0) {
			velocity.y = vertInput * riseSpeed;
		} else {
			velocity.y = -sinkSpeed;
		}

		velocity.x = horizInput * horizSpeed;

		Debug.Log(velocity);
		rb.MovePosition((Vector2)transform.position + velocity * Time.fixedDeltaTime);
	}
}
