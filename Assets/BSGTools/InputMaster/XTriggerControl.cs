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
	public sealed class XTriggerControl : XboxControl<XTriggerControl>, IXboxControl {

		/// <value>
		/// The assigend trigger that will manipulate this control's states and values.
		/// </value>
		public XTrigger trigger = XTrigger.TriggerLeft;

		/// <summary>
		/// Creates an XTriggerControl for a single player game.
		/// </summary>
		/// <param name="trigger">The bound trigger.</param>
		public XTriggerControl(XTrigger trigger)
			: this(0, trigger) {
		}

		private XTriggerControl(byte controllerIndex, XTrigger trigger)
			: base(controllerIndex) {
			this.trigger = trigger;
		}

		/// <summary>
		/// Creates a clone of this XTriggerControl.
		/// </summary>
		/// <param name="controller">The <see cref="ControllerIndex"/> to assign to the new clone.</param>
		/// <returns>The cloned XTriggerControl.</returns>
		protected override XTriggerControl CreateClone(byte controller) {
			return new XTriggerControl(controller, this.trigger) {
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
		/// In this specialized case, the values are updated here, not the states.
		/// </summary>
		protected override void UpdateStates() {
#if XBOX_ALLOWED
			var gpState = XboxUtils.ControllerStates[ControllerIndex];
			var triggerVal = (trigger == XTrigger.TriggerLeft) ? gpState.Triggers.Left : gpState.Triggers.Right;

			if(triggerVal > realValue)
				down = ControlState.Positive;
			else if(triggerVal < realValue)
				up = ControlState.Positive;
			else if(Mathf.Approximately(triggerVal, realValue))
				held = ControlState.Positive;
#endif
		}

		/// <summary>
		///
		/// </summary>
		/// <see cref="UpdateStates"/>
		protected override void UpdateValues() {
#if XBOX_ALLOWED
			var gpState = XboxUtils.ControllerStates[ControllerIndex];

			realValue = (trigger == XTrigger.TriggerLeft) ? gpState.Triggers.Left : gpState.Triggers.Right;
			realValue = (invert) ? -realValue : realValue;
			value = realValue;
			fixedValue = GetFV();
#else
		Reset();
#endif
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