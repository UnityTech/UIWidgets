⚠️ 本项目的开发与维护目前已转移至https://github.com/Unity-Technologies/com.unity.uiwidgets. 如果您遇到Issue或者对UIWidgets有任何意见和建议，请移步我们的新仓库。谢谢您的关注！

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

## 示例

<div style="text-align: center"><table><tr>
<td style="text-align: center">
  <img src="https://connect-prd-cdn.unity.com/20190323/p/images/2a27606f-a2cc-4c9f-9e34-bb39ae64d06c_uiwidgets1.gif" width="200"/>
</td>
<td style="text-align: center">
  <img src="https://connect-prd-cdn.unity.com/20190323/p/images/097a7c53-19b3-4e0a-ad27-8ec02506905d_uiwidgets2.gif" width="200" />
</td>
<td style="text-align: center">
  <img src="https://connect-prd-cdn.unity.com/20190323/p/images/1f03c1d0-758c-4dde-b3a9-2f5f7216b7d9_uiwidgets3.gif" width="200"/>
</td>
<td style="text-align: center">
  <img src="https://connect-prd-cdn.unity.com/20190323/p/images/a8884fbd-9e7c-4bd7-af46-0947e01d01fd_uiwidgets4.gif" width="200"/>
</td>
</tr></table></div>

### 基于UIWidgets的项目

