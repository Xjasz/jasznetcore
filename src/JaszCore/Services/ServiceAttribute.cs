using System;

namespace JaszCore.Models
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute(Type type)
        {
            AttributeType = type;
            Singleton = true;
        }

        public ServiceAttribute(Type type, bool singleton)
        {
            AttributeType = type;
            Singleton = singleton;
        }

        public Type AttributeType { get; private set; }
        public bool Singleton { get; private set; }
    }
}
