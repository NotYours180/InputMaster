#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
#define XBOX_ALLOWED
using XInputDotNetPure;
#endif

using System;
using UnityEngine;

namespace BSGTools.IO.Xbox {

	/// <summary>
	/// Used for integrating Xbox 360 controller triggers into InputMaster's Control system.
	/// </summary>
	[Serializable]
	public sealed class XTriggerControl : XboxControl {

		/// <value>
		/// The assigend trigger that will manipulate this control's states and values.
		/// </value>
		public XTrigger trigger = XTrigger.TriggerLeft;

		public XTriggerControl(XTrigger trigger) {
			this.trigger = trigger;
		}

		/// <summary>
		/// In this specialized case, the values are updated here, not the states.
		/// </summary>
		protected override void UpdateStates() {
#if XBOX_ALLOWED
			for(byte i = 0;i < 4;i++) {
				currentController = i;
				var gpState = XboxUtils.ControllerStates[currentController];
				if(gpState.IsConnected == false)
					continue;

				var triggerVal = (trigger == XTrigger.TriggerLeft) ? gpState.Triggers.Left : gpState.Triggers.Right;

				if(triggerVal > realValue)
					down = ControlState.Positive;
				else if(triggerVal < realValue)
					up = ControlState.Positive;
				else if(Mathf.Approximately(triggerVal, realValue))
					held = ControlState.Positive;
			}
			currentController = 0;
#endif
		}

		/// <summary>
		///
		/// </summary>
		/// <see cref="UpdateStates"/>
		protected override void UpdateValues() {
			for(byte i = 0;i < 4;i++) {
#if XBOX_ALLOWED
				currentController = i;
				var gpState = XboxUtils.ControllerStates[currentController];

				realValue = (trigger == XTrigger.TriggerLeft) ? gpState.Triggers.Left : gpState.Triggers.Right;
				realValue = (invert) ? -realValue : realValue;
				value = realValue;
				fixedValue = GetFV();
#else
				Reset();
#endif
			}
			currentController = 0;
		}
	}

	/// <summary>
	/// Represents the two triggers of an Xbox 360 Controller.
	/// </summary>
	public enum XTrigger {
		TriggerLeft,
		TriggerRight
	}
}