using UnityEngine;

public class EnterBossRoom : MonoBehaviour {

	public CritterCrab boss;

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.GetComponentInParent<SubController>()) {
			boss.Enrage();
			Destroy(gameObject);
		}
	}
}
