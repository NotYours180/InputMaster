using UnityEngine;
using System.Collections;
using UnityEditor;
using BSGTools.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using BSGTools.IO.Xbox;

namespace BSGTools.Editors {
	[CustomEditor(typeof(CombinedOutputsConfig))]
	public class CombinedOutputsConfigEditor : Editor {
		public CombinedOutputsConfig coConfig;
		public StandaloneControlConfig sConfig;
		public XboxControlConfig xConfig;
		public Vector2 scroll;
		public List<bool> foldouts;
		public string filterStr;
		public bool filterFoldout;
		public int lastSelectedID = 0;

		void OnEnable() {
			scroll = Vector2.zero;
			coConfig = target as CombinedOutputsConfig;
			foldouts = Enumerable.Repeat(false, coConfig.outputs.Count).ToList();
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
			if(sConfig != null)
				names.AddRange(sConfig.controls.Select(c => c.identifier));
			if(xConfig != null)
				names.AddRange(xConfig.controls.Select(c => c.identifier));

			for(int i = 0;i < coConfig.outputs.Count;i++) {
				//Setup foldout
				EditorGUI.indentLevel = 1;
				var co = coConfig.outputs[i];
				foldouts[i] = EditorGUILayout.Foldout(foldouts[i], (i + 1) + ": " + co.identifier);

				if(foldouts[i]) {
					//Header controls
					EditorGUI.indentLevel = 2;
					co.identifier = EditorGUILayout.TextField("Identifier", co.identifier);
					co.controllerIndex = (byte)EditorGUILayout.IntSlider(new GUIContent("Controller", "The index of the controller to get Xbox control values from."), co.controllerIndex, 0, 3);

					//Identifier selector and button
					var selectableNames = names.Where(s => !co.identifiers.Contains(s)).ToList();
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(EditorGUI.indentLevel * 18);
					if(selectableNames.Count == 0)
						GUI.enabled = false;
					var shouldAdd = GUILayout.Button("Add New") && co.identifiers.Count < names.Count;
					GUI.enabled = true;
					Mathf.Clamp(lastSelectedID, 0, selectableNames.Count);
					lastSelectedID = EditorGUILayout.Popup(lastSelectedID, selectableNames.ToArray());
					if(shouldAdd)
						co.identifiers.Add(selectableNames[lastSelectedID]);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Separator();

					//Identifier list
					EditorGUI.indentLevel = 3;
					for(int ii = 0;ii < co.identifiers.Count;ii++) {
						var idAt = co.identifiers[ii];
						if(names.Contains(idAt) == false) {
							co.identifiers.RemoveAt(ii);
							continue;
						}
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(EditorGUI.indentLevel * 18);
						if(GUILayout.Button("Delete"))
							co.identifiers.RemoveAt(ii);
						EditorGUILayout.LabelField(idAt);
						EditorGUILayout.EndHorizontal();
					}
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
			sConfig = (StandaloneControlConfig)EditorGUILayout.ObjectField("Standalone Config", sConfig, typeof(StandaloneControlConfig));
			xConfig = (XboxControlConfig)EditorGUILayout.ObjectField("Xbox Config", xConfig, typeof(XboxControlConfig));
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("New CombinedOutput")) {
				coConfig.outputs.Add(new CombinedOutput());
				foldouts.Add(false);
			}
			if(GUILayout.Button("Remove All")) {
				coConfig.outputs.Clear();
				foldouts.Clear();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

		}


	}
}