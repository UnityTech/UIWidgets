# UI Widgets

## Development

1. Start **Unity(2018.3 or above)**, create a local empty project.
   
1. In a console (or terminal) application, go to the newly created project folder, then clone this repo into the packages directory.

    ```none
    cd <YourProjectPath>/Packages
    git clone https://gitlab.cds.internal.unity3d.com/upm-packages/ui-widgets/com.unity.uiwidgets.git com.unity.uiwidgets
    ```
## IDE

1. Use JetBrains Rider

2. Open File -> Settings Repository....
Specify Upstream URL as "git@gitlab.cds.internal.unity3d.com:upm-packages/ui-widgets/idea-settings.git"
and click "Overwrite Local" or "Merge".

REF: https://www.jetbrains.com/help/idea/sharing-your-ide-settings.html#settings-repository


## Code Style Cleanup

1. **Import the Customized Code Cleanup Setting**: Open Preferences -> Manage Layers, 
Choose 'Solution "\<YourProjectName\>" personal' and Click "Add Layer" ("+") -> "Open Settings File...".
and Open the file "UIWidgetCleanupPlugin.DotSettings" under \<YourProjectPath\>/Packages/com.unity.uiwidgets/"

2. **Cleanup Code style using the Customized Code Cleanup Setting**: Open Code -> Code Cleanup,
Pick a Cleanup scope as you want and Choose the "UIWidgets" as the "Code cleanup profile", then click "OK"

3. **Refine Code Style Rules**: Edit the ".editorconfig" file under \<YourProjectPath\>/Packages/com.unity.uiwidgets/"