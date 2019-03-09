using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AlgorithmX;
using BenchmarkDotNet.Attributes;
using AlgorithmX.ProblemBuilders;

using ProblemInstanceParallel = AlgorithmX.Parallel.ProblemInstance;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class QueenBenchmarks
    {
        private const int N = 14;
        private const int K = 2;

        private readonly NQueen Problem;

        public QueenBenchmarks()
        {
            Problem = new NQueen(N);
        }

        [Benchmark]
        public ConcurrentQueue<Solution> WithIds()
        {
            var pi = ProblemInstanceParallel.Create(Problem.Matrix, Problem.PrimaryColumnCount);
            pi.Search(K).Wait();
            return pi.Solutions;
        }

        [Benchmark]
        public List<Solution> WithObjects()
        {
            var pi = ProblemInstance.Create(Problem.Matrix, Problem.PrimaryColumnCount);
            return pi.Search().ToList();
        }
    }
}
