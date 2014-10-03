# README #

## InputMaster ##

A revolution of Input management for [Unity](http://unity3d.com/) designed for programmers, by a programmer.

**Latest Version: v5.0.0**

Uses Semantic Versioning (Major.Minor.Fixes):

Major is obvious -- Some giant change or addition to the project was made. Minor versions normally indicate new features or existing feature updates, and fixes are... fixes.

This was originally part of a larger project of random shit called BSGTools (BirdStreet Games Tools for short). This large package is being discontinued, and instead, its individual components will be distributed, open source, right here on GitHub. It is designed to be plug-n-play of sorts. Importing any BSGTools component .unitypackage file will import under a single BSGTools directory to keep everything nice and clean. Please note that during this update process, some documentation will be out of date until it can be updated.

[BSGTools Unity Community Wiki Page](http://wiki.unity3d.com/index.php/BSGTools)

---
### Legal Stuff ###

If I have to clarify one thing, its this: I want the community to be able to contribute and modify what they like. I love the idea of open sourcing these sorts of things to help the community be better at what they do. However, I also want to make sure that I'm credited for all of the hard work I put into making these in the first place. That's why all of the code here will be licensed under the BSD 3-Clause License ([http://opensource.org/licenses/BSD-3-Clause](READ MORE ON THE LICENSE)). Basically, this means (from the previous link, edited for brevity):

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the copyright notice (not shown here), this list of conditions and the following disclaimer (not shown here).

2. Redistributions in binary form must reproduce the copyright notice (not shown here), this list of conditions and the following disclaimer (not shown here) in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

Pretty easy, right? Just don't edit the big comment at the top of each code file, or the LICENSE file located at the root of the repo. Maybe even find it in your heart to give me credit when distributing :). Otherwise, use it in your game, even if you're selling the game, I don't care.

### Project Information ###

#### About ###
InputMaster was designed to accomplish several tasks:
* Make Input management variable based instead of string based. (COMPLETE!)
* Allow for configuration of *any* property of a control at runtime. (COMPLETE!)
* Integrate modern support for Xbox 360 Controllers using XInputDotNET (COMPLETE!)
 * Note this is only supported for Windows builds and the Windows Unity Editor. The feature is disabled using preprocessor directives when this is not the case, so you shouldn't get any exceptions.
* Be comfortable primarily for programmers.
* Be open source (**Definitely** COMPLETE!)

Here's a simple example to show simple it is to set up your controls:
```cs
using UnityEngine;
using BSGTools.IO;
#if (UNITY_STANDALONE_WIN || UNITY_METRO) && !UNITY_EDITOR_OSX
using BSGTools.IO.Xbox;
#endif

public class MyInputManager : Singleton<MyInputManager> {

	public InputMaster Master { get; private set; }
	
	#region KeyControls
	[HideInInspector]
	public KeyControl jump = new KeyControl(KeyCode.Space, KeyCode.None) {
		Dead = 0f,
		Gravity = 0.15f,
		Invert = false,
		IsBlocked = false,
		IsDebugControl = false,
		Name = "Jump",
		Sensitivity = 999f,
		Snap = false
	};
	[HideInInspector]
	public KeyControl walkLR = new KeyControl(KeyCode.D, KeyCode.A) {
		Dead = 0f,
		Gravity = 0.5f,
		Invert = false,
		IsBlocked = false,
		IsDebugControl = false,
		Name = "Walk Left/Right",
		Sensitivity = 999f,
		Snap = false
	};
	[HideInInspector]
	public KeyControl fireGun = new KeyControl(KeyCode.Mouse0, KeyCode.None) {
		Dead = 0f,
		Gravity = 999f,
		Invert = false,
		IsBlocked = false,
		IsDebugControl = false,
		Name = "Fire Gun",
		Sensitivity = 999f,
		Snap = false
	};
	#endregion

	#region XButtonControls
	[HideInInspector]
	public XButtonControl xJump = new XButtonControl(0, XButton.A, XButton.None) {
		Dead = 0f,
		Gravity = 0.15f,
		Invert = false,
		IsBlocked = false,
		IsDebugControl = false,
		Name = "Jump (Xbox)",
		Sensitivity = 999f,
		Snap = false
	};
	#endregion

	#region XStickControls
	[HideInInspector]
	public XStickControl xWalk = new XStickControl(0, XStick.StickLeft) {
		Dead = 0f,
		Gravity = 0.5f,
		Invert = false,
		IsBlocked = false,
		IsDebugControl = false,
		Name = "Walk Left/Right (Xbox)",
		Sensitivity = 999f,
		Snap = false
	};
	#endregion

	#region XTriggerControls
	[HideInInspector]
	public XTriggerControl xFireGun = new XTriggerControl(0, XTrigger.TriggerRight) {
		Dead = 0f,
		Gravity = 1f,
		Invert = false,
		IsBlocked = false,
		IsDebugControl = false,
		Name = "Fire Gun (Xbox)",
		Sensitivity = 1f,
		Snap = false
	};
	#endregion



	void Awake(){
		Master = InputMaster.CreateMaster(this);
	}
}
```

For multiple Xbox 360 controls for multiple players, there is a simple utility method for that too:

```cs
#region XButtonControls
[HideInInspector]
public XButtonControl[] xJump = XButtonControl.CreateMultiple(4, new XButtonControl(0, XButton.A, XButton.None) {
	Dead = 0f,
	Gravity = 0.15f,
	Invert = false,
	IsBlocked = false,
	IsDebugControl = false,
	Name = "Jump (Xbox)",
	Sensitivity = 999f,
	Snap = false
});
#endregion
	#region XStickControls
[HideInInspector]
public XStickControl[] xWalk = XStickControl.CreateMultiple(4, new XStickControl(0, XStick.StickLeft) {
	Dead = 0f,
	Gravity = 0.5f,
	Invert = false,
	IsBlocked = false,
	IsDebugControl = false,
	Name = "Walk Left/Right (Xbox)",
	Sensitivity = 999f,
	Snap = false
});
#endregion
	#region XTriggerControls
[HideInInspector]
public XTriggerControl[] xFireGun = XTriggerControl.CreateMultiple(4, new XTriggerControl(0, XTrigger.TriggerRight) {
	Dead = 0f,
	Gravity = 1f,
	Invert = false,
	IsBlocked = false,
	IsDebugControl = false,
	Name = "Fire Gun (Xbox)",
	Sensitivity = 1f,
	Snap = false
});
#endregion
```

To see how InputMaster works in more detail, please visit the [BSGTools Unity Community Wiki Page](http://wiki.unity3d.com/index.php/BSGTools).

#### Dependencies ####
None! InputMaster works in Unity Basic with no requirements or additional packages.

#### How do I use this in my game? ###
Getting started is easy peasy, just run the **.unitypackage** file located at the root of the repository.

There are additional steps that are required for XInputDotNET to function with Unity Basic. Visit the XInputDotNET GitHub repository to view these instructions (they are not listed here because they are subject to change by the author).

#### Where can I find tutorials? ####
There are example scenes included with all BSGTools packages, this one being no exception. There are also tutorials located at the bottom of the [BSGTools Unity Community Wiki Page](http://wiki.unity3d.com/index.php/BSGTools)!

#### Contribution guidelines ####
Nothing too crazy here, folks. I'll check anything that is added before it is merged into the master branch. A merge request may be denied for several reasons, however:

* The changes made do not help to further the goals of the project, or are not complementary to the current project goals. An example of a complementary goal for this project would be PS3 Controller support.
* The changes made hinder any individual part of the project.
* The changes made are so drastic to the core components of the project that it can be considered a complete conversion. Depending on the scenario changes like these may be approved. 

#### Who do I talk to? ####
Oh yeah, I'm Michael. You can contact me at [michaeln@birdstreetgames.com](mailto:michaeln@birdstreetgames.com)!
