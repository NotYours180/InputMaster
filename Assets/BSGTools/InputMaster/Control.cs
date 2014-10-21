/**
Copyright (c) 2014, Michael Notarnicola
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Text;
using UnityEngine;

namespace BSGTools.IO {

	/// <summary>
	/// Describes the current state of a control's specific state.
	/// Use (control.Down/Held/Up & flag) != 0 for Positive & Negative.
	/// Use == for Neither, Both, and Either.
	/// </summary>
	[Flags]
	public enum ControlState {
		Positive = 1 << 0,
		Negative = 1 << 1,
		Neither = Positive & Negative,
		Both = Positive | Negative,
		Either = Positive ^ Negative
	}

	/// <summary>
	/// Provided to give a common base class and common functionality for <see cref="KeyControl"/> and <see cref="XboxControl"/>.
	/// <para />
	/// Controls should be instantiated as parameters in a custom MonoBehaviour using block initialization. See example below.
	/// <para />
	/// </summary>
	/// <strong>Declaration Example</strong>
	/// <code>
	///	<pre>
	///		KeyControl use = new KeyControl(KeyCode.E) {
	///			Name = "Jump",
	///			Gravity = 2f,
	///			Sensitivity = 2f
	///		});
	/// </pre>
	/// </code>
	[Serializable]
	public abstract class Control {

		/// <value>
		/// The current "down" state of the control.
		/// </value>
		public ControlState Down { get; set; }

		/// <value>
		/// The current "held" state of the control.
		/// </value>
		public ControlState Held { get; set; }

		/// <value>
		/// The current "up" state of the control.
		/// </value>
		public ControlState Up { get; set; }

		/// <value>
		/// Functionally identical to the Dead property of Unity's native Input system.
		/// The absolute value of a control's real value reports as 0 if it's less than this value.
		/// </value>
		public float Dead { get; set; }

		/// <value>
		/// Functionally identical to the Gravity property of Unity's native Input system.
		/// Speed per second that a control at rest returns to 0.
		/// </value>
		public float Gravity { get; set; }

		/// <value>
		/// Functionally identical to the Sensitivity property of Unity's native Input system.
		/// Speed per second that a control in motion approaches 1.
		/// </value>
		public float Sensitivity { get; set; }

		/// <value>
		/// Returns an analog representation of the current real value.
		/// This is functionally identical to calling Input.GetAxis() from Unity's native Input system.
		/// </value>
		public float Value { get; protected set; }

		/// <value>
		/// Returns a digital, ceiling-rounded representation of <see cref="Value"/>.
		/// This is functionally identical to calling Input.GetAxisRaw() from Unity's native Input system.
		/// </value>
		public sbyte FixedValue { get; protected set; }

		/// <value>
		/// Functionally identical to the Invert property of Unity's native Input system.
		/// If true, the contol's value will report as -(value).
		/// However, the state of the control will remain the same (positive down will still report as positive down, etc).
		/// Keep in mind that this functions whether or not a negative binding is supplied.
		/// </value>
		public bool Invert { get; set; }

		/// <value>
		/// This is used to block any control from receiving updates.
		/// Keep in mind that if you block a control, it maintains its values from it's most recent update.
		/// If you want to block and reset a control, you can use the <see cref="Reset(bool)"/> function.
		/// </value>
		public bool IsBlocked { get; set; }

		/// <value>
		/// Used to specify controls that automatically
		/// only work in the Editor or in Debug builds.
		/// </value>
		public bool IsDebugControl { get; set; }

		/// <value>
		/// Can be used as a display name or for string metadata.
		/// </value>
		public string Name { get; set; }

		/// <value>
		/// Functionally identical to the Snap property of Unity's native Input system.
		/// If true, and if the control has a positive and negative binding, the control's value will snap to 0 if provided an opposite input.
		/// </value>
		public bool Snap { get; set; }

		/// <value>
		/// An internally used property that keeps track of the "real value" across updates.
		/// This is necessary so that properties like <see cref="Dead"/> can be applied to the final value.
		/// Think of this as the "real value" and the <see cref="Value"/> property as this value after post processing.
		/// This value is not necessary to use for input.
		/// </value>
		protected float RealValue { get; set; }

		protected Control() {
			Sensitivity = 1f;
			Gravity = 1f;
			Dead = 0f;

			if(string.IsNullOrEmpty(Name))
				Name = GetRandomName();
		}

		/// <summary>
		/// Reset all non-configuration values and states for this control.
		/// </summary>
		/// <seealso cref="Reset(bool)"/>
		public void Reset() {
			Down = ControlState.Neither;
			Up = ControlState.Neither;
			Held = ControlState.Neither;
			FixedValue = 0;
			Value = 0f;
			RealValue = 0f;
		}

		/// <summary>
		/// Reset all non-configuration values for this control. This is the best method to use for cutscenes.
		/// </summary>
		/// <param name="block">Whether or not to block this input after resetting.</param>
		/// <seealso cref="Reset"/>
		///	<seealso cref="IsBlocked"/>
		public void Reset(bool block) {
			Reset();
			IsBlocked = block;
		}

		/// <summary>
		/// Returns debug information in a single line.
		/// </summary>
		/// <returns>The debug information.</returns>
		public override string ToString() {
			return ToStringBlock().Replace(Environment.NewLine, " ");
		}

		/// <summary>
		/// Returns debug information as a formatted string block.
		/// </summary>
		/// <returns>The debug information.</returns>
		public virtual string ToStringBlock() {
			var sb = new StringBuilder();

			sb.AppendLine(Name);
			sb.AppendLine(string.Format("D: {0}", Down));
			sb.AppendLine(string.Format("H: {0}", Held));
			sb.AppendLine(string.Format("U: {0}", Up));

			sb.AppendLine(string.Format("RV: {0}", RealValue));
			sb.AppendLine(string.Format("FV: {0}", FixedValue));
			sb.AppendLine(string.Format("V: {0}", Value));

			sb.AppendLine(string.Format("B: {0}", IsBlocked));
			sb.AppendLine(string.Format("I: {0}", Invert));
			sb.AppendLine(string.Format("Sn: {0}", Snap));

			sb.AppendLine(string.Format("G: {0}", Gravity));
			sb.AppendLine(string.Format("Se: {0}", Sensitivity));
			sb.AppendLine(string.Format("D: {0}", Dead));

			return sb.ToString().Trim();
		}

		/// <summary>
		/// Updates the control.
		/// This should never be used by any user-made script.
		/// This is public specifically for the use of <see cref="InputMaster"/>.
		/// </summary>
		public void Update() {
			SoftReset();
			if(IsBlocked == false)
				UpdateStates();
			UpdateValues();
		}

		/// <summary>
		/// Internally used for maintaining the <see cref="RealValue"/> inbetween updates while resetting everything else.
		/// </summary>
		protected void SoftReset() {
			var realVal = RealValue;
			Reset();
			RealValue = realVal;
		}

		/// <summary>
		/// Internally used for updating the Up/Held/Down states of a control.
		/// </summary>
		protected abstract void UpdateStates();

		/// <summary>
		/// Internally used for updating a control's values using it's current states.
		/// </summary>
		protected virtual void UpdateValues() {
			if(Held.HasFlag(ControlState.Positive))
				RealValue += Time.deltaTime * Sensitivity;
			else if(RealValue > 0f) {
				RealValue -= Time.deltaTime * Sensitivity;
				if(RealValue < 0f)
					RealValue = 0f;
			}
			if(Held.HasFlag(ControlState.Negative))
				RealValue -= Time.deltaTime * Sensitivity;
			else if(RealValue < 0f) {
				RealValue += Time.deltaTime * Sensitivity;
				if(RealValue > 0f)
					RealValue = 0f;
			}

			RealValue = Mathf.Clamp(RealValue, -1f, 1f);

			if(Held != ControlState.Both && Snap) {
				if(RealValue > 0f && Held.HasFlag(ControlState.Negative))
					RealValue = 0f;
				else if(RealValue < 0f && Held.HasFlag(ControlState.Positive))
					RealValue = 0f;
			}

			Value = RealValue;

			//We dont want to mess with the real value
			//When considering dead values
			if(Mathf.Abs(RealValue) <= Mathf.Abs(Dead))
				Value = 0f;

			FixedValue = RoundFixed(Value);
		}

		/// <summary>
		/// Internally used to give a random name to a control if one is not provided.
		/// </summary>
		/// <returns>A new random string.</returns>
		private static string GetRandomName() {
			return "UNNAMED_" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
		}

		/// <summary>
		/// Returns a fixed control range float.
		/// </summary>
		/// <param name="f">A float to clamp.</param>
		/// <returns>A float fixed to -1...1</returns>
		public static float ClampRange(float f) {
			return Mathf.Clamp(f, -1f, 1f);
		}

		/// <summary>
		/// Rounds and clamps to a FixedValue sbyte.
		/// </summary>
		/// <param name="f">The value to round and clamp.</param>
		/// <returns>-1, 1 or 0</returns>
		public static sbyte RoundFixed(float f) {
			if(f < 0f)
				return -1;
			if(f > 0f)
				return 1;
			else
				return 0;
		}

		/// <summary>
		/// Rounds and clamps to a FixedValue float.
		/// </summary>
		/// <param name="f">The value to round and clamp.</param>
		/// <returns>-1, 1 or 0</returns>
		public static float RoundFixedF(float f) {
			if(f < 0f)
				return -1f;
			if(f > 0f)
				return 1f;
			else
				return 0f;
		}
	}

	/// <summary>
	/// Used for all Keyboard and Mouse Button controls.
	/// Any binding present in Unity's KeyCode enumeration is valid here.
	/// </summary>
	[Serializable]
	public sealed class KeyControl : Control {

		/// <value>
		/// The OPTIONAL modifier key for this control.
		/// </value>
		public ModifierKey Modifier { get; set; }

		/// <value>
		/// The OPTIONAL negative binding for this control.
		/// </value>
		public KeyCode Negative { get; set; }

		/// <value>
		/// The REQUIRED positive binding for this control.
		/// CANNOT BE KeyCode.None!
		/// </value>
		public KeyCode Positive { get; set; }

		/// <summary>
		/// Creates a new KeyControl.
		/// This is the "new version" of OneWayControl from previous versions of InputMaster.
		/// </summary>
		/// <param name="positive"><see cref="Positive"/></param>
		public KeyControl(KeyCode positive)
			: base() {
			if(positive == KeyCode.None)
				throw new ArgumentException("Positive must != KeyCode.None");

			this.Positive = positive;
			this.Negative = KeyCode.None;
			this.Modifier = ModifierKey.None;
		}

		/// <summary>
		/// Creates a new KeyControl with a negative binding.
		/// This is the "new version" of AxisControl from previous versions of InputMaster.
		/// </summary>
		/// <param name="positive"><see cref="Positive"/></param>
		/// <param name="negative"><see cref="Negative"/></param>
		public KeyControl(KeyCode positive, KeyCode negative)
			: this(positive) {
			this.Negative = negative;
		}

		public override string ToStringBlock() {
			var sb = new StringBuilder();
			sb.AppendLine("PositiveKey: " + Positive);
			sb.AppendLine("NegativeKey: " + Negative);
			sb.AppendLine("Modifier: " + Modifier.DisplayName);
			sb.AppendLine(base.ToStringBlock());
			return sb.ToString().Trim();
		}

		/// <summary>
		/// Updates the current states of this control.
		/// </summary>
		protected override void UpdateStates() {
			var pos = (Invert) ? Negative : Positive;
			var neg = (Invert) ? Positive : Negative;

			var hasMod = Modifier != null && Modifier != ModifierKey.None;
			var modPass = !hasMod;
			if(hasMod)
				modPass = Input.GetKeyDown(Modifier) || Input.GetKey(Modifier);

			if(modPass == false) {
				if(RealValue > 0f)
					Up |= ControlState.Positive;
				else if(RealValue < 0f)
					Up |= ControlState.Positive;

				return;
			}

			if(Input.GetKeyDown(pos))
				Down |= ControlState.Positive;
			if(Input.GetKey(pos))
				Held |= ControlState.Positive;
			if(Input.GetKeyUp(pos))
				Up |= ControlState.Positive;

			if(Negative != null) {
				if(Input.GetKey(neg))
					Held |= ControlState.Negative;
				if(Input.GetKeyUp(neg))
					Up |= ControlState.Negative;
				if(Input.GetKeyDown(neg))
					Down |= ControlState.Negative;
			}
		}
	}
}