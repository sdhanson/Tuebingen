using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingHills : MonoBehaviour {


	GameObject Small;
	RaycastHit hit;
	float original; 

	// Use this for initialization
	void Start () {
		Small = GameObject.Find ("Small");
		Vector3 dwn = transform.TransformDirection (Vector3.down);
		if (Physics.Raycast (Small.transform.position, dwn, out hit, Mathf.Infinity)) {
			if (hit.collider.tag == "Ground") {
				original = hit.distance;
				//Debug.Log (string.Format ("original: {0}", original));
			}
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 dwn = transform.TransformDirection (Vector3.down);
		if (Physics.Raycast (Small.transform.position, dwn, out hit, Mathf.Infinity)) {
			if (hit.collider.tag == "Ground") {
				Vector3 stmp = Small.transform.position;
				float htmp = hit.distance;
				Debug.DrawRay (Small.transform.position, hit.distance * dwn);
				string name = hit.transform.name;
				//Debug.Log (string.Format("{0}, {1}", htmp, name));
				float change = original - htmp;
				//if (change > 0.1f) {
				//	change = 0f;
				//}
				stmp.y += change;
				Small.transform.position = stmp;
			}
		}
	}
}
