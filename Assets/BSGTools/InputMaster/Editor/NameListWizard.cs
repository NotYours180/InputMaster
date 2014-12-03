using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;


public class NameListWizard : ScriptableWizard {
	private const string TEMPLATE = "/BSGTools/InputMaster/AutoGenTemplates/NameListTemplate.txt";

	List<string> nameDict = new List<string>();
	List<string> valueDict = new List<string>();
	string scriptName = "NameList";

	Vector2 scroll = Vector2.zero;
	CSharpCodeProvider provider = new CSharpCodeProvider();

	[MenuItem("Assets/Create/BSGTools/InputMaster/Create NameList")]
	public static void ShowWizard() {
		ScriptableWizard.DisplayWizard<NameListWizard>("NameList Wizard", "Create NameList");
	}

	void OnWizardCreate() {
		var templateText = File.ReadAllText(Application.dataPath + TEMPLATE, Encoding.Default);
		var sb = new StringBuilder();

		for(int i = 0;i < nameDict.Count;i++) {
			var key = nameDict[i];
			var value = valueDict[i];
			sb.AppendLine("\t" + string.Format(@"public const string {0} = ""{1}"";", key, value));
		}
		templateText = string.Format(templateText, scriptName, sb.ToString().TrimEnd());
		File.WriteAllText(string.Format("{0}/{1}.cs", Application.dataPath, scriptName), templateText);
		AssetDatabase.Refresh();
	}

	void OnWizardUpdate() {
		for(int i = 0;i < nameDict.Count;i++) {
			var key = nameDict[i];
			var value = valueDict[i];
			if(!provider.IsValidIdentifier(key)) {
				errorString = string.Format("'{0}' at index {1} is not a valid C# identifier.", key, i);
				isValid = false;
				return;
			}
			else if(value.Length == 0) {
				errorString = string.Format("'{0}' at index {1} is not a valid control identifier (length must be > 0).", key, i);
				isValid = false;
				return;
			}
		}
		if(!provider.IsValidIdentifier(scriptName)) {
			errorString = "Script name is not a valid C# class identifier.";
			isValid = false;
			return;
		}



		errorString = "";
		isValid = true;
	}

	protected override bool DrawWizardGUI() {
		DrawHeader();
		DrawList();
		return true;
	}

	private void DrawList() {
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		scroll = EditorGUILayout.BeginScrollView(scroll);
		for(int i = 0;i < nameDict.Count;i++) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(i + ":", GUILayout.Width(GUIStyle.none.CalcSize(new GUIContent(nameDict.Count + ":")).x + 2f));
			if(!GUILayout.Button("Delete")) {
				nameDict[i] = EditorGUILayout.TextField(nameDict[i]);
				valueDict[i] = EditorGUILayout.TextField(valueDict[i]);
			}
			else {
				nameDict.RemoveAt(i);
				valueDict.RemoveAt(i);
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
	}

	private void DrawHeader() {
		EditorGUILayout.Space();
		scriptName = EditorGUILayout.TextField("Script Name:", scriptName);
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Add New")) {
			nameDict.Add("NEW_" + Guid.NewGuid().ToString().ToUpper().Split('-')[0]);
			valueDict.Add("New Value");
		}
		if(GUILayout.Button("Remove All")) {
			nameDict.Clear();
			valueDict.Clear();
		}
		EditorGUILayout.EndHorizontal();
	}

}
