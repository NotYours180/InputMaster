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
#if UNITY_4_6
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
	public class InputMaster : MonoBehaviour {

		[Header("Base Configurations")]
		public StandaloneControlConfig standaloneControls;
		public XboxControlConfig xboxControls;

		public Control[] controls;

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

		/// <summary>
		/// Uses reflection to get all controls in a class.
		/// Depending on the control count from your controlClass,
		/// this could have a noticable performance spike unless used during loading.
		/// </summary>
		/// <param name="controlClass">The instance of a class to get the controls from.</param>
		/// <returns>The new InputMaster instance.</returns>
		public static InputMaster CreateMaster(object controlClass) {
			return CreateMaster(GetAllControls(controlClass));
		}

		/// <summary>
		/// Creates a new, empty, hidden GameObject, adds a new instance of InputMaster to it,
		/// and adds the provided controls to the master's control list.
		/// </summary>
		/// <param name="controls">A full listing of all of the games controls with the default bindings.</param>
		/// <returns>The new InputMaster instance.</returns>
		public static InputMaster CreateMaster(params Control[] controls) {
			var parent = new GameObject("_InputMaster");
			DontDestroyOnLoad(parent);
			var master = parent.AddComponent<InputMaster>();
			master.controls = controls;
			return master;
		}

		/// <summary>
		/// Destroys the InputMaster object.
		/// </summary>
		public void DestroyMaster() {
			Destroy(gameObject);
		}

		/// <summary>
		/// Resets all states/values on all controls.
		/// </summary>
		/// <seealso cref="ResetAll(bool)"/>
		public void ResetAll() {
			foreach(var c in controls)
				c.Reset();
		}

		/// <summary>
		/// Blocks or unblocks all controls.
		/// This has the side effect of resetting all control states.
		/// </summary>
		/// <param name="blocked">To block/unblock.</param>
		/// <seealso cref="Control.blocked"/>
		public void SetBlockAll(bool blocked) {
			foreach(var c in controls)
				c.blocked = blocked;
		}

		///// <summary>
		///// Searches the control list for the provided Control objects
		///// and replaces them with said provided object
		///// </summary>
		///// <param name="controls">The Control objects to search and replace</param>
		//public void UpdateControls(params Control[] controls) {
		//	foreach(var c in controls) {
		//		int index = this.controls.IndexOf(c);
		//		if(index == -1)
		//			throw new System.ArgumentException(string.Format("Could not find Control {0} in master control list. Aborting!", c.ToString()));
		//		this.controls[index] = c;
		//	}
		//}

		/// <summary>
		/// Internally used to get all <see cref="Control"/> variables using reflection from a class instance.
		/// Not the most performance efficient method of supplying InputMaster with controls.
		/// </summary>
		/// <param name="controlClass">The instance of a class to get the controls from.</param>
		/// <returns>An array of all Control objects.</returns>
		private static Control[] GetAllControls(object controlClass) {
			var type = controlClass.GetType();

			var bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

			var properties = type.GetProperties(bf).Where(p => p.GetValue(controlClass, null) != null).Select(p => p.GetValue(controlClass, null));
			var fields = type.GetFields(bf).Where(f => f.GetValue(controlClass) != null).Select(f => f.GetValue(controlClass));

			var p_bare = properties.OfType<Control>();
			var p_enum = properties.OfType<IEnumerable<Control>>();
			var f_bare = fields.OfType<Control>();
			var f_enum = fields.OfType<IEnumerable<Control>>();

			var final = p_bare.Concat(f_bare);

			foreach(var p_array in p_enum)
				final = final.Concat(p_array);
			foreach(var f_array in f_enum)
				final = final.Concat(f_array);

			return final.ToArray();
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

#if UNITY_4_6
			UpdateInputModule();
#endif

			if(string.IsNullOrEmpty(mouseXAxisName) == false) {
				mouseX = Input.GetAxis(mouseXAxisName);
				mouseXRaw = Input.GetAxisRaw(mouseXAxisName);
			}
			if(string.IsNullOrEmpty(mouseYAxisName) == false) {
				mouseY = Input.GetAxis(mouseYAxisName);
				mouseYRaw = Input.GetAxisRaw(mouseYAxisName);
			}
			if(string.IsNullOrEmpty(mouseWheelAxisName) == false) {
				mouseWheel = Input.GetAxis(mouseWheelAxisName);
				mouseWheelRaw = Input.GetAxisRaw(mouseWheelAxisName);
			}

			anyControlDown = false;
			anyControlHeld = false;
			anyControlUp = false;

			foreach(var c in controls) {
				if((c.debugOnly && Debug.isDebugBuild) || c.debugOnly == false) {
					c.Update();

					if(c.Down.HasFlag(ControlState.Either))
						anyControlDown = true;
					if(c.Held.HasFlag(ControlState.Either))
						anyControlHeld = true;
					if(c.Up.HasFlag(ControlState.Either))
						anyControlUp = true;
				}
			}
		}

		private void UpdateInputModule() {
#if UNITY_4_6
			if(EventSystem.current == null)
				return;

			var im = EventSystem.current.gameObject.GetComponent<InputMasterInputModule>();
			if(im == null)
				return;

			im.coUIHorizontal = coUIHorizontal;
			im.coUIVertical = coUIVertical;
			im.coUISubmit = coUISubmit;
			im.coUICancel = coUICancel;
#endif
		}

	}
}