#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
#define XBOX_ALLOWED
#endif

using System;
using System.Collections.Generic;
using BSGTools.IO;
using BSGTools.IO.XInput;
using UnityEngine;
using XInputDotNetPure;
using System.Linq;

namespace BSGTools.IO {
	/// <summary>
	/// Used for all Keyboard and Mouse Button controls.
	/// Any binding present in Unity's KeyCode enumeration is valid here.
	/// </summary>
	[Serializable]
	public sealed class AxisControl : Control {
		public Dictionary<Binding, float> bindings = new Dictionary<Binding, float>();
		/// <value>
		/// Returns an analog representation of the current real value.
		/// This is functionally identical to calling Input.GetAxis() from Unity's native Input system.
		/// </value>
		public float rValue { get; protected set; }
		/// <value>
		/// The -1...1 clamped value.
		/// </value>
		public float value { get; protected set; }


		internal AxisControl() { }

		/// <summary>
		/// Creates a new KeyControl with a negative binding.
		/// This is the "new version" of AxisControl from previous versions of InputMaster.
		/// </summary>
		/// <param name="positive"><see cref="positive"/></param>
		/// <param name="negative"><see cref="negative"/></param>
		public AxisControl(Binding[] bindings, float[] multipliers) {
			if(bindings.Length != multipliers.Length)
				throw new ArgumentException("Bindings length must == multipliers length");
			for(int i = 0;i < bindings.Length;i++)
				this.bindings.Add(bindings[i], multipliers[i]);
		}

		/// <summary>
		/// Updates the current states of this control.
		/// </summary>
		protected override void UpdateValues() {
			foreach(var b in bindings) {
				if(BindingUtils.IsKeyCode(b.Key))
					rValue += BindingUtils.GetKCValue(b.Key) ? b.Value : 0f;
#if XBOX_ALLOWED
				else
					rValue += BindingUtils.GetXInputValue(b.Key, gpState) * b.Value;
#endif
			}
			value = Mathf.Clamp(rValue, -1f, 1f);
		}

		protected override void ResetControl() {
			rValue = 0f;
		}
	}
}