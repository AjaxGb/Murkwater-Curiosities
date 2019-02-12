using UnityEngine;

public class CritterBoxFish : CritterBase {

	void Update() {
		transform.position += (Vector3)Random.insideUnitCircle;
	}
}
