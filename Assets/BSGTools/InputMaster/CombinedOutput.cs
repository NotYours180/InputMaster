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
using System.Linq;

namespace BSGTools.IO {

	/// <summary>
	/// Allows for easy combination of multiple controls' outputs.
	/// </summary>
	public class CombinedOutput {
		public delegate float OnAddPost();
		public event OnAddPost AddPostClamped;
		public event OnAddPost AddPost;

		/// <value>
		/// The clamped, combined FixedValue as a float.
		/// </value>
		public float FixedValueF {
			get {
				float total = 0f;
				foreach(var c in Controls)
					total += c.FixedValue;

				float post = 0f;
				float postClamped = 0f;
				if(AddPost != null)
					post = AddPost();
				if(AddPostClamped != null)
					postClamped = AddPostClamped();
				return Control.ClampRange(total + postClamped) + post;
			}
		}

		/// <value>
		/// The clamped, combined FixedValue.
		/// </value>
		public sbyte FixedValue {
			get {
				float total = 0f;
				foreach(var c in Controls)
					total += c.FixedValue;

				float post = 0f;
				float postClamped = 0f;
				if(AddPost != null)
					post = AddPost();
				if(AddPostClamped != null)
					postClamped = AddPostClamped();
				return (sbyte)(Control.ClampRange(total + postClamped) + post);
			}
		}

		/// <value>
		/// The clamped, combined Value.
		/// </value>
		public float Value {
			get {
				float total = 0f;
				foreach(var c in Controls)
					total += c.Value;

				float post = 0f;
				float postClamped = 0f;
				if(AddPost != null)
					post = AddPost();
				if(AddPostClamped != null)
					postClamped = AddPostClamped();
				return Control.ClampRange(total + postClamped) + post;
			}
		}

		#region Control States
		/// <value>
		/// Are any controls in a Down+ state?
		/// </value>
		public bool AnyDownPositive {
			get {
				return Controls.Any(c => c.Down == ControlState.Positive);
			}
		}
		/// <value>
		/// Are any controls in a Down- state?
		/// </value>
		public bool AnyDownNegative {
			get {
				return Controls.Any(c => c.Down == ControlState.Negative);
			}
		}

		/// <value>
		/// Are any controls in a Held+ state?
		/// </value>
		public bool AnyHeldPositive {
			get {
				return Controls.Any(c => c.Held == ControlState.Positive);
			}
		}
		/// <value>
		/// Are any controls in a Held- state?
		/// </value>
		public bool AnyHeldNegative {
			get {
				return Controls.Any(c => c.Held == ControlState.Negative);
			}
		}

		/// <value>
		/// Are any controls in a Up+ state?
		/// </value>
		public bool AnyUpPositive {
			get {
				return Controls.Any(c => c.Up == ControlState.Positive);
			}
		}
		/// <value>
		/// Are any controls in a Up- state?
		/// </value>
		public bool AnyUpNegative {
			get {
				return Controls.Any(c => c.Up == ControlState.Negative);
			}
		}
		#endregion

		/// <value>
		/// The combined Controls.
		/// </value>
		public Control[] Controls { get; private set; }

		/// <summary>
		/// Creates a new CombinedOutput.
		/// </summary>
		/// <param name="controls">The controls to combine into a single output.</param>
		public CombinedOutput(params Control[] controls) {
			this.Controls = controls;
		}
	}
}