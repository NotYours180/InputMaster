using UnityEngine;
using System.Collections;
using BSGTools.IO;
using BSGTools.IO.Xbox;

public class ControlCharacter : MonoBehaviour {

	[SerializeField]
	float moveSpeed, strafeSpeed, sprintAdditive;
	[SerializeField, Range(0, 3)]
	byte controller = 0;

	Rigidbody rb;

	// Use this for initialization
	void Start() {
		rb = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update() {
		var io = InputMaster.instance;

		var moveFB = io.GetCombinedOutput(NameList.coMoveFB);
		var strafe = io.GetCombinedOutput(NameList.coStrafe);
		var sprint = io.GetCombinedOutput(NameList.coSprint);

		moveFB.controllerIndex = strafe.controllerIndex = sprint.controllerIndex = controller;

		var moveVal = moveFB.value * moveSpeed;
		if(moveVal > 0f)
			moveVal += sprintAdditive * sprint.value;
		else if(moveVal < 0f)
			moveVal -= sprintAdditive * sprint.value;

		var strafeVal = strafe.value * strafeSpeed;
		if(strafeVal > 0f)
			strafeVal += sprintAdditive * sprint.value;
		else if(strafeVal < 0f)
			strafeVal -= sprintAdditive * sprint.value;

		rb.velocity = new Vector3(strafeVal, rb.velocity.y, moveVal);
	}
}
