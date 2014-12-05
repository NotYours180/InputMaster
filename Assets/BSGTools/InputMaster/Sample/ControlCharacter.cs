using UnityEngine;
using System.Collections;
using BSGTools.IO;
using BSGTools.IO.Xbox;

public class ControlCharacter : MonoBehaviour {

	[SerializeField]
	float moveSpeed, strafeSpeed, sprintAdditive;

	Rigidbody rb;

	// Use this for initialization
	void Start() {
		rb = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update() {
		var io = InputMaster.instance;

		var moveFB = io.GetControl<XStickControl>(NameList.xMoveFB);
		var strafe = io.GetControl<XStickControl>(NameList.xStrafe);
		var sprint = io.GetControl<XButtonControl>(NameList.xSprint);

		moveFB.currentController = strafe.currentController = sprint.currentController = 0;

		print(sprint.value);

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
