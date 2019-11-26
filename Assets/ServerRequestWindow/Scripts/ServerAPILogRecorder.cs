using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public static class ServerAPILogRecorder
{
    public static string saveLogPath = "ServerRequestWindow/Logs/";
    public static int maxLog = 100;
    static string extension = ".log";

    public static void RecordAPILog(string filename, string json) {
        string path = Application.dataPath + saveLogPath + filename + extension;
        File.Create(path);
        File.WriteAllText(path,json);
    }

    public static FileInfo[] GetAllLogFileInfo() {
        DirectoryInfo di = new DirectoryInfo(saveLogPath);
        FileInfo[] files = di.GetFiles("*"+extension, SearchOption.TopDirectoryOnly);
        return files;
    }

    public static string[] GetAllLogFilePath() {
        return GetAllLogFileInfo().Select(f => f.FullName).ToArray();
    }
}
