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
                content = string.IsNullOrEmpty(content) ? "(无内容)" : content,
                version = version,
                type = "评价"
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
                            callback?.Invoke(response.success, response.message ?? "提交成功，感谢您的反馈！");
                        }
                        else
                        {
                            callback?.Invoke(true, "提交成功，感谢您的反馈！");
                        }
                    }
                    catch
                    {
                        callback?.Invoke(true, "提交成功，感谢您的反馈！");
                    }
                }
                else
                {
                    string errorMsg = "提交失败，请稍后重试";
                    if (request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        errorMsg = "网络连接失败，请检查网络";
                    }
                    else if (request.responseCode == 429)
                    {
                        errorMsg = "您今天已经评价过了，感谢支持！";
                        callback?.Invoke(true, errorMsg); 
                        yield break;
                    }
                    callback?.Invoke(false, errorMsg);
                }
            }
        }
    }
}