using System.Numerics;
namespace Task3;

public sealed class SparseMatrix<T> : Matrix<T> where T : struct, INumber<T> {
    private readonly Dictionary<(int Row, int Column), T> _data;

    #region CONSTRUCTION
    public SparseMatrix(int rows, int columns, T[]? values = null) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);

        int cells = rows * columns;

        if (values is not null && values.Length > cells) {
            throw new ArgumentException(
                $"Too many values: {values.Length} given, {cells} cells available for {rows}x{columns}.", nameof(values));
        }

        Rows = rows;
        Columns = columns;

        _data = [];
        if (values is not null) {
            for (int i = 0; i < values.Length; i++) {
                if (ZERO != values[i]) {
                    _data[(i / columns, i % columns)] = values[i];
                }
            }
        }
    }

    public static SparseMatrix<T> Identity(int size) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        var m = new SparseMatrix<T>(size, size);
        for (int i = 0; i < size; i++) {
            m._data[(i, i)] = ONE;
        }
        return m;
    }
    #endregion
    #region INDEXING
    public override T this[int row, int column] {
        get {
            CheckIndex(row, column);
            return _data.TryGetValue((row, column), out var value) ? value : ZERO;
        }
        set {
            CheckIndex(row, column);
            if (EqualityComparer<T>.Default.Equals(value, ZERO)) {
                _data.Remove((row, column));
            } else {
                _data[(row, column)] = value;
            }
        }
    }

    public override T[] GetRow(int row) {
        if (row < 0 || row >= Rows) {
            throw new IndexOutOfRangeException(nameof(row));
        }
        var result = new T[Columns];
        for (int j = 0; j < Columns; j++) {
            if (_data.TryGetValue((row, j), out var value)) {
                result[j] = value;
            }
        }
        return result;
    }

    public override T[] GetColumn(int column) {
        if (column < 0 || column >= Columns) {
            throw new IndexOutOfRangeException(nameof(column));
        }
        var result = new T[Rows];
        for (int i = 0; i < Rows; i++) {
            if (_data.TryGetValue((i, column), out var value)) {
                result[i] = value;
            }
        }
        return result;
    }
    #endregion
    #region OPERATORS
    public static SparseMatrix<T> operator +(SparseMatrix<T> a, SparseMatrix<T> b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        RequireSameShape(a, b);

        var result = a.Clone();
        foreach (var (key, value) in b._data) {
            if (result._data.TryGetValue(key, out var existing)) {
                var sum = existing + value;
                if (sum == ZERO) {
                    result._data.Remove(key);
                } else {
                    result._data[key] = sum;
                }
            } else {
                result._data[key] = value;
            }
        }
        return result;
    }

    public static SparseMatrix<T> operator -(SparseMatrix<T> a, SparseMatrix<T> b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        RequireSameShape(a, b);

        var result = a.Clone();
        foreach (var (key, value) in b._data) {
            if (result._data.TryGetValue(key, out var existing)) {
                var diff = existing - value;
                if (diff == ZERO) {
                    result._data.Remove(key);
                } else {
                    result._data[key] = diff;
                }
            } else {
                result._data[key] = ZERO - value;
            }
        }
        return result;
    }

    public static SparseMatrix<T> operator *(SparseMatrix<T> a, T scalar) {
        ArgumentNullException.ThrowIfNull(a);

        var result = new SparseMatrix<T>(a.Rows, a.Columns);

        if (scalar == ZERO) {
            return result;   
        }

        foreach (var (key, value) in a._data) {
            result._data[key] = value * scalar;
        }
        return result;
    }

    public static SparseMatrix<T> operator *(T scalar, SparseMatrix<T> a) => a * scalar;

    // a[M,K] * b[K,N]
    // i = [0,M) j = [0,N)
    // C_ij = S_{k=0:K-1} A_ik * B_kj
    public static SparseMatrix<T> operator *(SparseMatrix<T> a, SparseMatrix<T> b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.Columns != b.Rows) {
            throw new InvalidOperationException(
                $"Cannot multiply {a.Rows}x{a.Columns} by {b.Rows}x{b.Columns}: left columns ({a.Columns}) != right rows ({b.Rows}).");
        }

        var result = new SparseMatrix<T>(a.Rows, b.Columns);
        foreach (var (aKey, aValue) in a._data) {
            int i = aKey.Row;
            int k = aKey.Column;

            foreach (var (bKey, bValue) in b._data) {
                if (bKey.Row != k) {
                    continue;
                }
                int j = bKey.Column;
                var cell = (i, j);

                if (result._data.TryGetValue(cell, out var existing)) {
                    var sum = existing + aValue * bValue;
                    if (sum != ZERO) {
                        result._data.Remove(cell);
                    } else {
                        result._data[cell] = sum;
                    }
                } else {
                    var product = aValue * bValue;
                    if (product != ZERO) {
                        result._data[cell] = product;
                    }
                }
            }
        }

        return result;
    }
    #endregion
    #region OTHER
    public SparseMatrix<T> Clone() {
        var clone = new SparseMatrix<T>(Rows, Columns);
        foreach (var (key, value) in _data) {
            clone._data[key] = value;
        }
        return clone;
    }

    public override IEnumerator<T> GetEnumerator() {
        for (int i = 0; i < Rows; i++) {
            for (int j = 0; j < Columns; j++) {
                if (_data.TryGetValue((i, j), out var value)) {
                    yield return value;
                } else {
                    yield return ZERO;
                }
            }
        }
    }
    #endregion
}