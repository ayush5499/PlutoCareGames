﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Stores the configuration for a level
    // One level contains:
    // One or many punching bags at unique configurations
    // A total time permitted for the level 
    // A score threshold to beat the level
    [System.Serializable]
    public class LevelConfiguration
    {
        public int levelTime;
        public int thresholdScore;
        public GameObject bagConfiguration;
    }

    public static GameManager instance;

    [SerializeField]
    private Transform headTransform;

    [SerializeField]
    private List<LevelConfiguration> levels = new List<LevelConfiguration>();

    [SerializeField]
    private float angleThresholdDegrees = 30.0f;

    private int levelIndex = 0;
    private float angleThresholdDot;
    private LevelConfiguration currentConfig;

    public enum ScoreState
    {
        None,
        CanScore, 
        CannotScore
    }

    public ScoreState state = ScoreState.None;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogError("More than once instance of the GameManager Singleton. Deleting the old instance.");
            DestroyImmediate(instance);

            instance = this;
        }

        if (headTransform == null) {
            Debug.LogError("Missing Head Transform! Game cannot continue correctly");
        }

        // This number converts the look threshold to a dot product-comparable value
        // Save us from computing it every frame
        angleThresholdDot = Mathf.Cos(Mathf.Deg2Rad * angleThresholdDegrees);
        state = ScoreState.CannotScore;
        levelIndex = 0;
    }

    private void Start() {
        StartNextLevel();
    }

    public void StartNextLevel() {
        if (levelIndex < levels.Count) {
            Debug.Log("Starting Level " + levelIndex);
            StartCoroutine(RunLevel(levels[levelIndex]));
            levelIndex++;
        }
        else {
            Debug.Log("Finished All Levels");
            EndGame();
        }
    }
    
    public void EndGame() {
        Debug.Log("Ending Game");

    }

    private IEnumerator RunLevel(LevelConfiguration config) {
        // Deactivate bags from previous config
        if (currentConfig != null) {
            if (currentConfig.bagConfiguration != null) {
                currentConfig.bagConfiguration.SetActive(false);
            }
        }

        // Setup new config
        currentConfig = config;

        if (currentConfig.bagConfiguration != null) {
            currentConfig.bagConfiguration.SetActive(true);
        }

        int counter = currentConfig.levelTime;

        // Initialize timer for the level
        Debug.Log("Time: " + counter);

        while (counter > 0) {
            yield return new WaitForSeconds(1);
            counter--;
            
            // Count down timer
            Debug.Log("Time: " + counter);
        }

        // If we are here the timer is done.
        // See if the score threshold was met!
        if (ScoreManager.instance.GetLeftScore() >= currentConfig.thresholdScore || ScoreManager.instance.GetRightScore() >= currentConfig.thresholdScore) {
            StartNextLevel();
        }
        else {
            EndGame();
        }
    }

    void Update() {

        // Get the dot product between the head's forward direction and the world forward direction
        float lookDeviation = Vector3.Dot(Vector3.forward, headTransform.forward);

        // Only process a threshold violation if changing states
        if (Mathf.Abs(lookDeviation) > angleThresholdDot) {
            if (state == ScoreState.CannotScore) {
                state = ScoreState.CanScore;

                // Update visuals here
            }
        }
        else {
            if (state == ScoreState.CanScore) {
                state = ScoreState.CannotScore;

                // Update visuals again
            }
        }
    }
}