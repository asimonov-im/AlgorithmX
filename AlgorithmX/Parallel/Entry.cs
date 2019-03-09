namespace AlgorithmX.Parallel
{
    struct Entry
    {
        public ushort LeftId;
        public ushort RightId;
        public ushort UpId;
        public ushort DownId;

        public Entry(ushort id)
        {
            LeftId = RightId = UpId = DownId = id;
        }
    }
}
