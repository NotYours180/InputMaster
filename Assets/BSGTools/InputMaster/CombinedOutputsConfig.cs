using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BSGTools.IO.Xbox;

namespace BSGTools.IO {
	public class CombinedOutputsConfig : ScriptableObject {

		public List<CombinedOutput> outputs = new List<CombinedOutput>();

		public StandaloneControlConfig sConfig;
		public XboxControlConfig xConfig;

	}
}