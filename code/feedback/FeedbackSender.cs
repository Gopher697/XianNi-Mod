using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace xn.feedback
{
    public static class FeedbackSender
    {
        private const string FEEDBACK_URL = "https://1334698288-1wpjyathul.ap-guangzhou.tencentscf.com/feedback";
        private const string EmptyContent = "(\u65e0\u5185\u5bb9)";
        private const string FeedbackType = "\u8bc4\u4ef7";
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
        [Serializable]
        private class FeedbackData
        {
            public int rating;
            public string content;
            public string version;
            public string type;
        }
        [Serializable]
        private class FeedbackResponse
        {
            public bool success;
            public string message;
        }
        public static IEnumerator SendFeedback(int rating, string content, string version, Action<bool, string> callback)
        {
            var data = new FeedbackData
            {
                rating = rating,
                content = string.IsNullOrEmpty(content) ? EmptyContent : content,
                version = version,
                type = FeedbackType
            };
            string json = JsonUtility.ToJson(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            using (var request = new UnityWebRequest(FEEDBACK_URL, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 15;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<FeedbackResponse>(request.downloadHandler.text);
                        if (response != null)
                        {
                            callback?.Invoke(response.success, response.message ?? T("feedback_submit_success", "Submitted successfully. Thank you for your feedback!"));
                        }
                        else
                        {
                            callback?.Invoke(true, T("feedback_submit_success", "Submitted successfully. Thank you for your feedback!"));
                        }
                    }
                    catch
                    {
                        callback?.Invoke(true, T("feedback_submit_success", "Submitted successfully. Thank you for your feedback!"));
                    }
                }
                else
                {
                    string errorMsg = T("feedback_submit_failed", "Submission failed. Please try again later.");
                    if (request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        errorMsg = T("feedback_connection_failed", "Network connection failed. Please check your connection.");
                    }
                    else if (request.responseCode == 429)
                    {
                        errorMsg = T("feedback_already_submitted_today", "You have already submitted feedback today. Thank you for the support!");
                        callback?.Invoke(true, errorMsg); 
                        yield break;
                    }
                    callback?.Invoke(false, errorMsg);
                }
            }
        }
    }
}
