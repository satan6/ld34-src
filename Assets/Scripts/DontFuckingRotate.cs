using UnityEngine;
using System.Collections;

public class DontFuckingRotate : MonoBehaviour {
	void LateUpdate()
	{
		transform.rotation = Quaternion.identity;
	}
}
