namespace Matrices.Matrix;


class DenseMatrix : Matrix
{
    public override float this[int row, int col]
    {
        get
        {
            if (!ValidIndices(row, col))
                throw new ArgumentException("Index out of range.");
            
            return _data[row, col];
        }
        set
        {
            if (!ValidIndices(row, col))
                throw new ArgumentException("Index out of range.");
            
            _data[row, col] = value;
        }
    }

    private float[,] _data;

    public DenseMatrix(int rows, int cols): base(rows, cols)
    {
        // In C# tables are zero initialized
        _data = new float[rows, cols];
    }

    public override Matrix Transpose()
    {
        Matrix result = new DenseMatrix(Columns, Rows );
        
        for (int row = 0; row < result.Rows; row++)
        {
            for (int col = 0; col < result.Columns; col++)
            {
                result[row, col] = this[col, row];
            }
        }
        
        return result;
    }
    
    public override void Identity()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Matrix is not a square matrix");

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                float value = row == col ? 1.0f : 0.0f;
                this[row, col] = value;
            }
        }
    }
    
    public override float Norm()
    {
        float result = 0.0f;
        
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                result += this[row, col] * this[row, col];
            }
        }
        
        return MathF.Sqrt(result);
    }

    protected override Matrix GetInstance(int rows, int cols)
    {
        return new DenseMatrix(rows, cols);
    }
}