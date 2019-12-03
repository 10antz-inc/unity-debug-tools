using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SRF;
using UnityEngine.Networking;

namespace AppliDebugTool {
	public class LogSlackMessenger : MonoBehaviour {
		/// <summary>
		/// Slackに送信する際に必要なパラメータ
		/// </summary>
		public class SendParam {
			/// <summary>
			/// Slackに送信する際につけるコメント
			/// </summary>
			public string comment;
			/// <summary>
			/// 直近のエラーログを何個送信するか
			/// </summary>
			public int maxErrorLogCount;
			/// <summary>
			/// html形式で全てのログを送信するかどうか
			/// </summary>
			public bool isSendAllLog;
		}

		/// <summary>
		/// ファイル送信用アプリ(bot)のアクセストークン
		/// </summary>
		[SerializeField]
		private string accessToken = "xoxb-xxxxxxxxxxxx-xxxxxxxxxxxx-xxxxxxxxxxxxxxxxxxxxxxxx";
		/// <summary>
		/// 送信先のチャンネルid
		/// </summary>
		[SerializeField]
		private string channelId = "XXXXXXXXX";
		/// <summary>
		/// 送信先のWebHookURL
		/// </summary>
		[SerializeField]
		private string webHookURL = "https://hooks.slack.com/services/XXXXXXXXX/XXXXXXXXX/xxxxxxxxxxxxxxxxxxxxxxxx";
		/// <summary>
		/// スクショ、html以外のログ・情報を送るユーザーの名前
		/// </summary>
		[SerializeField]
		private string userName = "LogSlackMessenger";

		private Texture2D screenshotTexture;

		/// <summary>
		/// スクリーンショットを撮る前に行う処理
		/// </summary>
		void ScreenShotPreProcess() {
			//***********************************************************************************************プロジェクトごとに定義//
		}

		/// <summary>
		/// スクリーンショットを撮った後に行う処理
		/// </summary>
		void ScreenShotPostProcess() {
			//***********************************************************************************************プロジェクトごとに定義//
		}

        /// <summary>
        /// Slackにメッセージを送信する
        /// </summary>
		public void SendToSlack(SendParam　sendParam) {
			StartCoroutine(PostMainRoutine(sendParam));
		}

        /// <summary>
        /// 全体のログを取得する
        /// </summary>
        private List<LogInfo> GetLogInfoList() {
            List<LogInfo> ret = new List<LogInfo>();
			var logCtrl = GameObject.FindObjectOfType<LogController>();
			if(logCtrl == null) {
				Debug.LogWarning("LogControllerがシーン上に存在しません");
			} else {
				ret = logCtrl.LogInfoList;
			}
			
            return ret;
        }

        /// <summary>
        /// システム情報の文字列を取得する
        /// </summary>
        /// <returns></returns>
		string GetSystemString() {
			string ret = "";
			ret += string.Format("[Operationg System,{0}]\n", SystemInfo.operatingSystem);
			ret += string.Format("[Device Type,{0}]\n", SystemInfo.deviceType);
			ret += string.Format("[Device Model,{0}]\n", SystemInfo.deviceModel);
			ret += string.Format("[CPU Type,{0}]\n", SystemInfo.processorType);
			ret += string.Format("[CPU Count,{0}]\n", SystemInfo.processorCount);
			ret += string.Format("[System Memory,{0}MB]", SystemInfo.systemMemorySize);

			return ret;
		}

        /// <summary>
        /// ランタイム情報の文字列を取得する
        /// </summary>
		string GetRunTimeString() {
			string ret = "";
			ret += string.Format("[Play Time,{0}Sec]\n",Time.realtimeSinceStartup);
			ret += string.Format("[Quality Level,{0}]", QualitySettings.GetQualityLevel());

			return ret;
		}

        /// <summary>
        /// ユーザー情報文字列を取得する
        /// </summary>
		string GetUserInfoString() {
			//***********************************************************************************************プロジェクトごとに定義//
			string ret = "";
			ret += string.Format("[Player ID,{0}]\n","[playerId]");
			ret += string.Format("[Version,{0}]\n","[version]");
			ret += string.Format("[API Server,{0}]\n","[api server]");

			return ret;
		}

