using JaszCore.Utils;

namespace JaszCore.Objects
{
    public class QueryObject
    {
        public string QueryString { get; }

        public object[] QueryParams { get; }

        public bool IsValidQuery = false;

        public QueryObject(string queryString, object[] queryParams)
        {
            QueryString = queryString;
            QueryParams = queryParams;

            QueryParams = queryParams;
            if (!queryString.IsEmpty() && !queryParams.IsEmpty())
            {
                IsValidQuery = true;
            }
        }
    }
}
