using System.Collections.Generic;
using BSGTools.IO.Xbox;
using UnityEngine;

namespace BSGTools.IO {
	public class CombinedOutputsConfig : ScriptableObject {

		public List<CombinedOutput> outputs = new List<CombinedOutput>();

		public StandaloneControlConfig sConfig;
		public XboxControlConfig xConfig;

	}
}