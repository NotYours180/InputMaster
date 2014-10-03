/**
Copyright (c) 2014, Michael Notarnicola
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using UnityEngine;
using System.Collections;
using System;

namespace BSGTools.IO {
	/// <summary>
	/// Represents a Modifier for a KeyBinding.
	/// </summary>
	public sealed class ModifierKey {

		#region Modifiers
		/// <summary>
		/// Default Modifier for KeyControls with no modifier.
		/// </summary>
		public static readonly ModifierKey None = new ModifierKey(KeyCode.None, "None");
		/// <summary>
		/// The Left Shift modifier key.
		/// </summary>
		public static readonly ModifierKey LShift = new ModifierKey(KeyCode.LeftShift, "Left Shift");
		/// <summary>
		/// The Left Control modifier key.
		/// </summary>
		public static readonly ModifierKey LCtrl = new ModifierKey(KeyCode.LeftControl, "Left Control");
		/// <summary>
		/// The Left Alt modifier key.
		/// </summary>
		public static readonly ModifierKey LAlt = new ModifierKey(KeyCode.LeftAlt, "Left Alt");
		/// <summary>
		/// The Left Windows modifier key.
		/// </summary>
		public static readonly ModifierKey LWindows = new ModifierKey(KeyCode.LeftWindows, "Left Windows");
		/// <summary>
		/// The Left Command modifier key on Apple based keyboards.
		/// </summary>
		public static readonly ModifierKey LCommand = new ModifierKey(KeyCode.LeftCommand, "Left Command");

		/// <summary>
		/// The Right Shift modifier key.
		/// </summary>
		public static readonly ModifierKey RShift = new ModifierKey(KeyCode.RightShift, "Right Shift");
		/// <summary>
		/// The Right Control modifier key.
		/// </summary>
		public static readonly ModifierKey RCtrl = new ModifierKey(KeyCode.RightControl, "Right Control");
		/// <summary>
		/// The Right Alt modifier key.
		/// </summary>
		public static readonly ModifierKey RAlt = new ModifierKey(KeyCode.RightAlt, "Right Alt");
		/// <summary>
		/// The Right Windows modifier key.
		/// </summary>
		public static readonly ModifierKey RWindows = new ModifierKey(KeyCode.RightWindows, "Right Windows");
		/// <summary>
		/// The Right Command modifier key on Apple based keyboards.
		/// </summary>
		public static readonly ModifierKey RCommand = new ModifierKey(KeyCode.RightCommand, "Right Command");

		/// <summary>
		/// Can be used for listing.
		/// </summary>
		public static readonly ModifierKey[] modifiers = new ModifierKey[]{
		None,
		LShift,
		LCtrl,
		LAlt,
		LWindows,
		LCommand,
		RShift,
		RCtrl,
		RAlt,
		RWindows,
		RCommand,
	};
		#endregion

		/// <summary>
		/// Unity's keycode for this Modifier.
		/// </summary>
		public KeyCode UKeyCode { get; private set; }
		/// <summary>
		/// Full name of this modifier key.
		/// </summary>
		public string DisplayName { get; private set; }

		private ModifierKey(KeyCode keyCode, string displayName) {
			this.UKeyCode = keyCode;
			this.DisplayName = displayName;
		}

		/// <summary>
		/// Assists in checking key status.
		/// </summary>
		public static implicit operator KeyCode(ModifierKey modifier) {
			return modifier.UKeyCode;
		}

		/// <summary>
		/// Utility operator overload for listing purposes.
		/// </summary>
		public static implicit operator ModifierKey(ModEnums modifier) {
			return FromMEnum(modifier);
		}

		/// <summary>
		/// Utility method for listing purposes.
		/// </summary>
		/// <param name="me">The <see cref="ModEnums"/> to convert.</param>
		/// <returns>The proper static modifier.</returns>
		public static ModifierKey FromMEnum(ModEnums me) {
			switch(me) {
				case ModEnums.None:
					return None;
				case ModEnums.LShift:
					return LShift;
				case ModEnums.LCtrl:
					return LCtrl;
				case ModEnums.LAlt:
					return LAlt;
				case ModEnums.LWindows:
					return LWindows;
				case ModEnums.LCommand:
					return LCommand;
				case ModEnums.RShift:
					return RShift;
				case ModEnums.RCtrl:
					return RCtrl;
				case ModEnums.RAlt:
					return RAlt;
				case ModEnums.RWindows:
					return RWindows;
				case ModEnums.RCommand:
					return RCommand;
				default:
					throw new ArgumentException();
			}
		}

		/// <summary>
		/// Used with <see cref="FromMEnum"/> for listing options.
		/// </summary>
		public enum ModEnums {
			None,
			LShift,
			LCtrl,
			LAlt,
			LWindows,
			LCommand,
			RShift,
			RCtrl,
			RAlt,
			RWindows,
			RCommand,
		}
	}
}