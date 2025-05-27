using System;

namespace JaszCore.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DatabaseAttribute : Attribute
    {
        public DATABASE_TYPE DatabaseType { get; } = DATABASE_TYPE.COMPARE;
        public string SourceColumnName { get; }
        public string SourceColumnAltName { get; }
        public enum DATABASE_TYPE { LOOKUP, COMPARE, ALL, NONE }
        public DatabaseAttribute(string sourceColumnName, string sourceColumnAltName, DATABASE_TYPE databaseType) { SourceColumnName = sourceColumnName; SourceColumnAltName = sourceColumnAltName; DatabaseType = databaseType; }
        public DatabaseAttribute(string sourceColumnName, DATABASE_TYPE databaseType) { SourceColumnName = sourceColumnName; DatabaseType = databaseType; }
        public DatabaseAttribute(string sourceColumnName) { SourceColumnName = sourceColumnName; }
        public DatabaseAttribute(DATABASE_TYPE databaseType) { DatabaseType = databaseType; }
        public DatabaseAttribute() { }
    }
}
