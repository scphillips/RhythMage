// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using TMPro;
using UnityEngine;

namespace RhythMage
{
    public class GameOverUIController : MonoBehaviour
    {
        public GameOverSceneController gameOverController;

        public TextMeshProUGUI FinalScoreLabel;

        void Start()
        {
            FinalScoreLabel.text = "Total Kills: " + gameOverController.GetFinalScore();
        }

        public void OnReplayButtonPressed()
        {
            gameOverController.LoadNextScene();
        }
    }
}
