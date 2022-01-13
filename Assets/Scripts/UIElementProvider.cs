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
        Debug.Log(string.Format("Width = {0}, Height = {1}, Is Portrait = {2}", Screen.width, Screen.height, isPortrait));
        GetComponent<RectTransform>().anchoredPosition = isPortrait ? m_positionPortrait : m_positionLandscape;
        transform.eulerAngles = isPortrait ? m_rotationPortrait : m_rotationLandscape;

        lastScreenSize.x = Screen.width;
        lastScreenSize.y = Screen.height;
    }
}
