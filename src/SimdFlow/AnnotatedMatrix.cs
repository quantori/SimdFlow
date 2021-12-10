using System;
using System.Collections.Generic;

namespace SimdFlow
{
    public class AnnotatedMatrix<TKey, T> where T : unmanaged
    {
        public AnnotatedMatrix(IReadOnlyList<TKey> rowKeys, IReadOnlyList<TKey> colKeys, VectorizedMatrix<T> matrix)
        {
            RowKeys = rowKeys;
            ColKeys = colKeys;
            Matrix = matrix;

            if (rowKeys.Count != matrix.Rows.Count)
            {
                throw new InvalidOperationException("Row Key count should be equal to matrix' row count.");
            }

            if (colKeys.Count != matrix.ColumnCount)
            {
                throw new InvalidOperationException("Column Key count should be equal to matrix' column count.");
            }
        }

        public IReadOnlyList<TKey> RowKeys { get; }
        public IReadOnlyList<TKey> ColKeys { get; }
        public VectorizedMatrix<T> Matrix { get; }
    }
}
