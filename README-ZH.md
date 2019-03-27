# UIWidgets


## 介绍

UIWidgets是Unity编辑器的一个插件包，可帮助开发人员通过Unity引擎来创建、调试和部署高效的跨平台应用。

UIWidgets主要来自[Flutter](https://github.com/flutter/flutter)。但UIWidgets通过使用强大的Unity引擎为开发人员提供了许多新功能，显著地改进他们开发的应用性能和工作流程。

#### 效率
通过使用最新的Unity渲染SDK，UIWidgets应用可以非常快速地运行并且大多数时间保持大于60fps的速度。

#### 跨平台
与任何其他Unity项目一样，UIWidgets应用可以直接部署在各种平台上，包括PC，移动设备和网页等。

#### 多媒体支持
除了基本的2D UI之外，开发人员还能够将3D模型，音频，粒子系统添加到UIWidgets应用中。


#### 开发者友好
开发者可以使用许多高级工具，如CPU/GPU Profiling和FPS Profiling，直接在Unity Editor中调试UIWidgets应用。


## 使用要求

#### Unity
安装 Unity 2018.3 或更高版本。 你可以从[https://unity3d.com/get-unity/download](https://unity3d.com/get-unity/download)下载最新的Unity。

#### UIWidgets包

访问我们的Github存储库 [https://github.com/UnityTech/UIWidgets](https://github.com/UnityTech/UIWidgets)下载最新的UIWidgets包。

将下载的包文件夹移动到 Unity项目 的 Package 文件夹中。

通常，你可以在控制台（或终端）应用程序中输入下面的代码来完成这个操作：

    
   ```none
    cd <YourProjectPath>/Packages
    git clone https://github.com/UnityTech/UIWidgets.git com.unity.uiwidgets
   ```

## 入门指南

#### 一、 概观
在本教程中，我们将创建一个非常简单的UIWidgets应用。 该应用只包含文本标签和按钮。 文本标签将计算按钮上的点击次数。

首先，请打开或创建Unity项目并使用Unity编辑器打开它。

然后打开Project Settings，转到Player部分并将“UIWidgets_DEBUG”添加到Scripting Define Symbols字段中。

这样就启动了UIWidgets的调试模式。 在之后发布版本的时候清空这个字段。

#### 二、 场景构建

UIWidgets应用通常构建在Unity UI Canvas上。 请按照以下步骤在Unity中创建一个
UI Canvas。
1. 选择 File > New Scene来创建一个新场景。
2. 选择 GameObject > UI > Canvas 在场景中创建UI Canvas。
3. 右键单击Canvas并选择UI > Panel，将面板（即面板1）添加到UI Canvas中。 然后删除面板中的 **Image** 组件。

#### 三、创建小部件

UIWidgets应用是用**C＃脚本**来编写的。 请按照以下步骤创建应用程序并在Unity编辑器中播放。
1. 创建一个新C＃脚本，命名为“ExampleCanvas.cs”，并将以下代码粘贴到其中。

   ```none
    using System.Collections.Generic;
    using Unity.UIWidgets.engine;
    using Unity.UIWidgets.foundation;
    using Unity.UIWidgets.material;
    using Unity.UIWidgets.painting;
    using Unity.UIWidgets.widgets;
    
    namespace UIWidgetsSample {
        public class ExampleCanvas : WidgetCanvas {
            protected override void OnEnable() {
                base.OnEnable();
                    
                // Application.targetFrameRate = 60; // or higher if you want a smoother scrolling experience.
                
                // if you want to use your own font or font icons.
                // use the font family name instead of the file name in FontStyle.fontFamily.
                // you can get the font family name by clicking the font file and check its Inspector.                 
                // FontManager.instance.addFont(Resources.Load<Font>(path: "path to your font"));                
            }
                
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
   
2. 保存此脚本，并将其附加到Panel 1中作为其组件。
3. 在Unity编辑器中，点击Play按钮来启动应用。

#### 四、构建应用程序

最后，你可以按以下步骤将UIWidgets应用构建成适用于任何特定平台的应用程序包。
1. 选择**File** > **Build Settings...**打开Build Settings面板。
2. 选择目标平台，点击Build。 之后Unity编辑器将自动组装所有相关资源并生成最终的应用程序包。

#### 如何加载图像？
1. 将你的图像文件，如image1.png，放在Resources文件夹中。
2. 你可以在同一文件夹中添加image1@2.png和image1@3.png以支持高清屏幕显示。
3. 使用Image.asset（“image1”）加载图像。 注意：因为是在Unity中，所以不需要添加.png后缀。


UIWidgets也支持Gif！
1. 假设你有一个loading1.gif文件，将其重命名为loading1.gif.bytes并复制到Resources文件夹。
2. 你可以在同一文件夹中添加loading1@2.gif.bytes和loading1@3.gif.bytes以支持高清屏幕显示。
3. 使用Image.asset（“loading1.gif”）加载gif图像。


## 调试UIWidgets应用程序

#### 定义UIWidgets_DEBUG
我们建议在Unity编辑器中定义 UIWidgets_DEBUG 脚本符号，这将打开UIWidgets中的调试断言（debug assertion），有助于更早发现潜在的Bug。 
因此选择 **Player Settings** > **Other Settings** > **Configuration** > **Scripting Define Symbols** ，并添加 UIWidgets_DEBUG。
该符号仅供调试使用，请在发布版本中删除它。

#### UIWidgets Inspector

UIWidgets Inspector工具用于可视化和浏览窗口小部件树。 你可以在Unity编辑器的**Window** > **Analysis** > **UIWidget Inspector** 中的找到它。

注意
- 需要定义 UIWidgets_DEBUG 使inspector正常工作。
- Inspector目前仅适用于编辑器的播放模式，目前不支持独立版本的应用程序。


## 学习

#### 示例

你可以在**Samples**文件夹的UIWidgets包中找到许多UIWidgets应用示例。请随意尝试并进行修改以查看结果。

你也可以在支持**UIWidgets**的编辑器中，点击主菜单上的UIWidgets，并在下拉窗口中选择一个示例。

#### Wiki

目前开发团队仍在改进UIWidgets Wiki。 由于UIWidgets主要来源于Flutter，你也可以参考Flutter Wiki中与UIWidgets API对应部分的详细描述。
 
#### 常问问题解答

| 问题     | 回答  |
| :-----------------------------------------------| ---------------------: |
| 我可以使用UIWidgets创建独立应用吗？     | 可以  |
| 我可以使用UIWidgets构建游戏UI吗？   | 可以    |
| 我可以使用UIWidgets开发Unity编辑器插件吗？ | 可以 |
| UIWidgets是UGUI / NGUI的扩展吗？ | 不是 |
| UIWidgets只是Flutter的副本吗？ | 不是 | 
| 我可以通过简单的拖放操作来创建带有UIWidgets的UI吗？ | 不可以 |
| 我是否需要付费使用UIWidgets？ | 不需要 |
| 有推荐的适用于UIWidgets的IDE吗？ | Rider, VSCode(Open .sln) |

## 如何贡献
如果你想加入我们，请通过Github与我们联系，我们将尽快回复。

#### 代码风格
1. 导入自定义代码清理设置
    打开首选项 - >管理图层，选择“解决方案“<YourProjectName>“个人”，然后单击“添加图层”（“+”） > “打开设置文件...”。并打开<YourProjectPath> /Packages/com.unity.uiwidgets/下的文件“UIWidgetCleanupPlugin.DotSettings”。

2. 使用自定义代码清理设置清理代码样式
    打开代码 - >代码清理，根据需要选择一个清理范围，选择“UIWidgets”作为“代码清理配置文件”，然后单击“确定”。


3. 优化代码样式规则
    编辑<YourProjectPath> /Packages/com.unity.uiwidgets/“下的”.editorconfig“文件。获得更多详细信息，请访问[https://www.jetbrains.com/help/rider/EditorConfig_Index.html](https://www.jetbrains.com/help/rider/EditorConfig_Index.html)。

#### 生成njk代码

1. 转到脚本文件夹并运行npm install。
```
cd <YourProjectPath>/Packages/com.unity.uiwidgets/scripts
npm install
```
2. 运行codegen命令。
```
node uiwidgets-cli.js codegen . generate mixin code
```
