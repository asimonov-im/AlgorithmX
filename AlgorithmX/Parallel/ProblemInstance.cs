using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AlgorithmX.Parallel
{
    public class ProblemInstance
    {
        private const int RootId = 0;

        private DoublingArray<Entry> Entries { get; }
            = new DoublingArray<Entry>();

        private DoublingArray<int> ColumnSizes { get; }
            = new DoublingArray<int>();

        // Stored outside of Entry since they do not change.
        private Dictionary<ushort, ushort> EntryIdToColumnId { get; }
            = new Dictionary<ushort, ushort>();

        // Stored outside of Entry since they do not change.
        private Dictionary<ushort, int> EntryIdToRowNumber { get; }
            = new Dictionary<ushort, int>();

        private List<int> Solution { get; }
            = new List<int>();

        public ConcurrentQueue<Solution> Solutions { get; }
            = new ConcurrentQueue<Solution>();

        private ProblemInstance()
        {
            // Create root entry
            NewEntry();
        }

        /// <summary>
        /// Constructor used for cloning the ProblemInstance.
        /// </summary>
        public ProblemInstance(ProblemInstance other)
        {
            // Require a deep copy, since these fields will change during
            // the search for the exact cover.
            Entries = new DoublingArray<Entry>(other.Entries);
            ColumnSizes = new DoublingArray<int>(other.ColumnSizes);
            Solution = new List<int>(other.Solution);

            // Shallow copy, since the mappings are constant after construction
            EntryIdToColumnId = other.EntryIdToColumnId;
            EntryIdToRowNumber = other.EntryIdToRowNumber;

            // Shallow copy, by design of the ConcurrentCollection
            Solutions = other.Solutions;
        }

        public async Task Search(int k)
        {
            if (k < 0)
            {
                throw new ArgumentException("The branching factor must be positive.");
            }
            else if (k == 0)
            {
                RecursiveSearch();
            }
            else
            {
                await ParallelSearch(k);
            }
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

            var result = new ProblemInstance();

            // Get row/column counts for convenience
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            primaryColumnCount = primaryColumnCount ?? colCount;

            // Create all column headers, so that they are in a contiguous block of the
            // Entries array, starting at index 1.
            for (int c = 0; c < colCount; ++c)
            {
                ushort columnId = result.CreateColumnHeader();

                // Do not link secondary columns into the header
                if (c < primaryColumnCount)
                {
                    result.AppendToRow(RootId, columnId);
                }
            }

            for (int r = 0; r < rowCount; ++r)
            {
                ushort? firstEntryId = null;
                for (int c = 0; c < colCount; ++c)
                {
                    if (predicate(matrix[r, c]))
                    {
                        ushort headerId = (ushort)(c + 1);
                        ushort entryId = result.CreateEntry(headerId, r);

                        if (firstEntryId.HasValue)
                        {
                            result.AppendToRow(firstEntryId.Value, entryId);
                        }
                        else
                        {
                            firstEntryId = entryId;
                        }
                    }
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort LeftId(ushort id) => Entries[id].LeftId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort RightId(ushort id) => Entries[id].RightId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort UpId(ushort id) => Entries[id].UpId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort DownId(ushort id) => Entries[id].DownId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort ColumnId(ushort id) => EntryIdToColumnId[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int RowId(ushort id) => EntryIdToRowNumber[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref int ColumnSize(ushort columnId) => ref ColumnSizes[columnId - 1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Entry Entry(ushort id) => ref Entries[id];

        private void UnlinkFromColumn(ushort id)
        {
            Entry(DownId(id)).UpId = UpId(id);
            Entry(UpId(id)).DownId = DownId(id);
        }

        private void RelinkIntoColumn(ushort id)
        {
            Entry(DownId(id)).UpId = id;
            Entry(UpId(id)).DownId = id;
        }

        private void UnlinkFromRow(ushort id)
        {
            Entry(RightId(id)).LeftId = LeftId(id);
            Entry(LeftId(id)).RightId = RightId(id);
        }

        private void RelinkIntoRow(ushort id)
        {
            Entry(RightId(id)).LeftId = id;
            Entry(LeftId(id)).RightId = id;
        }

        private void CoverColumn(ushort columnId)
        {
            UnlinkFromRow(columnId);

            for (var i = DownId(columnId); i != columnId; i = DownId(i))
            {
                for (var j = RightId(i); j != i; j = RightId(j))
                {
                    UnlinkFromColumn(j);
                    --ColumnSize(ColumnId(j));
                }
            }
        }

        private void UncoverColumn(ushort columnId)
        {
            for (var i = UpId(columnId); i != columnId; i = UpId(i))
            {
                for (var j = LeftId(i); j != i; j = LeftId(j))
                {
                    ++ColumnSize(ColumnId(j));
                    RelinkIntoColumn(j);
                }
            }

            RelinkIntoRow(columnId);
        }

        private ushort CreateColumnHeader()
        {
            ushort columnId = NewEntry();
            ColumnSizes.Append(0);
            return columnId;
        }

        private ushort CreateEntry(ushort headerId, int rowNumber)
        {
            if (headerId <= RootId)
            {
                throw new ArgumentException($"Invalid {nameof(headerId)}.");
            }
            else if (rowNumber < 0)
            {
                throw new ArgumentException($"Invalid {nameof(rowNumber)}.");
            }

            ushort entryId = NewEntry();

            // Link entry into the specified column
            AppendToColumn(headerId, entryId);
            EntryIdToColumnId[entryId] = headerId;
            ++ColumnSize(headerId);

            // Record row for the entry
            EntryIdToRowNumber[entryId] = rowNumber;

            return entryId;
        }

        private ushort NewEntry()
        {
            ushort id = Convert.ToUInt16(Entries.Count);
            Entries.Append(new Entry(id));
            return id;
        }

        private void AppendToRow(ushort id, ushort idToAppend)
        {
            Entry(LeftId(id)).RightId = idToAppend;
            Entry(idToAppend).RightId = id;
            Entry(idToAppend).LeftId = LeftId(id);
            Entry(id).LeftId = idToAppend;
        }

        private void AppendToColumn(ushort columnId, ushort idToAppend)
        {
            Entry(UpId(columnId)).DownId = idToAppend;
            Entry(idToAppend).DownId = columnId;
            Entry(idToAppend).UpId = UpId(columnId);
            Entry(columnId).UpId = idToAppend;
        }

        private bool RootIsEmpty() => RightId(RootId) == RootId;

        private void RecordSolution()
        {
            if (Solution.Count > 0)
            {
                Solutions.Enqueue(new Solution(Solution.ToArray()));
            }
        }

        private ushort SelectColumnIdWithFewestOnes()
        {
            ushort? bestId = null;
            for (var c = RightId(RootId); c != RootId; c = RightId(c))
            {
                if (bestId == null || ColumnSize(c) < ColumnSize(bestId.Value))
                {
                    bestId = c;
                }
            }

            return bestId.Value;
        }

        private async Task ParallelSearch(int k)
        {
            if (RootIsEmpty())
            {
                RecordSolution();
            }
            else
            {
                var c = SelectColumnIdWithFewestOnes();
                CoverColumn(c);

                var tasks = new List<Task>(ColumnSize(c));
                for (var r = DownId(c); r != c; r = DownId(r))
                {
                    var problemCopy = new ProblemInstance(this);
                    problemCopy.Solution.Add(RowId(r));

                    for (var j = RightId(r); j != r; j = RightId(j))
                    {
                        problemCopy.CoverColumn(ColumnId(j));
                    }

                    if (k > 0)
                    {
                        Task task = Task.Run(async () => await problemCopy.ParallelSearch(k - 1).ConfigureAwait(false));
                        tasks.Add(task);
                    }
                    else
                    {
                        problemCopy.RecursiveSearch();
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private void RecursiveSearch()
        {
            if (RootIsEmpty())
            {
                RecordSolution();
            }
            else
            {
                var c = SelectColumnIdWithFewestOnes();
                CoverColumn(c);

                for (var r = DownId(c); r != c; r = DownId(r))
                {
                    Solution.Add(RowId(r));

                    for (var j = RightId(r); j != r; j = RightId(j))
                    {
                        CoverColumn(ColumnId(j));
                    }

                    RecursiveSearch();

                    for (var j = LeftId(r); j != r; j = LeftId(j))
                    {
                        UncoverColumn(ColumnId(j));
                    }

                    Solution.RemoveLast();
                }

                UncoverColumn(c);
            }
        }

        private static bool DefaultPredicate<T>(T obj)
            => !EqualityComparer<T>.Default.Equals(obj, default(T));
    }
}
