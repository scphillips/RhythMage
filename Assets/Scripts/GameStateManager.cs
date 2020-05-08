namespace Outplay.RhythMage
{
    public class GameStateManager
    {
        [System.Serializable]
        public class Settings
        {
            public SceneReference gameOverScene;
            public SceneReference gameScene;
            public SceneReference menuScene;

            public float delayTransitionToGameOverDuration;
        }
        
        public bool IsGameRunning { get; set; }
    }
}
