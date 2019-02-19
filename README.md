# UI Widgets


## Introduction

UIWidget is a plugin package of Unity Editor which helps developers to create, debug and deploy efficient, 
cross-platform Apps using Unity Engine. 

UIWidget is mainly derived from Flutter @https://github.com/flutter/flutter. However, taking advantage of
the powerful Unity Engine, it offers developers many new features to improve their Apps 
as well as the develop workflow significantly.


#### Efficiency
Using the latest Unity rendering SDKs, a UIWidget App can run very fast and keep >60fps in most times.


#### Cross-Platform
A UIWidget App can be deployed on all kinds of platforms including PCs, mobile devices and web page directly, like 
any other Unity projects.

#### 3D-Support
Except for basic 2D UIs, Developers are also able to include 3D Models, particle-systems to their UIWidget Apps.


#### Developer-Friendliness
A UIWidget App can be debug in the Unity Editor directly with many advanced tools like
CPU/GPU Profiling, FPS Profiling.

## Requirement

#### Unity
Install **Unity 2018.3** or above. You can download the latest Unity on https://unity3d.com/get-unity/download.

#### UIWidget Package
Visit our Github repository https://gitlab.cds.internal.unity3d.com/upm-packages/ui-widgets/com.unity.uiwidgets.git
 to download the latest UIWidget package.
 
Move the downloaded package folder into the **Package** folder of your Unity project.

Generally, you can make it using a console (or terminal) application by just a few commands as below:
    
   ```none
    cd <YourProjectPath>/Packages
    git clone https://gitlab.cds.internal.unity3d.com/upm-packages/ui-widgets/com.unity.uiwidgets.git com.unity.uiwidgets
   ```

## Getting Start

#### i. Overview

#### ii. Scene Build

#### iii. Create Widget

#### iv. Build App

#### v. Move to Editor

## Learn

#### Samples

#### Wiki


## How to Contribute


#### Code Style
1. **Import the Customized Code Cleanup Settings**: Open Preferences -> Manage Layers, 
Choose 'Solution "\<YourProjectName\>" personal' and Click "Add Layer" ("+") -> "Open Settings File...".
and Open the file "UIWidgetCleanupPlugin.DotSettings" under \<YourProjectPath\>/Packages/com.unity.uiwidgets/"

2. **Cleanup Code style using the Customized Code Cleanup Settings**: Open Code -> Code Cleanup,
Pick a Cleanup scope as you want and Choose "UIWidgets" as the "Code cleanup profile", then click "OK"

3. **Refine Code Style Rules**: Edit the ".editorconfig" file under \<YourProjectPath\>/Packages/com.unity.uiwidgets/". Visit
 https://www.jetbrains.com/help/rider/EditorConfig_Index.html for the detailed.

