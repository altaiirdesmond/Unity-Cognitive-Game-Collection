﻿using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataComponent.Database;
using Assets.Scripts.DataComponent.Model;
using Assets.Scripts.GlobalScripts.Managers;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using TMPro;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GlobalScripts.Game {
    public class RemarkManager : MonoBehaviour {

        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Button _btnContinue;
        [SerializeField] private Button _btnCancel;
        [SerializeField] private Sprite _circleSprite;

        private ScoreDataHolder _scoreData;
        private RectTransform _graphContainer;
        private List<float> _values;

        private void Awake() {
            if (!UserPrefs.SessionActive()) {
                _btnCancel.GetComponent<TextMeshProUGUI>().SetText("Done");
                _btnContinue.gameObject.SetActive(false);
            }

            SceneManager.activeSceneChanged += RemoveEvents;

            _values = new List<float>();

            GameObject _coreGameBehaviourGameObject = new GameObject("CoreGameBehvaiour");
            CoreGameBehaviour _coreGameBehaviourScript = _coreGameBehaviourGameObject.AddComponent<CoreGameBehaviour>();

            // Programmatically add button click events
            _btnContinue.onClick.AddListener(_coreGameBehaviourScript.LoadNextScene);

            _scoreData = FindObjectOfType<ScoreDataHolder>();

            // Know which scene script will ONLY live
            _scoreData.ParentScene = SceneManager.GetActiveScene().name;

            _scoreText.SetText($"{_scoreData.MinScore}/{_scoreData.MaxScore}");

            _graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();

            ShowGraph(_scoreData.category, _scoreData.MinScore, _scoreData.MaxScore);
        }

        private void RemoveEvents(Scene current, Scene next) {
            _btnContinue.onClick = null;
        }

        private GameObject CreateCircle(Vector2 anchoredPosition) {
            GameObject dotInstance = new GameObject("circle", typeof(Image));
            dotInstance.transform.SetParent(_graphContainer, false);
            dotInstance.GetComponent<Image>().sprite = _circleSprite;
            RectTransform rectTransform = dotInstance.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(50, 50);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return dotInstance;
        }

        private void ShowGraph(UserStat.GameCategory category, int score, int maxScore) {
            transform.gameObject.SetActive(true);

            _scoreText.SetText($"Score: {score}/{maxScore}");

            DatabaseManager databaseManager = new DatabaseManager();
            string loggedUser = databaseManager.GetUsers().FirstOrDefault(i => i.IsLogged)?.Username;

            _values.Clear();

            foreach (var userScoreHistory in databaseManager.GetScoreHistory(loggedUser).Where(i => i.Category == (int)category).ToList()) {
                _values.Add(userScoreHistory.SessionScore);
            }

            PopulateGraph(_values);
        }

        private void PopulateGraph(List<float> values) {
            float graphHeight = _graphContainer.sizeDelta.y;
            const float yMaximum = 100f;
            const float xSize = 125f;

            GameObject lastCircleGameObject = null;

            // Show only 5 session scores on remarks
            for (int i = 0; i < 5; i++) {
                float xPosition = xSize + i * xSize;
                float yPosition = (_values[i] / yMaximum) * graphHeight;
                GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
                if (lastCircleGameObject != null) {
                    CreateDotConnection(
                        lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition,
                        circleGameObject.GetComponent<RectTransform>().anchoredPosition);
                }

                lastCircleGameObject = circleGameObject;
            }
        }

        private void CreateDotConnection(Vector2 dotPosA, Vector2 dotPosB) {
            GameObject dotConnection = new GameObject("dotConnection", typeof(Image));
            dotConnection.transform.SetParent(_graphContainer, false);
            dotConnection.GetComponent<Image>().color = Color.blue;
            RectTransform rectTransform = dotConnection.GetComponent<RectTransform>();
            Vector2 dir = (dotPosB - dotPosA).normalized;
            float dist = Vector2.Distance(dotPosA, dotPosB);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(dist, 3f);
            rectTransform.anchoredPosition = dotPosA + dir * dist * 0.5f;
            rectTransform.localEulerAngles = new Vector3(0,0, UtilsClass.GetAngleFromVectorFloat(dir));
        }
    }
}
