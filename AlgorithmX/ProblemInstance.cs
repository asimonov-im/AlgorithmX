using System;
using System.Collections.Generic;

namespace AlgorithmX
{
    public class ProblemInstance
    {
        private Column Root { get; } = new Column();

        private Dictionary<int, Entry> FirstEntryInRow { get; }
            = new Dictionary<int, Entry>();

        private List<int> Solution { get; }
            = new List<int>();

        private ProblemInstance()
        {
        }

        public IEnumerable<Solution> Search()
        {
            return Search(0);
        }

        public static ProblemInstance Create<T>(T[,] matrix)
        {
            return Create(matrix, DefaultPredicate);
        }

        public static ProblemInstance Create<T>(T[,] matrix, int primaryColumnCount)
        {
            return Create(matrix, DefaultPredicate, primaryColumnCount);
        }

        public static ProblemInstance Create<T>(T[,] matrix, Predicate<T> predicate, int? primaryColumnCount = null)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            // Get row/column counts for convenience
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            primaryColumnCount = primaryColumnCount ?? colCount;

            var result = new ProblemInstance();

            // Create the root and headers first
            //var root = new Column();
            var columns = new List<Column>();
            for (int c = 0; c < colCount; ++c)
            {
                var column = new Column();
                columns.Add(column);

                // Do not link secondary columns into the header
                if (c < primaryColumnCount)
                {
                    result.Root.AppendToRow(column);
                }
            }

            for (int r = 0; r < rowCount; ++r)
            {
                Entry firstEntryInRow = null;
                for (int c = 0; c < colCount; ++c)
                {
                    if (predicate(matrix[r, c]))
                    {
                        var entry = new Entry(columns[c], r);
                        columns[c].AppendEntry(entry);

                        if (firstEntryInRow != null)
                        {
                            firstEntryInRow.AppendToRow(entry);
                        }
                        else
                        {
                            firstEntryInRow = entry;
                            result.FirstEntryInRow[r] = entry;
                        }
                    }
                }
            }

            return result;
        }

        private Solution ConstructSolution()
        {
            Solution solution = null;
            if (Solution.Count > 0)
            {
                solution = new Solution(Solution.ToArray());
            }

            return solution;
        }

        private bool RootIsEmpty()
        {
            return Root.Right == Root;
        }

        private IEnumerable<Solution> Search(int k)
        {
            if (RootIsEmpty())
            {
                var solution = ConstructSolution();
                if (solution != null)
                {
                    yield return solution;
                }
            }
            else
            {
                var c = FindColumnWithFewestOnes();
                c.Cover();

                for (var r = c.Down; r != c; r = r.Down)
                {
                    Solution.Add(r.RowId);
                    for (var j = r.Right; j != r; j = j.Right)
                    {
                        j.Column.Cover();
                    }

                    foreach (var solution in Search(k + 1))
                    {
                        yield return solution;
                    }

                    for (var j = r.Left; j != r; j = j.Left)
                    {
                        j.Column.Uncover();
                    }
                    Solution.RemoveLast();
                }

                c.Uncover();
            }
        }

        private Column FindColumnWithFewestOnes()
        {
            Column result = null;
            for (var c = Root.Right; c != Root; c = c.Right)
            {
                if (result == null || c.RowCount < result.RowCount)
                {
                    result = c;
                }
            }

            return result;
        }

        private static bool DefaultPredicate<T>(T obj)
            => !EqualityComparer<T>.Default.Equals(obj, default(T));
    }
}
