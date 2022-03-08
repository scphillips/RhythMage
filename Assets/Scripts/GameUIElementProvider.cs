// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, March 2022

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIElementProvider : MonoBehaviour
{
    [SerializeField] private List<Image> m_healthImages;
    public List<Image> HealthImages => m_healthImages;

    [SerializeField] private TextMeshProUGUI m_enemyCounter;
    public TextMeshProUGUI EnemyCounter => m_enemyCounter;

    [SerializeField] private Image m_leftHand;
    public Image LeftHand => m_leftHand;

    [SerializeField] private Image m_rightHand;
    public Image RightHand => m_rightHand;

    [SerializeField] private Image m_damageOverlayImage;
    public Image DamageOverlayImage => m_damageOverlayImage;

    [SerializeField] private CanvasGroup m_portalOverlayImage;
    public CanvasGroup PortalOverlayImage => m_portalOverlayImage;

    [SerializeField] private Image m_incomingEnemyDisplay;
    public Image IncomingEnemyDisplay => m_incomingEnemyDisplay;
}
