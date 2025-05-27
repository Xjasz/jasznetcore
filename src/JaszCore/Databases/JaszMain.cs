using JaszCore.Common;
using JaszCore.Models;
using JaszCore.Models.Main;
using JaszCore.Objects;
using JaszCore.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using static JaszCore.Models.DatabaseAttribute;

namespace JaszCore.Databases
{
    public class JaszMain : DbContext
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        private DbSet<ConfigApplication> ConfigApplications { get; set; }

        internal JaszMain(DbContextOptions optionsBuilder) : base(optionsBuilder)
        {
            Log.Debug($"JaszMain starting....");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Log.Debug($"JaszMain creating models....");
            modelBuilder.Entity<ConfigApplication>().ToTable(ConfigApplication.GetDestinationObject());
            modelBuilder.Entity<XjzApplication>().ToTable(XjzApplication.GetDestinationObject());
            modelBuilder.Entity<XjzKeyMapper>().ToTable(XjzKeyMapper.GetDestinationObject());
        }

        internal void BeginTransaction<T>() where T : class
        {
            Database.BeginTransaction();
        }

        internal void CommitTransaction<T>() where T : class
        {
            SaveChanges();
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
            var props = entity.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(ColumnAttribute), true).Any());
            var tableName = (entity.GetType().GetCustomAttributes(typeof(OrgTableAttribute), false).First() as OrgTableAttribute).Name;
            var sql = $"SELECT * FROM {tableName} WHERE ";
            var parameters = new List<object>();
            int idx = 0;
            foreach (var prop in props)
            {
                var dbAttr = prop.GetCustomAttribute<DatabaseAttribute>();
                var colAttr = prop.GetCustomAttribute<ColumnAttribute>();
                if (dbAttr?.DatabaseType == DATABASE_TYPE.LOOKUP && colAttr != null)
                {
                    var val = prop.GetValue(entity);
                    if (val != null && !string.IsNullOrWhiteSpace(val.ToString()))
                    {
                        sql += $"{colAttr.Name} = {{{idx}}} AND ";
                        parameters.Add(val);
                        idx++;
                    }
                }
            }
            if (!parameters.Any())
                return default;

            sql = sql.Substring(0, sql.Length - 5);
            return new QueryObject(sql, parameters.ToArray());
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
