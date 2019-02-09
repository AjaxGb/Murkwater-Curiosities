using UnityEngine;

[ExecuteInEditMode]
public class WaterParallax : MonoBehaviour {

	public new Camera camera;

	public float cameraYInitial = 0.0f;
	public float waterYInitial = 0.0f;
	public float movementScale = 0.01f;
	
	void LateUpdate() {
		float cameraTopY = camera.ViewportToWorldPoint(Vector2.up).y;

		Vector3 pos = transform.position;
		pos.y = (cameraTopY - cameraYInitial) * movementScale + waterYInitial;
		transform.position = pos;
	}
}
