namespace JaszCore.Objects
{
    public class TableObject
    {
        public string TableName { get; }
        public object[] Data { get; }

        public TableObject(string tablename, object[] data)
        {
            TableName = tablename;
            Data = data;
        }
    }
}
