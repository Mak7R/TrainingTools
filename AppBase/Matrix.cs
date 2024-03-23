using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppBase;

public class Matrix<T>
{
    private int _rowCapacity = 4;
    private int _columnCapacity = 4;
    private const int GrowthValue = 2;
    private T[][] _matrix;

    public int Rows { get; private set; }
    public int Columns { get; private set; }

    public T this[int row, int column]
    {
        get => _matrix[row][column];
        set => _matrix[row][column] = value;
    }

    public Matrix()
    {
        InitializeMatrix(_rowCapacity, _columnCapacity);
    }

    public Matrix(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _rowCapacity = Math.Max(Rows, _rowCapacity);
        _columnCapacity = Math.Max(Rows, _rowCapacity);
        InitializeMatrix(_rowCapacity, _columnCapacity);
    }

    public Matrix(T[][] data)
    {
        int columns = data.Length == 0 ? 0 : data[0].Length;
        
        if (data.Any(row => row.Length != columns))
            throw new ArgumentException("All rows must have the same length.");

        Rows = data.Length;
        Columns = columns;
        _rowCapacity = Math.Max(Rows, _rowCapacity);
        _columnCapacity = Math.Max(Columns, _columnCapacity);
        
        InitializeMatrix(_rowCapacity, _columnCapacity);
        for (int i = 0; i < data.Length; i++)
        {
            Array.Copy(data[i], _matrix[i], columns);
        }
    }

    public void InsertRow(int index)
    {
        if (index < 0 || index > Rows)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (Rows == _rowCapacity)
            GrowRows();
        
        Rows++;
    }

    public void InsertColumn(int index)
    {
        if (index < 0 || index > Columns)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (Columns == _columnCapacity)
            GrowColumns();

        Columns++;
    }

    public IEnumerable<T[]> GetRowsEnumerable()
    {
        for (int i = 0; i < Rows; i++)
        {
            var row = new T[Columns];
            Array.Copy(_matrix[i], row, Columns);
            yield return row;
        }
    }

    public IEnumerable<T[]> GetColumnsEnumerable()
    {
        for (int j = 0; j < Columns; j++)
        {
            T[] column = new T[Rows];
            for (int i = 0; i < Rows; i++)
            {
                column[i] = _matrix[i][j];
            }
            yield return column;
        }
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                stringBuilder.Append(_matrix[i][j]?.ToString() ?? string.Empty);
                if (j < Columns - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            if (i < Rows - 1)
            {
                stringBuilder.AppendLine();
            }
        }

        return stringBuilder.ToString();
    }

    public void DeleteColumn(int index)
    {
        if (index < 0 || index >= Columns)
            throw new ArgumentOutOfRangeException(nameof(index));

        for (int i = 0; i < Rows; i++)
        {
            for (int j = index + 1; j < Columns; j++)
            {
                _matrix[i][j - 1] = _matrix[i][j];
            }
            Array.Resize(ref _matrix[i], Columns - 1);
        }

        Columns--;
    }

    public void DeleteRow(int index)
    {
        if (index < 0 || index >= Rows)
            throw new ArgumentOutOfRangeException(nameof(index));

        Array.Copy(_matrix, index + 1, _matrix, index, Rows - index - 1);
        Rows--;
    }

    private void InitializeMatrix(int rows, int columns)
    {
        _matrix = new T[rows][];
        for (int i = 0; i < rows; i++)
        {
            _matrix[i] = new T[columns];
        }
    }

    private void GrowColumns()
    {
        _columnCapacity += GrowthValue;
        for (int i = 0; i < _rowCapacity; i++)
        {
            Array.Resize(ref _matrix[i], _columnCapacity);
        }
    }
    
    private void GrowRows()
    {
        _rowCapacity += GrowthValue;
        Array.Resize(ref _matrix, _rowCapacity);
        for (int i = Rows; i < _rowCapacity; i++)
        {
            _matrix[i] = new T[_columnCapacity];
        }
    }

    public static Matrix<T> FromJson(string jsonString)
    {
        var model = JsonSerializer.Deserialize<MatrixModel>(jsonString) ?? throw new Exception("Deserialization result was null");
        return new Matrix<T>(model.Matrix);
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(new MatrixModel(Rows, Columns, _matrix));
    }
    
    private class MatrixModel
    {
        [JsonPropertyName("matrix")]
        public T[][] Matrix { get; set; }

        public MatrixModel()
        {
            
        }
        
        public MatrixModel(int rows, int columns, T[][] matrix)
        {
            var trimmedMatrix = new T[rows][];
            for (int i = 0; i < rows; i++)
            {
                trimmedMatrix[i] = new T[columns];
                Array.Copy(matrix[i], trimmedMatrix[i], columns);
            }

            Matrix = trimmedMatrix;
        }
    }
}

public static class MatrixExtensions{
    public static void AddColumn<T>(this Matrix<T> matrix)
    {
        matrix.InsertColumn(matrix.Columns);
    }
    
    public static void AddRow<T>(this Matrix<T> matrix)
    {
        matrix.InsertRow(matrix.Rows);
    }
}