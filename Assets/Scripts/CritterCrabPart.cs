using UnityEngine;

public class CritterCrabPart : MonoBehaviour {

	public float damage = 10;

	void OnTriggerEnter2D(Collider2D collider) {
		SubController player = collider.GetComponentInParent<SubController>();
		if (player) {
			player.CurrHealth -= damage;
		}
	}
}
