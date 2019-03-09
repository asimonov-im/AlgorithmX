using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgorithmX.ProblemBuilders
{
    public class Sudoku<T>
    {
        private const int BlankDigit = -1;

        private int BlockDimension { get; set; }

        private int BoardDimension { get; set; }

        private int BoardSize { get; set; }

        private int CurrentRow => RowIds.Count;

        private List<int> RowIds { get; }
            = new List<int>();

        public bool[,] Matrix { get; set; }

        public int PrimaryColumnCount => Matrix.GetLength(1);

        /// <summary>
        /// Constructs an exact cover problem matrix for an empty Sudoku grid with
        /// the specified dimensions.
        /// </summary>
        /// <param name="n">Sudoku grid width and height.</param>
        public Sudoku(int n)
        {
            ValidateDimensions(n, n);

            Init(null, null, n);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid">Sudoku grid representation.</param>
        /// <param name="map">
        /// Function mapping grid values to integers.
        /// The function should return -1 for an empty cell, and an integer between 0 and (Dimension -1) otherwise.
        /// </param>
        public Sudoku(T[,] grid, Func<T, int> map)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }
            else if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            int n = grid.GetLength(0);
            int m = grid.GetLength(1);
            ValidateDimensions(n, m);

            Init(grid, map, n);
        }

        /// <summary>
        /// Returns the filled out Sudoku grid.
        /// </summary>
        /// <param name="solutionRows"></param>
        /// <param name="map">Function mapping between integer digit values and Sudoku grid values.</param>
        /// <returns></returns>
        public T[,] GetSolution(IEnumerable<int> solutionRows, Func<int, T> map)
        {
            var result = new T[BoardDimension, BoardDimension];
            foreach (var row in solutionRows)
            {
                (int r, int c, int d) = RowIdToRCD(row);
                result[r, c] = map(d);
            }

            return result;
        }

        private void ValidateDimensions(int n, int m)
        {
            if (n < 4)
            {
                throw new ArgumentException("Dimension must be >= 4.");
            }

            int blockSize = (int)Math.Sqrt(n);
            if (n != m || blockSize * blockSize != n)
            {
                throw new ArgumentException("Grid dimensions are not a perfect square.");
            }
        }

        private void Init(T[,] grid, Func<T, int> map, int n)
        {
            BoardDimension = n;
            BoardSize = n * n;
            BlockDimension = (int)Math.Sqrt(n);

            int blankCount = grid != null ? CountBlanks(grid, map) : BoardSize;
            int filledCount = BoardSize - blankCount;
            Matrix = new bool[n * blankCount + filledCount, 4 * BoardSize];

            GenerateConstraints(grid, map);
        }

        private void GenerateConstraints(T[,] grid, Func<T, int> map)
        {
            for (int row = 0; row < BoardDimension; ++row)
            {
                for (int col = 0; col < BoardDimension; ++col)
                {
                    int block = GetBlock(row, col);

                    int gridDigit = grid != null ? map(grid[row, col]) : BlankDigit;
                    foreach (var digit in GetPossibleDigits(gridDigit))
                    {
                        AddConstraints(row, col, digit, block);
                    }
                }
            }
        }

        private IEnumerable<int> GetPossibleDigits(int digit)
        {
            return digit <= BlankDigit ?
                Enumerable.Range(0, BoardDimension) :
                Enumerable.Repeat(digit, 1);
        }

        private void AddConstraints(int row, int column, int digit, int block)
        {
            Matrix[CurrentRow, CellConstraintIdx(row, column)] = true;
            Matrix[CurrentRow, RowConstraintIdx(row, digit)] = true;
            Matrix[CurrentRow, ColConstraintIdx(column, digit)] = true;
            Matrix[CurrentRow, BlockConstraintIdx(block, digit)] = true;

            AddRowMapping(row, column, digit);
        }

        private void AddRowMapping(int row, int column, int digit)
        {
            int fixedRowId = EncodeRowId(row, column, digit);
            RowIds.Add(fixedRowId);
        }

        private static int CountBlanks(T[,] grid, Func<T, int> map)
        {
            int blankCount = 0;
            foreach (var val in grid)
            {
                if (map(val) < 0) ++blankCount;
            }

            return blankCount;
        }

        private int GetBlock(int r, int c)
        {
            return r / BlockDimension * BlockDimension + c / BlockDimension;
        }

        private int CellConstraintIdx(int r, int c)
        {
            return BoardDimension * r + c;
        }

        private int RowConstraintIdx(int r, int d)
        {
            int offset = BoardSize;
            return BoardDimension * r + d + offset;
        }

        private int ColConstraintIdx(int c, int d)
        {
            int offset = 2 * BoardSize;
            return BoardDimension * c + d + offset;
        }

        private int BlockConstraintIdx(int b, int d)
        {
            int offset = 3 * BoardSize;
            return BoardDimension * b + d + offset;
        }

        private int EncodeRowId(int r, int c, int d)
        {
            return r * BoardSize + c * BoardDimension + d;
        }

        private (int r, int c, int d) RowIdToRCD(int rowIdx)
        {
            int fixedRowId = RowIds[rowIdx];

            int r = fixedRowId / BoardSize;
            int c = (fixedRowId % BoardSize) / BoardDimension;
            int d = fixedRowId % BoardDimension;

            return (r, c, d);
        }
    }
}
