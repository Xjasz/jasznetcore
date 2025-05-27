using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace JaszCore.Models
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OrgTableAttribute : TableAttribute
    {
        public string TestName;
        public CONNECTION_TYPE ConnectionType { get; } = CONNECTION_TYPE.JASZ_MAIN;
        public enum CONNECTION_TYPE { JASZ_MAIN, JASZ_OUTER, NONE }
        public OrgTableAttribute(string name, CONNECTION_TYPE connectionType, string testName) : base(name) { ConnectionType = ConnectionType; TestName = testName; }
        public OrgTableAttribute(string name, CONNECTION_TYPE connectionType) : base(name) { ConnectionType = ConnectionType; TestName = name; }
        public OrgTableAttribute(string name) : base(name) { }
    }
}