        /// <summary>
        /// システム、ユーザー、ランタイム情報をJson化をする
        /// </summary>
		public string BuildInfoJson(SendParam　sendParam) {
			var ht = new Hashtable();
			ht.Add("username",userName);
			ht.Add("text", sendParam.comment);

			// SystemInfo
			var attachmentList = new List<Hashtable>();
			var systemInfoTable = new Hashtable();
			systemInfoTable.Add("color","#000000");
			systemInfoTable.Add("title","SystemInfo");
			systemInfoTable.Add("text",GetSystemString());
			attachmentList.Add(systemInfoTable);

			// RunTime
			var runtimeTable = new Hashtable();
			runtimeTable.Add("color","#000000");
			runtimeTable.Add("title","RunTime");
			runtimeTable.Add("text",GetRunTimeString());
			attachmentList.Add(runtimeTable);

			// UserInfo
			var userInfoTable = new Hashtable();
			userInfoTable.Add("color","#000000");
			userInfoTable.Add("title","UserInfo");
			userInfoTable.Add("text",GetUserInfoString());
			attachmentList.Add(userInfoTable);

			ht.Add("attachments", attachmentList);

			return Json.Serialize(ht);
		}

        /// <summary>
        /// エラー系のログのJsonを作成する
        /// </summary>
		public  IEnumerable<string> BuildConsoleErrorLogJson(SendParam　sendParam) {
			var ht = new Hashtable();
			ht.Add("username",userName);

			// 直近のエラーログを取得
			var logInfoList = GetLogInfoList();
			List<LogInfo> latestErrorLogInfoList = new List<LogInfo>();
			foreach(var logInfo in logInfoList) {
				if( logInfo.logType == LogType.Assert ||
					logInfo.logType == LogType.Error  ||
					logInfo.logType == LogType.Exception) {
						latestErrorLogInfoList.Add(logInfo);
				}

				int maxErrorLogNum = sendParam.maxErrorLogCount;
				if(latestErrorLogInfoList.Count > maxErrorLogNum) {
					latestErrorLogInfoList.RemoveAt(0);
				}
			}

			// テーブルを作成
			foreach(var logInfo in latestErrorLogInfoList) {
				string text = "`" + logInfo.logType.ToString() + "`\n";
				text += "`" + logInfo.logText + "`\n";
				StringReader sr = new StringReader(logInfo.stackTrace);
				while(sr.Peek() > -1) {
					text += ">" + sr.ReadLine() + "\n";
				}
				ht["text"] = text;

				yield return Json.Serialize(ht);
			}
		}

        /// <summary>
        /// 全てのログを出力するHtmlを作成する
        /// </summary>
		public string BuildAllLogsHtml() {
			// htmlページ全体のテンプレート
			string baseHtml = Resources.Load<TextAsset>("DebugLog/Text/base").text;
			// ログ一つ一つのhtmlテンプレート
			string itemTmp = Resources.Load<TextAsset>("DebugLog/Text/logitem").text;

			// ページ全体のテンプレートに挿入するhtml
			string logItems = "";
			var logInfoList = GetLogInfoList();
			foreach(var logInfo in logInfoList) {
				string style = "";
				string type = logInfo.logType.ToString();
				string logText = ReplaceSpecialCharacter(logInfo.logText);
				string stackTrace = ReplaceSpecialCharacter(logInfo.stackTrace);
				switch(logInfo.logType) {
					case LogType.Assert:
					case LogType.Error:
					case LogType.Exception:
						style = "error";
						break;
					case LogType.Warning:
						style = "warning";
						break;
					case LogType.Log:
						style = "normal";
						break;
				}

				logItems += string.Format(itemTmp, style, type, logText, stackTrace);
			}

			baseHtml = baseHtml.Replace("$$ALL_LOGS$$", logItems);

			return baseHtml;
		}
        
        /// <summary>
        /// 特殊文字を変換する
        /// </summary>
        private string ReplaceSpecialCharacter(string str) {
	        str = str.Replace("&","&amp;");
	        str = str.Replace("<","&lt;");
	        str = str.Replace(">","&gt;");
	        str = str.Replace("'", "&#39;");
	        str = str.Replace("\"","&quot;");
	        return str;
        }

        /// <summary>
        /// Slackに情報を送信するためのメインルーチン
        /// </summary>
		IEnumerator PostMainRoutine(SendParam　sendParam) {
			ScreenShotPreProcess();
			yield return null;

			yield return ScreenShot();

			ScreenShotPostProcess();

			yield return PostScreenShot();

			yield return PostInfo(sendParam);

			if(sendParam.isSendAllLog) {
				yield return PostAllLogHtml();
			} else {
				yield return PostErrorLogs(sendParam);
			}
		}

        /// <summary>
        /// システム、ユーザー、ランタイム情報をSlackに送信する
        /// </summary>
		public IEnumerator PostInfo(SendParam　sendParam) {
			var json = BuildInfoJson(sendParam);
			var jsonBytes = Encoding.UTF8.GetBytes(json);

			UnityWebRequest request = new UnityWebRequest(webHookURL, "POST");
			request.SetRequestHeader("Content-type", "application/json");
			request.SetRequestHeader("Accept", "application/json");
			request.SetRequestHeader("Method", "POST");
			request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonBytes);
			request.downloadHandler = new DownloadHandlerBuffer();
			yield return request.SendWebRequest();

