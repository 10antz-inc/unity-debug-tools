using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using System.Text;

public static class ServerAPILogRecorder {
    public delegate void OnRecorded(RecordData data);
    public static event OnRecorded onRecorded = null;
    
    // jsonに含まれている,を一時的に置き換える文字
    const string COMMA_AVOID = "%%|%%";
    // ログの保存上限数
    const int MAX_DATA = 100;
    public enum Type {
        POST,
        RESPONCE,
    }
    public class RecordData {
        public int id;
        public string date;
        public string url;
        public Type type;
        public string json;
    }
    
    public static string saveLogPath { get;} = "ServerRequestWindow/Logs/ServerLogs.csv";

    public static List<RecordData> _dataList { get; private set; }
    static StreamWriter _writer;
    static bool _isStreamBegan = false;

    public static void StartStream() {
        if (_isStreamBegan) {
            return;
        }
        
        ReadFile();
        
        string path = Application.dataPath + "/" + saveLogPath;
        _writer = new StreamWriter(path,true,Encoding.UTF8);

        _isStreamBegan = true;
    }

    public static void ReadFile() {
        if (_dataList != null) {
            _dataList.Clear();
            _dataList = null;
        }

        string path = Application.dataPath + "/" + saveLogPath;
        if (!File.Exists(path)) {
            File.Create(path).Close();
        }
        
        var reader = new StreamReader(path,Encoding.UTF8);
        _dataList = new List<RecordData>();
        while (!reader.EndOfStream) {
            string line = reader.ReadLine();
            string[] values = line.Split(',');

            RecordData data = new RecordData() {
                id = int.Parse(values[0]),
                date = values[1],
                url = values[2],
                type = (Type)Enum.Parse(typeof(Type),values[3]),
                json = values[4].Replace(COMMA_AVOID,","),
            };
            
            _dataList.Add(data);
        }
        reader.Close();
    }

    public static void EndStream() {
        if (!_isStreamBegan) {
            return;
        }
        
        if (_writer != null) {
            _writer.Close();
            _writer = null;
        }

        _isStreamBegan = false;
    }

    public static void Record(RecordData data) {
        if (_writer == null) {
            return;
        }

        if (_dataList.Count <= 0) {
            data.id = 1;
        } else {
            data.id = _dataList.Select(itr => itr.id).Max() + 1;
        }
        string str = string.Format("{0},{1},{2},{3},{4}", data.id, data.date, data.url, data.type.ToString(),
            data.json.Replace(",",COMMA_AVOID));
        _writer.WriteLine(str);
        _dataList.Add(data);

        if (_dataList.Count > MAX_DATA) {
            DeleteFirstRecord();
        }
        
        if (onRecorded != null) {
            onRecorded(data);
        }
    }

    static void DeleteFirstRecord() {
        if (_writer == null || _dataList.Count <= 0) {
            return;
        }

        EndStream();
        
        string path = Application.dataPath + "/" + saveLogPath;
        string[] lines = File.ReadAllLines(path);
        lines = lines.Skip(1).ToArray();
        File.WriteAllLines(path, lines);
        
        StartStream();
    }

    public static void Clear() {
        bool isBegan = _isStreamBegan;
        if (isBegan) {
            EndStream();
        }
        
        string path = Application.dataPath + "/" + saveLogPath;
        if (File.Exists(path)) {
            File.Delete(path);
        }

        if (isBegan) {
            StartStream();
        }
    }
}
