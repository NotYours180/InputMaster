using UnityEngine;
using BSGTools.IO;
using BSGTools.Structure;
using UnityEngine.UI;
#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
using BSGTools.IO.Xbox;
#endif

public class InputManager : Singleton<InputManager> {

	public InputMaster Master { get; private set; }

	[SerializeField]
	Text txt;
	[SerializeField]
	Text txt2;

	#region KeyControls
	[HideInInspector]
	public KeyControl test = new KeyControl(KeyCode.UpArrow, KeyCode.DownArrow) {
		Modifier = ModifierKey.None,
		Dead = 0f,
		Gravity = 1f,
		Invert = false,
		IsBlocked = false,
		IsDebugControl = false,
		Name = "UNNAMED_a8931fd5",
		Sensitivity = 1f,
		Snap = false
	};
	[HideInInspector]
	public KeyControl test2 = new KeyControl(KeyCode.W, KeyCode.A) {
		Modifier = ModifierKey.None,
		Dead = 0f,
		Gravity = 1f,
		Invert = false,
		IsBlocked = false,
		IsDebugControl = false,
		Name = "UNNAMED_e276ebfa",
		Sensitivity = 1f,
		Snap = false
	};
	#endregion

	public CombinedOutput co;


	void Awake() {
		co = new CombinedOutput(test, test2);
		Master = InputMaster.CreateMaster(this);
	}

	void Update() {
		var fv = test.Down == ControlState.Positive;
		if(fv)
			print("fv:" + Time.frameCount);
		var gar = Input.GetKeyDown(KeyCode.UpArrow);
		if(gar)
			print("gar:" + Time.frameCount);
		var equal = fv == gar;
		txt.text = equal.ToString();
		print(fv + " " + gar);
	}
}