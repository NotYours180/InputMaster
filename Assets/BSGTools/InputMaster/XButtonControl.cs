#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
#define XBOX_ALLOWED
using XInputDotNetPure;
#endif

using System;

namespace BSGTools.IO.Xbox {
	/// <summary>
	/// Used for integrating Xbox 360 controller digital buttons/inputs into InputMaster's Control system.
	/// </summary>
	[Serializable]
	public sealed class XButtonControl : XboxControl<XButtonControl>, IXboxControl {
		private bool reportedPosUp, reportedPosDown, reportedNegUp, reportedNegDown;

		/// <value>
		/// The positive binding for this control. CANNOT BE XBinding.None!
		/// </value>
		public XButton positive = XButton.A;

		/// <value>
		/// The negative binding for this control.
		/// </value>
		public XButton negative = XButton.None;


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
			this.positive = positive;
			this.negative = negative;
		}

		/// <summary>
		/// Creates a clone of this XButtonControl.
		/// </summary>
		/// <param name="controller">The index of the controller that will manipulate this control's states and values.</param>
		/// <returns>The cloned control.</returns>
		protected override XButtonControl CreateClone(byte controller) {
			return new XButtonControl(controller, this.positive, this.negative) {
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
		/// Updates this control's states.
		/// </summary>
		protected override void UpdateStates() {
#if XBOX_ALLOWED
			var gpState = XboxUtils.ControllerStates[ControllerIndex];
			var pos = (invert) ? negative : positive;
			var neg = (invert) ? positive : negative;

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
}