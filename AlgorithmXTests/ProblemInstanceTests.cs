using System;
using System.Collections.Generic;
using System.Linq;
using AlgorithmX;
using AlgorithmX.ProblemBuilders;
using Xunit;

namespace AlgorithmXTests
{
    public class ProblemInstanceTests
    {
        [Fact]
        public void NullMatrixThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ProblemInstance.Create((int[,])null));
        }

        [Fact]
        public void SimpleIntMatrix()
        {
            var matrix = new[,]
            {
                {1, 0, 0},
                {0, 1, 0},
                {0, 0, 1}
            };

            var problem = ProblemInstance.Create(matrix);
            var result = problem.Search().Select(x => x.RowIndices).ToList();

            Assert.Single(result);
            Assert.Contains(new int[] { 0, 1, 2 }, result);
        }

        [Fact]
        public void SimpleCharMatrixAndCustomPredicate()
        {
            var matrix = new[,]
            {
                {'X', 'O', 'O'},
                {'O', 'X', 'O'},
                {'O', 'O', 'X'}
            };

            var problem = ProblemInstance.Create(matrix, x => x == 'X');
            var result = problem.Search().Select(x => x.RowIndices).ToList();

            Assert.Single(result);
            Assert.Contains(new int[] { 0, 1, 2 }, result);
        }

        [Fact]
        public void DancingLinksPaperMatrix()
        {
            var matrix = new[,]
            {
                {0, 0, 1, 0, 1, 1, 0},
                {1, 0, 0, 1, 0, 0, 1},
                {0, 1, 1, 0, 0, 1, 0},
                {1, 0, 0, 1, 0, 0, 0},
                {0, 1, 0, 0, 0, 0, 1},
                {0, 0, 0, 1, 1, 0, 1},
            };

            var problem = ProblemInstance.Create(matrix);
            var result = problem.Search().Select(x => x.RowIndices).ToList();

            Assert.Single(result);
            Assert.Contains(new int[] { 0, 3, 4 }, result);
        }

        [Fact]
        public void MatrixWithMultipleSolutions()
        {
            var matrix = new[,]
            {
                {1, 0, 0, 0},
                {0, 1, 1, 0},
                {1, 0, 0, 1},
                {0, 0, 1, 1},
                {0, 1, 0, 0},
                {0, 0, 1, 0}
            };

            var problem = ProblemInstance.Create(matrix);
            var result = problem.Search().Select(x => x.RowIndices).ToList();

            Assert.Equal(3, result.Count);
            Assert.Contains(new int[] { 0, 3, 4 }, result);
            Assert.Contains(new int[] { 1, 2 }, result);
            Assert.Contains(new int[] { 2, 4, 5 }, result);
        }

        [Fact]
        public void FiveQueenProblem()
        {
            var fiveQueen = new NQueen(5);
            var problem = ProblemInstance.Create(fiveQueen.Matrix, fiveQueen.PrimaryColumnCount);
            var result = problem.Search().Select(x => x.RowIndices).ToList();

            Assert.Equal(10, result.Count);
            Assert.Contains(new int[] { 0, 7, 14, 16, 23 }, result);
            Assert.Contains(new int[] { 0, 8, 11, 19, 22 }, result);
            Assert.Contains(new int[] { 1, 8, 10, 17, 24 }, result);
            Assert.Contains(new int[] { 1, 9, 12, 15, 23 }, result);
            Assert.Contains(new int[] { 2, 5, 13, 16, 24 }, result);
            Assert.Contains(new int[] { 2, 9, 11, 18, 20 }, result);
            Assert.Contains(new int[] { 3, 5, 12, 19, 21 }, result);
            Assert.Contains(new int[] { 3, 6, 14, 17, 20 }, result);
            Assert.Contains(new int[] { 4, 6, 13, 15, 22 }, result);
            Assert.Contains(new int[] { 4, 7, 10, 18, 21 }, result);
        }

        [Fact]
        public void HardSudoku()
        {
            var unsolvedSudoku = new int[,]
            {
                { 8, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 3, 6, 0, 0, 0, 0, 0 },
                { 0, 7, 0, 0, 9, 0, 2, 0, 0 },
                { 0, 5, 0, 0, 0, 7, 0, 0, 0 },
                { 0, 0, 0, 0, 4, 5, 7, 0, 0 },
                { 0, 0, 0, 1, 0, 0, 0, 3, 0 },
                { 0, 0, 1, 0, 0, 0, 0, 6, 8 },
                { 0, 0, 8, 5, 0, 0, 0, 1, 0 },
                { 0, 9, 0, 0, 0, 0, 4, 0, 0 }
            };

            var solution = new int[,]
            {
                { 8, 1, 2, 7, 5, 3, 6, 4, 9 },
                { 9, 4, 3, 6, 8, 2, 1, 7, 5 },
                { 6, 7, 5, 4, 9, 1, 2, 8, 3 },
                { 1, 5, 4, 2, 3, 7, 8, 9, 6 },
                { 3, 6, 9, 8, 4, 5, 7, 2, 1 },
                { 2, 8, 7, 1, 6, 9, 5, 3, 4 },
                { 5, 2, 1, 9, 7, 4, 3, 6, 8 },
                { 4, 3, 8, 5, 2, 6, 9, 1, 7 },
                { 7, 9, 6, 3, 1, 8, 4, 5, 2 },
            };


            var sudoku = new Sudoku<int>(unsolvedSudoku, x => x - 1);
            var problem = ProblemInstance.Create(sudoku.Matrix, sudoku.PrimaryColumnCount);
            var result = problem.Search().Select(x => x.RowIndices).ToList();

            Assert.Single(result);
            Assert.Equal(solution, sudoku.GetSolution(result[0], x => x + 1));
        }
    }
}
