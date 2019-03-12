package com.unity.uiwidgets.plugin.editing;

import android.content.Context;
import android.util.Log;
import android.view.KeyCharacterMap;
import android.view.KeyEvent;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputConnection;
import android.view.inputmethod.InputMethodManager;

import org.json.JSONException;

import static com.unity.uiwidgets.plugin.Utils.TAG;

public class TextInputView extends View  {
    private InputConnection mLastInputConnection;
    private final InputMethodManager mImm;
    public TextInputView(Context context) {
        super(context);
        setFocusable(true);
        setFocusableInTouchMode(true);
        mImm = (InputMethodManager) getContext().getSystemService(Context.INPUT_METHOD_SERVICE);
    }

    @Override
    public InputConnection onCreateInputConnection(EditorInfo outAttrs) {
        Log.i(TAG, "onCreateInputConnection");
        try {
            mLastInputConnection = TextInputPlugin.getInstance().createInputConnection(this, outAttrs);
            return mLastInputConnection;
        } catch (JSONException e) {
            Log.e(TAG, "Failed to create input connection", e);
            return null;
        }
    }

    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (event.getDeviceId() != KeyCharacterMap.VIRTUAL_KEYBOARD) {
            if (mLastInputConnection != null && mImm.isAcceptingText()) {
                mLastInputConnection.sendKeyEvent(event);
            }
        }

        return super.onKeyDown(keyCode, event);
    }
}
