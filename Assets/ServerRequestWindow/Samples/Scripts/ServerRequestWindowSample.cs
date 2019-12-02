using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerRequestWindowSample : MonoBehaviour {
    [SerializeField] InputField urlInput;
    [SerializeField] InputField jsonInput;
    bool _isWait = false;

    void Awake() {
        ServerAPILogRecorder.StartStream();
    }

    void OnDestroy() {
        ServerAPILogRecorder.EndStream();
    }

    public void OnClickSend() {
        if (_isWait) {
            return;
        }

        _isWait = true;
        ServerAPILogRecorder.Record(new ServerAPILogRecorder.RecordData() {
            date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
            url = urlInput.text,
            type = ServerAPILogRecorder.Type.POST,
            json = jsonInput.text,
        });

        StartCoroutine(ResponseRoutine());
    }

    IEnumerator ResponseRoutine() {
        yield return new WaitForSeconds(0.5f);
        
        ServerAPILogRecorder.Record(new ServerAPILogRecorder.RecordData() {
            date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
            url = urlInput.text,
            type = ServerAPILogRecorder.Type.RESPONCE,
            json = "{\"Info\":{\"User\":{\"nickName\":\"taro\",\"level\":8,\"Link\":{\"Email\":[],\"Social\":[]}}},\"additional\":{\"progressMissionList\":[]}}",
        });

        _isWait = false;
    }
}
