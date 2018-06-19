using UnityEngine;
using System.Collections;
using UnityEngine.VR;
using System.IO;
using System;

public class AccelerometerInput4 : MonoBehaviour {
    private float yaw;
    private float rad;
    private float xVal;
    private float zVal;

	public static float velocity = 0f;
	public static float method1StartTimeGrow = 0f;
	public static float method1StartTimeDecay = 0f;
	public static bool wasOne = false; //phase one when above (+/-) 0.105 threshold
	public static bool wasTwo = true; //phase two when b/w -0.105 and 0.105 thresholds


	void Start () {
		Input.gyro.enabled = true;
	}

	void FixedUpdate() //was previously FixedUpdate()
	{
		string path = Application.persistentDataPath + "/CW4Practice_Data.txt";

		// This text is always added, making the file longer over time if it is not deleted
		string appendText = "\n" + DateTime.Now.ToString() + "\t" + 
			Time.time + "\t" + 

			Input.GetMouseButtonDown(0) + "\t" +

			Input.gyro.userAcceleration.x + "\t" + 
			Input.gyro.userAcceleration.y + "\t" + 
			Input.gyro.userAcceleration.z + "\t" + 

			gameObject.transform.position.x + "\t" + 
			gameObject.transform.position.y + "\t" + 
			gameObject.transform.position.z + "\t" +

			InputTracking.GetLocalRotation (VRNode.Head).eulerAngles.x + "\t" +
			InputTracking.GetLocalRotation (VRNode.Head).eulerAngles.y + "\t" +
			InputTracking.GetLocalRotation (VRNode.Head).eulerAngles.z;

		
		File.AppendAllText(path, appendText);

		move ();
	}

	void move ()
	{
		yaw = InputTracking.GetLocalRotation (VRNode.Head).eulerAngles.y;
		rad = yaw * Mathf.Deg2Rad;
		zVal = 0.55f * Mathf.Cos (rad);
		xVal = 0.55f * Mathf.Sin (rad);

		if ((Input.gyro.userAcceleration.y >= 0.105f || Input.gyro.userAcceleration.y <= -0.105f) &&
		    (Input.gyro.userAcceleration.z < 0.08f && Input.gyro.userAcceleration.z > -0.08f)) {
			if (wasTwo) { //we are transitioning from phase 2 to 1
				method1StartTimeGrow = Time.time;
				wasTwo = false;
				wasOne = true;
			}
		} else {
			if (wasOne) {
				method1StartTimeDecay = Time.time;
				wasOne = false;
				wasTwo = true;
			}
		}

		if ((Input.gyro.userAcceleration.y >= 0.105f || Input.gyro.userAcceleration.y <= -0.105f) &&
		    (Input.gyro.userAcceleration.z < 0.08f && Input.gyro.userAcceleration.z > -0.08f)) { //0.08 is an arbitrary threshold

			velocity = 3f - (3f - velocity) * Mathf.Exp ((method1StartTimeGrow - Time.time) / 1.6f); //grow
		} else {

			velocity = 0f - (0f - velocity) * Mathf.Exp ((method1StartTimeDecay - Time.time) / 1.6f); //decay
		}

		transform.Translate (xVal * velocity * Time.fixedDeltaTime, 0, zVal * velocity * Time.fixedDeltaTime); 

	}
}