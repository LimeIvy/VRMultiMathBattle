using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace VRMultiMathBattle.Gameplay
{
    /// <summary>
    /// 数学問題の管理とUI表示を担当するマネージャー
    /// </summary>
    public class ProblemManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI problemText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private Button newProblemButton;

        [Header("Settings")]
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private bool autoCreateUiIfMissing = true;

        private API.ApiService.Problem currentProblem;

        public API.ApiService.Problem CurrentProblem => currentProblem;

        private void Start()
        {
            if (autoCreateUiIfMissing)
            {
                EnsureUi();
            }

            if (newProblemButton != null)
            {
                newProblemButton.onClick.AddListener(LoadRandomProblem);
            }

            if (autoLoadOnStart)
            {
                LoadRandomProblem();
            }
        }

        /// <summary>
        /// ランダムな問題を読み込む
        /// </summary>
        public void LoadRandomProblem()
        {
            if (problemText != null)
            {
                problemText.text = "問題を読み込み中...";
            }

            StartCoroutine(API.ApiService.Instance.GetRandomProblem(
                onSuccess: OnProblemLoaded,
                onError: OnProblemLoadError
            ));
        }

        /// <summary>
        /// 問題読み込み成功時の処理
        /// </summary>
        private void OnProblemLoaded(API.ApiService.Problem problem)
        {
            currentProblem = problem;
            UpdateUI();
            Debug.Log($"問題を読み込みました: {problem.question}");
        }

        /// <summary>
        /// 問題読み込みエラー時の処理
        /// </summary>
        private void OnProblemLoadError(string error)
        {
            Debug.LogError($"問題の読み込みに失敗: {error}");
            
            if (problemText != null)
            {
                problemText.text = "問題の読み込みに失敗しました";
            }
        }

        /// <summary>
        /// UIを更新
        /// </summary>
        private void UpdateUI()
        {
            if (currentProblem == null) return;

            if (problemText != null)
            {
                problemText.text = currentProblem.question;
            }

            if (difficultyText != null)
            {
                difficultyText.text = $"難易度: {currentProblem.difficulty}";
            }

            if (categoryText != null)
            {
                categoryText.text = $"カテゴリ: {currentProblem.category}";
            }
        }

        private void EnsureUi()
        {
            if (problemText != null && difficultyText != null && categoryText != null && newProblemButton != null)
            {
                return;
            }

            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("MathBattleCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            if (FindObjectOfType<EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<StandaloneInputModule>();
            }

            var panel = new GameObject("ProblemPanel");
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.02f, 0.7f);
            panelRect.anchorMax = new Vector2(0.6f, 0.98f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.45f);

            problemText ??= CreateText(panel.transform, "ProblemText", "問題を読み込み中...", 36, new Vector2(0.02f, 0.6f), new Vector2(0.98f, 0.98f));
            difficultyText ??= CreateText(panel.transform, "DifficultyText", "難易度: -", 24, new Vector2(0.02f, 0.3f), new Vector2(0.48f, 0.6f));
            categoryText ??= CreateText(panel.transform, "CategoryText", "カテゴリ: -", 24, new Vector2(0.52f, 0.3f), new Vector2(0.98f, 0.6f));

            if (newProblemButton == null)
            {
                newProblemButton = CreateButton(panel.transform, "NewProblemButton", "新しい問題", new Vector2(0.02f, 0.05f), new Vector2(0.3f, 0.25f));
            }
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            var textGo = new GameObject(name);
            textGo.transform.SetParent(parent, false);
            var rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var buttonGo = new GameObject(name);
            buttonGo.transform.SetParent(parent, false);
            var rect = buttonGo.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = buttonGo.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 0.9f, 0.9f);

            var button = buttonGo.AddComponent<Button>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 24;
            labelTmp.color = Color.white;
            labelTmp.alignment = TextAlignmentOptions.Center;

            return button;
        }

        /// <summary>
        /// 現在の問題IDを取得
        /// </summary>
        public int GetCurrentProblemId()
        {
            return currentProblem?.id ?? -1;
        }

        /// <summary>
        /// 正解を取得（デバッグ用）
        /// </summary>
        public string GetCorrectAnswer()
        {
            return currentProblem?.correctAnswer ?? "";
        }
    }
}
