using System.Collections.Generic;

namespace AlgorithmX.ProblemBuilders
{
    public class NQueen
    {
        private int constraintId;

        private int BoardSize { get; }
        public bool[,] Matrix { get; }
        public int PrimaryColumnCount => 2 * BoardSize;

        public NQueen(int n)
        {
            BoardSize = n;
            Matrix = new bool[n * n, 6 * n - 6];
            constraintId = 0;

            GenerateConstraits();
        }

        public T[,] GetSolution<T>(IEnumerable<int> solutionRows, T queenValue)
        {
            var result = new T[BoardSize, BoardSize];
            foreach (var row in solutionRows)
            {
                (int r, int c) = RowIdToRowCol(row);
                result[r, c] = queenValue;
            }

            return result;
        }

        private (int r, int c) RowIdToRowCol(int rowId)
            => (rowId / BoardSize, rowId % BoardSize);

        private int RowColToRowId(int r, int c)
            => r * BoardSize + c;

        private void GenerateConstraits()
        {
            for (int r = 0; r < BoardSize; ++r)
                AppendRowConstraint(r);

            for (int c = 0; c < BoardSize; ++c)
                AppendColumnConstraint(c);

            AppendDiagonalConstraint(0, 0, 1, 1);
            AppendDiagonalConstraint(BoardSize - 1, 0, -1, 1);
            for (int c = 1; c < BoardSize - 1; ++c)
            {
                AppendDiagonalConstraint(0, c, 1, 1);
                AppendDiagonalConstraint(0, c, 1, -1);
                AppendDiagonalConstraint(BoardSize - 1, c, -1, -1);
                AppendDiagonalConstraint(BoardSize - 1, c, -1, 1);
            }
        }

        private void AppendColumnConstraint(int c)
        {
            for (int r = 0; r < BoardSize; ++r)
            {
                int rowId = RowColToRowId(r, c);
                Matrix[rowId, constraintId] = true;
            }

            ++constraintId;
        }

        private void AppendRowConstraint(int r)
        {
            for (int c = 0; c < BoardSize; ++c)
            {
                int rowId = RowColToRowId(r, c);
                Matrix[rowId, constraintId] = true;
            }

            ++constraintId;
        }

        private void AppendDiagonalConstraint(int r, int c, int deltaR, int deltaC)
        {
            while (r >= 0 && r < BoardSize &&
                   c >= 0 && c < BoardSize)
            {
                int rowId = RowColToRowId(r, c);
                Matrix[rowId, constraintId] = true;

                r += deltaR;
                c += deltaC;
            }

            ++constraintId;
        }
    }
}
