/// <summary>
/// Used for all Keyboard and Mouse Button controls.
/// Any binding present in Unity's KeyCode enumeration is valid here.
/// </summary>
using System;
using BSGTools.IO;
using UnityEngine;
[Serializable]
public sealed class StandaloneControl : Control {
	/// <value>
	/// The REQUIRED positive binding for this control.
	/// CANNOT BE KeyCode.None!
	/// </value>
	public KeyCode positive;

	/// <value>
	/// The OPTIONAL negative binding for this control.
	/// </value>
	public KeyCode negative;

	/// <value>
	/// The OPTIONAL modifier key for this control.
	/// </value>
	public ModifierKey modifier;



	/// <summary>
	/// Creates a new KeyControl.
	/// This is the "new version" of OneWayControl from previous versions of InputMaster.
	/// </summary>
	/// <param name="positive"><see cref="positive"/></param>
	public StandaloneControl(KeyCode positive)
		: base() {
		if(positive == KeyCode.None)
			throw new ArgumentException("Positive must != KeyCode.None");

		this.positive = positive;
		this.negative = KeyCode.None;
		this.modifier = ModifierKey.None;
	}

	/// <summary>
	/// Creates a new KeyControl with a negative binding.
	/// This is the "new version" of AxisControl from previous versions of InputMaster.
	/// </summary>
	/// <param name="positive"><see cref="positive"/></param>
	/// <param name="negative"><see cref="negative"/></param>
	public StandaloneControl(KeyCode positive, KeyCode negative)
		: this(positive) {
		this.negative = negative;
	}

	/// <summary>
	/// Updates the current states of this control.
	/// </summary>
	protected override void UpdateStates() {
		var pos = (invert) ? negative : positive;
		var neg = (invert) ? positive : negative;

		var hasMod = modifier != null && modifier != ModifierKey.None;
		var modPass = !hasMod;
		if(hasMod)
			modPass = Input.GetKeyDown(modifier) || Input.GetKey(modifier);

		if(modPass == false) {
			if(realValue > 0f)
				Up |= ControlState.Positive;
			else if(realValue < 0f)
				Up |= ControlState.Negative;

			return;
		}

		if(Input.GetKeyDown(pos))
			Down |= ControlState.Positive;
		if(Input.GetKey(pos))
			Held |= ControlState.Positive;
		if(Input.GetKeyUp(pos))
			Up |= ControlState.Positive;

		if(negative != KeyCode.None) {
			if(Input.GetKey(neg))
				Held |= ControlState.Negative;
			if(Input.GetKeyUp(neg))
				Up |= ControlState.Negative;
			if(Input.GetKeyDown(neg))
				Down |= ControlState.Negative;
		}
	}
}