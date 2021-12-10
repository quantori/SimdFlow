using System;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace SimdFlow
{
    [DebuggerDisplay("{" + nameof(DebugView) + "}")]
    public class VectorSet<T> where T : unmanaged
    {
        private static readonly int VectorSize = Vector<T>.Count;

        public static VectorSet<T> Create(ReadOnlyMemory<T> span)
        {
            var dataLength = span.Length;
            return new VectorSet<T>(span, dataLength);
        }

        public VectorSet(ReadOnlyMemory<T> src, int dataLength = 0)
        {
            var srcLength = src.Length;
            dataLength = dataLength == 0 ? srcLength : dataLength;

            _tail = new T[VectorSize];
            var tailLength = srcLength % VectorSize;
            if (tailLength != 0)
            {
                var tailStart = VectorSize * (srcLength / VectorSize);
                src.Slice(tailStart, tailLength).CopyTo(_tail);
            }

            DataLength = dataLength;
            _memory = src;
        }

        private readonly ReadOnlyMemory<T> _memory;
        private readonly T[] _tail;
        public int DataLength { get; }

        public ReadOnlyMemory<T> GetData()
        {
            return _memory[..DataLength];
        }

        public VectorSet<T> Map(Func<Vector<T>, Vector<T>> func, ReturningArrayPool<T>? pool = null)
        {
            var memoryLength = _memory.Length;
            var result = AllocateResultArr(pool, memoryLength);

            for (var i = 0; i < memoryLength; i += VectorSize)
            {
                var left = _memory.Length - i;
                var slice = left >= VectorSize ? _memory.Slice(i, VectorSize).Span : _tail;

                var vector = new Vector<T>(slice);
                var r = func(vector);
                r.CopyTo(result, i);
            }

            var set = new VectorSet<T>(result, DataLength);
            return set;
        }

        public VectorSet<T> Slice(int start, int length)
        {
            if (start >= _memory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (start + length > _memory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            var slicedData = _memory.Slice(start, length);
            return Create(slicedData);
        }

        public VectorSet<T> Merge(
            VectorSet<T> vectorSet,
            Func<Vector<T>, Vector<T>, Vector<T>> func,
            bool includeHangovers = false,
            ReturningArrayPool<T>? pool = null)
        {
            var maxLength = Math.Max(vectorSet.DataLength, DataLength);
            var resultArr = AllocateResultArr(pool, maxLength);

            for (var i = 0; i < maxLength; i += VectorSize)
            {
                var a = GetVectorAtPosition(this, i);
                var b = GetVectorAtPosition(vectorSet, i);

                var result = func(a, b);
                result.CopyTo(resultArr, i);
            }

            var dataLength = includeHangovers
                ? Math.Max(DataLength, vectorSet.DataLength)
                : Math.Min(DataLength, vectorSet.DataLength);

            var results = new ReadOnlyMemory<T>(resultArr);
            return new VectorSet<T>(results, dataLength);
        }

        private static T[] AllocateResultArr(ReturningArrayPool<T>? pool, int length)
        {
            var setsCount = length / VectorSize;
            var reminder = length % VectorSize;
            if (reminder > 0)
            {
                setsCount++;
            }

            var resultArr = pool != null
                ? pool.Rent(setsCount * VectorSize)
                : new T[setsCount * VectorSize];
            return resultArr;
        }

        private static Vector<T> GetVectorAtPosition(VectorSet<T> set, int start)
        {
            var memory = set._memory;
            var left = memory.Length - start;
            if (left < VectorSize)
            {
                return new Vector<T>(set._tail);
            }

            var slice = memory.Slice(start, VectorSize);
            return new Vector<T>(slice.Span);
        }

        public string DebugView
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            var sa = new StringBuilder();
            var sb = new StringBuilder();
            for (var i = 0; i < _memory.Span.Length; i++)
            {
                sa.Append(i.ToString().PadLeft(3));

                var val = _memory.Span[i];
                sb.Append(val.ToString()!.PadLeft(3));
            }

            return sb + Environment.NewLine + sa;
        }
    }
}