#### Unity Connect App
Unity Connect App是使用UIWidgets开发的一个移动App产品，您随时可以在Android (https://connect.unity.com/connectApp/download)
以及iOS (Searching for "Unity Connect" in App Store)端下载到它最新的版本. 本项目的所有代码均开源@https://github.com/UnityTech/ConnectAppCN.

#### Unity中文官方文档
Unity的线上中文官方文档由UIWidgets开发，您可以点击以下网址 https://connect.unity.com/doc 来访问它的全部内容。该项目目前已开源，所有代码可以在
https://github.com/UnityTech/DocCN 查看。

## 使用要求

#### Unity

安装 **Unity 2018.4.10f1(LTS)** 或 **Unity 2019.1.14f1** 及其更高版本。 你可以从[https://unity3d.com/get-unity/download](https://unity3d.com/get-unity/download)下载最新的Unity。

#### UIWidgets包

访问我们的Github存储库 [https://github.com/UnityTech/UIWidgets](https://github.com/UnityTech/UIWidgets)下载最新的UIWidgets包。

将下载的包文件夹移动到Unity项目的Package文件夹中。

通常，你可以在控制台（或终端）应用程序中输入下面的代码来完成这个操作：

   ```none
    cd <YourProjectPath>/Packages
    git clone https://github.com/UnityTech/UIWidgets.git com.unity.uiwidgets
   ```

## 入门指南

#### 一、 概观
在本教程中，我们将创建一个非常简单的UIWidgets应用。 该应用只包含文本标签和按钮。 文本标签将计算按钮上的点击次数。

首先，请打开或创建Unity项目并使用Unity编辑器打开它。

然后打开Project Settings，转到Player部分并**将“UIWidgets_DEBUG”添加到Scripting Define Symbols字段中。**

这样就启动了UIWidgets的调试模式。 在之后发布版本的时候清空这个字段。

#### 二、 场景构建

UIWidgets应用通常构建在Unity UI Canvas上。 请按照以下步骤在Unity中创建一个
UI Canvas。
1. 选择 File > New Scene来创建一个新场景。
2. 选择 GameObject > UI > Canvas 在场景中创建UI Canvas。
3. 右键单击Canvas并选择UI > Panel，将面板（即面板1）添加到UI Canvas中。 然后删除面板中的 **Image** 组件。

#### 三、创建小部件

UIWidgets应用是用**C＃脚本**来编写的。 请按照以下步骤创建应用程序并在Unity编辑器中播放。
1. 创建一个新C＃脚本，命名为“UIWidgetsExample.cs”，并将以下代码粘贴到其中。

```csharp
    using System.Collections.Generic;
    using Unity.UIWidgets.animation;
    using Unity.UIWidgets.engine;
    using Unity.UIWidgets.foundation;
    using Unity.UIWidgets.material;
    using Unity.UIWidgets.painting;
    using Unity.UIWidgets.ui;
    using Unity.UIWidgets.widgets;
    using UnityEngine;
    using FontStyle = Unity.UIWidgets.ui.FontStyle;

    namespace UIWidgetsSample {
        public class UIWidgetsExample : UIWidgetsPanel {
            protected override void OnEnable() {
                // if you want to use your own font or font icons.
                // FontManager.instance.addFont(Resources.Load<Font>(path: "path to your font"), "font family name");

                // load custom font with weight & style. The font weight & style corresponds to fontWeight, fontStyle of
                // a TextStyle object
                // FontManager.instance.addFont(Resources.Load<Font>(path: "path to your font"), "Roboto", FontWeight.w500,
                //    FontStyle.italic);

                // add material icons, familyName must be "Material Icons"
                // FontManager.instance.addFont(Resources.Load<Font>(path: "path to material icons"), "Material Icons");

                base.OnEnable();
            }

            protected override Widget createWidget() {
                return new WidgetsApp(
                    home: new ExampleApp(),
                    pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (BuildContext context, Animation<float> animation,
                                Animation<float> secondaryAnimation) => builder(context)
                        )
                );
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
                                    this.setState(() => {
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

#### 五、如何加载图像？
1. 将你的图像文件，如image1.png，放在Resources文件夹中。
2. 你可以在同一文件夹中添加image1@2.png和image1@3.png以支持高清屏幕显示。
3. 使用Image.asset（“image1”）加载图像。 注意：因为是在Unity中，所以不需要添加.png后缀。


UIWidgets也支持Gif！
1. 假设你有一个loading1.gif文件，将其重命名为loading1.gif.bytes并复制到Resources文件夹。
2. 你可以在同一文件夹中添加loading1@2.gif.bytes和loading1@3.gif.bytes以支持高清屏幕显示。
3. 使用Image.asset（“loading1.gif”）加载gif图像。

#### 六、在安卓上显示状态栏
当一个Unity项目运行在Android设备上时，状态栏是默认隐藏且无法在编辑内进行调整的。
如果您希望在您的UIWidgets App中显示状态栏，您可以使用这个[解决方案](https://github.com/Over17/UnityShowAndroidStatusBar)。我们将尽快推出我们自己的解决方案，并保证届时开发者可以进行无缝切换。

此外，为了让上述插件在Android P及以上Android系统中正常工作，请勾选上"Player Settings"中的"Render Outside Safe Area"选项。

#### 七、自动调节帧率
如果要使得构建出的应用能够自动调节帧率，请打开Project Settings，将构建目标平台对应的Quality选项卡中的V Sync Count设置为Don't Sync。
默认的逻辑是在界面静止时将帧率降低，在界面变动时再将帧率提高至60。
如果您不想开启该功能，请将`Window.onFrameRateSpeedUp`和/或`Window.onFrameRateCoolDown`设置为空函数，()=> {}即可。

在Unity 2019.3版本及以上，UIWidgets将使用OnDemandRenderAPI来实现帧率调节，它将在不影响UI响应速度的情况下大幅降低耗电和发热问题。

#### 八、WebGL Canvas分辨率调整插件
因为浏览器中Canvas的宽高和其在显示器上的像素数可能不一致，所以构建出的WebGL程序中画面可能会模糊。
插件`Plugins/platform/webgl/UIWidgetsCanvasDevicePixelRatio_20xx.x.jslib`（目前有2018.3和2019.1）解决了这个问题。
请根据您的项目的Unity版本选择对应的插件，并禁用此插件的其他版本。方法如下：在Project面板中选中该插件，在Inspector面板中的Select platforms for plugin中，去掉WebGL后面的对勾。
如果您因为任何原因需要完全禁止此插件的功能，请按上述方法禁用此插件的所有版本。

此插件覆盖了Unity WebGL构建模块中的如下参数：
```none
JS_SystemInfo_GetWidth
JS_SystemInfo_GetHeight
JS_SystemInfo_GetCurrentCanvasWidth
JS_SystemInfo_GetCurrentCanvasHeight
$Browser
$JSEvents
```
如果您需要实现自己的WebGL插件，并且您的插件覆盖了这些参数中的至少一种，您需要采用上文中提到的方法禁用`UIWidgetsCanvasDevicePixelRatio`插件，以防止可能的冲突。
如果您仍然需要此插件所提供的功能，您可以手动将此插件对Unity WebGL构建模块的修改应用到您的插件中。
`UIWidgetsCanvasDevicePixelRatio`插件中所有的修改之处都以`////////// Modification Start ////////////`和`////////// Modification End ////////////`标识。
在被标识的代码中，所有乘/除以`devicePixelRatio`都来自于我们的修改。
若您需要详细了解此插件所修改的脚本，请参考您的Unity Editor安装目录下的`PlaybackEngines/WebGLSupport/BuildTools/lib`文件夹中的`SystemInfo.js`和`UnityNativeJS/UnityNative.js`。

#### 九、图片导入设置
默认情况下，Unity会将导入图片的宽和高放缩为最近的等于2的幂的整数。
在UIWidgets中使用图片时，记得将这一特性关闭，以免图片被意外放缩，方法如下：在Project面板中选中图片，在"Inspector"面板中将"Non Power of 2"（在"Advanced"中）设置为"None"。

#### 十、更新表情(Emoji)
UIWidgets支持渲染文本中包含的表情。
默认的表情资源为[iOS 13.2](https://emojipedia.org/apple/ios-13.2)。
我们也准备了[Google Emoji](https://emojipedia.org/google)的表情资源。
如果您希望切换到Google版本的表情，请按如下步骤操作：

1. 拷贝`Runtime/Resources/backup~/EmojiGoogle.png`到`Runtime/Resources/images`目录。
2. 在**Project**面板中，找到`EmojiGoogle`资源，在**Inspector**面板中，将**Max Size**更改为4096，取消选中**Generate Mipmaps**，并选中**Alpha Is Transparency**。
3. 在您的代码中继承`UIWidgetsPanel`的类的`OnEnable()`函数中，添加如下代码

```csharp
EmojiUtils.configuration = EmojiUtils.googleEmojiConfiguration;
```

如果您希望使用自己的表情图片，请按如下步骤操作：

1. 参照`EmojiGoogle.png`，创建您自己的Emoji表单，并放到工程目录下的某个`Resources`目录中，例如`Resources/myImages/MyEmoji.png`）。
2. 在`OnEnable()`函数中，添加如下代码（记得将示例的值改为真实的值）。注意Emoji的编码的顺序要和Emoji表单一致。

```csharp
EmojiUtils.configuration = new EmojiResourceConfiguration(
  spriteSheetAssetName: "myImage/MyEmoji",
  emojiCodes: new List<int> {
    0x1f004, 0x1f0cf, 0x1f170, ...
  },
  spriteSheetNumberOfRows: 36,
  spriteSheetNumberOfColumns: 37,
);
```

#### 十一、与GameObject进行拖拽交互

<p align="center">
  <img src="https://connect-prd-cdn.unity.com/20190718/p/images/e3c9cf9b-c732-4eb2-9afd-fe7de894f342_Custom_Inspector_Showcase_320px.gif" width="300"/>
</p>

我们提供了一个包装好的`UnityObjectDetector`组件以及`onRelease`回调函数，借此您可以实现简单地将物体（例如Hierarchy内的场景物体、Project窗口下的文件等）拖拽至区域内，来获得`UnityEngine.Object[] `类型的引用并进行操作。

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

#### 教程

包括开发组在内的广大开发者为UIWidgets提供了许多可供学习的样例和教程，你可以根据你的需求进行学习：
- UIWidgets官方示例。目前所有官方使用的示例项目均维护在一个独立的Github仓库（ https://github.com/UIWidgets/UIWidgetsSamples ）中。你可以
将它clone到你项目本地的Assets目录下使用。
具体的，你可以在Sample项目的**Scene**子文件夹中浏览所有示例UI场景。
此外，你还可以点击主菜单上的新增的UIWidgetsTests选项卡，并在下拉菜单中选择一个EditorWindow UI示例来运行。
- UIWidgets凉鞋系列教程。你可以在凉鞋老师整理的Github仓库（ https://github.com/liangxiegame/awesome-uiwidgets ）中学习UIWidgets的基本用法
以及许多有趣的小Demo。
- ConnectApp开源项目。这是一个完整的线上、开源、完全基于UIWidgets的第一方App项目。其中包含了大量产品级的UIWidgets工程实践细节，
如果你想深入了解UIWidgets并且使用它构建线上项目，请访问项目Github仓库了解更多（ https://github.com/UnityTech/ConnectAppCN ）。

#### Wiki

目前开发团队仍在改进UIWidgets Wiki。 由于UIWidgets主要来源于Flutter，你也可以参考Flutter Wiki中与UIWidgets API对应部分的详细描述。
同时，你可以加入我们的讨论组( https://connect.unity.com/g/uiwidgets )。

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
请查看[CONTRIBUTING.md](CONTRIBUTING.md)
