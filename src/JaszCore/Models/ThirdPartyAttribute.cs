using System;
using static JaszCore.Models.ThirdPartyAttribute;

namespace JaszCore.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ThirdPartyAttribute : Attribute
    {
        public TPA_TYPE ThirdPartyType { get; } = TPA_TYPE.NONE;
        public string ThirdPartyAttributeName { get; }
        public enum TPA_TYPE { THIRD_PARTY_API, NONE }
        public ThirdPartyAttribute(string thirdpartyAttributeName, TPA_TYPE tpaType) { ThirdPartyAttributeName = thirdpartyAttributeName; ThirdPartyType = tpaType; }
        public ThirdPartyAttribute(string thirdpartyAttributeName) { ThirdPartyType = TPA_TYPE.THIRD_PARTY_API; ThirdPartyAttributeName = thirdpartyAttributeName; }
        public ThirdPartyAttribute(TPA_TYPE tpaType) { ThirdPartyType = tpaType; }
        public ThirdPartyAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ThirdPartyObject : Attribute
    {
        public TPA_TYPE ThirdPartyType { get; } = TPA_TYPE.NONE;
        public string ThirdPartyObjectName { get; }
        public string GetThirdPartyBasicRequest()
        {
            if (ThirdPartyType == TPA_TYPE.THIRD_PARTY_API)
                return null;
            throw new ApplicationException($"Request Type Error {ThirdPartyType} is invalid....  ThirdPartyType must be a valid ThirdPartyType.TPA_TYPE");
        }
        public ThirdPartyObject(string thirdpartyObjectName) { ThirdPartyType = TPA_TYPE.THIRD_PARTY_API; ThirdPartyObjectName = thirdpartyObjectName; }
    }
}
