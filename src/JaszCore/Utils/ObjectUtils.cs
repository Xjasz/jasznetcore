using Microsoft.Data.SqlClient;

namespace JaszCore.Utils
{
    public static class ObjectUtils
    {
        public static object SafeGetObject(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
            {
                return reader.GetValue(colIndex);
            }
            return null;
        }
    }
}
