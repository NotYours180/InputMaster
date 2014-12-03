#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
#define XBOX_ALLOWED
using XInputDotNetPure;
#endif

using System;

namespace BSGTools.IO.Xbox {

	/// <summary>
	/// Used for integrating Xbox 360 controller analog sticks into InputMaster's Control system.
	/// </summary>
	[Serializable]
	public sealed class XStickControl : XboxControl<XStickControl>, IXboxControl {
		/// <value>
		/// The assigned analog stick.
		/// </value>
		public XStick stick = XStick.StickLeftX;

		/// <summary>
		/// Creates an XStickControl for a single player game.
		/// </summary>
		/// <param name="stick">The bound stick.</param>
		public XStickControl(XStick stick)
			: this(0, stick) {
		}

		private XStickControl(byte controllerIndex, XStick stick)
			: base(controllerIndex) {
			this.stick = stick;
		}

		/// <summary>
		/// Creates a clone of this XStickControl.
		/// </summary>
		/// <param name="controller">The <see cref="ControllerIndex"/> to assign to the new clone.</param>
		/// <returns>The cloned XStickControl.</returns>
		protected override XStickControl CreateClone(byte controller) {
			return new XStickControl(controller, this.stick) {
				gravity = this.gravity,
				sensitivity = this.sensitivity,
				snap = this.snap,
				invert = this.invert,
				dead = this.dead,
				blocked = this.blocked,
				identifier = this.identifier
			};
		}

		/// <summary>
		/// States are not used for stick controls.
		/// </summary>
		protected override void UpdateStates() {
#if XBOX_ALLOWED
			var gpState = XboxUtils.ControllerStates[ControllerIndex];
			var sticks = gpState.ThumbSticks;

			if(stick == XStick.StickLeftX)
				realValue = sticks.Left.X;
			else if(stick == XStick.StickLeftY)
				realValue = sticks.Left.Y;
			else if(stick == XStick.StickRightX)
				realValue = sticks.Right.X;
			else if(stick == XStick.StickRightY)
				realValue = sticks.Right.Y;
			realValue = (invert) ? -realValue : realValue;



			if(value == 0f && realValue > 0f)
				Down = ControlState.Positive;
			else if(value == 0f && realValue < 0f)
				Down = ControlState.Negative;

			if(value > 0f && realValue == 0f)
				Up = ControlState.Positive;
			else if(value < 0f && realValue == 0f)
				Up = ControlState.Negative;

			if(realValue > 0f)
				Held = ControlState.Positive;
			else if(realValue < 0f)
				Held = ControlState.Negative;

			value = realValue;
			fixedValue = GetFV();
#endif
		}

		/// <summary>
		/// Does nothing. Stick's value/state dependencies are switched.
		/// Updates happen in <see cref="UpdateStates"/>.
		/// </summary>
		protected override void UpdateValues() { }
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
}