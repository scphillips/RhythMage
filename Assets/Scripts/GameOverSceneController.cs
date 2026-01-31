// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythMage
{
    public class GameOverSceneController : MonoBehaviour
    {
        public int GetFinalScore()
        {
            AvatarModel avatar = Utils.FindAvatarModel();
            return avatar?.killCount ?? 0;
        }

        public void LoadNextScene()
        {
            Utils.FindGameStateManager()?.ResetAvatar();
            SceneManager.LoadScene(Utils.FindGameSettings().GameStateManagerSettings.menuScene);
        }
    }
}
