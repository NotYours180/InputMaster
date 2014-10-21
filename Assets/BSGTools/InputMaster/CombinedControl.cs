/**
Copyright (c) 2014, Michael Notarnicola
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Text;
using BSGTools.IO.Xbox;
using UnityEngine;

namespace BSGTools.IO {
	/// <summary>
	/// Allows for easy combination of two control's outputs.
	/// </summary>
	/// <typeparam name="C1">Any control.</typeparam>
	/// <typeparam name="C2">Any control.</typeparam>
	public class CombinedControl<C1, C2>
		where C1 : Control
		where C2 : Control {

		public float FixedValueF {
			get {
				return Control.RoundFixedF(Control1.FixedValue + Control2.FixedValue);
			}
		}
		public sbyte FixedValue {
			get {
				return Control.RoundFixed(Control1.FixedValue + Control2.FixedValue);
			}
		}

		public float Value {
			get {
				return Control.ClampRange(Control1.Value + Control2.Value);
			}
		}

		/// <value>
		/// The first control to combine.
		/// </value>
		public C1 Control1 { get; private set; }
		/// <value>
		/// The second control to combine.
		/// </value>
		public C2 Control2 { get; private set; }

		protected CombinedControl(C1 c1, C2 c2) {
			this.Control1 = c1;
			this.Control2 = c2;
		}
	}
}