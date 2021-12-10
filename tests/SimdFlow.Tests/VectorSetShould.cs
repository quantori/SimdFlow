using System;
using System.Linq;
using System.Numerics;
using NUnit.Framework;

namespace SimdFlow.Tests
{
    public class VectorSetShould
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(10)]
        [TestCase(32)]
        [TestCase(100)]
        public void ReturnData(short size)
        {
            var src = Enumerable.Range(0, size).Select(i => (short) i).ToArray();
            var vs = new VectorSet<short>(new ReadOnlyMemory<short>(src));
            var data = vs.GetData();
            CollectionAssert.AreEqual(src, data.ToArray());
        }

        [TestCase(10)]
        [TestCase(32)]
        [TestCase(100)]
        public void MapUsingVectorFunction(short size)
        {
            var src = Enumerable.Range(0, size).Select(i => (short) i).ToArray();
            var vs1 = new VectorSet<short>(new ReadOnlyMemory<short>(src));
            var vs = vs1.Map(vector => vector + Vector<short>.One);
            CollectionAssert.AreEqual(src.Select(i => i + 1).ToList(), vs.GetData().ToArray());
        }

        [TestCase(10)]
        [TestCase(32)]
        [TestCase(100)]
        public void MergeTwoSetsUsingVectorFunction(short size)
        {
            var src = Enumerable.Range(0, size).Select(i => (short) i).ToArray();
            var vs1 = new VectorSet<short>(new ReadOnlyMemory<short>(src));
            var vs2 = new VectorSet<short>(new ReadOnlyMemory<short>(src));
            var vs = vs1.Merge(vs2, (v1, v2) => v1 + v2);
            CollectionAssert.AreEqual(src.Select(i => i + i).ToList(), vs.GetData().ToArray());
        }

        [TestCase(10)]
        [TestCase(32)]
        [TestCase(100)]
        public void SliceData(short size)
        {
            var src = Enumerable.Range(0, size).Select(i => (short) i).ToArray();
            var vs1 = new VectorSet<short>(new ReadOnlyMemory<short>(src));
            var half = size / 2;
            var vs = vs1.Slice(half, size - half);
            CollectionAssert.AreEqual(src.Skip(half).ToList(), vs.GetData().ToArray());
        }
    }
}