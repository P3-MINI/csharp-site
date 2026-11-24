#define DENSE
#define SPARSE
#define BOTH

using Task3;

int passed = 0;
int failed = 0;

void Check(string name, bool condition) {
    if (condition) {
        passed++;
        Console.WriteLine($"  [PASS] {name}");
    } else {
        failed++;
        Console.WriteLine($"  [FAIL] {name}");
    }
}

void CheckThrows<E>(string name, Action action) where E : Exception {
    try {
        action();
        failed++;
        Console.WriteLine($"  [FAIL] {name}  (no exception)");
    } catch (E) {
        passed++;
        Console.WriteLine($"  [PASS] {name}");
    } catch (Exception ex) {
        failed++;
        Console.WriteLine($"  [FAIL] {name}  (got {ex.GetType().Name})");
    }
}

#if !DENSE && !SPARSE && !BOTH
Console.WriteLine("Uncomment appropriate stage at the top of Program.cs to check your solution.");
#endif

#if DENSE
// ================================================================
// DenseMatrix / construction
// ================================================================
Console.WriteLine("DenseMatrix / construction");

var d1 = new DenseMatrix<int>(2, 3, [1, 2, 3, 4, 5, 6]);
Check("2x3 shape", d1.Rows == 2 && d1.Columns == 3);
Check("row-major values", d1[0, 0] == 1 && d1[0, 2] == 3 && d1[1, 0] == 4 && d1[1, 2] == 6);

var d2 = new DenseMatrix<int>(2, 3, [1, 2]);
Check("short array zero-pads", d2[0, 0] == 1 && d2[0, 1] == 2 && d2[0, 2] == 0 && d2[1, 0] == 0);

var d3 = new DenseMatrix<int>(2, 2);
Check("zero-filled", d3[0, 0] == 0 && d3[1, 1] == 0);

CheckThrows<ArgumentException>("too many values throws", () => new DenseMatrix<int>(2, 2, [1, 2, 3, 4, 5]));
CheckThrows<ArgumentOutOfRangeException>("zero rows throws", () => new DenseMatrix<int>(0, 2));

// ================================================================
// DenseMatrix / identity
// ================================================================
Console.WriteLine();
Console.WriteLine("DenseMatrix / identity");

var id3 = DenseMatrix<int>.Identity(3);
Check("identity diagonal = 1", id3[0, 0] == 1 && id3[1, 1] == 1 && id3[2, 2] == 1);
Check("identity off-diagonal = 0", id3[0, 1] == 0 && id3[2, 0] == 0);

// ================================================================
// DenseMatrix / arithmetic
// ================================================================
Console.WriteLine();
Console.WriteLine("DenseMatrix / arithmetic");

var a = new DenseMatrix<int>(2, 2, [1, 2, 3, 4]);
var b = new DenseMatrix<int>(2, 2, [5, 6, 7, 8]);

var s = a + b;
Check("a + b", s[0, 0] == 6 && s[0, 1] == 8 && s[1, 0] == 10 && s[1, 1] == 12);

var df = a - b;
Check("a - b", df[0, 0] == -4 && df[1, 1] == -4);

var sc = a * 3;
Check("a * 3", sc[0, 0] == 3 && sc[1, 1] == 12);
Check("3 * a", (3 * a)[1, 1] == 12);

var prod = a * b;
Check("a * b", prod[0, 0] == 19 && prod[0, 1] == 22 && prod[1, 0] == 43 && prod[1, 1] == 50);

var c = new DenseMatrix<int>(2, 3, [1, 2, 3, 4, 5, 6]);
var e = new DenseMatrix<int>(3, 2, [7, 8, 9, 10, 11, 12]);
var ce = c * e;
Check("2x3 * 3x2 shape", ce.Rows == 2 && ce.Columns == 2);
Check("2x3 * 3x2 values", ce[0, 0] == 58 && ce[0, 1] == 64 && ce[1, 0] == 139 && ce[1, 1] == 154);

CheckThrows<InvalidOperationException>("shape mismatch in a + b", () => { var _ = a + new DenseMatrix<int>(3, 3); });
CheckThrows<InvalidOperationException>("inner-dimension mismatch in a * b", () => { var _ = a * new DenseMatrix<int>(3, 3); });

// ================================================================
// DenseMatrix / Clone
// ================================================================
Console.WriteLine();
Console.WriteLine("DenseMatrix / Clone");

var orig = new DenseMatrix<int>(2, 2, [1, 2, 3, 4]);
var copy = orig.Clone();
Check("clone equals original", copy.Equals(orig));
Check("clone is a different instance", !ReferenceEquals(copy, orig));
copy[0, 0] = 99;
Check("mutating clone does not affect original", orig[0, 0] == 1);
#endif
#if SPARSE
// ================================================================
// SparseMatrix / construction
// ================================================================
Console.WriteLine();
Console.WriteLine("SparseMatrix / construction");

var sp = new SparseMatrix<int>(3, 3, [1, 0, 0, 0, 5, 0, 0, 0, 9]);
Check("sparse shape", sp.Rows == 3 && sp.Columns == 3);
Check("sparse non-zero cells", sp[0, 0] == 1 && sp[1, 1] == 5 && sp[2, 2] == 9);
Check("sparse zero cells", sp[0, 1] == 0 && sp[1, 0] == 0 && sp[2, 0] == 0);

sp[1, 2] = 42;
Check("sparse write", sp[1, 2] == 42);
sp[1, 2] = 0;
Check("sparse write zero removes entry", sp[1, 2] == 0);

