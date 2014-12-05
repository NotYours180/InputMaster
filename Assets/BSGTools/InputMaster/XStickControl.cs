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
	public sealed class XStickControl : XboxControl {
		/// <value>
		/// The assigned analog stick.
		/// </value>
		public XStick stick = XStick.StickLeftX;

		public XStickControl(XStick stick) {
			this.stick = stick;
		}

		/// <summary>
		/// States are not used for stick controls.
		/// </summary>
		protected override void UpdateStates() {
#if XBOX_ALLOWED
			for(byte i = 0;i < 4;i++) {
				currentController = i;
				var gpState = XboxUtils.ControllerStates[currentController];
				if(gpState.IsConnected == false)
					continue;
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
					down = ControlState.Positive;
				else if(value == 0f && realValue < 0f)
					down = ControlState.Negative;

				if(value > 0f && realValue == 0f)
					up = ControlState.Positive;
				else if(value < 0f && realValue == 0f)
					up = ControlState.Negative;

				if(realValue > 0f)
					held = ControlState.Positive;
				else if(realValue < 0f)
					held = ControlState.Negative;

				value = realValue;
				fixedValue = GetFV();
			}
			currentController = 0;
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