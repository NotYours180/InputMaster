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
#define NEW_UI
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BSGTools.IO.XInput;
using UnityEngine;
using YamlDotNet.Serialization;

namespace BSGTools.IO {

	/// <summary>
	/// A single instance of this exists in the application.
	/// Updates and maintains all Control states.
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("BSGTools/InputMaster/InputMaster")]
	public class InputMaster : MonoBehaviour {
		#region Fields
		public static InputMaster instance { get; private set; }

		List<Control> controls = new List<Control>();

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

		public bool mouseMovementBlocked = false;


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
		#endregion

		bool initialized = false;

		void Start() {
			InputMaster.instance = this;
			Initialize(Application.dataPath + "/config2.cfg");
		}

		public void Initialize(string cfgPath) {
			//ReadControls(cfgPath);
			controls.Add(new ActionControl(
				new Binding[] { 
					Binding.A 
				},
				new ModifierFlags[] { 
					ModifierFlags.None 
				})
			);
			controls.Add(new AxisControl(
				new Binding[] { 
					Binding.A 
				},
				new float[]{ 
					1f
				})
			);

			WriteControls(cfgPath);
			initialized = true;
		}

		public void ReadControls(string cfgPath) {
			var d = new Deserializer();
			using(var reader = new StreamReader(cfgPath)) {
				var value = d.Deserialize<YAMLView[]>(reader);
				controls = value.Select(y => Control.FromYAMLView(y)).ToList();
			}
		}

		public void WriteControls(string cfgPath) {
			var s = new Serializer();
			var graph = controls.Select(c => c.GetYAMLView()).ToArray();
			using(var writer = new StreamWriter(cfgPath)) {
				writer.AutoFlush = true;
				s.Serialize(writer, graph);
			}
		}

		/// <summary>
		/// Resets all states/values on all controls.
		/// </summary>
		/// <seealso cref="ResetAll(bool)"/>
		public void ResetAll() {
			//foreach(var c in controlConfig.controls)
			//	c.Reset();
		}

		/// <summary>
		/// Blocks or unblocks all controls.
		/// This has the side effect of resetting all control states.
		/// </summary>
		/// <param name="blocked">To block/unblock.</param>
		/// <seealso cref="Control.blocked"/>
		public void SetBlockAll(bool blocked) {
			//foreach(var c in controlConfig.controls)
			//	c.blocked = blocked;
		}


		private void OnApplicationFocus(bool focused) {
#if XBOX_ALLOWED
			if(XInputUtils.StopVibrateOnAppFocusLost && focused == false)
				XInputUtils.SetVibrationAll(0f);
#endif
		}

		private void OnApplicationPause(bool paused) {
#if XBOX_ALLOWED
			if(XInputUtils.StopVibrateOnAppPause && paused == true)
				XInputUtils.SetVibrationAll(0f);
#endif
		}

		private void OnApplicationQuit() {
#if XBOX_ALLOWED
			XInputUtils.SetVibrationAll(0f);
#endif
			SetBlockAll(false);
		}


		void Update() {
			if(!initialized) {
				Debug.LogError("Must call <color=blue>Initialize()</color> before updates can begin!");
				return;
			}

#if XBOX_ALLOWED
			XInputUtils.UpdateStates();
#endif
			if(mouseMovementBlocked) {
				mouseX = 0f;
				mouseY = 0f;
			}
			else {
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
			}

			anyControlDown = false;
			anyControlHeld = false;
			anyControlUp = false;

			if(controls != null)
				foreach(var c in controls)
					UpdateControl(c);
		}

		private void UpdateControl(Control c) {
			if(CheckScope(c)) {
				c.Update();

				//if((c.down & ControlState.Either) != 0)
				//	anyControlDown = true;
				//if((c.held & ControlState.Either) != 0)
				//	anyControlHeld = true;
				//if((c.up & ControlState.Either) != 0)
				//	anyControlUp = true;
			}
		}

		private static bool CheckScope(Control c) {
			switch(c.scope) {
				case Scope.Release:
					return Application.isEditor == false;
				case Scope.Editor:
					return Application.isEditor;
				default:
					return true;
			}
		}

		public T GetControl<T>(string identifier) where T : Control {
			return controls.OfType<T>().FirstOrDefault(c => c.identifier == identifier);
		}

		public bool TryGetControl<T>(string identifier, out T control) where T : Control {
			control = GetControl<T>(identifier);
			return control != null;
		}

		public ActionControl GetAction(string identifier) {
			return controls.OfType<ActionControl>().FirstOrDefault(c => c.identifier == identifier);
		}

		public bool TryGetAction(string identifier, out ActionControl control) {
			control = GetAction(identifier);
			return control != null;
		}

		public AxisControl GetAxis(string identifier) {
			return controls.OfType<AxisControl>().FirstOrDefault(c => c.identifier == identifier);
		}

		public bool TryGetAxis(string identifier, out AxisControl control) {
			control = GetAxis(identifier);
			return control != null;
		}
	}
}