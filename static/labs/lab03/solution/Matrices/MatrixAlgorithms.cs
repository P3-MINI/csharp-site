using Matrices.Matrix;


class MatrixAlgorithms
{
    public const float Epsilon = 1e-7f;
    
    public static Matrix PseudoInverse(Matrix a, int maxIter = 1000)
    {
        if (a.Rows != a.Columns)
            throw new ArgumentException("Rows and columns must be equal.");
        
        float norm = a.Norm();
        float alpha = 1.0f / (norm * norm);
        
        Matrix a1 = alpha * a.Transpose();
        Matrix a2;
        Matrix I = new SparseMatrix(a.Rows, a.Columns);
        I.Identity();

        int i = 0;
        
        do
        { 
            a2 = a1 + a1 * (I - a * a1);
            if ((a2 - a1).Norm() < Epsilon)
            {
                break;
            }
                

            a1 = a2;
        } while (++i <= maxIter);

        return a2;
    }
}