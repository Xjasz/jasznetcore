using JaszCore.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using static JaszCore.Models.DatabaseAttribute;

namespace JaszCore.Models
{
    public abstract class BaseDataModel<T> : IDataModel<T> where T : class
    {
        protected BaseDataModel() { }

        public virtual int GetID()
        {
            return 0;
        }

        public static string GetDestinationObject()
        {
            var derivedType = typeof(T);
            if (S.AppMode == S.APP_MODE.PROD)
                return (derivedType.GetCustomAttributes(typeof(OrgTableAttribute), true).FirstOrDefault() as OrgTableAttribute).Name;
            if (S.AppMode == S.APP_MODE.DEV)
                return (derivedType.GetCustomAttributes(typeof(OrgTableAttribute), true).FirstOrDefault() as OrgTableAttribute).Name;
            throw new System.NotImplementedException();
        }

        public void UpdateDataModel(T entity)
        {
            var properties = entity.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var column = property.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;
                var dbAttr = property.GetCustomAttributes(typeof(DatabaseAttribute), true).FirstOrDefault() as DatabaseAttribute;
                if (column == null || dbAttr?.DatabaseType == DATABASE_TYPE.NONE)
                {
                    continue;
                }
                var currentValue = property.GetValue(entity);
                var previousValue = property.GetValue(this);
                if (currentValue?.ToString() != previousValue?.ToString())
                {
                    property.SetValue(this, currentValue);
                }
            }
        }

        public bool CompareDataModel(T entity)
        {
            var match = true;
            var properties = entity.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var column = property.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;
                var dbAttr = property.GetCustomAttributes(typeof(DatabaseAttribute), true).FirstOrDefault() as DatabaseAttribute;
                if (column == null || dbAttr?.DatabaseType == DATABASE_TYPE.NONE)
                {
                    continue;
                }
                var currentValue = property.GetValue(entity);
                var previousValue = property.GetValue(this);
                if (currentValue?.ToString() != previousValue?.ToString())
                {
                    match = false;
                    break;
                }
            }
            return match;
        }

        public Dictionary<string, int> GetMergedEntity()
        {
            var mergedItems = new Dictionary<string, int>();
            var position = 0;
            var properties = this.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(ColumnAttribute), true).Any());
            foreach (PropertyInfo property in properties)
            {
                var column = property.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;
                var dbAttr = property.GetCustomAttributes(typeof(DatabaseAttribute), true).FirstOrDefault() as DatabaseAttribute;
                var tpAttr = property.GetCustomAttributes(typeof(ThirdPartyAttribute), true).FirstOrDefault() as ThirdPartyAttribute;
                if (column == null)
                {
                    continue;
                }
                if (dbAttr == null && tpAttr == null)
                {
                    continue;
                }
                mergedItems.Add(column.Name, position++);
            }
            return mergedItems;
        }

        Dictionary<string, int> IDataModel<T>.GetMergedEntity()
        {
            throw new System.NotImplementedException();
        }
    }
}
