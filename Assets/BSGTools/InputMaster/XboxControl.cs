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
using XInputDotNetPure;
#endif

using System;
using UnityEngine;

namespace BSGTools.IO.Xbox {
	/// <summary>
	/// Used for type constraints.
	/// Contains nor enforces any functionality.
	/// </summary>
	public interface IXboxControl { }

	/// <value>
	/// The base class for all Xbox 360 control classes.
	/// </value>
	/// <typeparam name="T">Used for generic self-creation.</typeparam>
	[Serializable]
	public abstract class XboxControl<T> : Control where T : IXboxControl {

		/// <value>
		/// The index of the controller that will manipulate this control's states and values.
		/// This is used in conjunction to the PlayerIndex enumeration in XInput.
		/// </value>
		public byte ControllerIndex { get; private set; }

		/// <summary>
		/// Creates a new <typeparamref name="XboxControl"/>.
		/// </summary>
		/// <param name="controllerIndex"><see cref="ControllerIndex"/></param>
		protected XboxControl(byte controllerIndex) {
			if(controllerIndex < 0 || controllerIndex > 3)
				throw new ArgumentOutOfRangeException("controllerIndex");
			this.ControllerIndex = controllerIndex;
		}

		/// <summary>
		/// Allows for the creation of up to 4 XboxControls at the same time (one for each controller index).
		/// An <typeparamref name="XboxControl"/> must be provided for cloning.
		/// The <see cref="ControllerIndex"/> of the provided clone is ignored.
		/// </summary>
		/// <param name="count">The number of controllers to create this control for.</param>
		/// <param name="toClone">The instance to take values from for clone creation.</param>
		/// <returns>An array of type T, each assigned to a specific controller index.</returns>
		/// <strong>Example</strong>
		/// <code>
		///	<pre>
		///		// Defines a jump control for all 4 players.
		///		XButtonControl[] jump = XButtonControl.CreateMultiple(4, new XButtonControl(0, XButton.A) {
		///			Name = "Jump",
		///			Gravity = 2f,
		///			Sensitivity = 2f
		///		});
		///
		///		// Usage
		///		jump[0].Value; //Player One's jump value
		///		jump[3].Value; //Player Four's jump value
		/// </pre>
		/// </code>
		public static T[] CreateMultiple(byte count, T toClone) {
			if(count < 2 || count > 4)
				throw new ArgumentOutOfRangeException("count");
			var xbc = new T[count];
			for(byte i = 0;i < count;i++)
				xbc[i] = (toClone as XboxControl<T>).CreateClone(i);
			return xbc;
		}

		/// <summary>
		/// Creates a clone of an this XboxControl.
		/// </summary>
		/// <param name="controller">The index of the controller that will manipulate this control's states and values.</param>
		/// <returns></returns>
		protected abstract T CreateClone(byte controller);
	}
}