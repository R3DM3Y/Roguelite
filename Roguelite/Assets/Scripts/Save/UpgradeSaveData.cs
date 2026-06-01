using System;

namespace Save
{
    [Serializable]
    public class UpgradeSaveData
    {
        public int swordDamageLevel;
        public int airDamageLevel;
        public int airBounceLevel;

        public int hpLevel;
        public int speedLevel;

        public int shieldLevel;
        public int shieldEfficiencyLevel;

        public int jumpLevel;

        public bool dashUnlocked;
    }
}