using System.Collections.Generic;
using UnityEngine;

namespace BSGTools.IO {
	public class StandaloneControlConfig : ScriptableObject {
		public List<StandaloneControl> controls = new List<StandaloneControl>();
	}
}