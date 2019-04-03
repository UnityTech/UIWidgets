using System;
using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.Networking;

public class HttpRequestSample : UIWidgetsPanel
{
    protected override Widget createWidget() {
        return new MaterialApp(
            title: "Http Request Sample",
            home: new Scaffold(
                body:new AsyncRequestWidget(this.gameObject)
                )
        );
    }
}

public class AsyncRequestWidget : StatefulWidget {
    
    public readonly GameObject gameObjOfUIWidgetsPanel;

    public AsyncRequestWidget(GameObject gameObjOfUiWidgetsPanel, Key key = null) : base(key) {
        this.gameObjOfUIWidgetsPanel = gameObjOfUiWidgetsPanel;
    }

    public override State createState() {
        return new _AsyncRequestWidgetState();
    }
}

[Serializable]
public class TimeData {
    public long currentFileTime;
}

class _AsyncRequestWidgetState : State<AsyncRequestWidget> {
    
    long _fileTime;
    
    public override Widget build(BuildContext context) {
        
        return new Column(
            children: new List<Widget>() {
                new FlatButton(child: new Text("Click To Get Time"), onPressed: () => {
                    UnityWebRequest www = UnityWebRequest.Get("http://worldclockapi.com/api/json/est/now");
                    var asyncOperation  = www.SendWebRequest();
                    asyncOperation.completed += operation => {
                        var timeData = JsonUtility.FromJson<TimeData>(www.downloadHandler.text);
                        using(WindowProvider.of(this.widget.gameObjOfUIWidgetsPanel).getScope())
                        {
                            this.setState(() => { this._fileTime = timeData.currentFileTime; });
                        }
                       
                    };
                }),
                new Text($"current file time: {this._fileTime}")
            });
    }
}