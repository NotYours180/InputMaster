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
	/// Provided to give a common base class and common functionality for <see cref="StandaloneControl"/> and <see cref="XboxControl"/>.
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
		/// A required, unique identifiers.
		/// </value>
		public string identifier = "new_" + Guid.NewGuid().ToString().ToUpper().Split('-')[0];

		/// <value>
		/// The current "down" state of the control.
		/// </value>
		public ControlState down { get; protected set; }

		/// <value>
		/// The current "held" state of the control.
		/// </value>
		public ControlState held { get; protected set; }

		/// <value>
		/// The current "up" state of the control.
		/// </value>
		public ControlState up { get; protected set; }

		/// <value>
		/// Functionally identical to the Dead property of Unity's native Input system.
		/// The absolute value of a control's real value reports as 0 if it's less than this value.
		/// </value>
		public float dead = 0f;

		/// <value>
		/// Functionally identical to the Gravity property of Unity's native Input system.
		/// Speed per second that a control at rest returns to 0.
		/// </value>
		public float gravity = 1f;

		/// <value>
		/// Functionally identical to the Sensitivity property of Unity's native Input system.
		/// Speed per second that a control in motion approaches 1.
		/// </value>
		public float sensitivity = 1f;

		/// <value>
		/// Functionally identical to the Invert property of Unity's native Input system.
		/// If true, the contol's value will report as -(value).
		/// However, the state of the control will remain the same (positive down will still report as positive down, etc).
		/// Keep in mind that this functions whether or not a negative binding is supplied.
		/// </value>
		public bool invert = false;

		/// <value>
		/// This is used to block any control from receiving updates.
		/// Keep in mind that if you block a control, it maintains its values from it's most recent update.
		/// If you want to block and reset a control, you can use the <see cref="Reset(bool)"/> function.
		/// </value>
		public bool blocked = false;

		/// <value>
		/// Used to specify controls that automatically
		/// only work in the Editor or in Debug builds.
		/// </value>
		public bool debugOnly = false;


		/// <value>
		/// Functionally identical to the Snap property of Unity's native Input system.
		/// If true, and if the control has a positive and negative binding, the control's value will snap to 0 if provided an opposite input.
		/// </value>
		public bool snap = false;

		/// <value>
		/// Returns an analog representation of the current real value.
		/// This is functionally identical to calling Input.GetAxis() from Unity's native Input system.
		/// </value>
		public float value { get; protected set; }

		/// <value>
		/// Returns a digital, ceiling-rounded representation of <see cref="value"/>.
		/// This is functionally identical to calling Input.GetAxisRaw() from Unity's native Input system.
		/// </value>
		public sbyte fixedValue { get; protected set; }


		/// <value>
		/// An internally used property that keeps track of the "real value" across updates.
		/// This is necessary so that properties like <see cref="dead"/> can be applied to the final value.
		/// Think of this as the "real value" and the <see cref="value"/> property as this value after post processing.
		/// This value is not necessary to use for input.
		/// </value>
		protected float realValue { get; set; }

		protected Control() {
			if(string.IsNullOrEmpty(identifier.Trim()))
				throw new UnityException("Identifier must not be null or empty!");
		}

		/// <summary>
		/// Reset all non-configuration values and states for this control.
		/// </summary>
		/// <seealso cref="Reset(bool)"/>
		public void Reset() {
			down = ControlState.Neither;
			up = ControlState.Neither;
			held = ControlState.Neither;
			fixedValue = 0;
			value = 0f;
			realValue = 0f;
		}

		/// <summary>
		/// Reset all non-configuration values for this control. This is the best method to use for cutscenes.
		/// </summary>
		/// <param name="block">Whether or not to block this input after resetting.</param>
		/// <seealso cref="Reset"/>
		///	<seealso cref="blocked"/>
		public void Reset(bool block) {
			Reset();
			blocked = block;
		}

		/// <summary>
		/// Updates the control.
		/// This should never be used by any user-made script.
		/// This is public specifically for the use of <see cref="InputMaster"/>.
		/// </summary>
		public void Update() {
			SoftReset();
			if(blocked == false)
				UpdateStates();
			UpdateValues();
		}

		/// <summary>
		/// Internally used for maintaining the <see cref="realValue"/> inbetween updates while resetting everything else.
		/// </summary>
		void SoftReset() {
			var realVal = realValue;
			Reset();
			realValue = realVal;
		}

		/// <summary>
		/// Internally used for updating the Up/Held/Down states of a control.
		/// </summary>
		protected abstract void UpdateStates();

		/// <summary>
		/// Internally used for updating a control's values using it's current states.
		/// </summary>
		protected virtual void UpdateValues() {
			if(held.HasFlag(ControlState.Positive))
				realValue += Time.deltaTime * sensitivity;
			else if(realValue > 0f) {
				realValue -= Time.deltaTime * sensitivity;
				if(realValue < 0f)
					realValue = 0f;
			}
			if(held.HasFlag(ControlState.Negative))
				realValue -= Time.deltaTime * sensitivity;
			else if(realValue < 0f) {
				realValue += Time.deltaTime * sensitivity;
				if(realValue > 0f)
					realValue = 0f;
			}

			realValue = Mathf.Clamp(realValue, -1f, 1f);

			if(held != ControlState.Both && snap) {
				if(realValue > 0f && held.HasFlag(ControlState.Negative))
					realValue = 0f;
				else if(realValue < 0f && held.HasFlag(ControlState.Positive))
					realValue = 0f;
			}

			value = realValue;

			//We dont want to mess with the real value
			//When considering dead values
			if(Mathf.Abs(realValue) <= Mathf.Abs(dead))
				value = 0f;

			fixedValue = GetFV();
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
		public sbyte GetFV() {
			if(down == ControlState.Positive || held == ControlState.Positive)
				return 1;
			else if(down == ControlState.Negative || held == ControlState.Negative)
				return -1;
			else
				return 0;
		}

		/// <summary>
		/// Rounds and clamps to a FixedValue float.
		/// </summary>
		/// <param name="f">The value to round and clamp.</param>
		/// <returns>-1, 1 or 0</returns>
		public float GetFVF() {
			return (float)GetFV();
		}
	}
}