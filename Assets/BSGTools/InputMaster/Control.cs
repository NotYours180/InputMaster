/**
Copyright (c) 2014, Michael Notarnicola
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
#define XBOX_ALLOWED
#endif
using System;
using BSGTools.IO.XInput;
using UnityEngine;
using XInputDotNetPure;
using System.Linq;
using System.Collections.Generic;


namespace BSGTools.IO {
	public enum Scope {
		All,
		Release,
		Editor
	}

	[Serializable]
	public abstract class Control {
		/// <value>
		/// A required, unique identifiers.
		/// </value>
		public string identifier = "new_" + Guid.NewGuid().ToString().ToUpper().Split('-')[0];

		/// <value>
		/// This is used to block any control from receiving updates.
		/// Keep in mind that if you block a control, it maintains its values from it's most recent update.
		/// If you want to block and reset a control, you can use the <see cref="Reset(bool)"/> function.
		/// </value>
		[NonSerialized]
		public bool blocked = false;

		/// <value>
		/// Used to specify controls that automatically
		/// only work in the Editor or in Debug builds.
		/// </value>
		public Scope scope = Scope.All;

		public byte controllerIndex = 0;

		protected GamePadState gpState { get; private set; }

		internal Control() { }

		/// <summary>
		/// Reset all non-configuration values and states for this control.
		/// </summary>
		/// <seealso cref="Reset(bool)"/>
		public void Reset() {
			Reset(false);
		}

		/// <summary>
		/// Reset all non-configuration values for this control. This is the best method to use for cutscenes.
		/// </summary>
		/// <param name="block">Whether or not to block this input after resetting.</param>
		/// <seealso cref="Reset"/>
		///	<seealso cref="blocked"/>
		public void Reset(bool block) {
			blocked = block;
		}

		protected abstract void ResetControl();

		/// <summary>
		/// Updates the control.
		/// This should never be used by any user-made script.
		/// This is public specifically for the use of <see cref="InputMaster"/>.
		/// </summary>
		public void Update() {
			Reset();
			if(blocked)
				return;

#if XBOX_ALLOWED
			gpState = XInputUtils.ControllerStates[controllerIndex];
#endif
			UpdateValues();

		}

		public YAMLView GetYAMLView() {
			return new YAMLView(this);
		}

		public static Control FromYAMLView(YAMLView view) {
			Control c;
			if(view.controlType == 0)
				c = ACFromYAMLView(view);
			else if(view.controlType == 1)
				c = AXFromYAMLView(view);
			else
				throw new InvalidCastException();
			c.identifier = view.action;
			c.controllerIndex = view.controllerIndex;
			c.scope = view.scope;
			return c;
		}

		private static AxisControl AXFromYAMLView(YAMLView view) {
			var control = new AxisControl();
			for(int i = 0;i < view.bindings.Length;i++)
				control.bindings.Add(view.bindings[0], view.multipliers[0]);
			return control;
		}

		private static ActionControl ACFromYAMLView(YAMLView view) {
			var control = new ActionControl();
			for(int i = 0;i < view.bindings.Length;i++)
				control.bindings.Add(view.bindings[0], view.modifiers[0]);
			return control;
		}

		/// <summary>
		/// Internally used for updating the Up/Held/Down states of a control.
		/// </summary>
		protected abstract void UpdateValues();

		/// <summary>
		/// Internally used to give a random name to a control if one is not provided.
		/// </summary>
		/// <returns>A new random string.</returns>
		private static string GetRandomName() {
			return "UNNAMED_" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
		}
	}

	public class YAMLView {
		public string action { get; set; }
		public byte controlType { get; set; }
		public Scope scope { get; set; }
		public byte controllerIndex { get; set; }
		public Binding[] bindings { get; set; }
		public ModifierFlags[] modifiers { get; set; }
		public float[] multipliers { get; set; }

		public YAMLView(Control c) {
			this.action = c.identifier;
			this.scope = c.scope;
			this.controllerIndex = c.controllerIndex;

			if(c is ActionControl) {
				this.controlType = 0;
				var ac = c as ActionControl;
				this.bindings = ac.bindings.Select(b => b.Key).ToArray();
				this.modifiers = ac.bindings.Values.ToArray();
			}
			else if(c is AxisControl) {
				this.controlType = 1;
				var ax = c as AxisControl;
				this.bindings = ax.bindings.Select(b => b.Key).ToArray();
				this.multipliers = ax.bindings.Values.ToArray();
			}
		}

		// For YAML serialization
		public YAMLView() { }
	}
}