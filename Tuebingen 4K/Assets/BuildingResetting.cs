using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingResetting : MonoBehaviour {

	public static Vector3 _pos = new Vector3 ();
	public static Vector3 _rot = new Vector3 ();
	public static Vector3 deltaT = new Vector3 ();
	public Vector3 intendedCenter = new Vector3 ();
	public float totalInjectedRotation;

	public string text;

	GameObject feather;
	GameObject featherDestination;	
	GameObject Rot;
	GameObject Small;
	GameObject Canvas;
	GameObject Text;

	//RaycastHit hit;
	//GameObject Ground;

	// Use this for initialization
	void Start () 
	{
		intendedCenter = Camera.main.transform.localPosition;
		feather = GameObject.Find ("The Lead Feather");
		featherDestination = GameObject.Find ("FeatherDestination");
		Rot = GameObject.Find ("Rot");
		Small = GameObject.Find ("Small");
		Canvas = GameObject.Find ("Canvas");
		Text = GameObject.Find ("Text");
		Text.GetComponent<Text> ().text = "";
		Canvas.SetActive (true);
	}

	// Update is called once per frame
	void Update () 
	{
		prevPos = _pos;
		_pos = Camera.main.transform.localPosition;
		_rot = Camera.main.transform.localEulerAngles;
		//Small.transform.position.y = Ground.transform.position.y + 8.9f;
		resettingFSM();
	}
		
	//private Vector3 intendedCenter = new Vector3 (0.7f, 0f, -0.6f);
	private float prevXAngle = 0f;
	private Vector3 prevPos = new Vector3();
	private bool resetNeeded = false;
	private bool hasNotReturnedToBounds = false;
	private float virtualAngleTurned = 0f; //each reset
	private float cumulativeAngleTurned = 0f; //total

	public void resettingFSM()
	{
		//Gather pertinent data
		Vector3 deltaTranslationByFrame = _pos - prevPos;
		float realWorldRotation = Camera.main.transform.localEulerAngles.y;
		float deltaRotationByFrame = realWorldRotation - prevXAngle;
		//if crossed threshold from + to - (1 to 359)
		if (deltaRotationByFrame > 90) {
			deltaRotationByFrame = deltaRotationByFrame - 360;
		}
		//if crossed threshold from - to + (359 to 1)
		else if (deltaRotationByFrame < -90) {
			deltaRotationByFrame = deltaRotationByFrame + 360;
		}

		if (!resetNeeded) {
			deltaT = _pos - prevPos;
			Small.transform.Translate(deltaT.x, 0, deltaT.z, Rot.transform);
		}

		//check to see if a reset is needed (only check if no reset has
		//	been triggered yet, and the subject has returned to inner bounds
		if (!resetNeeded && !hasNotReturnedToBounds && OutOfBounds ()) {
			resetNeeded = true;
			hasNotReturnedToBounds = true;
			virtualAngleTurned = 0f;
			feather.SetActive (true);
			Vector3 featherPosition = new Vector3 (featherDestination.transform.position.x, featherDestination.transform.position.y, featherDestination.transform.position.z);
			feather.transform.position = featherPosition;
			Vector3 featherEuler = new Vector3(90, Camera.main.transform.eulerAngles.y, 0);
			feather.transform.eulerAngles = featherEuler;
		}
		//perform reset by manipulating gain (to do this we will rotate the object in the opposite direction)
		else if (resetNeeded) {
			if (Canvas.activeInHierarchy == false) {
				Canvas.SetActive (true);
			}
			//Calculate the total rotation neccesary
			//added - 180 f
			float calc1 = Mathf.Rad2Deg * Mathf.Atan2 (intendedCenter.x - _pos.x, intendedCenter.z - _pos.z); //+ 180f; //- 180f;
			float calc2 = realWorldRotation;
			if (calc1 > 180) {
				calc1 = calc1 - 360;
			}
			if (calc2 > 180) {
				calc2 = calc2 - 360;
			}
			//fix rotation variables
			float rotationRemainingToCenterL = 0;
			float rotationRemainingToCenterR = 0;
			float rotationRemainingToCenter = 0;
			rotationRemainingToCenterL = calc2 - calc1;
			if (rotationRemainingToCenterL < 0) {
				rotationRemainingToCenterL += 360;
			}
			rotationRemainingToCenterR = 360 - rotationRemainingToCenterL;
			//Debug.Log (string.Format("calc1: {0}, calc2: {1}, rrcL: {2}, rrcR: {3}", calc1, calc2, rotationRemainingToCenterL, rotationRemainingToCenterR));
			if (rotationRemainingToCenter < -360) {
				rotationRemainingToCenter += 360;
			}
			if (rotationRemainingToCenter > 360) {
				rotationRemainingToCenter -= 360;
			}

			//determine gain based on direction subject has rotated already
			//tuned so that at 360 virtual angle turned the person is pointing back to the center
			float gain = 0;
			if (virtualAngleTurned > 0) {
				gain = (360f - virtualAngleTurned) / rotationRemainingToCenterR - 1;
			} else {
				gain = (360f + virtualAngleTurned) / rotationRemainingToCenterL - 1;
			}
			if (gain < 0) {
				gain = Mathf.Abs(gain);
			}
			//inject rotation
			float injectedRotation = (deltaRotationByFrame) * gain;
			virtualAngleTurned += deltaRotationByFrame; //baseline turn
			virtualAngleTurned += injectedRotation; //amount we make them turn as well
			totalInjectedRotation += injectedRotation;
			cumulativeAngleTurned -= injectedRotation; //to keep the person moving in the correct direction
			//Debug.Log(string.Format("vat: {0}, deltarot: {1}, injected: {2}, gain: {3}, rrcL: {4}, rrcR: {5}, posx: {6}, posy: {7}, calc1: {8}, calc2: {9}, tir: {10}", virtualAngleTurned, deltaRotationByFrame, injectedRotation, gain, rotationRemainingToCenterL, rotationRemainingToCenterR, _pos.x, _pos.y, calc1, calc2, totalInjectedRotation));

			//add the injected rotation to the parent object
 			Vector3 tmp = Rot.transform.localEulerAngles;
			tmp.y = totalInjectedRotation;
			Rot.transform.localEulerAngles = tmp;
			//if a full turn has occured then stop resetting
			if (Mathf.Abs (virtualAngleTurned) > 359.9f || ReturnedToBounds()) {
				resetNeeded = false;
				//GUI.SetActive (false);
			}
			Text.GetComponent<Text>().text = "Please turn around";
		} 
		//Subject needs to walk forward two steps to prevent further triggers
		else if (hasNotReturnedToBounds) {
			if (ReturnedToBounds ()) {
				hasNotReturnedToBounds = false;
			}
			Text.GetComponent<Text>().text = "Please walk forward";
			feather.SetActive (false);
			transform.Translate(deltaTranslationByFrame);
		}
		//General Operating
		else {
			Text.GetComponent<Text>().text = "Please go to the destination";
			transform.Translate(deltaTranslationByFrame);
		}
		//update position incrementally using sin and cos
		//float delX = Mathf.Cos(cumulativeAngleTurned * Mathf.Deg2Rad) * deltaTranslationByFrame.x + Mathf.Sin(cumulativeAngleTurned * Mathf.Deg2Rad) * deltaTranslationByFrame.z;
		//float delZ = Mathf.Cos(cumulativeAngleTurned * Mathf.Deg2Rad) * deltaTranslationByFrame.z + Mathf.Sin(cumulativeAngleTurned * Mathf.Deg2Rad) * deltaTranslationByFrame.x;
		//store data for use next frame
		prevPos = _pos;
		prevXAngle = realWorldRotation;
		//myText.text = feather.transform.position.ToString();
	}


	public bool OutOfBounds() {
		if (_pos.x > intendedCenter.x + 0.94f)
			return true;
		if (_pos.x < intendedCenter.x - 2.2f)
			return true;
		if (_pos.z > intendedCenter.z + 1.65f)
			return true;
		if (_pos.z < intendedCenter.z - 1.26f)
			return true;
		return false;
	}

	public bool ReturnedToBounds() {
		if (_pos.x > intendedCenter.x + 0.64f)
			return false;
		if (_pos.x < intendedCenter.x - 1.9f)
			return false;
		if (_pos.z > intendedCenter.z + 1.35f)
			return false;
		if (_pos.z < intendedCenter.z - 0.96f)
			return false;
		return true;
	}
}