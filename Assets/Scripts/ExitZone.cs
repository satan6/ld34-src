using UnityEngine;
using System.Collections;

public class ExitZone : MonoBehaviour {

	public string nextLevel;

	void OnTriggerEnter2D(Collider2D trigger) { 

		if (trigger.gameObject.tag == "Player") {
			Application.LoadLevel (nextLevel);
		}

	}
}
