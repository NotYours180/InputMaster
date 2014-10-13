/**
Copyright (c) 2014, Michael Notarnicola
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
using BSGTools.IO.Xbox;
#endif


namespace BSGTools.IO.Tools {
	public class InputManagerWizard : EditorWindow {
		#region Templates
		const string INPUT_MASTER_LOCATION = "/BSGTools/InputMaster/AutoGenTemplates/";
		const string MASTER_TEMPLATE_LOCATION = INPUT_MASTER_LOCATION + "InputManagerTemplate.txt";
		const string KEYCONTROL_TEMPLATE_LOCATION = INPUT_MASTER_LOCATION + "KeyControlTemplate.txt";
		const string XBUTTONCONTROL_TEMPLATE_LOCATION = INPUT_MASTER_LOCATION + "XButtonControlTemplate.txt";
		const string XSTICKCONTROL_TEMPLATE_LOCATION = INPUT_MASTER_LOCATION + "XStickControlTemplate.txt";
		const string XTRIGGERCONTROL_TEMPLATE_LOCATION = INPUT_MASTER_LOCATION + "XTriggerControlTemplate.txt";
		const string XBUTTONCONTROLMULTI_TEMPLATE_LOCATION = INPUT_MASTER_LOCATION + "XButtonControlTemplateMulti.txt";
		const string XSTICKCONTROLMULTI_TEMPLATE_LOCATION = INPUT_MASTER_LOCATION + "XStickControlTemplateMulti.txt";
		const string XTRIGGERCONTROLMULTI_TEMPLATE_LOCATION = INPUT_MASTER_LOCATION + "XTriggerControlTemplateMulti.txt";
		#endregion

		static readonly string[] reserved = { "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while" };

		Dictionary<Control, ControlMetadata> controlDict = new Dictionary<Control, ControlMetadata>();
		bool[] foldout = new bool[4];
		Vector2 scroll = Vector2.zero;
		string scriptName = "InputManager";

		FieldInfo[] _modFields = null;
		FieldInfo[] ModFields {
			get {
				if(_modFields == null)
					_modFields = typeof(ModifierKey).GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => f.FieldType == typeof(ModifierKey)).ToArray();
				return _modFields;
			}
		}

		#region GUI Functions
		[MenuItem("Assets/Create/BSGTools/InputMaster/Create InputManager")]
		public static void ShowWizard() {
			var wizard = EditorWindow.GetWindow<InputManagerWizard>(true, "InputManager Generator");
		}

		void OnGUI() {
			//Draw Create Section
			EditorGUILayout.LabelField("Create", EditorStyles.boldLabel);
			scriptName = EditorGUILayout.TextField("Script Name: ", scriptName);

			var createRequested = GUILayout.Button(string.Format(@"Create InputManager ""{0}""", scriptName));
			var hasErrors = FillErrorList();

			if(createRequested && hasErrors == false && EditorUtility.DisplayDialog("Confirm Creation.", "Are you sure?", "Create", "Cancel"))
				CreateInputManager();

			GUILayout.Space(20f);

			DrawCreateButtons();

			GUILayout.Space(20f);

			//Draw Control List
			EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);
			scroll = EditorGUILayout.BeginScrollView(scroll);
			DrawControls<KeyControl>(0);
#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
			DrawControls<XButtonControl>(1);
			DrawControls<XStickControl>(2);
			DrawControls<XTriggerControl>(3);
#endif
			EditorGUILayout.EndScrollView();
		}

		private bool FillErrorList() {
			var sb = new StringBuilder();
			if(controlDict.Count == 0)
				sb.AppendLine("Must have at least one control.");
			if(controlDict.Values.Any(c => string.IsNullOrEmpty(c.varName.Trim())))
				sb.AppendLine("C# var names must all follow valid C# variable syntax.");
			if(controlDict.Values.Select(v => v.varName).Distinct().Count() != controlDict.Count)
				sb.AppendLine("All C# var names must be unique");
			EditorGUILayout.LabelField("Error Status", EditorStyles.boldLabel);
			EditorGUILayout.TextArea(sb.ToString());
			return sb.Length > 0;
		}

		private void DrawCreateButtons() {
			EditorGUILayout.LabelField("Contents", EditorStyles.boldLabel);

			Control c = null;
			if(GUILayout.Button("Create New KeyControl"))
				c = new KeyControl(KeyCode.A);
#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
			else if(GUILayout.Button("Create New XButtonControl"))
				c = new XButtonControl(XButton.A, XButton.None);
			else if(GUILayout.Button("Create New XStickControl"))
				c = new XStickControl(XStick.StickLeft);
			else if(GUILayout.Button("Create New XTriggerControl"))
				c = new XTriggerControl(XTrigger.TriggerLeft);
#endif

			if(c != null)
				controlDict.Add(c, new ControlMetadata() { expanded = false, varName = c.Name.ToLower(), createCount = 0 });
		}

		void DrawControls<T>(int foldoutIndex) where T : Control {
			var typeFiltered = controlDict.Keys.OfType<T>();
			var count = typeFiltered.Count();
			foldout[foldoutIndex] = (count == 0) ? false : foldout[foldoutIndex];
			foldout[foldoutIndex] = EditorGUILayout.Foldout(foldout[foldoutIndex], typeof(T).Name + "s: " + typeFiltered.Count());
			if(foldout[foldoutIndex]) {
				for(int i = 0;i < typeFiltered.Count();i++) {
					var c = typeFiltered.ElementAt(i);
					var info = controlDict[c];

					EditorGUI.indentLevel = 2;
					info.expanded = EditorGUILayout.Foldout(info.expanded, c.Name);
					EditorGUI.indentLevel = 4;
					if(EditorGUILayout.Toggle("Delete", false) && EditorUtility.DisplayDialog("Confirm Delete.", "Are you sure?", "Delete", "Cancel")) {
						controlDict.Remove(c);
						continue;
					}
					if(info.expanded)
						DrawControlProperties<T>(c, ref info);
					controlDict[c] = info;
				}
			}
			EditorGUI.indentLevel = 0;
		}

		void DrawControlProperties<T>(T c, ref  ControlMetadata info) where T : Control {
			c.Name = EditorGUILayout.TextField("Name:", c.Name);

			var newVarName = EditorGUILayout.TextField("C# Var Name: ", info.varName);
			if(Regex.IsMatch(newVarName, "^[a-zA-Z_][a-zA-Z0-9_]*$") && reserved.Contains(newVarName) == false)
				info.varName = newVarName;


			if(c is KeyControl) {
				var kc = c as KeyControl;


				var newPositive = (KeyCode)EditorGUILayout.EnumPopup("Positive: ", kc.Positive);
				if(newPositive != KeyCode.None)
					kc.Positive = newPositive;
				else
					EditorUtility.DisplayDialog("Error!", "Positive Binding Cannot Be KeyCode.None!", "I Apologize");


				kc.Negative = (KeyCode)EditorGUILayout.EnumPopup("Negative: ", kc.Negative);
				info.modEnum = (ModifierKey.ModEnums)EditorGUILayout.EnumPopup("Modifier: ", info.modEnum);
				kc.Modifier = ModifierKey.FromMEnum(info.modEnum);
			}

#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
			else if(c is XButtonControl)
				info = DrawXboxSpecific<XButtonControl>(c as XButtonControl, info);
			else if(c is XStickControl)
				info = DrawXboxSpecific<XStickControl>(c as XStickControl, info);
			else if(c is XTriggerControl)
				info = DrawXboxSpecific<XTriggerControl>(c as XTriggerControl, info);
#endif

			c.IsDebugControl = EditorGUILayout.Toggle("Debug Control: ", c.IsDebugControl);
			c.Sensitivity = Mathf.Clamp(EditorGUILayout.FloatField("Sensitivity: ", c.Sensitivity), 0f, Mathf.Infinity);
			c.Gravity = Mathf.Clamp(EditorGUILayout.FloatField("Gravity: ", c.Gravity), 0f, Mathf.Infinity);
			c.Dead = Mathf.Clamp(EditorGUILayout.FloatField("Dead: ", c.Dead), 0f, Mathf.Infinity);
			c.Snap = EditorGUILayout.Toggle("Snap: ", c.Snap);
			c.Invert = EditorGUILayout.Toggle("Invert: ", c.Invert);
			c.IsBlocked = EditorGUILayout.Toggle("Is Blocked: ", c.IsBlocked);

			controlDict[c] = info;
		}

#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
		ControlMetadata DrawXboxSpecific<T>(T c, ControlMetadata info) where T : IXboxControl {
			var xc = c as XboxControl<T>;

			if(c is XButtonControl) {
				var xbc = c as XButtonControl;
				var newPositive = (XButton)EditorGUILayout.EnumPopup("Positive: ", xbc.Positive);
				if(newPositive != XButton.None)
					xbc.Positive = newPositive;
				else
					EditorUtility.DisplayDialog("Error!", "Positive Binding Cannot Be XButton.None!", "I Apologize");
				xbc.Negative = (XButton)EditorGUILayout.EnumPopup("Negative: ", xbc.Negative);
			}
			else if(c is XStickControl) {
				var xsc = c as XStickControl;
				xsc.Stick = (XStick)EditorGUILayout.EnumPopup("Stick: ", xsc.Stick);
			}
			else if(c is XTriggerControl) {
				var xtc = c as XTriggerControl;
				xtc.Trigger = (XTrigger)EditorGUILayout.EnumPopup("Trigger: ", xtc.Trigger);
			}

			info.createCount = EditorGUILayout.IntSlider("Count: ", info.createCount, 1, 4);

			return info;
		}
#endif
		#endregion

		#region Meta Functions
		void CreateInputManager() {
			var templateText = File.ReadAllText(Application.dataPath + MASTER_TEMPLATE_LOCATION, Encoding.Default);

			var sb = new StringBuilder();

			var kcs = controlDict.Keys.OfType<KeyControl>();
			if(kcs.Count() > 0) {
				sb.AppendLine("\t#region KeyControls");
				foreach(var c in kcs)
					sb.AppendLine(GetKeyControlString(c));
				sb.AppendLine("\t#endregion");
				sb.AppendLine();
			}

#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
			var xbs = controlDict.Keys.OfType<XButtonControl>();
			if(xbs.Count() > 0) {
				sb.AppendLine("\t#region XButtonControls");
				foreach(var c in xbs)
					sb.AppendLine(GetXButtonControlString(c));
				sb.AppendLine("\t#endregion");
				sb.AppendLine();
			}

			var xss = controlDict.Keys.OfType<XStickControl>();
			if(xss.Count() > 0) {
				sb.AppendLine("\t#region XStickControls");
				foreach(var c in xss)
					sb.AppendLine(GetXStickControlString(c));
				sb.AppendLine("\t#endregion");
				sb.AppendLine();
			}

			var xts = controlDict.Keys.OfType<XTriggerControl>();
			if(xts.Count() > 0) {
				sb.AppendLine("\t#region XTriggerControls");
				foreach(var c in xts)
					sb.AppendLine(GetXTriggerControlString(c));
				sb.AppendLine("\t#endregion");
				sb.AppendLine();
			}
#endif

			var finalText = string.Format(templateText, scriptName, sb.ToString());

			File.WriteAllText(string.Format("{0}/{1}.cs", Application.dataPath, scriptName), finalText);
			AssetDatabase.Refresh();
			this.Close();
		}

		string GetKeyControlString(KeyControl control) {
			var argList = new object[]{
				controlDict[control].varName,
				control.Positive,
				control.Negative,
				GetModStr(control.Modifier),
				control.Dead,
				control.Gravity,
				control.Invert.ToString().ToLower(),
				control.IsBlocked.ToString().ToLower(),
				control.IsDebugControl.ToString().ToLower(),
				control.Name,
				control.Sensitivity,
				control.Snap.ToString().ToLower()
			};
			var format = File.ReadAllText(Application.dataPath + KEYCONTROL_TEMPLATE_LOCATION);
			return string.Format(format, argList);
		}

		private string GetModStr(ModifierKey modifier) {
			foreach(var f in ModFields)
				Debug.Log((f.GetValue(null) as ModifierKey).UKeyCode);
			var field = ModFields.Single(f => (f.GetValue(null) as ModifierKey).UKeyCode == modifier.UKeyCode);
			return typeof(ModifierKey).Name + field.Name;
		}

#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
		string GetXButtonControlString(XButtonControl control) {
			var meta = controlDict[control];
			var multi = meta.createCount > 1;
			var argList = (multi) ? new object[]{
				meta.varName,
				meta.createCount,
				control.Positive,
				control.Negative,
				control.Dead,
				control.Gravity,
				control.Invert.ToString().ToLower(),
				control.IsBlocked.ToString().ToLower(),
				control.IsDebugControl.ToString().ToLower(),
				control.Name,
				control.Sensitivity,
				control.Snap.ToString().ToLower()
			} : new object[]{
				meta.varName,
				control.Positive,
				control.Negative,
				control.Dead,
				control.Gravity,
				control.Invert.ToString().ToLower(),
				control.IsBlocked.ToString().ToLower(),
				control.IsDebugControl.ToString().ToLower(),
				control.Name,
				control.Sensitivity,
				control.Snap.ToString().ToLower()
			};
			var format = File.ReadAllText(Application.dataPath + ((multi) ? XBUTTONCONTROLMULTI_TEMPLATE_LOCATION : XBUTTONCONTROL_TEMPLATE_LOCATION));
			return string.Format(format, argList);
		}

		string GetXStickControlString(XStickControl control) {
			var meta = controlDict[control];
			var multi = meta.createCount > 1;
			var argList = (multi) ? new object[]{
				meta.varName,
				meta.createCount,
				control.Stick,
				control.Dead,
				control.Gravity,
				control.Invert.ToString().ToLower(),
				control.IsBlocked.ToString().ToLower(),
				control.IsDebugControl.ToString().ToLower(),
				control.Name,
				control.Sensitivity,
				control.Snap.ToString().ToLower()
			} : new object[]{
				meta.varName,
				control.Stick,
				control.Dead,
				control.Gravity,
				control.Invert.ToString().ToLower(),
				control.IsBlocked.ToString().ToLower(),
				control.IsDebugControl.ToString().ToLower(),
				control.Name,
				control.Sensitivity,
				control.Snap.ToString().ToLower()
			};
			var format = File.ReadAllText(Application.dataPath + ((multi) ? XSTICKCONTROLMULTI_TEMPLATE_LOCATION : XSTICKCONTROL_TEMPLATE_LOCATION));
			return string.Format(format, argList);
		}

		string GetXTriggerControlString(XTriggerControl control) {
			var meta = controlDict[control];
			var multi = meta.createCount > 1;
			var argList = (multi) ? new object[]{
				meta.varName,
				meta.createCount,
				control.Trigger,
				control.Dead,
				control.Gravity,
				control.Invert.ToString().ToLower(),
				control.IsBlocked.ToString().ToLower(),
				control.IsDebugControl.ToString().ToLower(),
				control.Name,
				control.Sensitivity,
				control.Snap.ToString().ToLower()
			} : new object[]{
				meta.varName,
				control.Trigger,
				control.Dead,
				control.Gravity,
				control.Invert.ToString().ToLower(),
				control.IsBlocked.ToString().ToLower(),
				control.IsDebugControl.ToString().ToLower(),
				control.Name,
				control.Sensitivity,
				control.Snap.ToString().ToLower()
			};
			var format = File.ReadAllText(Application.dataPath + ((multi) ? XTRIGGERCONTROLMULTI_TEMPLATE_LOCATION : XTRIGGERCONTROL_TEMPLATE_LOCATION));
			return string.Format(format, argList);
		}
#endif
		#endregion

		struct ControlMetadata {
			public bool expanded;
			public string varName;
			public int createCount;
			public ModifierKey.ModEnums modEnum;
		}
	}
}