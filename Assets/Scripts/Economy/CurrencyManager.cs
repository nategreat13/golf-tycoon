using System;
using UnityEngine;
using GolfGame.Core;

namespace GolfGame.Economy
{
    public class CurrencyManager : MonoBehaviour
    {
        public long Amount { get; private set; }

        public event Action<long, long> OnCurrencyChanged; // old, new

        public void SetAmount(long amount)
        {
            long old = Amount;
            Amount = Math.Max(0, amount);
            if (old != Amount)
            {
                EventBus.Publish(new CurrencyChangedEvent { oldAmount = old, newAmount = Amount });
                OnCurrencyChanged?.Invoke(old, Amount);
            }
        }

        public void Add(long amount)
        {
            if (amount <= 0) return;
            SetAmount(Amount + amount);
        }

        public bool CanAfford(long cost) => Amount >= cost;

        public bool TrySpend(long amount)
        {
            if (amount <= 0) return true;
            if (Amount < amount) return false;
            SetAmount(Amount - amount);
            return true;
        }
    }
}
