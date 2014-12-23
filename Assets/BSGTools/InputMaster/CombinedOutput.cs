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
using System.Collections.Generic;

namespace BSGTools.IO {

	/// <summary>
	/// Allows for easy combination of multiple controls' outputs.
	/// </summary>
	[Serializable]
	public class CombinedOutput {
		public delegate float OnAddPost();
		public event OnAddPost AddPostClamped;
		public event OnAddPost AddPost;

		/// <value>
		/// The clamped, combined FixedValue as a float.
		/// </value>
		public float fixedValueF {
			get {
				var controls = this.controls;
				foreach(var c in controls.OfType<XboxControl>())
					c.currentController = controllerIndex;
				float total = 0f;
				foreach(var c in controls)
					total += c.fixedValue;

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
		public sbyte fixedValue {
			get {
				var controls = this.controls;
				foreach(var c in controls.OfType<XboxControl>())
					c.currentController = controllerIndex;
				float total = 0f;
				foreach(var c in controls)
					total += c.fixedValue;

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
		public float value {
			get {
				var controls = this.controls;
				foreach(var c in controls.OfType<XboxControl>())
					c.currentController = controllerIndex;
				float total = 0f;
				foreach(var c in controls)
					total += c.value;

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
		public bool anyDownPositive {
			get {
				return controls.Any(c => c.down == ControlState.Positive);
			}
		}
		/// <value>
		/// Are any controls in a Down- state?
		/// </value>
		public bool anyDownNegative {
			get {
				return controls.Any(c => c.down == ControlState.Negative);
			}
		}

		/// <value>
		/// Are any controls in a Held+ state?
		/// </value>
		public bool anyHeldPositive {
			get {
				return controls.Any(c => c.held == ControlState.Positive);
			}
		}
		/// <value>
		/// Are any controls in a Held- state?
		/// </value>
		public bool anyHeldNegative {
			get {
				return controls.Any(c => c.held == ControlState.Negative);
			}
		}

		/// <value>
		/// Are any controls in a Up+ state?
		/// </value>
		public bool anyUpPositive {
			get {
				return controls.Any(c => c.up == ControlState.Positive);
			}
		}
		/// <value>
		/// Are any controls in a Up- state?
		/// </value>
		public bool anyUpNegative {
			get {
				return controls.Any(c => c.up == ControlState.Negative);
			}
		}
		#endregion

		public void SetBlockedAll(bool blocked) {
			var controls = this.controls;
			foreach(var c in controls)
				c.blocked = blocked;
		}

		/// <value>
		/// The combined Controls.
		/// </value>
		public List<string> identifiers = new List<string>();

		public string identifier = "new_" + Guid.NewGuid().ToString().ToUpper().Split('-')[0];
		public byte controllerIndex = 0;

		IEnumerable<Control> controls {
			get {
				var io = InputMaster.instance;
				return identifiers.Select(s => io.GetControl(s));
			}
		} 
	}
}