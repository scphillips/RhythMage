using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    public List<GameObject> fadeEntities;
    public AudioSource audioSource;

    readonly string nextScene = "MenuScene";
    readonly float m_fadeSpeed = 2.0f;
    float m_opacity;
    bool m_isEnding;

	void Start()
    {
        m_opacity = 0.0f;
        m_isEnding = false;
    }
	
	void Update()
    {
        if (m_isEnding == false && m_opacity == 1.0f)
        {
            m_isEnding = (Input.anyKeyDown || audioSource.isPlaying == false);
        }

        float mag = Time.deltaTime * m_fadeSpeed;
        float delta = (m_isEnding) ? -mag : mag;
        m_opacity = System.Math.Max(0.0f, System.Math.Min(1.0f, m_opacity + delta));
        foreach (var entry in fadeEntities)
        {
            var color = entry.GetComponent<Image>().color;
            color.a = m_opacity;
            entry.GetComponent<Image>().color = color;
        }

        if (m_isEnding && m_opacity == 0.0f)
        {
            SceneManager.LoadScene(nextScene);
        }
	}
}
