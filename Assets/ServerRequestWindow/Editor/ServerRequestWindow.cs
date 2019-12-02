using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Runtime.Serialization.Json;
using HC.Common;
using UnityEditor.PackageManager.Requests;

public class ServerRequestWindow : EditorWindow {
    public enum RequestType {
        POST,
        RESPONCE
    }

    public class LogItem {
        public DateTime date;
        public string url;
        public RequestType requestType;
        public string jsonStr;
    }

    [SerializeField]
    TreeViewState treeViewState;
    ServerRequestLogArea _simpleTreeView;
    float _horizontalSplitHorizontalNorm = 0.5f;
    Rect _logAreaRect;
    Rect _dataAreaRect;
    float _padding = 3f;
    float _topSpace = 15f;
    float _horizontalSplitX;
    bool _resizingHorizontalSplitter = false;
    List<LogItem> _logItems = new List<LogItem>();

    public List<LogItem> LogItems {
        get{return _logItems;}
    }
    private string _dataString;
    float _vSbarValue = 0f;

    [MenuItem("DebugTools/ServerRequestWindow")]
    public static void GetWindow() {
        GetWindow(typeof(ServerRequestWindow)).Show();
    }

    void OnEnable() {
        Debug.Log("Enable ServerRequestWindow");
        if (treeViewState == null)
            treeViewState = new TreeViewState();
        
        ServerAPILogRecorder.ReadFile();

        _logItems = CreateData();
        _simpleTreeView = new ServerRequestLogArea(treeViewState, this);

        ReshapeSubArea();
    }

    List<LogItem> CreateData() {
        List<LogItem> ret = new List<LogItem>();
        if (ServerAPILogRecorder._dataList == null) {
            return ret;
        }
        
        foreach (var data in ServerAPILogRecorder._dataList) {
            ret.Add(new LogItem() {
                date = DateTime.ParseExact(data.date,"yyyy/MM/dd HH:mm:ss",null),
                url = data.url,
                requestType = (RequestType)Enum.Parse(typeof(RequestType),data.type.ToString()),
                jsonStr = data.json
            });
        }

        return ret;
    }

    void OnGUI() {
        if(GUI.Button(new Rect(_logAreaRect.x,0, 50,15),"Clear")) {
            _logItems.Clear();
            _simpleTreeView.Reload();
            _dataString = "";
            ServerAPILogRecorder.Clear();
        }
        if(GUI.Button(new Rect(_logAreaRect.x + 50,0, 50,15),"Reload")) {
            _logItems.Clear();
            _logItems = CreateData();
            _simpleTreeView.Reload();
            _dataString = "";
        }

        HandleHorizontalResize();
        ReshapeSubArea();

        _simpleTreeView.OnGUI(_logAreaRect);


        if(GUI.Button(new Rect(_dataAreaRect.x,0, 50,15),"Copy")) {
            EditorGUIUtility.systemCopyBuffer = _dataString;
        }
        DrawOutline(_dataAreaRect, 1.0f);
        
        Rect dataAreaRect = new Rect();
        dataAreaRect.x = _dataAreaRect.x + _padding;
        dataAreaRect.y = _dataAreaRect.y + _padding;
        dataAreaRect.width = _dataAreaRect.width - _padding * 2f;
        dataAreaRect.height = _dataAreaRect.height - _padding * 2f;
        GUI.TextArea(dataAreaRect, _dataString);
        
        Repaint();
    }

    static void DrawOutline(Rect rect, float size)
    {
        Color color = new Color(0.6f, 0.6f, 0.6f, 1.333f);
        if(EditorGUIUtility.isProSkin)
        {
            color.r = 0.12f;
            color.g = 0.12f;
            color.b = 0.12f;
        }

        if (Event.current.type != EventType.Repaint)
            return;

        Color orgColor = GUI.color;
        GUI.color = GUI.color * color;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, size), EditorGUIUtility.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - size, rect.width, size), EditorGUIUtility.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - size, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);

        GUI.color = orgColor;
    }

    private void ReshapeSubArea() {
        _horizontalSplitX = position.width * _horizontalSplitHorizontalNorm;
        _logAreaRect = new Rect(_padding, _padding + _topSpace, _horizontalSplitX, position.height - _padding * 2f - _topSpace);
        _dataAreaRect = new Rect(_logAreaRect.xMax + _padding, _padding + _topSpace, position.width * (1.0f - _horizontalSplitHorizontalNorm) - _padding * 3f, position.height - _padding * 2f - _topSpace);
    }

    private void HandleHorizontalResize() {
        Rect splitRect = new Rect(_horizontalSplitX + _padding, _padding, _padding * 2f, position.height - _padding * 2f);
        EditorGUIUtility.AddCursorRect(splitRect, MouseCursor.ResizeHorizontal);

        if (Event.current.type == EventType.MouseDown && splitRect.Contains(Event.current.mousePosition)) {
            _resizingHorizontalSplitter = true;
        }

        if(_resizingHorizontalSplitter) {
            _horizontalSplitHorizontalNorm = Mathf.Clamp((Event.current.mousePosition.x - _padding) / position.width,0.1f,0.9f);
        }

        if(Event.current.type == EventType.MouseUp) {
            _resizingHorizontalSplitter = false;
        }
    }

    public void OnClickItem(int itemId) {
        _dataString = JsonFormatter.ToPrettyPrint(_logItems[itemId].jsonStr,JsonFormatter.IndentType.Space);
    }
}
