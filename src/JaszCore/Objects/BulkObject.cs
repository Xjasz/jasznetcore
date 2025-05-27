using System.Collections.Generic;

namespace JaszCore.Objects
{
    public class BulkObject
    {
        public string SourceTable { get; }
        public string DestinationTable { get; }
        public string SourceStatement { get; }
        public Dictionary<string, string> SourceArguments { get; }
        public string[] SourceTableColumns { get; set; }
        public bool ClearDestinationTable { get; }
        public Dictionary<string, int> DestinationTableColumns { get; set; }

        public List<object[]> Items = new List<object[]>();

        public BulkObject(string sourceTable, string destinationTable, string sourceStatement, Dictionary<string, string> sourceArguments, Dictionary<string, int> destinationTableColumns)
        {
            SourceTable = sourceTable;
            DestinationTable = destinationTable;
            SourceStatement = sourceStatement;
            SourceArguments = sourceArguments;
            DestinationTableColumns = destinationTableColumns;
        }
        public BulkObject(string sourceTable, string destinationTable, string sourceStatement, Dictionary<string, string> sourceArguments, bool clearDestinationTable)
        {
            SourceTable = sourceTable;
            DestinationTable = destinationTable;
            SourceStatement = sourceStatement;
            SourceArguments = sourceArguments;
            ClearDestinationTable = clearDestinationTable;
        }
        public BulkObject(string sourceTable, string destinationTable, string sourceStatement, Dictionary<string, string> sourceArguments)
        {
            SourceTable = sourceTable;
            DestinationTable = destinationTable;
            SourceStatement = sourceStatement;
            SourceArguments = sourceArguments;
        }
        public BulkObject(string sourceTable, string sourceStatement, Dictionary<string, string> sourceArguments)
        {
            SourceTable = sourceTable;
            SourceStatement = sourceStatement;
            SourceArguments = sourceArguments;
        }
        public BulkObject(string sourceStatement, Dictionary<string, string> sourceArguments)
        {
            SourceStatement = sourceStatement;
            SourceArguments = sourceArguments;
        }
        public BulkObject(string sourceStatement)
        {
            SourceStatement = sourceStatement;
        }
        public BulkObject() { }
    }
}
