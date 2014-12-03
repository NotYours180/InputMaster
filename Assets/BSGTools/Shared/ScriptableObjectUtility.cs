using UnityEditor;
using UnityEngine;

public static class ScriptableObjectUtility {
	public static void CreateAssetFrom<T>() where T : ScriptableObject {
		var path = EditorUtility.SaveFilePanel("Save Location", Application.dataPath, "New " + typeof(T).Name, "asset");

		if(string.IsNullOrEmpty(path))
			return;

		//Get project relative path and ensure path is within project
		var projectRelative = FileUtil.GetProjectRelativePath(path);
		if(string.IsNullOrEmpty(projectRelative)) {
			EditorUtility.DisplayDialog("Error", "Please select somewhere within your assets folder.", "OK");
			return;
		}

		var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(projectRelative);

		var scriptableObject = ScriptableObject.CreateInstance<T>();
		AssetDatabase.CreateAsset(scriptableObject, assetPathAndName);
		AssetDatabase.SaveAssets();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = scriptableObject;
	}
}