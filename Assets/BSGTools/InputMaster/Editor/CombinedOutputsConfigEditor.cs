using System.Collections.Generic;
using System.Linq;
using BSGTools.IO;
using BSGTools.IO.Xbox;
using UnityEditor;
using UnityEngine;

namespace BSGTools.Editors {
	[CustomEditor(typeof(CombinedOutputsConfig))]
	public class CombinedOutputsConfigEditor : Editor {
		public CombinedOutputsConfig config;
		public Vector2 scroll;
		public List<bool> foldouts;
		public string filterStr;
		public bool filterFoldout;
		public List<int> lastSelectedID;

		void OnEnable() {
			scroll = Vector2.zero;
			config = target as CombinedOutputsConfig;
			foldouts = Enumerable.Repeat(false, config.outputs.Count).ToList();
			lastSelectedID = Enumerable.Repeat(0, config.outputs.Count).ToList();
		}

		public override void OnInspectorGUI() {
			DrawHeaderControls();
			DrawCombinedOutputs();
			if(GUI.changed)
				EditorUtility.SetDirty(target);
		}

		private void DrawCombinedOutputs() {
			scroll = EditorGUILayout.BeginScrollView(scroll);

			//Generate control name list
			var names = new List<string>();
			if(config.sConfig != null)
				names.AddRange(config.sConfig.controls.Select(c => c.identifier));
			if(config.xConfig != null)
				names.AddRange(config.xConfig.LinqSelect(c => c.identifier));

			for(int i = 0;i < config.outputs.Count;i++) {
				//Setup foldout
				EditorGUI.indentLevel = 1;
				var co = config.outputs[i];
				foldouts[i] = EditorGUILayout.Foldout(foldouts[i], (i + 1) + ": " + co.identifier);

				if(foldouts[i]) {
					//Header controls
					EditorGUI.indentLevel = 2;
					GUILayout.BeginHorizontal();
					EditorExts.GUILayoutIndent(2);
					var shouldDel = GUILayout.Button("Delete");
					GUILayout.EndHorizontal();
					if(shouldDel && EditorUtility.DisplayDialog("Confirm", "Are you sure you'd like to delete this output?", "DELETE", "CANCEL")) {
						config.outputs.RemoveAt(i);
						foldouts.RemoveAt(i);
						lastSelectedID.RemoveAt(i);
						break;
					}

					co.identifier = EditorGUILayout.TextField("Identifier", co.identifier);
					co.controllerIndex = (byte)EditorGUILayout.IntSlider(new GUIContent("Controller", "The index of the controller to get Xbox control values from."), co.controllerIndex, 0, 3);

					//Identifier selector and button
					var selectableNames = names.Where(s => !co.identifiers.Contains(s)).ToList();
					EditorGUILayout.BeginHorizontal();
					EditorExts.GUILayoutIndent();
					if(selectableNames.Count == 0)
						GUI.enabled = false;
					var shouldAdd = GUILayout.Button("Add New") && co.identifiers.Count < names.Count;
					lastSelectedID[i] = Mathf.Clamp(lastSelectedID[i], 0, selectableNames.Count);
					lastSelectedID[i] = EditorGUILayout.Popup(lastSelectedID[i], selectableNames.ToArray());
					if(shouldAdd)
						co.identifiers.Add(selectableNames[lastSelectedID[i]]);
					GUI.enabled = true;
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Separator();

					//Identifier list
					EditorGUI.indentLevel = 3;
					for(int ii = 0;ii < co.identifiers.Count;ii++) {
						var idAt = co.identifiers[ii];

						//delete if no longer a valid ID
						if(names.Contains(idAt) == false) {
							co.identifiers.RemoveAt(ii);
							continue;
						}
						EditorGUILayout.BeginHorizontal();
						EditorExts.GUILayoutIndent();
						if(GUILayout.Button("Delete"))
							co.identifiers.RemoveAt(ii);
						EditorGUILayout.LabelField(idAt);
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.Separator();
				}
			}
			EditorGUI.indentLevel = 0;
			EditorGUILayout.EndScrollView();
		}

		private void DrawHeaderControls() {
			EditorGUI.indentLevel = 0;
			var centeredBold = GUI.skin.label;
			centeredBold.alignment = TextAnchor.UpperCenter;
			centeredBold.fontStyle = FontStyle.Bold;

			EditorGUILayout.LabelField("CombinedOutputs Configuration", centeredBold);
			EditorGUILayout.Space();
			config.sConfig = (StandaloneControlConfig)EditorGUILayout.ObjectField("Standalone Config", config.sConfig, typeof(StandaloneControlConfig));
			config.xConfig = (XboxControlConfig)EditorGUILayout.ObjectField("Xbox Config", config.xConfig, typeof(XboxControlConfig));
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("New Output")) {
				config.outputs.Add(new CombinedOutput());
				foldouts.Add(false);
				lastSelectedID.Add(0);
			}
			if(GUILayout.Button("Delete All") && EditorUtility.DisplayDialog("Confirm", "Are you sure you'd like to DELETE ALL OUTPUTS?", "DELETE ALL", "CANCEL")) {
				config.outputs.Clear();
				foldouts.Clear();
				lastSelectedID.Clear();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Expand All"))
				for(int i = 0;i < foldouts.Count;i++)
					foldouts[i] = true;
			if(GUILayout.Button("Collapse All"))
				for(int i = 0;i < foldouts.Count;i++)
					foldouts[i] = false;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
		}


	}
}