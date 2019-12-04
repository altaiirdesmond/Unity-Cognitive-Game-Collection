﻿using System;
using Assets.Scripts.DataComponent.Model;
using Assets.Scripts.GlobalScripts.Game;
using Assets.Scripts.GlobalScripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Assets.Scripts.GlobalScripts.Managers.UIManager;

namespace Assets.Scripts.PicturePuzzle {
    [RequireComponent(typeof(ActionManager))]
    public class PicturePuzzleGameManager : CoreGameBehaviour {

        [SerializeField] private PicturePuzzleCollection[] _picturePuzzleCollections;
        [SerializeField] private Image _puzzlePictureContainer;

        private BaseScoreHandler _baseScoreHandler;
        private TimerManager _timerManager;
        private PicturePuzzleGameManager _picturePuzzleGameManager;
        private UIManager _uiManager;
        private TMP_InputField _answerField;
        private Animator _scoreAddAnimator;
        private AudioManager _audioManager;
        private bool _paused;
        private int _currentNumber = 1;
        private int _score;

        private void Start() {
            _timerManager = GetComponent<TimerManager>();

            _uiManager = FindObjectOfType<UIManager>();

            _audioManager = FindObjectOfType<AudioManager>();

            Instantiate(Array.Find(_picturePuzzleCollections, i => i.puzzleId == _currentNumber).Image, _puzzlePictureContainer.transform);

            TimerManager.OnGameTimerEndEvent += EndGame;

            TimerManager.OnPreGameTimerEndEvent += StartTimer;

            _baseScoreHandler = new BaseScoreHandler(0, 50);
        }

        private void StartTimer() {
            TimerManager.OnPreGameTimerEndEvent -= StartTimer;

            _timerManager.StartTimerAt(0, 45f);
        }

        public override void EndGame() {
            TimerManager.OnGameTimerEndEvent -= EndGame;

            _timerManager.ChangeTimerState();

            // Add up the time left for each answered puzzle 
            _baseScoreHandler.AddScore(_timerManager.Minutes, _timerManager.Seconds);
            _baseScoreHandler.SaveScore(UserStat.GameCategory.Language);

            ShowGraph(
                UserStat.GameCategory.Language,
                _baseScoreHandler.Score,
                _baseScoreHandler.ScoreLimit);

            base.EndGame();
        }

        public void CheckAnswer() {
            string answer = Array.Find(_picturePuzzleCollections, i => i.puzzleId == _currentNumber).Answer;

            _answerField = (TMP_InputField)_uiManager.GetUI(UIType.InputField, "answer");

            _scoreAddAnimator =
                (Animator)_uiManager.GetUI(UIType.AnimatedMultipleState, "score add anim");

            if (_answerField.text.Contains(answer)) {
                FindObjectOfType<AudioManager>().PlayClip("sfx_correct");

                _scoreAddAnimator.GetComponent<TextMeshProUGUI>().SetText("CORRECT!");
                _scoreAddAnimator.SetTrigger("correct");

                // Clear picture puzzle container before next puzzle
                Destroy(_puzzlePictureContainer.transform.GetChild(0).gameObject);

                NextPuzzle();

                _answerField.text = string.Empty;
            }
            else {
                FindObjectOfType<AudioManager>().PlayClip("sfx_incorrect");

                _scoreAddAnimator.GetComponent<TextMeshProUGUI>().SetText("WRONG!");
                _scoreAddAnimator.SetTrigger("wrong");
            }
        }

        public void NextPuzzle() {
            _currentNumber++;
            
            if (_currentNumber > _picturePuzzleCollections.Length) {
                EndGame();

                return;
            }

            Instantiate(Array.Find(_picturePuzzleCollections, i => i.puzzleId == _currentNumber).Image, _puzzlePictureContainer.transform);
        }
    }
}
