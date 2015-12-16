using UnityEngine;
using System.Collections;

public class Dagger : MonoBehaviour {

	public PlayerController player;
	public bool canBePickedUp = false;
	public bool onGround = false;

	Rigidbody2D rb;

	IEnumerator Flight() {
		yield return new WaitForSeconds (0.5f);
		canBePickedUp = true;
	}

	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		StartCoroutine (Flight ());
	}

	void Update () {
		if (rb.velocity.magnitude > 0.1f) {
			Quaternion rotTarget = Quaternion.Euler (0.0f, 0.0f, Mathf.Atan2 (rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90.0f);
			transform.rotation = rotTarget;
			onGround = false;
		} else
			onGround = true;
	}

	void FixedUpdate () {
		rb.velocity = rb.velocity * 0.9f;
	}

	void OnTriggerEnter2D(Collider2D trigger) {
		if (!onGround && !trigger.isTrigger && (trigger.gameObject.tag == "Enemy" || trigger.gameObject.tag == "Wall")) {
			rb.velocity = Vector2.zero;
			onGround = true;

			trigger.gameObject.SendMessage("OnKill", (Vector2) transform.position, SendMessageOptions.DontRequireReceiver);
		}
	}
}
