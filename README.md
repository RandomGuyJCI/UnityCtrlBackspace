[![GitHub all releases](https://img.shields.io/github/downloads/RandomGuyJCI/UnityCtrlBackspace/total)](https://github.com/RandomGuyJCI/UnityCtrlBackspace/releases/latest)
[![Contributions Welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/RandomGuyJCI/UnityCtrlBackspace/issues)


<div align="center">
  <h1>UnityCtrlBackspace</h1>
  <i>A <a href="https://github.com/BepInEx/BepInEx">BepInEx</a> plugin that adds ctrl-backspace functionality to <a href="https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.InputField.html">Unity Input Fields</a>.</i>
</div>

---

<details><summary><h2>Demos</h2></summary>
 
 https://user-images.githubusercontent.com/22722393/169652584-487c406e-a371-4c9f-9126-fc4635fded80.mp4
 <p align="center"><i>This plugin has full parity with ctrl-backspace functionality on most programs such as Notepad++.</i></p>
 
 &emsp;
 
 https://user-images.githubusercontent.com/22722393/169652585-344be85d-b7f4-4784-8707-548f32d84680.mp4
 <p align="center"><i>It also supports non-english text!</i></p>
</details>

## Installation
1. Install the latest build of [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) for your Unity game. For more information, check out the [BepInEx installation guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html).
2. Download the latest version of the mod in the [releases](https://github.com/RandomGuyJCI/UnityCtrlBackspace/releases) page. It should be named `UnityCtrlBackspace_1.x.x.zip`.
3. Extract the file into your main game folder. you should now have a file at `BepInEx/plugins/UnityCtrlBackspace/UnityCtrlBackspace.dll`.

## Development
Although this plugin was originally built for Rhythm Doctor, it can also be installed/ported over to any Unity game that uses input fields for text input.

After cloning the repository, the Nuget.Config file should automatically install the BepInEx dependendies needed for development, although you still need to manually import a publicized version of the `UnityEngine.UI.dll` file. Instructions can be found over at the [NStrip](https://github.com/BepInEx/NStrip) GitHub page.
