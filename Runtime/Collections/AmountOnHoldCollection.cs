using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameDevKit.Collections
{
    public class AmountOnHoldCollection<T>
    {
        private HashSet<AmountOnHold<T>> _holdsCache;
        private HashSet<AmountOnHold<T>> Holds => _holdsCache ??= new();

        private Stack<AmountOnHold<T>> _pool;
        private Stack<AmountOnHold<T>> Pool => _pool ??= new();

        public event Action<AmountOnHold<T>> OnHoldRemoved;

        public AmountOnHold<T> Hold(T item, int amount)
        {
            if (Pool.TryPop(out var hold))
            {
                hold.Init(item, amount);
            }
            else
            {
                hold = new AmountOnHold<T>(item, amount);
            }

            hold.OnDispose += () => ReleaseHold(hold);
            Holds.Add(hold);

            return hold;
        }

        public void CancelHold(AmountOnHold<T> hold) => hold.Dispose();

        private void ReleaseHold(AmountOnHold<T> hold)
        {
            if (Holds.Remove(hold))
            {
                OnHoldRemoved?.Invoke(hold);
                Pool.Push(hold);
            }
        }

        public void Clear()
        {
            foreach (var hold in Holds.ToArray())
            {
                ReleaseHold(hold);
            }
        }

        public int GetTotalOnHold()
        {
            var total = 0;
            foreach (var hold in Holds)
            {
                total += hold.Amount;
            }
            return total;
        }

        public bool IsHolding() => Holds.Count > 0;
    }

    public class AmountOnHold<T> : IDisposable
    {
        public static readonly AmountOnHold<T> Empty = new(default, 0);

        public T Item { get; private set; }
        public int Amount { get; private set; }

        public event Action OnDispose;
        public AmountOnHold() { }
        public AmountOnHold(T item, int amount) => Init(item, amount);

        internal void Init(T item, int amount)
        {
            if (this == Empty)
            {
                Debug.LogError($"Trying to initialize the default [Empty] AmountOnHold with Item: {item}, Amount: {amount}");
                return;
            }

            Item = item;
            Amount = amount;
            OnDispose = null;
        }

        public void Dispose()
        {
            if (this == Empty) { return; }

            if (OnDispose == null)
            {
                Debug.LogError($"Possibly trying to dispose an already disposed AmountOnHold: Item {Item}");
            }

            OnDispose?.Invoke();
            OnDispose = null;

            Item = default;
            Amount = 0;
        }
    }
}