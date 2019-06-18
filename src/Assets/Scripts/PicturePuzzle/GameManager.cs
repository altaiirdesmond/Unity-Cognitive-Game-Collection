﻿using System;
using System.Collections;
using Assets.Scripts.Database.Enum;
using Assets.Scripts.GlobalScripts.Player;
using Assets.Scripts.GlobalScripts.UIComponents;
using Assets.Scripts.GlobalScripts.UITask;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
#pragma warning disable 649

namespace Assets.Scripts.PicturePuzzle {
    public class GameManager : MonoBehaviour {

        private GameManager _gameManager;
        private TextMeshProUGUI _gameResult;

        [SerializeField] private Timer _timer;
        [SerializeField] private TextMeshProUGUI _inputField;
        [SerializeField] private TextMeshProUGUI _placeHolder;
        [SerializeField] private GameObject _answerState;
        [SerializeField] private GameObject _puzzlePictureContainer;
        [SerializeField] private PicturePuzzleCollection[] _picturePuzzleCollections;

        private int _currentNumber = 1;
        private int _score;

        private void Start() {
            if (_gameManager != null) {
                Destroy(this);
            }
            else {
                _gameManager = this;
            }

            DontDestroyOnLoad(this);

            SceneManager.activeSceneChanged += ChangedActiveScene;

            _timer.StartTimerAt(0, 45f);

            // Will wait for timer up
            StartCoroutine(WaitForTimer());

            Instantiate(Array.Find(_picturePuzzleCollections, i => i.puzzleId == _currentNumber).Image, _puzzlePictureContainer.transform);
        }

        public void CheckAnswer() {
            string answer = Array.Find(_picturePuzzleCollections, i => i.puzzleId == _currentNumber).Answer;
            if (_inputField.text.Contains(answer)) {
                _answerState.GetComponent<TextMeshProUGUI>().SetText("CORRECT!");
                _answerState.GetComponent<Animator>().SetTrigger("correct");

                // Clear picture puzzle container before next puzzle
                Destroy(_puzzlePictureContainer.transform.GetChild(0).gameObject);

                NextPuzzle();
            }
            else {
                _answerState.GetComponent<TextMeshProUGUI>().SetText("WRONG");
                _answerState.GetComponent<Animator>().SetTrigger("wrong");
            }
        }

        public void NextPuzzle() {
            _currentNumber++;

            // End game
            if (_currentNumber > _picturePuzzleCollections.Length) {
                // Save score
                BaseScoreHandler baseScoreHandler = new BaseScoreHandler();
                baseScoreHandler.AddScore(_score, Game.GameType.ProblemSolving);

                // Load game over 
                SceneManager.LoadScene("GameOverPicturePuzzle");

                return;
            }

            Instantiate(Array.Find(_picturePuzzleCollections, i => i.puzzleId == _currentNumber).Image, _puzzlePictureContainer.transform);

            // Reset back timer every new puzzle
            _timer.StartTimerAt(0, 45f);
        }

        public void ClearField() {
            _placeHolder.text = string.Empty;
        }

        private void ChangedActiveScene(Scene current, Scene next) {
            if (next.name.Equals("GameOverPicturePuzzle")) {
                // Update meter bar
                FindObjectOfType<StatsManager>().Refresh("ProblemSolving");

                _gameResult.SetText(!_timer.TimerUp ? "SUCCESS!" : "FAILED");
            }
        }

        private IEnumerator WaitForTimer() {
            yield return new WaitUntil(() => _timer.TimerUp);

            // Stop game when timer is up and go to game over
            SceneManager.LoadScene("GameOverPicturePuzzle");
        }
    }
}