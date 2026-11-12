namespace GameDevKit
{
    public delegate ref T RefGetter<T>();

    public abstract class ObservableNumber<T, TInfo> where T : unmanaged
    {
        public readonly SourcedAction<TInfo> OnValueChanged = new();

        protected abstract ref T ValueRef { get; }
        protected abstract bool AreEqual(T a, T b);
        protected abstract T Sum(T a, T b);
        protected abstract TInfo CreateInfo(T prev, T next);

        public T Value
        {
            get => ValueRef;
            set => Set(value);
        }

        public void Set(T value)
        {
            var prev = ValueRef;
            if (AreEqual(value, prev)) { return; }

            ValueRef = value;
            OnValueChanged.Invoke(CreateInfo(prev, value));
        }

        public void SetWithoutNotify(T value) => ValueRef = value;
        public void Add(T amount) => Set(Sum(Value, amount));
    }
}