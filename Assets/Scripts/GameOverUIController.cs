// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using TMPro;
using UnityEngine;

namespace RhythMage
{
    public class GameOverUIController : MonoBehaviour
    {
        [Zenject.Inject]
        readonly GameOverController m_gameOverController;

        public TextMeshProUGUI FinalScoreLabel;

        void Start()
        {
            FinalScoreLabel.text = "Total Kills: " + m_gameOverController.GetFinalScore();
        }

        public void OnReplayButtonPressed()
        {
            m_gameOverController.LoadNextScene();
        }
    }
}
