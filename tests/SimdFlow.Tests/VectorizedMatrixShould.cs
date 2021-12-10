using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SimdFlow.Tests
{
    public class VectorizedMatrixShould
    {
        private static IEnumerable<(int x, int y)[]> GetSliceParameters()
        {
            return new[]
            {
                new[] {(10, 20), (5, 5), (5, 5)},
                new[] {(32, 64), (10, 40), (20, 24)},
                new[] {(100, 100), (0, 0), (100, 100)}
            };
        }

        [TestCaseSource(nameof(GetSliceParameters))]
        public void Slice(
            (int x, int y) size,
            (int x, int y) start,
            (int x, int y) sliceSize)
        {
            var src = Enumerable.Range(0, size.y)
                .Select(i => Enumerable.Range(0, size.x)
                    .Select(x => (byte) x)
                    .ToArray())
                .ToArray();

            var matrix = new VectorizedMatrix<byte>(src
                .Select(ar => new VectorSet<byte>(new ReadOnlyMemory<byte>(ar)))
                .ToArray());
            var subMatrix = matrix.Slice(start.y, sliceSize.y, start.x, sliceSize.x);

            Assert.AreEqual(sliceSize.y, subMatrix.Rows.Count);
            for (int i = 0; i < subMatrix.Rows.Count; i++)
            {
                CollectionAssert.AreEqual(
                    src[start.y + i].Skip(start.x).Take(sliceSize.x).ToArray(), 
                    subMatrix.Rows[i].GetData().ToArray());
            }
        }
    }
}