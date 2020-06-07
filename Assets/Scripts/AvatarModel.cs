// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

namespace RhythMage
{
    public class AvatarModel
    {
        public class HealthChangedEventArgs : System.EventArgs
        {
            public int HealthMod { get; set; }
        }

        public event System.Action<AvatarModel, HealthChangedEventArgs> OnHealthChange;
        public event System.Action<AvatarModel> OnMove;
        
        public int CurrentCellIndex
        {
            get => m_currentCellIndex;
            set
            {
                m_currentCellIndex = value;
                OnMove?.Invoke(this);
            }
        }

        public int MaxHealth { get; }
        public int CurrentHealth { get; private set; }

        public int killCount;
        
        int m_currentCellIndex;

        public AvatarModel()
        {
            MaxHealth = 5;
            CurrentHealth = MaxHealth;
            killCount = 0;
        }

        public void TakeDamage(int amount = 1)
        {
            CurrentHealth -= amount;
            OnHealthChange?.Invoke(this, new HealthChangedEventArgs
            {
                HealthMod = -amount
            });
        }

        public bool IsAlive()
        {
            return CurrentHealth > 0;
        }
    }
}
