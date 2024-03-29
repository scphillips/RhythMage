// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, January 2022

using UnityEngine;

public class UIElementProvider : MonoBehaviour
{
    public Vector3 m_positionLandscape;
    public Vector3 m_positionPortrait;
    public Vector3 m_rotationLandscape;
    public Vector3 m_rotationPortrait;

    Vector2 lastScreenSize;
    
    void Start()
    {
        UpdateCoordinates();
    }

    void Update()
    {
        if (lastScreenSize.x != Screen.width
            || lastScreenSize.y != Screen.height)
        {
            UpdateCoordinates();
        }
    }

    void UpdateCoordinates()
    {
        bool isPortrait = Screen.width < Screen.height;
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = isPortrait ? m_positionPortrait : m_positionLandscape;
        rectTransform.eulerAngles = isPortrait ? m_rotationPortrait : m_rotationLandscape;

        lastScreenSize.x = Screen.width;
        lastScreenSize.y = Screen.height;
    }
}
