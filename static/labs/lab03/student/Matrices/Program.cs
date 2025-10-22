#define STAGE01
// #define STAGE02
// #define STAGE03
// #define STAGE04


using Matrices.Matrix;


static class Program
{
    static void Main()
    {
#if STAGE01
        Console.WriteLine("----- Stage 01 -----");
        
        Matrix dense = new DenseMatrix(4, 5);
        Matrix sparse = new SparseMatrix(4, 5);
        Console.WriteLine("Matrices should be zero initialized:");
        Console.WriteLine($"Dense:\n{dense}\n");
        Console.WriteLine($"Sparse:\n{sparse}\n");

        dense[1, 2] = 5;
        sparse[1, 2] = 5;
        Console.WriteLine("Matrices after changing value (1, 2) to 5:");
        Console.WriteLine($"Dense:\n{dense}\n");
        Console.WriteLine($"Sparse:\n{sparse}\n");
#endif // STAGE01

#if STAGE02
        Console.WriteLine("----- Stage 02 -----");

        Matrix identity = new SparseMatrix(5, 5);
        identity.Identity();
        Console.WriteLine($"Identity matrix:\n{identity}");
        
        Random rnd = new Random(42);
        FillRandomly(dense, rnd);
        FillRandomly(sparse, rnd);
        Console.WriteLine("Random matrices:");
        Console.WriteLine($"Dense:\n{dense}\n");
        Console.WriteLine($"Sparse:\n{sparse}\n");

        dense = dense.Transpose();
        sparse = sparse.Transpose();
        
        Console.WriteLine("Transposed matrices:");
        Console.WriteLine($"Dense:\n{dense}\n");
        Console.WriteLine($"Sparse:\n{sparse}\n");
        
        Console.WriteLine($"Norm of a dense matrix: {dense.Norm()}");
        Console.WriteLine($"Norm of a sparse matrix: {sparse.Norm()}");
#endif // STAGE02
        
#if STAGE03
        Console.WriteLine("\n----- Stage 03 -----");
        
        Matrix m1 = new DenseMatrix(3, 5);
        Matrix m2 = new SparseMatrix(3, 5);
        Matrix m3 = new DenseMatrix(5, 4);
        
        FillRandomly(m1, rnd);
        FillRandomly(m2, rnd);
        FillRandomly(m3, rnd);
        
        Console.WriteLine("Random matrices:");
        Console.WriteLine($"m1:\n{m1}\n");
        Console.WriteLine($"m2:\n{m2}\n");
        Console.WriteLine($"m3:\n{m3}\n");

        Matrix addition = m1 + m2;
        Matrix subtraction = m2 - m1;
        Matrix scalarMul = 2.0f * m1;
        Matrix matrixMul = m2 * m3;
        
        Console.WriteLine($"m1, m2 addition result:\n{addition}");
        Console.WriteLine($"Addition result type: {addition.GetType()}\n");
        
        Console.WriteLine($"m2, m1 subtraction result:\n{subtraction}");
        Console.WriteLine($"Subtraction result type: {subtraction.GetType()}\n");
        
        Console.WriteLine($"m1 and 2.0f multiplication result:\n{scalarMul}");
        Console.WriteLine($"Multiplication with float result type: {scalarMul.GetType()}\n");
        
        Console.WriteLine($"m2, m3 multiplication result:\n{matrixMul}");
        Console.WriteLine($"Multiplication result type: {matrixMul.GetType()}\n");
#endif // STAGE03

#if STAGE04
        Console.WriteLine("\n----- Stage 04 -----");

        // This matrix is reversible
        Matrix m = new  DenseMatrix(3, 3);
        m[0, 0] = 5;    m[0, 1] = 4;    m[0, 2] = 2;
        m[1, 0] = 9;    m[1, 1] = 5;    m[1, 2] = 4;
        m[2, 0] = 0;    m[2, 1] = 8;    m[2, 2] = 9;
        
        Console.WriteLine($"Matrix m:\n{m}\n");

        Matrix pseudoInv = MatrixAlgorithms.PseudoInverse(m);
        
        Console.WriteLine($"pseudoInvM:\n{pseudoInv}\n");
        Console.WriteLine($"m*pseudoInvM:\n{m*pseudoInv}\n");
#endif // STAGE04
    }

    
    static void FillRandomly(Matrix matrix, Random generator, int minVal = 0, int maxVal = 10)
    {
        for (int row = 0; row < matrix.Rows; row++)
        {
            for (int col = 0; col < matrix.Columns; col++)
            {
                matrix[row, col] = generator.Next(minVal, maxVal);
            }
        }
    }
}
