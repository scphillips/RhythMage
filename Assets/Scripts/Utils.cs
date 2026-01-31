using UnityEditor;
using UnityEngine;

namespace RhythMage
{
    public class Utils
    {
        private static GameObject persistentComponents = null;
        private static RandomNumberProvider rng = null;

        public static bool FindGameObjectWithTag(string tag, out GameObject gameObject)
        {
            gameObject = null;
            GameObject[] gameObjects;
            gameObjects = GameObject.FindGameObjectsWithTag(tag);
            if (gameObjects.Length > 0)
            {
                gameObject = gameObjects[0];
            }
            return gameObject != null;
        }

        public static GameObject FindGameObjectWithTag(string tag)
        {
            FindGameObjectWithTag(tag, out var gameObject);
            return gameObject;
        }

        public static GameObject GetOrCreatePersistentComponents()
        {
            if (persistentComponents == null)
            {
                if (!FindGameObjectWithTag("PersistentComponents", out persistentComponents))
                {
                    Object prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/PersistentComponents.prefab", typeof(GameObject));
                    GameObject instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                    persistentComponents = instance;
                }
                Object.DontDestroyOnLoad(persistentComponents);
            }

            return persistentComponents;
        }

        public static RandomNumberProvider GetRng()
        {
            if (rng == null)
            {
                GameStateManager gameStateManager = FindGameStateManager();
                if (gameStateManager && gameStateManager.settings.LevelBuilderSettings.levelSeed != -1)
                {
                    rng = new RandomNumberProvider(gameStateManager.settings.LevelBuilderSettings.levelSeed);
                }
                else
                {
                    rng = new RandomNumberProvider();
                }
            }

            return rng;
        }

        public static GameStateManager FindGameStateManager()
        {
            return GetOrCreatePersistentComponents().GetComponent<GameStateManager>();
        }

        public static SoundManager FindSoundManager()
        {
            return GetOrCreatePersistentComponents().GetComponent<SoundManager>();
        }

        public static GameSettings FindGameSettings()
        {
            return FindGameStateManager().settings;
        }

        public static AvatarModel FindAvatarModel()
        {
            return FindGameStateManager().Avatar;
        }

        public static Transform FindLevelRoot()
        {
            return FindGameObjectWithTag("LevelContainer")?.transform;
        }

        public static CameraProvider FindCameraProvider()
        {
            return FindGameObjectWithTag("SceneContext")?.GetComponent<CameraProvider>();
        }

        public static GameSceneController FindGameSceneController()
        {
            return FindGameObjectWithTag("SceneContext")?.GetComponent<GameSceneController>();
        }

        public static UpdateManager FindUpdateManager()
        {
            return FindGameObjectWithTag("SceneContext")?.GetComponent<UpdateManager>();
        }

        public static DungeonModel FindDungeonModel()
        {
            return FindGameSceneController()?.dungeon;
        }
    }
}
