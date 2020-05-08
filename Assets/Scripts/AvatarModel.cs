// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

namespace RhythMage
{
    public class AvatarModel
    {
        public readonly int maxHealth;
        public int currentHealth;
        public int killCount;
        public int currentCellIndex;

        public class HealthChangedEventArgs : System.EventArgs
        {
            public int HealthMod { get; set; }
        }

        public event System.Action<AvatarModel, HealthChangedEventArgs> OnHealthChange;

        public AvatarModel()
        {
            maxHealth = 5;
            currentHealth = maxHealth;
            killCount = 0;
        }

        public void TakeDamage(int amount = 1)
        {
            currentHealth -= amount;
            OnHealthChange(this, new HealthChangedEventArgs
            {
                HealthMod = -amount
            });
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }
    }
}
