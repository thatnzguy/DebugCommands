// --- DebugCommands ----------------------------------------------------------
// MIT License
//
// Copyright (c) 2020 Joel Schroyen
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DebugCommands : MonoBehaviour {
    private static DebugCommands s_Instance;
    public static DebugCommands Instance {
        get {
            if (s_Instance == null) {
                DebugCommands foundDebugCommands = FindObjectOfType<DebugCommands>();
                if (foundDebugCommands != null) {
                    s_Instance = foundDebugCommands;
                } else {
                    GameObject instance = new GameObject("DebugCommands", typeof(DebugCommands));
                    DontDestroyOnLoad(instance);
                    s_Instance = instance.GetComponent<DebugCommands>();
                }
            }

            return s_Instance;
        }
    }

    [Header("GUI Display")]
    [SerializeField] private Vector2 m_Offset = new Vector2(10, 10);

    [SerializeField] private float m_Width = 200;
    [SerializeField] private float m_LineHeight = 20;
    [SerializeField, Range(0, 1)] private float m_BackgroundAlpha = 0.4f;

    private static GUIStyle s_BoxStyle;
    private static Texture2D s_BackgroundTexture;

    private GUIStyle BoxStyle {
        get {
            if (s_BoxStyle == null) {
                s_BoxStyle = CreateBoxStyle();
            }

            return s_BoxStyle;
        }
    }

    private void Awake() {
        if (!Application.isEditor) {
            enabled = false;
        }

        if (s_Instance != null && s_Instance != this) {
            Debug.LogError("DebugCommands Instance should not already exist!", this);
        }

        s_Instance = this;
    }

    public class Command {
        public Action Callback;
        public KeyCode KeyCode;

        public Command(Action callback, KeyCode keyCode) {
            Callback = callback;
            KeyCode = keyCode;
        }
    }

    private List<Command> m_Commands = new List<Command>();

    //TODO maybe we should instead bind many actions to a single key if desired and they're all called. 
    //TODO support keyboard modifiers, any combination of ctrl+shift+alt
    //TODO add groups, and pages
    
    public Command AddDebugCommand(Action callback, KeyCode keyCode) {
        foreach (Command command in m_Commands) {
            if (command.KeyCode == keyCode) {
                string name = command.Callback.GetMethodInfo().Name;
                Debug.LogWarning("Debug command " + name + " with keycode " + command.KeyCode + " already exists.");
                return null;
            }
        }
        Command newCommand = new Command(callback, keyCode);
        m_Commands.Add(newCommand);

        return newCommand;
    }

    public void RemoveCommand(Command command) {
        m_Commands.Remove(command);
    }

    public void RemoveCommand(Action callback) {
        for (int i = m_Commands.Count - 1; i >= 0; i--) {
            Command command = m_Commands[i];
            if (command.Callback == callback) {
                m_Commands.Remove(command);
            }
        }
    }

    private void Update() {
        foreach (Command command in m_Commands) {
            if (Input.GetKeyDown(command.KeyCode)) {
                command.Callback();
            }
        }
    }

    private void OnGUI() {
        int debugCommands = m_Commands.Count;
        float totalHeight = debugCommands * m_LineHeight;

        if (debugCommands == 0) {
            return;
        }

        Vector2 topLeftCorner = new Vector2(Screen.width - m_Offset.x - m_Width - 20, Screen.height - m_Offset.y - totalHeight - 20);
        
        GUI.Box(new Rect(topLeftCorner.x, topLeftCorner.y, m_Width, totalHeight + 20), GUIContent.none, BoxStyle);

        for (int i = 0; i < debugCommands; i++) {
            Command command = m_Commands[i];
            string name = command.Callback.GetMethodInfo().Name;
            string key = command.KeyCode.ToString();
            string display = name + ": " + key;
            GUI.Label(new Rect(topLeftCorner.x + 10.0f, topLeftCorner.y + 10.0f + i * m_LineHeight, m_Width, m_LineHeight), display);
        }
    }

    private void OnValidate() {
        s_BoxStyle = null;
    }

    private GUIStyle CreateBoxStyle() {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        s_BackgroundTexture = new Texture2D(2, 2);
        Color[] pix = new Color[2*2];

        for (int i = 0; i < pix.Length; i++) {
            pix[i] = new Color(0, 0, 0, m_BackgroundAlpha);
        }

        s_BackgroundTexture.SetPixels(pix);
        s_BackgroundTexture.Apply();
        boxStyle.normal.background = s_BackgroundTexture;
        return boxStyle;
    }
}