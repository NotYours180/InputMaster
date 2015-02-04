#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
#define XBOX_ALLOWED
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using XInputDotNetPure;

namespace BSGTools.IO {

	public enum State {
		None,
		Down,
		Up,
		Held
	}

	[Flags]
	public enum ModifierFlags {
		None = 0,
		Control = 1,
		Alt = 2,
		Shift = 4
	}

	/// <summary>
	/// Used for all Keyboard and Mouse Button controls.
	/// Any binding present in Unity's KeyCode enumeration is valid here.
	/// </summary>
	[Serializable]
	public sealed class ActionControl : Control {
		internal Dictionary<Binding, ModifierFlags> bindings = new Dictionary<Binding, ModifierFlags>();

		public State state { get; private set; }
		State previousState;

		internal ActionControl() { }

		/// <summary>
		/// Creates a new KeyControl with a negative binding.
		/// This is the "new version" of AxisControl from previous versions of InputMaster.
		/// </summary>
		/// <param name="positive"><see cref="positive"/></param>
		/// <param name="negative"><see cref="negative"/></param>
		public ActionControl(Binding[] bindings, ModifierFlags[] modifiers) {
			if(bindings.Length != modifiers.Length)
				throw new ArgumentException("Bindings length must == modifiers length");
			for(int i = 0;i < bindings.Length;i++)
				this.bindings.Add(bindings[i], modifiers[0]);
			ResetControl();
		}

		/// <summary>
		/// Updates the current states of this control.
		/// </summary>
		protected override void UpdateValues() {
			var value = 0f;
			foreach(var b in bindings) {
				if((b.Value & ModifierFlags.Control) != 0 && !Input.GetKey(KeyCode.LeftControl))
					continue;
				if((b.Value & ModifierFlags.Alt) != 0 && !Input.GetKey(KeyCode.LeftAlt))
					continue;
				if((b.Value & ModifierFlags.Shift) != 0 && !Input.GetKey(KeyCode.LeftShift))
					continue;

				if(BindingUtils.IsKeyCode(b.Key))
					value += BindingUtils.GetKCValue(b.Key) ? 1f : 0f;
#if XBOX_ALLOWED
				else
					value += BindingUtils.GetXInputValue(b.Key, gpState);
#endif
			}

			if(previousState == State.None && value != 0f)
				state = State.Down;
			else if((previousState == State.Held || previousState == State.Down) && value == 0f)
				state = State.Up;
			else if(value != 0f)
				state = State.Held;
			else if(value == 0f)
				state = State.None;

			previousState = state;
		}

		protected override void ResetControl() {
			previousState = state = State.Up;
		}
	}
}