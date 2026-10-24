using System.Numerics;
namespace Task3;

public sealed class DenseMatrix<T> : Matrix<T> where T : struct, INumber<T> {
    private readonly T[,] _data;

    #region CONSTRUCTION
    public DenseMatrix(int rows, int columns, T[]? values = null) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);

        int cells = rows * columns;

        if (values is not null && values.Length > cells) {
            throw new ArgumentException(
                $"Too many values: {values.Length} given, {cells} cells available for {rows}x{columns}.", nameof(values));
        }

        Rows = rows;
        Columns = columns;
        _data = new T[rows, columns];  

        if (values is not null) {
            for (int i = 0; i < values.Length; i++) {
                _data[i / columns, i % columns] = values[i];
            }
        }
    }

    public static DenseMatrix<T> Identity(int size) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        var m = new DenseMatrix<T>(size, size);
        for (int i = 0; i < size; i++) {
            m._data[i, i] = ONE;
        }
        return m;
    }
    #endregion
    #region INDEXING
    public override T this[int row, int column] {
        get {
            CheckIndex(row, column);
            return _data[row, column];
        }
        set {
            CheckIndex(row, column);
            _data[row, column] = value;
        }
    }

    public override T[] GetRow(int row) {
        if (row < 0 || row >= Rows) {
            throw new IndexOutOfRangeException(nameof(row));
        }
        var result = new T[Columns];
        for (int j = 0; j < Columns; j++) {
            result[j] = _data[row, j];
        }
        return result;
    }

    public override T[] GetColumn(int column) {
        if (column < 0 || column >= Columns) {
            throw new IndexOutOfRangeException(nameof(column));
        }
        var result = new T[Rows];
        for (int i = 0; i < Rows; i++) {
            result[i] = _data[i, column];
        }
        return result;
    }
    #endregion
    #region OPERATORS
    public static DenseMatrix<T> operator +(DenseMatrix<T> a, DenseMatrix<T> b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        RequireSameShape(a, b);

        var result = new DenseMatrix<T>(a.Rows, a.Columns);
        for (int i = 0; i < a.Rows; i++) {
            for (int j = 0; j < a.Columns; j++) {
                result._data[i, j] = a._data[i, j] + b._data[i, j];
            }
        }
        return result;
    }

    public static DenseMatrix<T> operator -(DenseMatrix<T> a, DenseMatrix<T> b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        RequireSameShape(a, b);

        var result = new DenseMatrix<T>(a.Rows, a.Columns);
        for (int i = 0; i < a.Rows; i++) {
            for (int j = 0; j < a.Columns; j++) {
                result._data[i, j] = a._data[i, j] - b._data[i, j];
            }
        }
        return result;
    }

    public static DenseMatrix<T> operator *(DenseMatrix<T> a, T scalar) {
        ArgumentNullException.ThrowIfNull(a);

        var result = new DenseMatrix<T>(a.Rows, a.Columns);
        for (int i = 0; i < a.Rows; i++) {
            for (int j = 0; j < a.Columns; j++) {
                result._data[i, j] = a._data[i, j] * scalar;
            }
        }
        return result;
    }

    public static DenseMatrix<T> operator *(T scalar, DenseMatrix<T> a) => a * scalar;

    // a[M,K] * b[K,N]
    // i = [0,M) j = [0,N)
    // C_ij = S_{k=0:K-1} A_ik * B_kj
    public static DenseMatrix<T> operator *(DenseMatrix<T> a, DenseMatrix<T> b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.Columns != b.Rows) {
            throw new InvalidOperationException(
                $"Cannot multiply {a.Rows}x{a.Columns} by {b.Rows}x{b.Columns}: left columns ({a.Columns}) != right rows ({b.Rows}).");
        }

        var result = new DenseMatrix<T>(a.Rows, b.Columns);
        for (int i = 0; i < a.Rows; i++) {
            for (int j = 0; j < b.Columns; j++) {
                T sum = ZERO;
                for (int k = 0; k < a.Columns; k++) {
                    sum += a._data[i, k] * b._data[k, j];
                }
                result._data[i, j] = sum;
            }
        }
        return result;
    }
    #endregion
    #region OTHER
    public DenseMatrix<T> Clone() {
        int rows = _data.GetLength(0);
        int columns = _data.GetLength(1);
        var values = new T[rows * columns];

        int i = 0;
        foreach(var value in this) {
            values[i++] = value;
        }

        return new DenseMatrix<T>(rows, columns, values);
    }

    public override IEnumerator<T> GetEnumerator() {
        for (int i = 0; i < Rows; i++) {
            for (int j = 0; j < Columns; j++) {
                yield return _data[i, j];
            }
        }
    }
    #endregion
}