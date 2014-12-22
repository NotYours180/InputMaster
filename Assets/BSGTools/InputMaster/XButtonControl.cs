#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
#define XBOX_ALLOWED
using XInputDotNetPure;
#endif

using System;
using UnityEngine;

namespace BSGTools.IO.Xbox {
	/// <summary>
	/// Used for integrating Xbox 360 controller digital buttons/inputs into InputMaster's Control system.
	/// </summary>
	[Serializable]
	public sealed class XButtonControl : XboxControl {
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
		/// Creates an XButtonControl.
		/// </summary>
		/// <param name="positive">The positive binding. CANNOT BE XButton.None!</param>
		public XButtonControl(XButton positive)
			: this(positive, XButton.None) {
		}

		private XButtonControl(XButton positive, XButton negative) {
			this.positive = positive;
			this.negative = negative;
		}

		/// <summary>
		/// Updates this control's states.
		/// </summary>
		protected override void UpdateStates() {
#if XBOX_ALLOWED
			for(byte i = 0;i < 4;i++) {
				currentController = i;
				var gpState = XboxUtils.ControllerStates[currentController];
				if(gpState.IsConnected == false)
					continue;

				var pos = (invert) ? negative : positive;
				var neg = (invert) ? positive : negative;

				var positivePressed = false;
				var negativePressed = false;

				positivePressed = XBindingVal(pos, gpState);
				negativePressed = XBindingVal(neg, gpState);

				if(positivePressed) {
					held |= ControlState.Positive;
					if(reportedPosDown == false) {
						down |= ControlState.Positive;
						reportedPosDown = true;
						reportedPosUp = false;
					}
				}
				else if(reportedPosUp == false) {
					up |= ControlState.Positive;
					reportedPosUp = true;
					reportedPosDown = false;
				}

				if(negativePressed) {
					held |= ControlState.Negative;
					if(reportedNegDown == false) {
						down |= ControlState.Negative;
						reportedNegDown = true;
						reportedNegUp = false;
					}
				}
				else if(reportedNegUp == false) {
					up |= ControlState.Negative;
					reportedNegUp = true;
					reportedNegDown = false;
				}
			}
			currentController = 0;
#endif
		}

		protected override void UpdateValues() {
			for(byte i = 0;i < 4;i++) {
				currentController = i;
				if((held & ControlState.Positive) != 0)
					realValue += Time.deltaTime * sensitivity;
				else if(realValue > 0f) {
					realValue -= Time.deltaTime * sensitivity;
					if(realValue < 0f)
						realValue = 0f;
				}
				if((held & ControlState.Negative) != 0)
					realValue -= Time.deltaTime * sensitivity;
				else if(realValue < 0f) {
					realValue += Time.deltaTime * sensitivity;
					if(realValue > 0f)
						realValue = 0f;
				}

				realValue = Mathf.Clamp(realValue, -1f, 1f);

				if(held != ControlState.Both && snap) {
					if(realValue > 0f && (held & ControlState.Negative) != 0)
						realValue = 0f;
					else if(realValue < 0f && (held & ControlState.Positive) != 0)
						realValue = 0f;
				}

				value = realValue;

				//We dont want to mess with the real value
				//When considering dead values
				if(Mathf.Abs(realValue) <= Mathf.Abs(dead))
					value = 0f;

				fixedValue = GetFV();
			}
			currentController = 0;
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