using UnityEngine;

public class CritterBoxFish : CritterBase {

	[SerializeField] private Transform spriteTransform;

	public float maxFollowRange = 15;
	public float chaseForce = 5;

	void FixedUpdate() {

		if (!IsAlive) return;

		Vector3 playerPos = SubController.Main.transform.position;

		Vector2 directionToPlayer = playerPos - transform.position;

		if (directionToPlayer.sqrMagnitude < maxFollowRange * maxFollowRange) {
			rb.AddForce(Vector2.ClampMagnitude(directionToPlayer * 2, chaseForce));
		}

		if (rb.velocity.x < 0) spriteTransform.localScale = new Vector3(1, 1, 1);
		else if (rb.velocity.x > 0) spriteTransform.localScale = new Vector3(-1, 1, 1);
	}
}
