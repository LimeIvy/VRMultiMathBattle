using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VRMultiMathBattle.Gameplay
{
    /// <summary>
    /// 黒板の描画内容をキャプチャしてAI採点に送信するクラス
    /// </summary>
    public class BlackboardCapture : MonoBehaviour
    {
        [Header("Capture Settings")]
        [SerializeField] private Camera captureCamera;
        [SerializeField] private int captureWidth = 1024;
        [SerializeField] private int captureHeight = 1024;
        [SerializeField] private LayerMask captureLayer;

        [Header("UI References")]
        [SerializeField] private Button submitButton;
        [SerializeField] private RawImage previewImage; // プレビュー表示用（オプション）

        [Header("Dependencies")]
        [SerializeField] private ProblemManager problemManager;

        private Texture2D capturedTexture;
        private bool isProcessing = false;

        private void Start()
        {
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitButtonClicked);
            }

            // キャプチャ用カメラの設定
            if (captureCamera == null)
            {
                Debug.LogWarning("Capture Camera が設定されていません");
            }
            else
            {
                captureCamera.enabled = false; // 通常時は無効
                captureCamera.cullingMask = captureLayer;
            }
        }

        /// <summary>
        /// 送信ボタンクリック時の処理
        /// </summary>
        private void OnSubmitButtonClicked()
        {
            if (isProcessing)
            {
                Debug.LogWarning("既に処理中です");
                return;
            }

            if (problemManager == null || problemManager.CurrentProblem == null)
            {
                Debug.LogError("問題が読み込まれていません");
                return;
            }

            StartCoroutine(CaptureAndSubmit());
        }

        /// <summary>
        /// 黒板をキャプチャして採点APIに送信
        /// </summary>
        private IEnumerator CaptureAndSubmit()
        {
            isProcessing = true;

            // ボタンを無効化
            if (submitButton != null)
            {
                submitButton.interactable = false;
            }

            // 黒板をキャプチャ
            yield return StartCoroutine(CaptureBlackboard());

            if (capturedTexture == null)
            {
                Debug.LogError("キャプチャに失敗しました");
                isProcessing = false;
                if (submitButton != null)
                {
                    submitButton.interactable = true;
                }
                yield break;
            }

            // プレビュー表示（オプション）
            if (previewImage != null)
            {
                previewImage.texture = capturedTexture;
            }

            // Base64エンコード
            byte[] imageBytes = capturedTexture.EncodeToJPG(75); // 品質75%で圧縮
            string base64Image = Convert.ToBase64String(imageBytes);

            Debug.Log($"画像サイズ: {imageBytes.Length / 1024}KB");

            // 採点リクエストを作成
            API.ApiService.GradeRequest gradeRequest = new API.ApiService.GradeRequest
            {
                problemId = problemManager.GetCurrentProblemId(),
                imageBase64 = base64Image,
                playerName = "Player1" // TODO: 実際のプレイヤー名を取得
            };

            // 採点APIに送信
            yield return StartCoroutine(
                API.ApiService.Instance.GradeAnswer(
                    gradeRequest,
                    onSuccess: OnGradeSuccess,
                    onError: OnGradeError
                )
            );

            isProcessing = false;

            // ボタンを有効化
            if (submitButton != null)
            {
                submitButton.interactable = true;
            }
        }

        /// <summary>
        /// 黒板の描画内容をキャプチャ
        /// </summary>
        private IEnumerator CaptureBlackboard()
        {
            if (captureCamera == null)
            {
                Debug.LogError("Capture Camera が設定されていません");
                yield break;
            }

            // RenderTextureを作成
            RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            RenderTexture previousRT = captureCamera.targetTexture;
            captureCamera.targetTexture = renderTexture;

            // カメラでレンダリング
            captureCamera.Render();

            // RenderTextureからTexture2Dに変換
            RenderTexture.active = renderTexture;
            capturedTexture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            capturedTexture.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            capturedTexture.Apply();

            // クリーンアップ
            captureCamera.targetTexture = previousRT;
            RenderTexture.active = null;
            Destroy(renderTexture);

            yield return null;
        }

        /// <summary>
        /// 採点成功時の処理
        /// </summary>
        private void OnGradeSuccess(API.ApiService.GradeResponse response)
        {
            Debug.Log($"採点結果: {(response.isCorrect ? "正解" : "不正解")}");
            Debug.Log($"フィードバック: {response.feedback}");

            // TODO: UIにフィードバックを表示
            ShowFeedback(response);
        }

        /// <summary>
        /// 採点エラー時の処理
        /// </summary>
        private void OnGradeError(string error)
        {
            Debug.LogError($"採点エラー: {error}");

            // TODO: エラーメッセージをUIに表示
        }

        /// <summary>
        /// フィードバックを表示
        /// </summary>
        private void ShowFeedback(API.ApiService.GradeResponse response)
        {
            // TODO: 3D空間にフィードバックを表示
            // 例: 黒板の横にテキストを表示、パーティクルエフェクト、サウンド再生など
            
            string message = response.isCorrect 
                ? $"✓ 正解！\n{response.feedback}" 
                : $"✗ 不正解\n{response.feedback}";
            
            Debug.Log(message);
        }

        /// <summary>
        /// 黒板をクリア
        /// </summary>
        public void ClearBlackboard()
        {
            // TODO: 描画システムと連携して黒板をクリア
            Debug.Log("黒板をクリアしました");
        }

        private void OnDestroy()
        {
            if (capturedTexture != null)
            {
                Destroy(capturedTexture);
            }
        }
    }
}