CheckThrows<ArgumentException>("too many values throws", () => new SparseMatrix<int>(2, 2, [1, 2, 3, 4, 5]));
CheckThrows<ArgumentOutOfRangeException>("zero rows throws", () => new SparseMatrix<int>(0, 2));

// ================================================================
// SparseMatrix / identity
// ================================================================
Console.WriteLine();
Console.WriteLine("SparseMatrix / identity");

var sid = SparseMatrix<int>.Identity(3);
Check("sparse identity diagonal", sid[0, 0] == 1 && sid[1, 1] == 1 && sid[2, 2] == 1);
Check("sparse identity off-diagonal", sid[0, 1] == 0 && sid[2, 0] == 0);

// ================================================================
// SparseMatrix / arithmetic
// ================================================================
Console.WriteLine();
Console.WriteLine("SparseMatrix / arithmetic");

var sa = new SparseMatrix<int>(2, 2, [1, 0, 0, 4]);
var sb = new SparseMatrix<int>(2, 2, [0, 2, 3, 0]);

var ssum = sa + sb;
Check("sa + sb", ssum[0, 0] == 1 && ssum[0, 1] == 2 && ssum[1, 0] == 3 && ssum[1, 1] == 4);

var sdiff = sa - sb;
Check("sa - sb", sdiff[0, 0] == 1 && sdiff[0, 1] == -2 && sdiff[1, 0] == -3 && sdiff[1, 1] == 4);

var sscaled = sa * 5;
Check("sa * 5", sscaled[0, 0] == 5 && sscaled[1, 1] == 20);

var sprod = sa * sb;
Check("sa * sb", sprod[0, 0] == 0 && sprod[0, 1] == 2 && sprod[1, 0] == 12 && sprod[1, 1] == 0);

CheckThrows<InvalidOperationException>("shape mismatch in sa + sb", () => { var _ = sa + new SparseMatrix<int>(3, 3); });
CheckThrows<InvalidOperationException>("inner-dimension mismatch in sa * sb", () => { var _ = sa * new SparseMatrix<int>(3, 3); });

// ================================================================
// SparseMatrix / Clone
// ================================================================
Console.WriteLine();
Console.WriteLine("SparseMatrix / Clone");

var sorig = new SparseMatrix<int>(2, 2, [1, 0, 0, 4]);
var scopy = sorig.Clone();
Check("sparse clone equals original", scopy.Equals(sorig));
Check("sparse clone is a different instance", !ReferenceEquals(scopy, sorig));
scopy[0, 0] = 99;
Check("mutating sparse clone does not affect original", sorig[0, 0] == 1);
#endif
#if BOTH
// ================================================================
// EQUALITY
// ================================================================
Console.WriteLine();
Console.WriteLine("EQUALITY");

var e1 = new DenseMatrix<int>(2, 2, [1, 2, 3, 4]);
var e2 = new DenseMatrix<int>(2, 2, [1, 2, 3, 4]);
var e3 = new DenseMatrix<int>(2, 2, [1, 2, 3, 5]);

Check("reflexive", e1.Equals(e1));
Check("symmetric", e1.Equals(e2) && e2.Equals(e1));
Check("different values not equal", !e1.Equals(e3));
Check("different shapes not equal", !e1.Equals(new DenseMatrix<int>(4, 1, [1, 2, 3, 4])));

Check("== true for equal", e1 == e2);
Check("!= false for equal", !(e1 != e2));
Check("== false for different", e1 != e3);
Check("== both null", (DenseMatrix<int>?)null == (DenseMatrix<int>?)null);
Check("== one null", (DenseMatrix<int>?)null != e1);

Check("equal dense hash equal", e1!.GetHashCode() == e2.GetHashCode());

// cross-type
var cd = new DenseMatrix<int>(2, 2, [1, 0, 0, 4]);
var cs = new SparseMatrix<int>(2, 2, [1, 0, 0, 4]);
Check("dense equals sparse", cd.Equals(cs));
Check("sparse equals dense", cs.Equals(cd));
Check("dense and sparse hash equally", cd.GetHashCode() == cs.GetHashCode());

var ns = new SparseMatrix<int>(2, 2, [1, 0, 0, 5]);
Check("dense != sparse when values differ", !cd.Equals(ns));

// sparse-specific
var s1 = new SparseMatrix<int>(3, 3, [1, 0, 0, 0, 0, 0, 0, 0, 0]);
var s2 = new SparseMatrix<int>(3, 3, [1, 0, 0, 0, 0, 0, 0, 0, 0]);
Check("equal sparse", s1.Equals(s2));
Check("equal sparse hash equal", s1.GetHashCode() == s2.GetHashCode());

// ================================================================
// TOSTRING
// ================================================================
Console.WriteLine();
Console.WriteLine("TOSTRING");

var tm = new DenseMatrix<int>(2, 3, [1, 2, 3, 4, 5, 6]);
Check("dense format", tm.ToString() == "[[1, 2, 3]; [4, 5, 6]]");

var ts = new SparseMatrix<int>(2, 3, [1, 0, 3, 0, 5, 0]);
Check("sparse format matches dense",
    ts.ToString() == "[[1, 0, 3]; [0, 5, 0]]");

var a1 = new DenseMatrix<int>(2, 2, [1, 2, 3, 4]);
var a2 = new DenseMatrix<int>(2, 2, [1, 2, 3, 4]);
Check("equal matrices print the same", a1.ToString() == a2.ToString());

Check("dense and sparse with same values print the same", cd.ToString() == cs.ToString());

// ================================================================
// SUMMARY
// ================================================================
Console.WriteLine();
Console.WriteLine($"Passed: {passed}  Failed: {failed}");
return failed == 0 ? 0 : 1;
#endif