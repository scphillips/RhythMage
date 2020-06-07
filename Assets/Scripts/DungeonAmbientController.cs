// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, June 2020

using System.Collections;
using UnityEngine;

namespace RhythMage
{
    public class DungeonAmbientController : MonoBehaviour
    {
        [System.Serializable]
        public class Settings
        {
            public Color tilePulseColor;
            public SpriteRenderer tilePulsePrefab;
            public float tilePulseDuration;
            public EasingFunction.Ease tilePulseEaseType;
        }

        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        readonly DungeonModel m_dungeon;

        [Zenject.Inject]
        readonly AvatarModel m_avatar;

        [Zenject.Inject]
        readonly SoundManager m_sound;

        void Start()
        {
            m_avatar.OnMove += OnAvatarMove;
        }
        
        void OnAvatarMove(AvatarModel avatar)
        {
            // Find next tile to pulse
            int nextCellIndex = avatar.CurrentCellIndex + 1;
            if (nextCellIndex < m_dungeon.GetCellCount())
            {
                var nextCell = m_dungeon.GetCellAtIndex(nextCellIndex);
                if (m_dungeon.HasEnemyAtCell(nextCell)
                    && m_dungeon.Floors.ActiveEntities.TryGetValue(nextCell, out var tile))
                {
                    var startDelay = (float)m_sound.TimeToNextBeat() - m_settings.tilePulseDuration * 0.5f;
                    var renderer = tile.GetComponentInChildren<MeshRenderer>();
                    StartCoroutine(PulseAnimation(startDelay, tile, m_settings.tilePulseDuration));
                }
            }
        }

        IEnumerator PulseAnimation(float startDelay, GameObject tile, float duration)
        {
            float elapsedTime = 0.0f;
            float fullDuration = startDelay + duration;
            var pulseSprite = Instantiate(m_settings.tilePulsePrefab, tile.transform);
            pulseSprite.color = Color.clear;
            var easeFunc = EasingFunction.GetEasingFunction(m_settings.tilePulseEaseType);

            while (elapsedTime < fullDuration)
            {
                elapsedTime = System.Math.Min(elapsedTime + Time.deltaTime, fullDuration);

                if (elapsedTime > startDelay)
                {
                    float elapsedAfterStartDelay = elapsedTime - startDelay;
                    float mag = 1.0f - System.Math.Abs(1.0f - elapsedAfterStartDelay / duration * 2.0f);
                    float easedMag = easeFunc(0.0f, 1.0f, mag);
                    var color = new Color(m_settings.tilePulseColor.r, m_settings.tilePulseColor.g, m_settings.tilePulseColor.b, m_settings.tilePulseColor.a * easedMag);
                    pulseSprite.color = color;
                }

                yield return null;
            }

            Destroy(pulseSprite.gameObject);
        }
    }
}
