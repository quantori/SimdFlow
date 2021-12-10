using System.Collections.Generic;

namespace SimdFlow
{
    /// <remarks>
    /// We use a custom pool for SIMD S-W because of the following limitations of default ArrayPool:
    /// 1. We care about getting arrays with precise length (ArrayPool can return arrays longer than requested, and trimming would cause an additional allocation)
    /// 2. We do not have information about allocated array instances so that we could Return() them explicitly
    /// 3. We want to be able to return all allocated instances at once
    /// The implementation is WITHOUT thread safety.
    /// </remarks>
    public class ReturningArrayPool<T>
    {
        private readonly Dictionary<int, ArrayBucket> _buckets = new();
        
        public T[] Rent(int length)
        {
            if (_buckets.TryGetValue(length, out var bucket))
            {
                if (bucket.Items.Count <= bucket.Index)
                {
                    var allocated = new T[length];
                    bucket.Items.Add(allocated);
                    bucket.Index++;
                    return allocated;
                }
                var free = bucket.Items[bucket.Index];
                bucket.Index++;
                return free;
            }
            var bucketItems = new List<T[]>();
            var array = new T[length];
            bucketItems.Add(array);
            _buckets.Add(length, new ArrayBucket(bucketItems)
            {
                Index = 1
            });
            return array;
        }

        public void ReturnRented()
        {
            foreach (var bucket in _buckets.Values)
            {
                bucket.Index = 0;
            }
        }

        private class ArrayBucket
        {
            public ArrayBucket(List<T[]> items)
            {
                Items = items;
            }

            public List<T[]> Items { get; }
            
            public int Index { get; set; }
        }
    }
}
