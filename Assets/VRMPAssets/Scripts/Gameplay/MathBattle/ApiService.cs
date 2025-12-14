using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace VRMultiMathBattle.API
{
    /// <summary>
    /// バックエンドAPIとの通信を管理するサービスクラス
    /// </summary>
    public class ApiService : MonoBehaviour
    {
        [Header("API Settings")]
        [SerializeField] private string baseUrl = "http://localhost:8787";

        private static ApiService _instance;
        public static ApiService Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ApiService");
                    _instance = go.AddComponent<ApiService>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #region Problem API

        /// <summary>
        /// 全ての問題を取得
        /// </summary>
        public IEnumerator GetAllProblems(Action<Problem[]> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/problems";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Problem[] problems = JsonConvert.DeserializeObject<Problem[]>(request.downloadHandler.text);
                        onSuccess?.Invoke(problems);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"JSONパースエラー: {e.Message}");
                    }
                }
                else
                {
                    onError?.Invoke($"エラー: {request.error}");
                }
            }
        }

        /// <summary>
        /// ランダムな問題を取得
        /// </summary>
        public IEnumerator GetRandomProblem(Action<Problem> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/problems/random";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Problem problem = JsonConvert.DeserializeObject<Problem>(request.downloadHandler.text);
                        onSuccess?.Invoke(problem);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"JSONパースエラー: {e.Message}");
                    }
                }
                else
                {
                    onError?.Invoke($"エラー: {request.error}");
                }
            }
        }

        /// <summary>
        /// 新規問題を作成
        /// </summary>
        public IEnumerator CreateProblem(CreateProblemRequest request, Action<Problem> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/problems";
            string jsonData = JsonConvert.SerializeObject(request);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Problem problem = JsonConvert.DeserializeObject<Problem>(webRequest.downloadHandler.text);
                        onSuccess?.Invoke(problem);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"JSONパースエラー: {e.Message}");
                    }
                }
                else
                {
                    onError?.Invoke($"エラー: {webRequest.error}");
                }
            }
        }

        #endregion

        #region Grade API

        /// <summary>
        /// 画像を送信して採点
        /// </summary>
        public IEnumerator GradeAnswer(GradeRequest gradeRequest, Action<GradeResponse> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/grade";
            string jsonData = JsonConvert.SerializeObject(gradeRequest);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        GradeResponse response = JsonConvert.DeserializeObject<GradeResponse>(webRequest.downloadHandler.text);
                        onSuccess?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"JSONパースエラー: {e.Message}");
                    }
                }
                else
                {
                    onError?.Invoke($"エラー: {webRequest.error}\n{webRequest.downloadHandler.text}");
                }
            }
        }

        #endregion

        #region Data Models

        [Serializable]
        public class Problem
        {
            public int id;
            public string question;
            public string correctAnswer;
            public int difficulty;
            public string category;
        }

        [Serializable]
        public class CreateProblemRequest
        {
            public string question;
            public string correctAnswer;
            public int difficulty;
            public string category;
        }

        [Serializable]
        public class GradeRequest
        {
            public int problemId;
            public string imageBase64;
            public string playerName;
        }

        [Serializable]
        public class GradeResponse
        {
            public int id;
            public bool isCorrect;
            public string feedback;
        }

        #endregion
    }
}
