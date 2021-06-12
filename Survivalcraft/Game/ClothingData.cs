using Engine.Graphics;
using System;

namespace Game
{
    public class ClothingData
    {
        public int Index;

        public int DisplayIndex;

        public ClothingSlot Slot;

        public float ArmorProtection;

        public float Sturdiness;

        public float Insulation;

        public float MovementSpeedFactor;

        public float SteedMovementSpeedFactor;

        public float DensityModifier;

        public Texture2D Texture;

        public string DisplayName;

        public string Description;

        public string ImpactSoundsFolder;

        public bool IsOuter;

        public bool CanBeDyed;

        public int Layer;

        public int PlayerLevelRequired;

        /// <summary>
        /// װ��
        /// </summary>
        public event Action Mount;
        /// <summary>
        /// ж��
        /// </summary>
        public event Action Dismount;
        /// <summary>
        /// ����
        /// </summary>
        public event Action Update;

        public void OnMount()
        {
            Mount?.Invoke();
        }
        public void OnDismount()
        {
            Dismount?.Invoke();
        }

        public void OnUpdate() {
            Update?.Invoke();
        }

    }
}
