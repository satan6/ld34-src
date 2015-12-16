using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {
	enum State {IDLE, HUNTING, CONFUSED};

	public float speed = 10.0f;
	public float rotSpeed = 10.0f;
	public float lungeForce = 30.0f;
	public float darknessVisibleRange = 4.0f;

	public GameObject blood;
	public GameObject player;
	public GameObject pike;
	public GameObject alertIndicator;
	public GameObject confusedIndicator;
	public GameObject bloodSplatter;
	public GameObject corpse;
	public AudioClip lungeSound;

	State state = State.IDLE;
	bool inCast = false;
	Quaternion rotTarget;
	bool move = false;
	Vector2 moveTarget;
	Coroutine routine;
	Rigidbody2D rb;

	bool isInDarkness(Vector2 pos) {
		Collider2D hit = Physics2D.OverlapPoint(pos, 1 << LayerMask.NameToLayer("Light"));

		return hit && hit.gameObject.tag == "Darkness";
	}

	IEnumerator Lunge(Vector2 target) {
		inCast = true;
		yield return new WaitForSeconds (0.3f);
		AudioSource.PlayClipAtPoint (lungeSound, transform.position);

		Vector2 pos = transform.position;
		Vector2 dir = target - pos;

		dir.Normalize ();

		Quaternion rot = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90.0f);
		transform.rotation = rot;

		float time = 0;
		float duration = 0.3f;

		while (time < duration) {
			pike.transform.localPosition = new Vector3(0, time / duration * 0.8f, 0);
			rb.AddForce (dir * lungeForce);
			rb.angularVelocity = 0;

			time += Time.fixedDeltaTime;

			yield return new WaitForFixedUpdate();
		}

		time = 0;
		duration = 0.5f;
		while (time < duration) {
			pike.transform.localPosition = new Vector3(0, 0.8f - time / duration * 0.8f, 0);
			rb.velocity = rb.velocity * 0.8f;

			time += Time.fixedDeltaTime;

			yield return new WaitForFixedUpdate();
		}

		yield return new WaitForSeconds (0.5f);

		inCast = false;
	}

	IEnumerator IdleState () {
		state = State.IDLE;

		speed = 5.0f;
		rotSpeed = 2.0f;
		move = false;

		alertIndicator.SetActive(false);
		confusedIndicator.SetActive(false);

		while (true) {
			rotSpeed = 2.0f;

			float rot = transform.rotation.eulerAngles.z;
			yield return new WaitForSeconds (0.5f);
			rotTarget = Quaternion.Euler (0.0f, 0.0f, rot + 70.0f);
			yield return new WaitForSeconds (2.0f);
			rotTarget = Quaternion.Euler (0.0f, 0.0f, rot - 70.0f);
			yield return new WaitForSeconds (2.5f);

			float moveSampleRange = 2.0f;
			Vector2 target = new Vector2 ();

			int attempts = 0;
			while (attempts++ < 30) {
				target = transform.position;
				target.x += Random.Range (-moveSampleRange, moveSampleRange);
				target.y += Random.Range (-moveSampleRange, moveSampleRange);

				if (Physics2D.Linecast (transform.position, target, 1 << LayerMask.NameToLayer ("Walls")) != null)
					break;
			}
			rotSpeed = 3.0f;
			Vector2 dir = (target - (Vector2)transform.position).normalized;
			rotTarget = Quaternion.Euler (0.0f, 0.0f, Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg - 90.0f);

			yield return new WaitForSeconds (0.5f);
			move = true;
			moveTarget = target;

			while(move)
				yield return new WaitForSeconds (0.5f);
		}
	}

	IEnumerator HuntState () {
		state = State.HUNTING;

		speed = 10.0f;
		rotSpeed = 7.0f;
		move = false;

		alertIndicator.SetActive(true);
		confusedIndicator.SetActive(false);

		GameObject target = player;

		while (player) {

			Vector2 pos = transform.position;
			Vector2 playerPos = target.transform.position;

			Vector2 dir = playerPos - pos;
			dir.Normalize ();

			Vector2 predPos = playerPos;// + target.GetComponent<Rigidbody2D>().velocity * 0.2f;
				
			if (Vector2.Distance (pos, predPos) < 3.0f) {
				StartCoroutine (Lunge (predPos));
				while(inCast)
					yield return new WaitForFixedUpdate ();

				float time = 0;
				float duration = 0.2f;

				while (time < duration) {
					pos = transform.position;
					playerPos = target.transform.position;

					dir = playerPos - pos;
					dir.Normalize ();

					rotTarget = Quaternion.Euler (0.0f, 0.0f, Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg - 90.0f);

					time += Time.fixedDeltaTime;

					yield return new WaitForFixedUpdate();
				}
				
			} else {
				move = true;
				moveTarget = predPos;
				rotTarget = Quaternion.Euler (0.0f, 0.0f, Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg - 90.0f);
			}

			yield return new WaitForFixedUpdate ();
		}

		routine = StartCoroutine (ConfusedState ());
	}

	IEnumerator ConfusedState () {
		state = State.CONFUSED;

		speed = 5.0f;
		rotSpeed = 7.0f;
		move = false;

		alertIndicator.SetActive(false);
		confusedIndicator.SetActive(true);

		for (int i = 0; i < 10; i++) {
			
			for (int j = 0; j < 3; j++) {
				rotTarget = Quaternion.Euler (0.0f, 0.0f, Random.Range(0.0f, 360.0f));
				yield return new WaitForSeconds (0.8f);
			}

			float moveSampleRange = 1.0f;
			Vector2 target = new Vector2 ();

			int attempts = 0;
			while (attempts++ < 30) {
				target = transform.position;
				target.x += Random.Range (-moveSampleRange, moveSampleRange);
				target.y += Random.Range (-moveSampleRange, moveSampleRange);

				if (Physics2D.Linecast (transform.position, target, 1 << LayerMask.NameToLayer ("Walls")) != null)
					break;
			}
			Vector2 dir = (target - (Vector2)transform.position).normalized;
			rotTarget = Quaternion.Euler (0.0f, 0.0f, Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg - 90.0f);

			yield return new WaitForSeconds (0.1f);
			move = true;
			moveTarget = target;

			while(move)
				yield return new WaitForSeconds (0.1f);
		}

		routine = StartCoroutine (IdleState ());
	}

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		routine = StartCoroutine (IdleState ());
		//StartCoroutine(Lunge(new Vector2(0.0f, 10.0f)));
	}

	void Update () {
		if(!inCast)
			transform.rotation = Quaternion.Slerp (transform.rotation, rotTarget, Time.deltaTime * rotSpeed);
	}

	void FixedUpdate() {
		if (move) {
			Vector2 pos = transform.position;

			Vector2 dir = moveTarget - pos;
			if (dir.magnitude < 0.1f) {
				move = false;
			} else {
				dir.Normalize ();
				rb.AddForce (dir * speed);
			}
		}

		rb.velocity = rb.velocity * 0.9f;
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, moveTarget);
	}

	void OnKill(Vector2 src) {
		Vector2 pos = transform.position;
		Vector2 dir = src - pos;
		dir.Normalize ();
		Quaternion rot = Quaternion.Euler (0.0f, 0.0f, Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg - 90.0f);
		GameObject splatter = Instantiate (bloodSplatter, transform.position, rot) as GameObject;
		Destroy (splatter, 5.0f);
		Instantiate (corpse, transform.position, rot);
		Instantiate (blood, transform.position, Quaternion.identity);

		Destroy (this.gameObject);
	}

	void OnCollisionStay2D(Collision2D trigger) {
		if (trigger.gameObject.tag == "Wall") {
			move = false;
		}
	}

	void OnTriggerStay2D(Collider2D trigger) {
		if (trigger.gameObject.tag == "Player") {
			if (trigger.gameObject.GetComponent<PlayerController> ().isInDarkness ()) {
				if (Vector2.Distance (transform.position, trigger.gameObject.transform.position) > darknessVisibleRange) {
					player = null;
					return;
				}
			}

			player = trigger.gameObject;

			if (state != State.HUNTING) {
				StopCoroutine (routine);
				routine = StartCoroutine (HuntState ());
			}
		}
	}

	void OnTriggerExit2D(Collider2D trigger) {
		if (trigger.gameObject.tag == "Player") {
			player = null;
		}
	}
}
