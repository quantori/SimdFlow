# SimdFlow
Code that helps to write logic based on SIMD operations

# VectorSet
is a wrapper around .Net `ReadOnlyMemory` that allows to process it as a set of Vectors and adds convenient methods to apply different functions to them.

## Public methods

### `ReadOnlyMemory<T> GetData()`
Returns the  data of the vector set.

### `VectorSet<T> Slice(int start, int length)`
Slices data according to the input parameters.

    var src = Enumerable.Range(0, size).Select(i => (short) i).ToArray();
    var vs = new VectorSet<short>(new ReadOnlyMemory<short>(src));
    var half = size / 2;
    var vs = vs.Slice(half, size - half);
    CollectionAssert.AreEqual(src.Skip(half).ToList(), vs.GetData().ToArray());


### `VectorSet<T> Map(Func<Vector<T>, Vector<T>> func, ReturningArrayPool<T>? pool = null)`

Maps every element from the input collection to another one using provided vector function.

    var src = Enumerable.Range(0, size).Select(i => (short) i).ToArray();
    var vs1 = new VectorSet<short>(new ReadOnlyMemory<short>(src));
    var vs = vs1.Map(vector => vector + Vector<short>.One);
    CollectionAssert.AreEqual(src.Select(i => i + 1).ToList(), vs.GetData().ToArray());


### `VectorSet<T> Merge(VectorSet<T> vectorSet, Func<Vector<T>, Vector<T>, Vector<T>> func, bool includeHangovers = false, ReturningArrayPool<T>? pool = null)`
Merges two `VectorSets` into a single one using provided vector function.

    var src = Enumerable.Range(0, size).Select(i => (short) i).ToArray();
    var vs1 = new VectorSet<short>(new ReadOnlyMemory<short>(src));
    var vs2 = new VectorSet<short>(new ReadOnlyMemory<short>(src));
    var vs = vs1.Merge(vs2, (v1, v2) => v1 + v2);
    CollectionAssert.AreEqual(src.Select(i => i + i).ToList(), vs.GetData().ToArray());

# ReturningArrayPool
We use a custom pool for SIMD S-W because of the following limitations of default ArrayPool:
1. We care about getting arrays with precise length (ArrayPool can return arrays longer than requested, and trimming would cause an additional allocation)
1. We do not have information about allocated array instances so that we could Return() them explicitly
1. We want to be able to return all allocated instances at once

`The implementation is WITHOUT thread safety.`
## Public Methods
### `T[] Rent(int length)`
is used to get an array rented from the pool. The array can be populated with random data, so the consumer method should not expect it to be empty and should populate it by itself.

### `void ReturnRented()`
is used to return all rented arrays to the pool.

# VectorizedMatrix`<T>`
is used to keep VectorSets in a form of a matrix.

## Public properties
### `int ColumnCount`
returns the number of columns.
### `int RowCount`
returns the number of rows.
### `IReadOnlyList<VectorSet<T>> Rows`
returns the  rows of the matrix.

## Public methods
### `VectorizedMatrix<T> Slice(int rowStart, int rowCount, int colStart, int colsCount)`
Slices the matrix according to the input parameters.

# AnnotatedMatrix`<TKey, T>`
represents a matrix that has custom identifiers of type `TKey` for every row and column.
For instance, `AnnotatedMatrix<char, byte>`:
|   | G | T | A | C |
| - | - | - | - | - |
| A | 0 | 2 | 0 | 0 |
| T | 0 | 0 | 4 | 0 |
| C | 2 | 0 | 0 | 2 |