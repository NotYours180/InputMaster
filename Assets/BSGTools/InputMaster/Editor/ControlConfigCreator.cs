using UnityEngine;
using System.Collections;
using UnityEditor;
using BSGTools.IO;
using BSGTools.IO.Xbox;

namespace BSGTools.Editors {
	public class ControlConfigCreator : MonoBehaviour {

		[MenuItem("BSGTools/InputMaster/Create New StandaloneControlConfig")]
		public static void CreateStandaloneConfig() {
			ScriptableObjectUtility.CreateAssetFrom<StandaloneControlConfig>();
		}

		[MenuItem("BSGTools/InputMaster/Create New XboxControlConfig")]
		public static void CreateXboxConfig() {
			ScriptableObjectUtility.CreateAssetFrom<XboxControlConfig>();
		}

		[MenuItem("BSGTools/InputMaster/Create New CombinedOutputsConfig")]
		public static void CreateCombinedOutputsConfig() {
			ScriptableObjectUtility.CreateAssetFrom<CombinedOutputsConfig>();
		}
	}
}