package com.unity.uiwidgets.plugin;

import android.util.Log;

import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.List;

import static com.unity.uiwidgets.plugin.Utils.TAG;

public class UIWidgetsMessageManager {

    private static UIWidgetsMessageManager _instance;

    private String gameObjectName;


    public static UIWidgetsMessageManager getInstance() {
        if (_instance == null) {
            _instance = new UIWidgetsMessageManager();
        }
        return _instance;
    }

    public void SetObjectName(String name) {
        gameObjectName = name;
    }

    public void UIWidgetsMethodMessage(String channel, String method, List<Object> args) {
        JSONObject object = new JSONObject();

        try {
            object.put("channel", channel);
            object.put("method", method);
            object.put("args", new JSONArray(args));
            UnityPlayer.UnitySendMessage(gameObjectName, "OnUIWidgetsMethodMessage", object.toString());
        } catch (JSONException e) {
            Log.e(TAG, "error parse json", e);
        }
    }
}
