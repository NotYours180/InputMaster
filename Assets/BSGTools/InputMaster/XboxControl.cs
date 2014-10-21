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
using XInputDotNetPure;
#endif

using System;
using UnityEngine;

namespace BSGTools.IO.Xbox {

	/// <summary>
	/// Represents all of the digital buttons of an Xbox 360 Controller.
	/// </summary>
	public enum XButton {
		None,
		A,
		B,
		X,
		Y,
		Back,
		Guide,
		Start,
		StickLeft,
		StickRight,
		ShoulderLeft,
		ShoulderRight,
		DPadUp,
		DPadDown,
		DPadLeft,
		DPadRight
	}

	/// <summary>
	/// Represents the two analog sticks of an Xbox 360 Controller.
	/// </summary>
	public enum XStick {
		StickLeftX,
		StickLeftY,
		StickRightX,
		StickRightY
	}

	/// <summary>
	/// Represents the two triggers of an Xbox 360 Controller.
	/// </summary>
	public enum XTrigger {
		TriggerLeft,
		TriggerRight
	}

	/// <summary>
	/// Used for type constraints.
	/// Contains nor enforces any functionality.
	/// </summary>
	public interface IXboxControl { }

	/// <value>
	/// The base class for all Xbox 360 control classes.
	/// </value>
	/// <typeparam name="T">Used for generic self-creation.</typeparam>
	[Serializable]
	public abstract class XboxControl<T> : Control where T : IXboxControl {

		/// <value>
		/// The index of the controller that will manipulate this control's states and values.
		/// This is used in conjunction to the PlayerIndex enumeration in XInput.
		/// </value>
		public byte ControllerIndex { get; private set; }

		/// <summary>
		/// Creates a new <typeparamref name="XboxControl"/>.
		/// </summary>
		/// <param name="controllerIndex"><see cref="ControllerIndex"/></param>
		protected XboxControl(byte controllerIndex) {
			if(controllerIndex < 0 || controllerIndex > 3)
				throw new ArgumentOutOfRangeException("controllerIndex");
			this.ControllerIndex = controllerIndex;
		}

		/// <summary>
		/// Allows for the creation of up to 4 XboxControls at the same time (one for each controller index).
		/// An <typeparamref name="XboxControl"/> must be provided for cloning.
		/// The <see cref="ControllerIndex"/> of the provided clone is ignored.
		/// </summary>
		/// <param name="count">The number of controllers to create this control for.</param>
		/// <param name="toClone">The instance to take values from for clone creation.</param>
		/// <returns>An array of type T, each assigned to a specific controller index.</returns>
		/// <strong>Example</strong>
		/// <code>
		///	<pre>
		///		// Defines a jump control for all 4 players.
		///		XButtonControl[] jump = XButtonControl.CreateMultiple(4, new XButtonControl(0, XButton.A) {
		///			Name = "Jump",
		///			Gravity = 2f,
		///			Sensitivity = 2f
		///		});
		///
		///		// Usage
		///		jump[0].Value; //Player One's jump value
		///		jump[3].Value; //Player Four's jump value
		/// </pre>
		/// </code>
		public static T[] CreateMultiple(byte count, T toClone) {
			if(count < 2 || count > 4)
				throw new ArgumentOutOfRangeException("count");
			var xbc = new T[count];
			for(byte i = 0;i < count;i++)
				xbc[i] = (toClone as XboxControl<T>).CreateClone(i);
			return xbc;
		}

		/// <summary>
		/// Creates a clone of an this XboxControl.
		/// </summary>
		/// <param name="controller">The index of the controller that will manipulate this control's states and values.</param>
		/// <returns></returns>
		protected abstract T CreateClone(byte controller);
	}

	/// <summary>
	/// Used for integrating Xbox 360 controller digital buttons/inputs into InputMaster's Control system.
	/// </summary>
	[Serializable]
	public sealed class XButtonControl : XboxControl<XButtonControl>, IXboxControl {
		private bool reportedPosUp, reportedPosDown, reportedNegUp, reportedNegDown;

		/// <value>
		/// The negative binding for this control.
		/// </value>
		public XButton Negative { get; set; }

		/// <value>
		/// The positive binding for this control. CANNOT BE XBinding.None!
		/// </value>
		public XButton Positive { get; set; }

		/// <summary>
		/// Creates an XButtonControl for a single player game.
		/// </summary>
		/// <param name="positive">The positive binding. CANNOT BE XButton.None!</param>
		public XButtonControl(XButton positive)
			: this(0, positive, XButton.None) {
		}

