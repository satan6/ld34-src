using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class PlayerController : MonoBehaviour {

	public float speed = 50.0f;
	public float tumbleSpeed = 500.0f;
	public float rotSpeed = 10.0f;
	public float daggerForce = 500.0f;
	public GameObject dagger;
	public bool isCalling = false;
	public float callingForce = 10.0f;
	public float biteForce = 100.0f;
	public float damagePunchForce = 200.0f;
	public float bloomIntensity = 3.0f;
	public float bloomSpeed = 5.0f;

	public float blinkCDTotal = 10.0f;
	public Text blinkUI;
	public float jumpCDTotal = 1.0f;
	public Text jumpUI;
	public float biteCDTotal = 2.0f;
	public Text biteUI;
	public Text daggerUI;

	public AudioClip biteSound;
	public AudioClip slurpSound;
	public AudioClip jumpSound;
	public AudioClip blinkSound;
	public AudioClip biteKillSound;
	public AudioClip hurtSound;

	public Image heart1;
	public Image heart2;
	public Image heart3;

	public Sprite emptyHeart;
	public Sprite fullHeart;

	float blinkCD = 0.0f;
	float jumpCD = 0.0f;
	float biteCD = 0.0f;

	Vector2 target;
	Rigidbody2D rb;
	SpriteRenderer sr;
	public Animator anim;
	BloomOptimized bloom;

	public int health = 2;
	public int daggers = 1;
	bool inCast = false;
	bool isBiting = false;
	int darkness = 0;

	public int currentDrops = 0;
	public int maxDrops = 1;
	public Text dropsUI;
	public GameObject door;
	public GameObject exitZone;

	Color colorOnCD = new Color(0.7f, 0.7f, 0.7f);
	Color colorReady = new Color(126.0f/255.0f, 238.0f/255.0f, 140.0f/255.0f);

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		sr = GetComponent<SpriteRenderer> ();
		//anim = GetComponent<Animator> ();
		bloom = Camera.main.GetComponent<BloomOptimized> ();
		GetComponent<TrailRenderer> ().sortingOrder = 2;
		heart1 = GameObject.Find ("Heart1").GetComponent<Image>();
		heart2 = GameObject.Find ("Heart2").GetComponent<Image>();
		heart3 = GameObject.Find ("Heart3").GetComponent<Image>();
		blinkUI = GameObject.Find ("BlinkUI").GetComponent<Text>();
		jumpUI = GameObject.Find ("JumpUI").GetComponent<Text>();
		biteUI = GameObject.Find ("BiteUI").GetComponent<Text>();
		daggerUI = GameObject.Find ("DaggerUI").GetComponent<Text>();
		dropsUI = GameObject.Find ("BloodCount").GetComponent<Text>();
		door = GameObject.Find ("Door");
		exitZone = GameObject.Find("ExitZone");
		exitZone.SetActive (false);
		ChangeHealth (0);
		ChangeDrops (0);
	}

	public bool isInDarkness() {
		return darkness > 0;
	}

	IEnumerator QSpell() {
		inCast = true;
		AudioSource.PlayClipAtPoint (blinkSound, transform.position);
		Vector2 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);

		/*float duration = 0.1f;
		float time = 0.0f;
		Color color = sr.material.color;
		while (time < duration) {
			color.a = 1.0f - time / duration;
			sr.material.color = color;
			time += Time.deltaTime;
			rb.velocity = rb.velocity * 0.9f;
			yield return null;
		}*/

		transform.position = mousePos;

		/*time = 0.0f;
		while (time < duration) {
			color.a = time / duration;
			sr.material.color = color;
			time += Time.deltaTime;
			yield return null;
		}*/

		yield return new WaitForSeconds (0.2f);

		inCast = false;
	}

	IEnumerator BiteSpell() {
		inCast = true;
		isBiting = true;
		anim.SetTrigger ("Bite");
		AudioSource.PlayClipAtPoint (biteSound, transform.position);
		Vector2 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);

		Vector2 pos = transform.position;
		Vector2 dir = mousePos - pos;

		dir.Normalize ();

		Quaternion rot = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90.0f);
		transform.rotation = rot;

		float time = 0;
		float duration = 0.1f;

		while (time < duration) {
			rb.AddForce (dir * biteForce);
			rb.angularVelocity = 0;

			time += Time.fixedDeltaTime;

			yield return new WaitForFixedUpdate();
		}

		rb.velocity *= 0.3f;

		yield return new WaitForSeconds (0.3f);

		isBiting = false;
		inCast = false;
	}

	Vector2 GetInputDirection () {
		Vector2 direction = new Vector2 (0, 0);

		if (Input.GetKey (KeyCode.W))
			direction.y += 1.0f;

		if (Input.GetKey (KeyCode.A))
			direction.x -= 1.0f;

		if (Input.GetKey (KeyCode.S))
			direction.y -= 1.0f;

		if (Input.GetKey (KeyCode.D))
			direction.x += 1.0f;

		return direction.normalized;
	}

	void Update () {
		Vector2 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		Vector2 pos = transform.position;

		Vector2 dir = mousePos - pos;
		dir.Normalize ();

		Quaternion rotTarget = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90.0f);
		transform.rotation = Quaternion.Slerp (transform.rotation, rotTarget, Time.deltaTime * rotSpeed);

		if (!inCast && Input.GetMouseButtonDown (0) && biteCD < 0.0f) {
			biteCD = biteCDTotal;
			StartCoroutine(BiteSpell());
		}

		if (!inCast && Input.GetMouseButtonDown (1) && daggers > 0) {
			daggers--;
			GameObject inst = Instantiate (dagger, transform.position, rotTarget) as GameObject;
			inst.GetComponent<Dagger> ().player = this;
			inst.GetComponent<Rigidbody2D> ().AddForce (dir * daggerForce);
		}

		if (!inCast && Input.GetKeyDown ("q") && blinkCD < 0.0f) {
			Collider2D hit = Physics2D.OverlapPoint(mousePos, 1 << LayerMask.NameToLayer("Light"));

			if (darkness > 0 && hit && hit.gameObject.tag == "Darkness") {
				blinkCD = blinkCDTotal;
				StartCoroutine (QSpell ());
			}
		}

		if (!inCast && Input.GetKeyDown ("space") && jumpCD < 0.0f) {
			jumpCD = jumpCDTotal;
			anim.SetTrigger ("Jump");
			AudioSource.PlayClipAtPoint (jumpSound, transform.position);
			rb.AddForce (dir * tumbleSpeed);
		}

		if (darkness == 0) {
			bloom.intensity = Mathf.Lerp (bloom.intensity, bloomIntensity, Time.deltaTime * bloomSpeed);
		} else {
			bloom.intensity = Mathf.Lerp (bloom.intensity, 0.0f, Time.deltaTime * bloomSpeed);
		}

		blinkCD -= Time.deltaTime;
		jumpCD -= Time.deltaTime; 
		biteCD -= Time.deltaTime;

		if (blinkCD > 0.0f) {
			blinkUI.text = blinkCD.ToString ("F1") + "s";
			blinkUI.color = colorOnCD;
		} else {
			if (darkness == 0) {
				blinkUI.text = "Not in shadow";
				blinkUI.color = colorOnCD;
			} else {
				blinkUI.text = "Ready";
				blinkUI.color = colorReady;
			}
		}
		if (jumpCD > 0.0f) {
			jumpUI.text = jumpCD.ToString ("F1") + "s";
			jumpUI.color = colorOnCD;
		} else {
			jumpUI.text = "Ready";
			jumpUI.color = colorReady;
		}
		if (biteCD > 0.0f) {
			biteUI.text = biteCD.ToString ("F1") + "s";
			biteUI.color = colorOnCD;
		} else {
			biteUI.text = "Ready";
			biteUI.color = colorReady;
		}
		if (daggers == 0) {
			daggerUI.text = "No Dagger";
			daggerUI.color = colorOnCD;
		} else {
			daggerUI.text = "Ready";
			daggerUI.color = colorReady;
		}
	}

	void FixedUpdate() {
		if (!inCast) {
			float currentSpeed = speed;
			if (darkness == 0)
				currentSpeed *= 0.3f;
			rb.AddForce (GetInputDirection() * currentSpeed);
		}

		rb.velocity = rb.velocity * 0.9f;
	}

	void ChangeHealth (int amount) {
		health += amount;
		if (health > 3)
			health = 3;

		if (health >= 1)
			heart1.sprite = fullHeart;
		else
			heart1.sprite = emptyHeart;

		if (health >= 2)
			heart2.sprite = fullHeart;
		else
			heart2.sprite = emptyHeart;

		if (health >= 3)
			heart3.sprite = fullHeart;
		else
			heart3.sprite = emptyHeart;

		if (health <= 0)
			Application.LoadLevel (Application.loadedLevelName);
	}

	void ChangeDrops (int amount) {
		currentDrops += amount;

		dropsUI.text = currentDrops + "/" + maxDrops;

		if (currentDrops == maxDrops) {
			Destroy (door);
			exitZone.SetActive (true);
		}
	}

	void SlurpBlood() {
		biteCD = 0.0f;
		blinkCD = 0.0f;
		jumpCD = 0.0f;
		ChangeHealth (1);
		ChangeDrops (1);
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.tag == "Enemy" && isBiting) {
			AudioSource.PlayClipAtPoint (biteKillSound, transform.position);
			collision.gameObject.SendMessage ("OnKill", (Vector2) transform.position);
		}
	}

	void OnTriggerEnter2D(Collider2D trigger) { 

		if (trigger.gameObject.tag == "Dagger") {
			if (trigger.gameObject.GetComponent<Dagger> ().canBePickedUp) {
				Destroy (trigger.gameObject);
				daggers++;
			}
		}

		if (trigger.gameObject.tag == "Pike") {
			Vector2 fromPos = trigger.gameObject.transform.parent.transform.parent.transform.position;
			Vector2 pos = transform.position;

			Vector2 dir = pos - fromPos;
			dir.Normalize ();

			rb.AddForce (dir * damagePunchForce);

			AudioSource.PlayClipAtPoint (hurtSound, transform.position);

			ChangeHealth (-1);
		}

		if (trigger.gameObject.tag == "Blood") {
			AudioSource.PlayClipAtPoint (slurpSound, transform.position);
			SlurpBlood ();
			Destroy (trigger.gameObject);
		}

		if (trigger.gameObject.tag == "Darkness") {
			darkness++;
		}
	}

	void OnTriggerExit2D(Collider2D trigger) {
		if (trigger.gameObject.tag == "Darkness") {
			darkness--;
		}
	}
}
