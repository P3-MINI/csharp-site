using System.Collections;
using System.Numerics;
using System.Text;

namespace Task3; 
public abstract class Matrix<T> : IEnumerable<T>, IEquatable<Matrix<T>> where T : struct, INumber<T> {
    protected static readonly T ZERO = T.AdditiveIdentity;
    protected static readonly T ONE = T.MultiplicativeIdentity;
    protected static readonly EqualityComparer<T> _cmp = EqualityComparer<T>.Default;

    public int Rows { get; init; }
    public int Columns { get; init; }

    public abstract T this[int row, int column] { get; set; }
    public abstract T[] GetRow(int row);
    public abstract T[] GetColumn(int column);

    public abstract IEnumerator<T> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #region HELPERS
    protected static void RequireSameShape(Matrix<T> a, Matrix<T> b) {
        if (a.Rows != b.Rows || a.Columns != b.Columns) {
            throw new InvalidOperationException($"Shape mismatch: {a.Rows}x{a.Columns} vs {b.Rows}x{b.Columns}.");
        }
    }

    protected void CheckIndex(int row, int column) {
        if (row < 0 || row >= Rows) {
            throw new IndexOutOfRangeException(nameof(row));
        }
        if (column < 0 || column >= Columns) {
            throw new IndexOutOfRangeException(nameof(column));
        }
    }
    #endregion
    #region EQUALITY
    public bool Equals(Matrix<T>? other) {
        if (other is null) {
            return false;
        }
        if (ReferenceEquals(this, other)) {
            return true;
        }
        if (Rows != other.Rows || Columns != other.Columns) {
            return false;
        }

        using var e1 = GetEnumerator();
        using var e2 = other.GetEnumerator();

        while (e1.MoveNext() && e2.MoveNext()) {
            if (!_cmp.Equals(e1.Current, e2.Current)) {
                return false;
            }
        }
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as Matrix<T>);

    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(Rows);
        hash.Add(Columns);

        foreach (var x in this) {
            hash.Add(_cmp.GetHashCode(x));
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(Matrix<T>? a, Matrix<T>? b) {
        if (ReferenceEquals(a, b)) {
            return true;
        }
        if (a is null || b is null) {
            return false;
        }
        return a.Equals(b);
    }

    public static bool operator !=(Matrix<T>? a, Matrix<T>? b) => !(a == b);
    #endregion

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append('[');
        for (int i = 0; i < Rows; i++) {
            if (i > 0) {
                sb.Append("; ");
            }
            sb.Append('[');
            for (int j = 0; j < Columns; j++) {
                if (j > 0) {
                    sb.Append(", ");
                }
                sb.Append(this[i, j].ToString() ?? "");
            }
            sb.Append(']');
        }
        sb.Append(']');
        return sb.ToString();
    }
}
