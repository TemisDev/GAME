using System;
using MareaAlta.Core;
using UnityEngine;

namespace MareaAlta.Resources
{
    public sealed class EnergyInventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int fragmentsPerEnergy = 3;

        public event Action<int, int> ResourceCountsChanged;

        public int FragmentCount { get; private set; }
        public int EnergyCount { get; private set; }
        public int TotalFragmentsCollected { get; private set; }
        public int FragmentsPerEnergy => fragmentsPerEnergy;

        public void AddFragments(int amount)
        {
            if (amount <= 0)
                return;

            TotalFragmentsCollected += amount;
            int totalFragments = FragmentCount + amount;
            int generatedEnergy = totalFragments / fragmentsPerEnergy;

            FragmentCount = totalFragments % fragmentsPerEnergy;
            EnergyCount += generatedEnergy;
            if (generatedEnergy > 0)
                GameAudio.Instance?.PlayEnergyCreated();
            NotifyChanged();
        }

        public bool TrySpendEnergy(int amount)
        {
            if (amount <= 0 || EnergyCount < amount)
                return false;

            EnergyCount -= amount;
            NotifyChanged();
            return true;
        }

        private void NotifyChanged()
        {
            ResourceCountsChanged?.Invoke(FragmentCount, EnergyCount);
        }

        private void OnValidate()
        {
            fragmentsPerEnergy = Mathf.Max(1, fragmentsPerEnergy);
        }
    }
}
