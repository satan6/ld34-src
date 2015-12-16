using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public Transform follow;
	public float smooth = 0.1f;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void LateUpdate () {
		Vector2 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		Vector2 target = Vector2.Lerp (follow.position, mousePos, 0.2f);
		Vector3 pos = Vector3.Lerp (transform.position, (Vector2) target, smooth);
		pos.z = transform.position.z;
		transform.position = pos;
	}
}
