using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AppliDebugTool;

public class DebugLogSample : MonoBehaviour {
    [SerializeField]
    InputField commentInput;
    [SerializeField]
    InputField maxErrorLogCountInput;
    [SerializeField]
    Toggle isSendAllLogToggle;
    [SerializeField]
    LogSlackMessenger logSlackMessenger;

    void Start() {
        for(int i = 0; i < 5; i++) {
            Debug.Log(i.ToString() + "test log");
        }
        for(int i = 0; i < 5; i++) {
            Debug.LogWarning(i.ToString() + "test log warning");
        }
        for(int i = 0; i < 5; i++) {
            Debug.LogError(i.ToString() + "test log error");
        }
    }

    public void OnClickSend() {
        LogSlackMessenger.SendParam sendParam = new LogSlackMessenger.SendParam();
        sendParam.comment = commentInput.text;
        sendParam.maxErrorLogCount = int.Parse(maxErrorLogCountInput.text);
        sendParam.isSendAllLog = isSendAllLogToggle.isOn;
        logSlackMessenger.SendToSlack(sendParam);
    }
}
