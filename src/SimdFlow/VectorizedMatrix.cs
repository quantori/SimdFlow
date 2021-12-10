using System;
using System.Collections.Generic;
using System.Linq;

namespace SimdFlow
{
    public class VectorizedMatrix<T> where T : unmanaged
    {
        public VectorizedMatrix(IReadOnlyList<VectorSet<T>> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            
            if (rows.Count == 0)
            {
                throw new InvalidOperationException($"`{nameof(rows)}` can't be empty.");
            }

            ColumnCount = rows[0].DataLength;
            if (rows.Any(cols => cols.DataLength != ColumnCount))
            {
                throw new InvalidOperationException($"`{nameof(rows)}` should have equal number of elements.");
            }

            Rows = rows;
        }

        public int ColumnCount { get; }
        public int RowCount => Rows.Count;
        public IReadOnlyList<VectorSet<T>> Rows { get; }

        public override string ToString()
        {
            return $"{nameof(VectorizedMatrix<T>)}({Rows.Count}x{ColumnCount})";
        }

        public VectorizedMatrix<T> Slice(int rowStart, int rowCount, int colStart, int colsCount)
        {
            if (rowStart > Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowStart));
            }

            if (rowStart + rowCount > Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowCount));
            }

            var rows = new VectorSet<T>[rowCount];
            for (var r = 0; r < rowCount; r++)
            {
                var origRow = Rows[r + rowStart];
                var vectorSet = origRow.Slice(colStart, colsCount);
                rows[r] = vectorSet;
            }

            return new VectorizedMatrix<T>(rows);
        }
    }
}
