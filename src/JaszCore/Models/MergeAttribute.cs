using System;

namespace JaszCore.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Merge : Attribute
    {
        public MERGE_TYPE MergeType { get; } = MERGE_TYPE.NONE;
        public string Name { get; }
        public enum MERGE_TYPE { MAIN, NONE, OVERWRITE }
        public Merge(string name, MERGE_TYPE mergeType) { Name = name; MergeType = mergeType; }
        public Merge(string name) { Name = name; }
        public Merge() { }
    }
}
