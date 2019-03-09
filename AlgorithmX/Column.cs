namespace AlgorithmX
{
    internal class Column : Entry
    {
        public int RowCount { get; private set; }

        public new Column Left
        {
            get => (Column)base.Left;
            set => base.Left = value;
        }

        public new Column Right
        {
            get => (Column)base.Right;
            set => base.Left = value;
        }

        public void AppendEntry(Entry entry)
        {
            AppendToColumn(entry);
            ++RowCount;
        }

        public void Cover()
        {
            UnlinkFromRow();

            for (var i = Down; i != this; i = i.Down)
            {
                for (var j = i.Right; j != i; j = j.Right)
                {
                    j.UnlinkFromColumn();
                    --j.Column.RowCount;
                }
            }
        }

        public void Uncover()
        {
            for (var i = Up; i != this; i = i.Up)
            {
                for (var j = i.Left; j != i; j = j.Left)
                {
                    ++j.Column.RowCount;
                    j.RelinkIntoColumn();
                }
            }

            RelinkIntoRow();
        }
    }
}