			if(!string.IsNullOrEmpty(request.downloadHandler.text)) {
				Debug.Log("DebugLogSendSystemInfo:" + request.downloadHandler.text);
			}
		}

        /// <summary>
        /// エラー系のログをSlackに送信する
        /// </summary>
		public IEnumerator PostErrorLogs(SendParam　sendParam) {
			byte[] jsonBytes;

			foreach(var logtrace in BuildConsoleErrorLogJson(sendParam)) {
				jsonBytes = Encoding.UTF8.GetBytes(logtrace);
				
				UnityWebRequest request = new UnityWebRequest(webHookURL, "POST");
				request.SetRequestHeader("Content-type", "application/json");
				request.SetRequestHeader("Accept", "application/json");
				request.SetRequestHeader("Method", "POST");
				request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonBytes);
				request.downloadHandler = new DownloadHandlerBuffer();
				yield return request.SendWebRequest();

				if(!string.IsNullOrEmpty(request.downloadHandler.text)) {
					Debug.Log("DebugLogSendErrorLog:" + request.downloadHandler.text);
				}
			}
		}

        /// <summary>
        /// スクリーンショットをとる
        /// </summary>
		IEnumerator ScreenShot() {
			yield return new WaitForEndOfFrame();

			// スクリーンショット
			var texture = new Texture2D(Screen.width, Screen.height);
			texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			texture.Apply();

			// 画像を小さくする
			float w = Screen.width / 320.0f;
			float h = Screen.height / 568.0f;
			float dst_w = 0;
			float dst_h = 0;
			if(h > w) {
				dst_h = 320 * Screen.height / Screen.width;
				dst_w = 320;
			} else {
				dst_h = 568;
				dst_w = 568 * Screen.width / Screen.height;
			}
			var resizedTexture = ResizeTexture(texture,(int)dst_w, (int)dst_h);

			screenshotTexture = resizedTexture;
		}

        /// <summary>
        /// スクリーンショットをSlackに送信する
        /// </summary>
		IEnumerator PostScreenShot() {
			// 画像データ送信
			var form = new WWWForm();
			var content = screenshotTexture.EncodeToPNG();

			form.AddField("token", accessToken);
			form.AddField("title", "ScreenShot");
			form.AddField("channels", channelId);

			form.AddBinaryData("file", content, "ScreenShot", "image/png");

			var url = "https://slack.com/api/files.upload";
			UnityWebRequest request = UnityWebRequest.Post(url, form);
			yield return request.SendWebRequest();

			if(!string.IsNullOrEmpty(request.downloadHandler.text)) {
				Debug.Log("DebugLogSendScreenShot:" + request.downloadHandler.text);
			}
		}

        /// <summary>
        /// 全てのログのHtmlをSlackに送信する
        /// </summary>
		public IEnumerator PostAllLogHtml() {
			var html = BuildAllLogsHtml();
			
			var form = new WWWForm();
			var content = System.Text.Encoding.UTF8.GetBytes(html);
			
			form.AddField("token", accessToken);
			form.AddField("title", "AllLogs");
			form.AddField("channels", channelId);

			form.AddBinaryData("file", content, "alllog.html", "text/html");

			var url = "https://slack.com/api/files.upload";
			UnityWebRequest request = UnityWebRequest.Post(url, form);
			yield return request.SendWebRequest();

			if(!string.IsNullOrEmpty(request.downloadHandler.text)) {
				Debug.Log("DebugLogSendAllLogs:" + request.downloadHandler.text);
			}
		}

        /// <summary>
        /// テクスチャのサイズを変換
        /// </summary>
        /// <param name="src">元テクスチャ</param>
        /// <param name="dst_w">変換後のテクスチャの幅</param>
        /// <param name="dst_h">変換後のテクスチャの高さ</param>
        /// <returns>変換後のテクスチャ</returns>
		Texture2D ResizeTexture(Texture2D src, int dst_w, int dst_h) {
			Texture2D dst = new Texture2D(dst_w, dst_h, src.format, false);

			float inv_w = 1f / dst_w;
			float inv_h = 1f / dst_h;

			for (int y = 0; y < dst_h; ++y) {
				for (int x = 0; x < dst_w; ++x) {
					dst.SetPixel(x, y, src.GetPixelBilinear((float) x * inv_w, (float) y * inv_h));
				}
			}

			return dst;
		}
	}
}