		/// <summary>
		/// Creates an XButtonControl for a single player game.
		/// </summary>
		/// <param name="positive">The positive binding. CANNOT BE XButton.None!</param>
		/// <param name="negative">The negative binding.</param>
		public XButtonControl(XButton positive, XButton negative)
			: this(0, positive, negative) {
		}

		private XButtonControl(byte controllerIndex, XButton positive, XButton negative)
			: base(controllerIndex) {
			this.Positive = positive;
			this.Negative = negative;
		}

		/// <summary>
		/// Creates a clone of this XButtonControl.
		/// </summary>
		/// <param name="controller">The index of the controller that will manipulate this control's states and values.</param>
		/// <returns>The cloned control.</returns>
		protected override XButtonControl CreateClone(byte controller) {
			return new XButtonControl(controller, this.Positive, this.Negative) {
				Gravity = this.Gravity,
				Sensitivity = this.Sensitivity,
				Snap = this.Snap,
				Invert = this.Invert,
				Dead = this.Dead,
				IsBlocked = this.IsBlocked,
				Name = this.Name
			};
		}

		/// <summary>
		/// Updates this control's states.
		/// </summary>
		protected override void UpdateStates() {
#if XBOX_ALLOWED
			var gpState = XboxUtils.ControllerStates[ControllerIndex];
			var pos = (Invert) ? Negative : Positive;
			var neg = (Invert) ? Positive : Negative;

			var positivePressed = false;
			var negativePressed = false;

			positivePressed = XBindingVal(pos, gpState);
			negativePressed = XBindingVal(neg, gpState);

			if(positivePressed) {
				Held |= ControlState.Positive;
				if(reportedPosDown == false) {
					Down |= ControlState.Positive;
					reportedPosDown = true;
					reportedPosUp = false;
				}
			}
			else if(reportedPosUp == false) {
				Up |= ControlState.Positive;
				reportedPosUp = true;
				reportedPosDown = false;
			}

			if(negativePressed) {
				Held |= ControlState.Negative;
				if(reportedNegDown == false) {
					Down |= ControlState.Negative;
					reportedNegDown = true;
					reportedNegUp = false;
				}
			}
			else if(reportedNegUp == false) {
				Up |= ControlState.Negative;
				reportedNegUp = true;
				reportedNegDown = false;
			}
#endif
		}

#if XBOX_ALLOWED

