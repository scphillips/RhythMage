using System.Collections;
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

	void Start()
    {
        m_opacity = 0;
    }
	
	void Update()
    {
        float mag = Time.deltaTime * m_fadeSpeed;
        float delta = (audioSource.isPlaying) ? mag : -mag;
        m_opacity += delta;
		foreach (var entry in fadeEntities)
        {
            var color = entry.GetComponent<Image>().color;
            color.a = m_opacity;
            entry.GetComponent<Image>().color = color;
        }

        if (m_opacity < 0.0f)
        {
            SceneManager.LoadScene(nextScene);
        }
	}
}
