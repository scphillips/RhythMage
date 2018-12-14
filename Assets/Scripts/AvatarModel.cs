﻿using System;

namespace Outplay.RhythMage
{
    public class AvatarModel
    {
        public int CurrentHealth;
        public int MaxHealth;
        public int KillCount;

        public class HealthChangedEventArgs : EventArgs
        {
            public int HealthMod { get; set; }
        }

        public event EventHandler OnHealthChange;

        public AvatarModel()
        {
            MaxHealth = 5;
            CurrentHealth = MaxHealth;
            KillCount = 0;
        }

        public void TakeDamage()
        {
            --CurrentHealth;
            HealthChangedEventArgs args = new HealthChangedEventArgs();
            args.HealthMod = -1;
            OnHealthChange(this, args);
        }

        public bool IsAlive()
        {
            return (CurrentHealth > 0);
        }
    }
}
