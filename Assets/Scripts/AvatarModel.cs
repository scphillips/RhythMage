using System;

namespace Outplay.RhythMage
{
    public class AvatarModel
    {
        public readonly int maxHealth;
        public int currentHealth;
        public int killCount;

        public class HealthChangedEventArgs : EventArgs
        {
            public int HealthMod { get; set; }
        }

        public event EventHandler OnHealthChange;

        public AvatarModel()
        {
            maxHealth = 5;
            currentHealth = maxHealth;
            killCount = 0;
        }

        public void TakeDamage()
        {
            --currentHealth;
            HealthChangedEventArgs args = new HealthChangedEventArgs();
            args.HealthMod = -1;
            OnHealthChange(this, args);
        }

        public bool IsAlive()
        {
            return (currentHealth > 0);
        }
    }
}
