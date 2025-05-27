using System.Collections.Generic;

namespace JaszCore.Objects
{
    public class RequestObject
    {
        public string RequestUrl { get; }
        public string RequestBody { get; }
        public string ResponseModel { get; }
        public Dictionary<string, string> ResponseMapper { get; }

        public string ResponseBody;
        public Dictionary<string, int> DestinationTableColumns { get; set; }

        public List<object[]> Items = new List<object[]>();

        public RequestObject(string requestUrl, string requestBody, string responseModel, Dictionary<string, string> responseMapper)
        {
            RequestUrl = requestUrl;
            RequestBody = requestBody;
            ResponseModel = responseModel;
            ResponseMapper = responseMapper;
        }
        public RequestObject() { }
    }
}
