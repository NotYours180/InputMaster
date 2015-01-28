using BSGTools.IO;
using BSGTools.IO.Xbox;
using BSGTools.Shared;
using UnityEditor;
using UnityEngine;

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