using UnityEngine;
using System.Collections;

namespace BSGTools.IO.Xbox {
	public class XboxControlConfig : ScriptableObject {
		public XButtonControl[] xButtonControls;
		public XStickControl[] xStickControls;
		public XTriggerControl[] xTriggerControls;
	}
}