using System;

namespace AlgorithmX.Parallel
{
    class DoublingArray<T>
    {
        private const int DefaultCapacity = 4;

        private T[] values;

        public int Count { get; private set; }

        public DoublingArray()
            : this(DefaultCapacity)
        {
        }

        public DoublingArray(int initialSize)
        {
            if (initialSize < 1)
            {
                throw new ArgumentException("Initial size must be >= 1.");
            }

            values = new T[initialSize];
            Count = 0;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public DoublingArray(DoublingArray<T> other)
        {
            values = new T[other.Count];
            Array.Copy(other.values, values, other.Count);

            Count = other.Count;
        }

        public void Append(T value)
        {
            if (Count == values.Length)
            {
                Array.Resize(ref values, 2 * Count);
            }

            values[Count] = value;
            ++Count;
        }

        public ref T this[int i] => ref values[i];
    }
}
