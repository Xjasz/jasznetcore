using JaszCore.Common;
using JaszCore.Models;
using JaszCore.Models.Outer;
using JaszCore.Objects;
using JaszCore.Services;
using JaszCore.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using static JaszCore.Models.DatabaseAttribute;

namespace JaszCore.Databases
{
    public class JaszOuter : DbContext
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        private DbSet<ConfigHistory> ConfigHistorys { get; set; }

        internal JaszOuter(DbContextOptions optionsBuilder) : base(optionsBuilder)
        {
            Log.Debug($"JaszOuter starting....");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Log.Debug($"JaszOuter creating models....");
            modelBuilder.Entity<ConfigHistory>().ToTable(ConfigHistory.GetDestinationObject());
        }

        internal void BeginTransaction<T>() where T : class
        {
            Database.BeginTransaction();
        }

        internal void CommitTransaction<T>(bool handleIdentity = false) where T : class
        {
            if (handleIdentity)
            {
                var tableName = (typeof(T)?.GetCustomAttributes(typeof(OrgTableAttribute), false)?.FirstOrDefault() as OrgTableAttribute).Name;
                if (tableName == null)
                    throw new ApplicationException($"Type Error OrgTableAttribute is missing.... OrgTableAttribute must exist in model!!");
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[" + tableName + "] ON;");
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[" + tableName + "] OFF;");
            }
            else
            {
                SaveChanges();
            }
            Database.CommitTransaction();
        }

        internal int Save<T>(T entity, bool force = false, bool update = false) where T : class
        {
            var ID = GetObjectID(entity);
            if (force)
            {
                Set<T>().Add(entity);
                return 1;
            }
            else if (update && ID != 0)
            {
                Set<T>().Update((T)entity);
                SaveChanges();
                return 2;
            }
            else if (ID == 0)
            {
                return TrueUpdate(entity);
            }
            else
            {
                Set<T>().Add(entity);
                return 1;
            }
        }

        internal void Delete<T>(T entity) where T : class
        {
            var ID = GetObjectID(entity);
            if (ID != 0)
            {
                Set<T>().Remove(entity);
            }
        }

        internal T Find<T>(T entity) where T : class
        {
            var ID = GetObjectID(entity);
            if (ID != 0)
            {
                var result = Set<IDataModel<T>>().Where(d => d.GetID() == ID).FirstOrDefault();
                return (T)result;
            }
            else
            {
                var query = CreateQuery(entity);
                var result = query.IsValidQuery ? Set<T>().FromSqlRaw(query.QueryString, query.QueryParams).FirstOrDefault() : default;
                return result;
            }
        }

        internal T FindLast<T>() where T : class
        {
            var result = Set<T>()?.ToArray()?.LastOrDefault();
            return result;
        }

        internal IList<T> FindList<T>(T entity) where T : class
        {
            var ID = GetObjectID(entity);
            if (ID != 0)
            {
                var results = Set<IDataModel<T>>().Where(d => d.GetID() == ID).ToList();
                return (IList<T>)results;
            }
            else
            {
                var query = CreateQuery(entity);
                var results = query.IsValidQuery ? Set<T>()?.FromSqlRaw(query.QueryString, query.QueryParams)?.ToList() : default;
                return results;
            }
        }

        internal IList<T> GetAll<T>() where T : class
        {
            IQueryable<T> query = Set<T>();
            var results = query.AsNoTracking().ToList();
            return results;
        }

        internal IList<T> GetAll<T>(string[] includes) where T : class
        {
            IQueryable<T> query = Set<T>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            var results = query.AsNoTracking().ToList();
            return results;
        }

        internal int GetObjectID<T>(T entity) where T : class
        {
            var model = entity as IDataModel<T>;
            var intId = model?.GetID();
            return intId == null ? 0 : (int)intId;
        }

        internal QueryObject CreateQuery<T>(T entity) where T : class
        {
            var propertyInfos = entity.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(ColumnAttribute), true).Any());
            var tableName = (entity.GetType().GetCustomAttributes(typeof(OrgTableAttribute), false).First() as OrgTableAttribute).Name;
            string sqlQuery = "SELECT * FROM " + tableName + " WHERE ";
            var paramList = new List<object>();
            var paramCount = 0;
            //Generate Columns
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                var dbAttr = propertyInfo.GetCustomAttributes(typeof(DatabaseAttribute), true).FirstOrDefault() as DatabaseAttribute;
                if (dbAttr?.DatabaseType == DATABASE_TYPE.LOOKUP)
                {
                    var value = propertyInfo.GetValue(entity);
                    if (value != null && !StringUtils.IsEmpty(value.ToString()) && propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), true).First() is ColumnAttribute column)
                    {
                        sqlQuery += column.Name + " = {" + paramCount + "} AND ";
                        paramList.Add(value.ToString());
                        paramCount++;
                    }
                }
            }
            if (!paramList.IsEmpty())
            {
                sqlQuery = sqlQuery[0..^5];
                var queryObject = new QueryObject(sqlQuery, paramList.ToArray());
                return queryObject;
            }
            return default;
        }

        internal void BulkInsert<T>(IEnumerable<T> entities) where T : class
        {
            var list = entities as IList<T> ?? entities.ToList();
            if (!list.Any()) return;
            Set<T>().AddRange(list);
            SaveChanges();
        }

        internal void BulkUpdate<T>(IEnumerable<T> entities) where T : class
        {
            var list = entities as IList<T> ?? entities.ToList();
            if (!list.Any()) return;
            Set<T>().UpdateRange(list);
            SaveChanges();
        }

        internal void BulkDelete<T>(IEnumerable<T> entities) where T : class
        {
            var list = entities as IList<T> ?? entities.ToList();
            if (!list.Any()) return;
            Set<T>().RemoveRange(list);
            SaveChanges();
        }

        internal int TrueUpdate<T>(T entity) where T : class
        {
            var query = CreateQuery(entity);
            var result = query != null && query.IsValidQuery ? Set<T>().FromSqlRaw(query.QueryString, query.QueryParams).FirstOrDefault() : null;
            var dataModel = result == null ? null : (IDataModel<T>)result;
            if (dataModel == null)
            {
                Set<T>().Add(entity);
                return 1;
            }
            else if (!dataModel.CompareDataModel(entity))
            {
                dataModel.UpdateDataModel(entity);
                Set<T>().Update((T)dataModel);
                return 2;
            }
            return 0;
        }
    }
}
