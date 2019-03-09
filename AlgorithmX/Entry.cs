using System;

namespace AlgorithmX
{
    /// <summary>
    /// Class representing a 1 in the problem matrix.
    /// </summary>
    internal class Entry
    {
        public Entry Left { get; protected set; }
        public Entry Right { get; protected set; }
        public Entry Up { get; protected set; }
        public Entry Down { get; protected set; }
        public Column Column { get; }
        public int RowId { get; }

        public Entry(Column column, int rowId)
        {
            Column = column ?? throw new ArgumentNullException(nameof(column));
            RowId = rowId;
            Left = Right = Up = Down = this;
        }

        protected Entry()
        {
            RowId = -1;
            Left = Right = Up = Down = this;
        }

        public void AppendToRow(Entry entry)
        {
            Left.Right = entry;
            entry.Right = this;
            entry.Left = Left;
            Left = entry;
        }

        public void AppendToColumn(Entry entry)
        {
            Up.Down = entry;
            entry.Down = this;
            entry.Up = Up;
            Up = entry;
        }

        public void UnlinkFromColumn()
        {
            Down.Up = Up;
            Up.Down = Down;
        }

        public void RelinkIntoColumn()
        {
            Down.Up = this;
            Up.Down = this;
        }

        public void UnlinkFromRow()
        {
            Right.Left = Left;
            Left.Right = Right;
        }

        public void RelinkIntoRow()
        {
            Right.Left = this;
            Left.Right = this;
        }
    }
}
