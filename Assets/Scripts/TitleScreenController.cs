using UnityEngine;
using System.Collections;

public class TitleScreenController : MonoBehaviour {

	public void OnClickPlay () {
		Application.LoadLevel ("Tutorial");
	}
	
	public void OnClickExit () {
		Application.Quit ();
	}
}
