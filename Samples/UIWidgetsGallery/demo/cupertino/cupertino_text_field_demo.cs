using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class CupertinoTextFieldDemo : StatefulWidget {
        public const string routeName = "/cupertino/text_fields";

        public override State createState() {
            return new _CupertinoTextFieldDemoState();
        }
    }

    class _CupertinoTextFieldDemoState : State<CupertinoTextFieldDemo> {
        TextEditingController _chatTextController;
        TextEditingController _locationTextController;

        public override void initState() {
            base.initState();
            this._chatTextController = new TextEditingController();
            this._locationTextController = new TextEditingController(text: "Montreal, Canada");
        }

        Widget _buildChatTextField() {
            return new CupertinoTextField(
                controller: this._chatTextController,
                textCapitalization: TextCapitalization.sentences,
                placeholder: "Text Message",
                decoration: new BoxDecoration(
                    border: Border.all(
                        width: 0.0f,
                        color: CupertinoColors.inactiveGray
                    ),
                    borderRadius: BorderRadius.circular(15.0f)
                ),
                maxLines: null,
                keyboardType: TextInputType.multiline,
                prefix: new Padding(padding: EdgeInsets.symmetric(horizontal: 4.0f)),
                suffix:
                new Padding(
                    padding: EdgeInsets.symmetric(horizontal: 4.0f),
                    child: new CupertinoButton(
                        color: CupertinoColors.activeGreen,
                        minSize: 0.0f,
                        child: new Icon(
                            CupertinoIcons.up_arrow,
                            size: 21.0f,
                            color: CupertinoColors.white
                        ),
                        padding: EdgeInsets.all(2.0f),
                        borderRadius:
                        BorderRadius.circular(15.0f),
                        onPressed: () => this.setState(() => this._chatTextController.clear())
                    )
                ),
                autofocus: true,
                suffixMode: OverlayVisibilityMode.editing,
                onSubmitted: (string text) => this.setState(() => this._chatTextController.clear())
            );
        }

        Widget _buildNameField() {
            return new CupertinoTextField(
                prefix: new Icon(
                    CupertinoIcons.person_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                clearButtonMode: OverlayVisibilityMode.editing,
                textCapitalization: TextCapitalization.words,
                autocorrect: false,
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                ),
                placeholder: "Name"
            );
        }

        Widget _buildEmailField() {
            return new CupertinoTextField(
                prefix: new Icon(
                    CupertinoIcons.mail_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                clearButtonMode: OverlayVisibilityMode.editing,
                keyboardType: TextInputType.emailAddress,
                autocorrect: false,
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                ),
                placeholder: "Email"
            );
        }

        Widget _buildLocationField() {
            return new CupertinoTextField(
                controller: this._locationTextController,
                prefix: new Icon(
                    CupertinoIcons.location_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                clearButtonMode: OverlayVisibilityMode.editing,
                textCapitalization: TextCapitalization.words,
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                ),
                placeholder: "Location"
            );
        }

        Widget _buildPinField() {
            return new CupertinoTextField(
                prefix: new Icon(
                    CupertinoIcons.padlock_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                clearButtonMode: OverlayVisibilityMode.editing,
                keyboardType: TextInputType.number,
                autocorrect: false,
                obscureText: true,
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                ),
                placeholder: "Create a PIN"
            );
        }

        Widget _buildTagsField() {
            return new CupertinoTextField(
                controller: new TextEditingController(text: "colleague, reading club"),
                prefix: new Icon(
                    CupertinoIcons.tags_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                enabled: false,
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                )
            );
        }

        public override Widget build(BuildContext context) {
            return new DefaultTextStyle(
                style: new TextStyle(
                    fontFamily: ".SF Pro Text", // ".SF UI Text",
                    inherit: false,
                    fontSize: 17.0f,
                    color: CupertinoColors.black
                ),
                child: new CupertinoPageScaffold(
                    navigationBar: new CupertinoNavigationBar(
                        previousPageTitle: "Cupertino",
                        middle: new Text("Text Fields")
                    ),
                    child: new SafeArea(
                        child: new ListView(
                            children: new List<Widget> {
                                new Padding(
                                    padding: EdgeInsets.symmetric(vertical: 32.0f, horizontal: 16.0f),
                                    child: new Column(
                                        children: new List<Widget> {
                                            this._buildNameField(),
                                            this._buildEmailField(),
                                            this._buildLocationField(),
                                            this._buildPinField(),
                                            this._buildTagsField(),
                                        }
                                    )
                                ),
                                new Padding(
                                    padding: EdgeInsets.symmetric(vertical: 32.0f, horizontal: 16.0f),
                                    child: this._buildChatTextField()
                                ),
                            }
                        )
                    )
                )
            );
        }
    }
}