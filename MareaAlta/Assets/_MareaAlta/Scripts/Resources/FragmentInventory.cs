using System;
using UnityEngine;

namespace MareaAlta.Resources
{
    public sealed class FragmentInventory : MonoBehaviour
    {
        public event Action<int> FragmentCountChanged;

        public int FragmentCount { get; private set; }

        public void AddFragments(int amount)
        {
            if (amount <= 0)
                return;

            FragmentCount += amount;
            FragmentCountChanged?.Invoke(FragmentCount);
        }
    }
}
