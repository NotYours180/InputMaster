using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BSGTools.IO.Xbox {
	[Serializable]
	public class XboxControlConfig : ScriptableObject {
		public List<XButtonControl> xbControls;
		public List<XStickControl> xsControls;
		public List<XTriggerControl> xtControls;
	}
}