		/// <summary>
		/// An internal method of translating from XButton to it's logical state.
		/// </summary>
		/// <param name="xb"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		private bool XBindingVal(XButton xb, GamePadState state) {
			switch(xb) {
				case XButton.None:
					return false;
				case XButton.A:
					return (state.Buttons.A == ButtonState.Pressed);
				case XButton.B:
					return (state.Buttons.B == ButtonState.Pressed);

				case XButton.X:
					return (state.Buttons.X == ButtonState.Pressed);

				case XButton.Y:
					return (state.Buttons.Y == ButtonState.Pressed);

				case XButton.Back:
					return (state.Buttons.Back == ButtonState.Pressed);

				case XButton.Guide:
					return (state.Buttons.Guide == ButtonState.Pressed);

				case XButton.Start:
					return (state.Buttons.Start == ButtonState.Pressed);

				case XButton.StickLeft:
					return (state.Buttons.LeftStick == ButtonState.Pressed);

				case XButton.StickRight:
					return (state.Buttons.RightStick == ButtonState.Pressed);

				case XButton.ShoulderLeft:
					return (state.Buttons.LeftShoulder == ButtonState.Pressed);

				case XButton.ShoulderRight:
					return (state.Buttons.RightShoulder == ButtonState.Pressed);

				case XButton.DPadUp:
					return (state.DPad.Up == ButtonState.Pressed);

				case XButton.DPadDown:
					return (state.DPad.Down == ButtonState.Pressed);

				case XButton.DPadLeft:
					return (state.DPad.Left == ButtonState.Pressed);

				case XButton.DPadRight:
					return (state.DPad.Right == ButtonState.Pressed);

				default:
					throw new ArgumentException("", "xb");
			}
		}

#endif
	}

	/// <summary>
	/// Used for integrating Xbox 360 controller analog sticks into InputMaster's Control system.
	/// </summary>
	[Serializable]
	public sealed class XStickControl : XboxControl<XStickControl>, IXboxControl {
		/// <value>
		/// The assigned analog stick.
		/// </value>
		public XStick Stick { get; set; }

		/// <summary>
		/// Creates an XStickControl for a single player game.
		/// </summary>
		/// <param name="stick">The bound stick.</param>
		public XStickControl(XStick stick)
			: this(0, stick) {
		}

		private XStickControl(byte controllerIndex, XStick stick)
			: base(controllerIndex) {
			this.Stick = stick;
		}

		/// <summary>
		/// Creates a clone of this XStickControl.
		/// </summary>
		/// <param name="controller">The <see cref="ControllerIndex"/> to assign to the new clone.</param>
		/// <returns>The cloned XStickControl.</returns>
		protected override XStickControl CreateClone(byte controller) {
			return new XStickControl(controller, this.Stick) {
				Gravity = this.Gravity,
				Sensitivity = this.Sensitivity,
				Snap = this.Snap,
				Invert = this.Invert,
				Dead = this.Dead,
				IsBlocked = this.IsBlocked,
				Name = this.Name
			};
		}

		/// <summary>
		/// States are not used for stick controls.
		/// </summary>
		protected override void UpdateStates() {
		}

		/// <summary>
		/// Updates this control's values.
		/// The <see cref="StickValue"/> property is updated in <see cref="UpdateStates"/>.
		/// </summary>
		protected override void UpdateValues() {
#if XBOX_ALLOWED
			var gpState = XboxUtils.ControllerStates[ControllerIndex];
			var sticks = gpState.ThumbSticks;
			var val = 0f;

			if(Stick == XStick.StickLeftX)
				val = sticks.Left.X;
			else if(Stick == XStick.StickLeftY)
				val = sticks.Left.Y;
			else if(Stick == XStick.StickRightX)
				val = sticks.Right.X;
			else if(Stick == XStick.StickRightY)
				val = sticks.Right.Y;
			val = (Invert) ? -val : val;

			Value = RealValue = val;
			FixedValue = RoundFixed(Value);
#endif
		}

		/// <summary>
		/// Because there are 2 axes to worry about, a specialized enumeration is used to allow for different inversion modes.
		/// </summary>
		[Flags]
		public enum InvertMode {
			X = 1 >> 0,
			Y = 1 >> 1
		}
	}

	/// <summary>
	/// Used for integrating Xbox 360 controller triggers into InputMaster's Control system.
	/// </summary>
	[Serializable]
	public sealed class XTriggerControl : XboxControl<XTriggerControl>, IXboxControl {

		/// <value>
		/// The assigend trigger that will manipulate this control's states and values.
		/// </value>
		public XTrigger Trigger { get; set; }

		/// <summary>
		/// Creates an XTriggerControl for a single player game.
		/// </summary>
		/// <param name="trigger">The bound trigger.</param>
		public XTriggerControl(XTrigger trigger)
			: this(0, trigger) {
		}

		private XTriggerControl(byte controllerIndex, XTrigger trigger)
			: base(controllerIndex) {
			this.Trigger = trigger;
		}

		/// <summary>
		/// Creates a clone of this XTriggerControl.
		/// </summary>
		/// <param name="controller">The <see cref="ControllerIndex"/> to assign to the new clone.</param>
		/// <returns>The cloned XTriggerControl.</returns>
		protected override XTriggerControl CreateClone(byte controller) {
			return new XTriggerControl(controller, this.Trigger) {
				Gravity = this.Gravity,
				Sensitivity = this.Sensitivity,
				Snap = this.Snap,
				Invert = this.Invert,
				Dead = this.Dead,
				IsBlocked = this.IsBlocked,
				Name = this.Name
			};
		}

		/// <summary>
		/// In this specialized case, the values are updated here, not the states.
		/// </summary>
		protected override void UpdateStates() {
#if XBOX_ALLOWED
			var gpState = XboxUtils.ControllerStates[ControllerIndex];
			var triggerVal = (Trigger == XTrigger.TriggerLeft) ? gpState.Triggers.Left : gpState.Triggers.Right;

			if(triggerVal > RealValue)
				Down = ControlState.Positive;
			else if(triggerVal < RealValue)
				Up = ControlState.Positive;
			else if(Mathf.Approximately(triggerVal, RealValue))
				Held = ControlState.Positive;
#endif
		}

		/// <summary>
		///
		/// </summary>
		/// <see cref="UpdateStates"/>
		protected override void UpdateValues() {
#if XBOX_ALLOWED
			var gpState = XboxUtils.ControllerStates[ControllerIndex];

			RealValue = (Trigger == XTrigger.TriggerLeft) ? gpState.Triggers.Left : gpState.Triggers.Right;
			RealValue = (Invert) ? -RealValue : RealValue;
			Value = RealValue;
			FixedValue = RoundFixed(Value);
#else
			Reset();
#endif
		}
	}
}