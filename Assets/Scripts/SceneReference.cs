using UnityEditor;
using UnityEngine;

// Implementation from:
// https://gist.github.com/JohannesMP/ec7d3f0bcf167dab3d0d3bb480e0e07b

[System.Serializable]
public class SceneReference : ISerializationCallbackReceiver
{
#if UNITY_EDITOR
    [SerializeField]
    SceneAsset m_sceneAsset;

    bool IsValid
    {
        get
        {
            return (m_sceneAsset != null);
        }
    }
#endif

    [SerializeField]
    string m_scenePath;

    public string ScenePath
    {
        get
        {
#if UNITY_EDITOR
            // In editor we always use the asset's path
            return GetScenePathFromAsset();
#else
            // At runtime we use the stored path value which was serialized at build time.
            // See OnBeforeSerialize and OnAfterDeserialize
            return m_scenePath;
#endif
        }
        set
        {
            m_scenePath = value;
#if UNITY_EDITOR
            m_sceneAsset = GetSceneAssetFromPath();
#endif
        }
    }

    public static implicit operator string(SceneReference sceneReference)
    {
        return sceneReference.ScenePath;
    }

    // Called to prepare this data for serialization.
    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (IsValid == false && string.IsNullOrEmpty(m_scenePath) == false)
        {
            // Asset is invalid but have Path to try and recover from
            m_sceneAsset = GetSceneAssetFromPath();
            if (m_sceneAsset == null)
            {
                m_scenePath = string.Empty;
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
        else
        {
            // Asset takes precendence and overwrites Path
            m_scenePath = GetScenePathFromAsset();
        }
#endif
    }

    // Called to set up data for deserialization.
    public void OnAfterDeserialize()
    {
#if UNITY_EDITOR
        // AssetDatabase cannot be queried during serialization, defer until next update.
        EditorApplication.update += HandleAfterDeserialize;
#endif
    }


#if UNITY_EDITOR
    SceneAsset GetSceneAssetFromPath()
    {
        if (string.IsNullOrEmpty(m_scenePath) == false)
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(m_scenePath);
        }
        return null;
    }

    string GetScenePathFromAsset()
    {
        if (m_sceneAsset != null)
        {
            return AssetDatabase.GetAssetPath(m_sceneAsset);
        }
        return string.Empty;
    }

    void HandleAfterDeserialize()
    {
        EditorApplication.update -= HandleAfterDeserialize;

        if (IsValid == false)
        {
            // Asset is invalid but have path to try and recover from
            if (string.IsNullOrEmpty(m_scenePath) == false)
            {
                m_sceneAsset = GetSceneAssetFromPath();

                if (m_sceneAsset == null)
                {
                    // No asset found, path was invalid. Make sure we don't carry over the old invalid path
                    m_scenePath = string.Empty;
                }

                if (Application.isPlaying == false)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }
    }
#endif
}
