using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        private API.ApiService.Problem currentProblem;

        public API.ApiService.Problem CurrentProblem => currentProblem;

        private void Start()
        {
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
