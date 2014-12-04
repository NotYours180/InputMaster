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
#if XBOX_ALLOWED
#define NEW_UI
using UnityEngine.EventSystems;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BSGTools.IO.Xbox;
using UnityEngine;

namespace BSGTools.IO {

	/// <summary>
	/// Simple extensions class for commonly used enum functionality.
	/// </summary>
	public static class EnumExt {

		/// <summary>
		/// Similar to .NET 4.0+'s method to check if a flag is set on an enum.
		/// </summary>
		/// <param name="value">The current value.</param>
		/// <param name="flag">The flag to check.</param>
		/// <returns></returns>
		public static bool HasFlag(this Enum value, Enum flag) {
			return (Convert.ToInt64(value) & Convert.ToInt64(flag)) != 0;
		}

	}

	/// <summary>
	/// A single instance of this exists in the application.
	/// Updates and maintains all Control states.
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("BSGTools/InputMaster/New InputMaster")]
	public class InputMaster : MonoBehaviour {
		#region Fields
		[Header("Base Configurations")]
		public StandaloneControlConfig standaloneConfig;
		public XboxControlConfig xboxConfig;
		public CombinedOutputsConfig combinedOutputs;

		public static InputMaster instance { get; private set; }

		/// <value>
		/// Are any controls in an active Down state?
		/// </value>
		public bool anyControlDown { get; private set; }
		/// <value>
		/// Are any controls in an active Held state?
		/// </value>
		public bool anyControlHeld { get; private set; }
		/// <value>
		/// Are any controls in an active Up state?
		/// </value>
		public bool anyControlUp { get; private set; }


		/// <value>
		/// The Mouse X Axis axis name in Unity's Input Manager
		/// </value>
		[SerializeField, Header("Mouse Axes")]
		string mouseXAxisName;
		/// <value>
		/// The Mouse X Axis axis value from Unity's native Input system.
		/// </value>
		public float mouseX { get; private set; }
		/// <value>
		/// The Mouse X Axis raw axis value from Unity's native Input system.
		/// </value>
		public float mouseXRaw { get; private set; }

		/// <value>
		/// The Mouse Y Axis axis name in Unity's Input Manager
		/// </value>
		[SerializeField]
		string mouseYAxisName;
		/// <value>
		/// The Mouse Y Axis axis value from Unity's native Input system.
		/// </value>
		public float mouseY { get; private set; }
		/// <value>
		/// The Mouse Y Axis raw axis value from Unity's native Input system.
		/// </value>
		public float mouseYRaw { get; private set; }

		/// <value>
		/// The MouseWheel Axis axis name in Unity's Input Manager
		/// </value>
		[SerializeField]
		string mouseWheelAxisName;
		/// <value>
		/// The MouseWheel Axis axis value from Unity's native Input system.
		/// </value>
		public float mouseWheel { get; private set; }
		/// <value>
		/// The MouseWheel Axis raw axis value from Unity's native Input system.
		/// </value>
		public float mouseWheelRaw { get; private set; }

		public CombinedOutput coUIHorizontal { get; set; }
		public CombinedOutput coUIVertical { get; set; }
		public CombinedOutput coUISubmit { get; set; }
		public CombinedOutput coUICancel { get; set; }
		#endregion

		void Awake() {
			InputMaster.instance = this;
		}

		/// <summary>
		/// Resets all states/values on all controls.
		/// </summary>
		/// <seealso cref="ResetAll(bool)"/>
		public void ResetAll() {
			foreach(var c in standaloneConfig.controls)
				c.Reset();
			foreach(var c in xboxConfig.controls)
				c.Reset();
		}

		/// <summary>
		/// Blocks or unblocks all controls.
		/// This has the side effect of resetting all control states.
		/// </summary>
		/// <param name="blocked">To block/unblock.</param>
		/// <seealso cref="Control.blocked"/>
		public void SetBlockAll(bool blocked) {
			foreach(var c in standaloneConfig.controls)
				c.blocked = blocked;
			foreach(var c in xboxConfig.controls)
				c.blocked = blocked;
		}

#if XBOX_ALLOWED
		private void OnApplicationFocus(bool focused) {
			if(XboxUtils.StopVibrateOnAppFocusLost && focused == false)
				XboxUtils.SetVibrationAll(0f);
		}

		private void OnApplicationPause(bool paused) {
			if(XboxUtils.StopVibrateOnAppPause && paused == true)
				XboxUtils.SetVibrationAll(0f);
		}

		private void OnApplicationQuit() {
			XboxUtils.SetVibrationAll(0f);
		}
#endif

		void Update() {
#if XBOX_ALLOWED
			XboxUtils.UpdateStates();
#endif
			if(!string.IsNullOrEmpty(mouseXAxisName)) {
				mouseX = Input.GetAxis(mouseXAxisName);
				mouseXRaw = Input.GetAxisRaw(mouseXAxisName);
			}
			if(!string.IsNullOrEmpty(mouseYAxisName)) {
				mouseY = Input.GetAxis(mouseYAxisName);
				mouseYRaw = Input.GetAxisRaw(mouseYAxisName);
			}
			if(!string.IsNullOrEmpty(mouseWheelAxisName)) {
				mouseWheel = Input.GetAxis(mouseWheelAxisName);
				mouseWheelRaw = Input.GetAxisRaw(mouseWheelAxisName);
			}

			anyControlDown = false;
			anyControlHeld = false;
			anyControlUp = false;

			if(standaloneConfig != null)
				foreach(var c in standaloneConfig.controls)
					UpdateControl(c);
			if(xboxConfig != null) {
				foreach(var c in xboxConfig.controls)
					UpdateControl(c);
			}
		}

		private void UpdateControl(Control c) {
			if((c.debugOnly && Debug.isDebugBuild) || c.debugOnly == false) {
				c.Update();

				if(c.down.HasFlag(ControlState.Either))
					anyControlDown = true;
				if(c.held.HasFlag(ControlState.Either))
					anyControlHeld = true;
				if(c.up.HasFlag(ControlState.Either))
					anyControlUp = true;
			}
		}

		public bool HasControl(string identifier) {
			bool hasControl = false;
			if(standaloneConfig != null)
				hasControl = standaloneConfig.controls.Any(c => c.identifier == identifier);
			if(hasControl == false && xboxConfig != null)
				hasControl = xboxConfig.controls.Any(c => c.identifier == identifier);
			return hasControl;
		}

		public T GetControl<T>(string identifier) where T : Control {
			T t = null;
			if(typeof(T) == typeof(StandaloneControl) && standaloneConfig != null)
				t = standaloneConfig.controls.SingleOrDefault(c => c.identifier == identifier) as T;
			if(t == null && typeof(T) == typeof(XboxControl) && xboxConfig != null)
				t = xboxConfig.controls.SingleOrDefault(c => c.identifier == name) as T;
			return t;
		}

		public Control GetControl(string identifier) {
			Control t = null;
			if(standaloneConfig != null)
				t = standaloneConfig.controls.SingleOrDefault(c => c.identifier == identifier);
			if(t == null && xboxConfig != null)
				t = xboxConfig.controls.SingleOrDefault(c => c.identifier == name);
			return t;
		}
	}
}