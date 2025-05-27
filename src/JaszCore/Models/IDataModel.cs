using System.Collections.Generic;

namespace JaszCore.Models
{
    public interface IDataModel<T> where T : class
    {
        int GetID();
        void UpdateDataModel(T entity);
        bool CompareDataModel(T entity);
        Dictionary<string, int> GetMergedEntity();
    }
}
