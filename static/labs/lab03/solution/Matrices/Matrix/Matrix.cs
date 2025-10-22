using System.Text;


namespace Matrices.Matrix;


abstract class Matrix
{
    public int Rows { get; }
    public int Columns { get; }
    
    public abstract float this[int row, int col] { get; set; }

    public abstract void Identity();

    protected Matrix(int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
            throw new ArgumentException("Row and Column count must be greater than zero");
        
        Rows = rows;
        Columns = cols;
    }

    public abstract Matrix Transpose();

    public abstract float Norm();

    public static Matrix operator +(Matrix a, Matrix b)
    {
        if (!SameSize(a, b))
            throw new ArgumentException("Invalid matrices sizes for addition");
        
        Matrix result = a.GetInstance(a.Rows, a.Columns);

        for (int row = 0; row < a.Rows; row++)
        {
            for (int col = 0; col < a.Columns; col++)
            {
                result[row, col] = a[row, col] + b[row, col];
            }
        }
        
        return result;
    }
    
    
    public static Matrix operator -(Matrix a, Matrix b)
    {
        if (!SameSize(a, b))
            throw new ArgumentException("Invalid matrices sizes for subtraction");
        
        Matrix result = a.GetInstance(a.Rows, a.Columns);

        for (int row = 0; row < a.Rows; row++)
        {
            for (int col = 0; col < a.Columns; col++)
            {
                result[row, col] = a[row, col] - b[row, col];
            }
        }
        
        return result;
    }
    
    
    public static Matrix operator *(float scalar, Matrix m)
    {
        Matrix result = m.GetInstance(m.Rows, m.Columns);

        for (int row = 0; row < m.Rows; row++)
        {
            for (int col = 0; col < m.Columns; col++)
            {
                result[row, col] = scalar * m[row, col];
            }
        }
        
        return result;
    }


    public static Matrix operator *(Matrix a, Matrix b)
    {
        if (a.Columns != b.Rows)
            throw new ArgumentException("Invalid matrices sizes for multiplication");
        
        Matrix result = a.GetInstance(a.Rows, b.Columns);
        
        for (int row = 0; row < result.Rows; row++)
        {
            for (int col = 0; col < result.Columns; col++)
            {
                for (int i = 0; i < a.Columns; i++)
                {
                    result[row, col] += a[row, i] * b[i, col];
                }
            }
        }
        
        return result;
    }


    protected abstract Matrix GetInstance(int rows, int cols);


    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                sb.AppendFormat($"{this[row, col],8:f3} ");
            }

            if (row < Rows - 1)
                sb.Append('\n');
        }

        return sb.ToString();
    }
    

    private static bool SameSize(Matrix a, Matrix b)
    {
        return a.Rows == b.Rows && a.Columns == b.Columns;
    }

    protected bool ValidIndices(int row, int col)
    {
        return row >= 0 && row < Rows && col >= 0 && col < Columns;
    }
}
