using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BSGTools.IO.Xbox {
	[Serializable]
	public class XboxControlConfig : ScriptableObject {
		public List<XboxControl> controls = new List<XboxControl>();
	}
}