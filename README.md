# UI Widgets


## Introduction

UIWidget is a plugin package for Unity Editor which helps developers to create, debug and deploy efficient, 
cross-platform Apps using the Unity Engine. 

UIWidget is mainly derived from Flutter @https://github.com/flutter/flutter. However, taking advantage of
the powerful Unity Engine, it offers developers many new features to improve their Apps 
as well as the develop workflow significantly.


#### Efficiency
Using the latest Unity rendering SDKs, a UIWidget App can run very fast and keep >60fps in most times.


#### Cross-Platform
A UIWidget App can be deployed on all kinds of platforms including PCs, mobile devices and web page directly, like 
any other Unity projects.

#### 3D-Support
Except for basic 2D UIs, developers are also able to include 3D Models, particle-systems to their UIWidget Apps.


#### Developer-Friendly
A UIWidget App can be debug in the Unity Editor directly with many advanced tools like
CPU/GPU Profiling, FPS Profiling.


## Requirement

#### Unity
Install **Unity 2018.3** or above. You can download the latest Unity on https://unity3d.com/get-unity/download.

#### UIWidget Package
Visit our Github repository https://github.com/UnityTech/UIWidgets
 to download the latest UIWidget package.
 
Move the downloaded package folder into the **Package** folder of your Unity project.

Generally, you can make it using a console (or terminal) application by just a few commands as below:
    
   ```none
    cd <YourProjectPath>/Packages
    git clone https://github.com/UnityTech/UIWidgets.git com.unity.uiwidgets
   ```

## Getting Start

#### i. Overview
In this tutorial, we will create a very simple UIWidget App as the kick-starter. The app contains 
only a text label and a button. The text label will count the times of clicks upon the button.

First of all, please open or create a Unity Project and open it with Unity Editor. 

#### ii. Scene Build
A UIWidget App is usually built upon a Unity UI Canvas. Please follow the steps to create a
UI Canvas in Unity.
1. Create a new Scene by "File -> New Scene";
1. Create a UI Canvas in the scene by "GameObject -> UI -> Canvas";
1. Add a Panel (i.e., **Panel 1**) to the UI Canvas by right click on the Canvas and select "UI -> Panel". Then remove the 
**Image** Component from the Panel.

#### iii. Create Widget
A UIWidget App is written in **C# Scripts**. Please follow the steps to create an App and play it
in Unity Editor.

1. Create a new C# Script named "ExampleCanvas.cs" and paste the following codes into it.
   ```none
    using System.Collections.Generic;
    using Unity.UIWidgets.engine;
    using Unity.UIWidgets.foundation;
    using Unity.UIWidgets.material;
    using Unity.UIWidgets.painting;
    using Unity.UIWidgets.widgets;
    
    namespace UIWidgetsSample {
        public class ExampleCanvas : WidgetCanvas {
            protected override Widget getWidget() {
                return new ExampleApp();
            }
    
            class ExampleApp : StatefulWidget {
                public ExampleApp(Key key = null) : base(key) {
                }
    
                public override State createState() {
                    return new ExampleState();
                }
            }
    
            class ExampleState : State<ExampleApp> {
                int counter = 0;
    
                public override Widget build(BuildContext context) {
                    return new Column(
                        children: new List<Widget> {
                            new Text("Counter: " + this.counter),
                            new GestureDetector(
                                onTap: () => {
                                    this.setState(()
                                     => {
                                        this.counter++;
                                    });
                                },
                                child: new Container(
                                    padding: EdgeInsets.symmetric(20, 20),
                                    color: Colors.blue,
                                    child: new Text("Click Me")
                                )
                            )
                        }
                    );
                }
            }
        }
    }
   ```
   
1. Save this script and attach it to **Panel 1** as its component.
1. Press the "Play" Button to start the App in Unity Editor.

#### iv. Build App
Finally, the UIWidget App can be built to packages for any specific platform by the following steps.
1. Open the Build Settings Panel by "File -> Build Settings..."
1. Choose a target platform and click "Build". Then the Unity Editor will automatically assemble 
all relevant resources and generate the final App package.


## Learn

#### Samples
You can find many UIWidget App samples in the UIWidget package in the **Samples** folder.
Feel free to try them out and make modifications to see the results.

You can also try UIWidget-based Editor windows by clicking **UIWidgetsTest** on the main menu 
and open one of the dropdown samples.

#### Wiki
The develop team is still working on the UIWidget Wiki. However, since UIWidget is mainly derived from Flutter,
 you can refer to Flutter Wiki to access detailed descriptions of UIWidget APIs 
 from those of their Flutter counterparts.
 
#### FAQ

| Question     | Answer  |
| :-----------------------------------------------| -----------: |
| Can I create standalone App using UIWidget?     | **Yes**  |
| Can I use UIWidget to build game UIs?   | **Yes**    |
| Can I develop Unity Editor plugins using UIWidget?  | **Yes** |
| Is UIWidget a extension of UGUI/NGUI? | **No** |
| Is UIWidget just a copy of Flutter? | **No** | 
| Can I create UI with UIWidget by simply drag&drop? | **No** |
| Do I have to pay for using UIWidget? | **No** |

## How to Contribute
If you want to join us, please contact us via Github and we will respond as soon as possible.

#### Code Style
1. **Import the Customized Code Cleanup Settings**: Open Preferences -> Manage Layers, 
Choose 'Solution "\<YourProjectName\>" personal' and Click "Add Layer" ("+") -> "Open Settings File...".
and Open the file "UIWidgetCleanupPlugin.DotSettings" under \<YourProjectPath\>/Packages/com.unity.uiwidgets/"

2. **Cleanup Code style using the Customized Code Cleanup Settings**: Open Code -> Code Cleanup,
Pick a Cleanup scope as you want and Choose "UIWidgets" as the "Code cleanup profile", then click "OK"

3. **Refine Code Style Rules**: Edit the ".editorconfig" file under \<YourProjectPath\>/Packages/com.unity.uiwidgets/". Visit
 https://www.jetbrains.com/help/rider/EditorConfig_Index.html for the detailed.

