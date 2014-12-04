using UnityEngine;
using System.Collections;
using BSGTools.IO;

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
		var moveFB = io.GetControl<StandaloneControl>(NameList.moveFB);
		var strafe = io.GetControl<StandaloneControl>(NameList.strafe);
		var sprint = io.GetControl<StandaloneControl>(NameList.sprint);
		var printPosition = io.GetControl<StandaloneControl>(NameList.printPosition);

		//print(strafe.value);
		//print(moveFB.value);
		var moveVal = moveFB.value * moveSpeed;
		if(moveVal > 0f)
			moveVal += sprintAdditive * sprint.value;
		else if(moveVal < 0f)
			moveVal -= sprintAdditive * sprint.value;

		rb.velocity = new Vector3(strafe.value * strafeSpeed, rb.velocity.y, moveVal);

		if(printPosition.down == ControlState.Positive)
			print(transform.position);
	}
}
