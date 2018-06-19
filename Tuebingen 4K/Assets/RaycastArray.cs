using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastArray : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit[] hits;
		hits = Physics.RaycastAll (transform.position, transform.TransformDirection (Vector3.down), 100.0F);

		for (int i = 0; i < hits.Length; i++) {
			RaycastHit hit = hits [i];
			Debug.Log (hit.distance);
		}
		Debug.Log ("Raycasting finsished");
	}
}
