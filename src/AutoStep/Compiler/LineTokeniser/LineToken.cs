namespace AutoStep
{
    public struct LineToken
    {
        public LineToken(int startPosition, LineTokenCategory category, LineTokenSubCategory subCategory = default)
        {
            StartPosition = startPosition;
            Category = category;
            SubCategory = subCategory;
        }

        public int StartPosition { get; }

        public LineTokenCategory Category { get; }

        public LineTokenSubCategory SubCategory { get; }
    }
}
