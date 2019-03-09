using System;

namespace AlgorithmX
{
    public class Solution
    {
        public int[] RowIndices { get; }

        public Solution(int[] solution)
        {
            RowIndices = solution ?? throw new ArgumentNullException(nameof(solution));
            Array.Sort(RowIndices);
        }
    }
}
