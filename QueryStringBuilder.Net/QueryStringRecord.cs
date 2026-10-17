namespace QueryStringBuilder.Net
{
    internal class QueryStringRecord
    {
        public string Key { get; }
        public string Value { get; }

        public QueryStringRecord(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
