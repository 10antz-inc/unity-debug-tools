using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AppliDebugTool {
    public class LogInfo {
        public LogType logType;
        public string logText;
        public string stackTrace;
    }

    /// <summary>
    /// 出力されたログを管理するクラス
    /// </summary>
    public class LogController : MonoBehaviour {
        /// <summary>
        /// 最大何個までログを残しておくか
        /// </summary>
        [SerializeField]
        int maxStackLogInfoCount = 100;
        List<LogInfo> logInfoList = new List<LogInfo>();
        public List<LogInfo> LogInfoList {
            get {
                return logInfoList;
            }
        }

        void OnEnable() {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable() {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type) {
            LogInfo logInfo = new LogInfo();
            logInfo.logType = type;
            logInfo.logText = logString;
            logInfo.stackTrace = stackTrace;
            logInfoList.Add(logInfo);

            if(logInfoList.Count > maxStackLogInfoCount) {
                logInfoList.RemoveAt(0);
            }
        }
    }
}
