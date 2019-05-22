# Contributing

## If you are interested in contributing, here are some ground rules:

### Code Style (using JetBrains Rider)
1. **Import the Customized Code Cleanup Settings**: Open Preferences -> Manage Layers, 
Choose 'Solution "\<YourProjectName\>" personal' and Click "Add Layer" ("+") -> "Open Settings File...".
and Open the file "UIWidgetCleanupPlugin.DotSettings" under \<YourProjectPath\>/Packages/com.unity.uiwidgets/"

2. **Cleanup Code style using the Customized Code Cleanup Settings**: Open Code -> Code Cleanup,
Pick a Cleanup scope as you want and Choose "UIWidgets" as the "Code cleanup profile", then click "OK"

3. **Refine Code Style Rules**: Edit the ".editorconfig" file under \<YourProjectPath\>/Packages/com.unity.uiwidgets/". Visit
 https://www.jetbrains.com/help/rider/EditorConfig_Index.html for the detailed.

### Generate Code.

Code files ending with ".gen.cs" are auto generated. Follow these steps to generate them:

1. **Go to scripts Folder and Run npm install**:
```
cd <YourProjectPath>/Packages/com.unity.uiwidgets/scripts~
npm install
```

2. **Run the codegen Command**:
```
node uiwidgets-cli.js codegen . generate mixin code
```


## All contributions are subject to the [Unity Contribution Agreement(UCA)](https://unity3d.com/legal/licenses/Unity_Contribution_Agreement)
By making a pull request, you are confirming agreement to the terms and conditions of the UCA, including that your Contributions are your original creation and that you have complete right and authority to make your Contributions.

## Once you have a change ready following these ground rules. Simply make a pull request
