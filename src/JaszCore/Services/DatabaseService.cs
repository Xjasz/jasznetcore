using JaszCore.Common;
using JaszCore.Databases;
using JaszCore.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JaszCore.Services
{
    [Service(typeof(DatabaseService))]
    public interface IDatabaseService
    {
        SqlConnection GetMainConnection();
        SqlConnection GetOuterConnection();
        int Save<T>(T entity, bool force = false) where T : class;
        void Save<T>(IEnumerable<T> entities, bool force = false) where T : class;
        void Delete<T>(T entity) where T : class;
        T Find<T>(T entity) where T : class;
        T FindLast<T>() where T : class;
        IList<T> FindList<T>(T entity) where T : class;
        IList<T> GetAll<T>() where T : class;
        IList<T> GetAll<T>(string[] includes) where T : class;
        void BeginTransaction<T>() where T : class;
        void CommitTransaction<T>() where T : class;
        void BulkInsert<T>(IEnumerable<T> items) where T : class;
        void BulkUpdate<T>(IEnumerable<T> items) where T : class;
        void BulkDelete<T>(IEnumerable<T> items) where T : class;
    }

    public class DatabaseService : IDatabaseService
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        private readonly JaszMain JaszMain;
        private readonly SqlConnection MainConnection;
        private readonly JaszOuter JaszOuter;
        private readonly SqlConnection OuterConnection;
        private const int BulkThreshold = 50;

        internal DatabaseService(params string[] args)
        {
            Log.Debug("DatabaseService starting....");
            if (args.Length > 0 && args[0].Length > 10)
            {
                MainConnection = new SqlConnection(args[0]);
                var optionsMain = new DbContextOptionsBuilder<JaszMain>().UseSqlServer(MainConnection).Options;
                JaszMain = new JaszMain(optionsMain);
            }
            if (args.Length > 1 && args[1].Length > 10)
            {
                OuterConnection = new SqlConnection(args[1]);
                var optionsOuter = new DbContextOptionsBuilder<JaszOuter>().UseSqlServer(OuterConnection).Options;
                JaszOuter = new JaszOuter(optionsOuter);
            }
        }

        public SqlConnection GetMainConnection() => MainConnection;
        public SqlConnection GetOuterConnection() => OuterConnection;

        public T FindLast<T>() where T : class => JaszMain.FindLast<T>();
        public IList<T> FindList<T>(T entity) where T : class => JaszMain.FindList(entity);
        public IList<T> GetAll<T>() where T : class => JaszMain.GetAll<T>();
        public IList<T> GetAll<T>(string[] includes) where T : class => JaszMain.GetAll<T>(includes);

        public T Find<T>(T entity) where T : class
        {
            var connType = ResolveConnection<T>();
            return connType == OrgTableAttribute.CONNECTION_TYPE.JASZ_MAIN ? JaszMain.Find(entity) : JaszOuter.Find(entity);
        }

        public int Save<T>(T entity, bool force = false) where T : class
        {
            var connType = ResolveConnection<T>();
            return connType == OrgTableAttribute.CONNECTION_TYPE.JASZ_MAIN ? JaszMain.Save(entity, force) : JaszOuter.Save(entity, force);
        }

        public void Save<T>(IEnumerable<T> entities, bool force = false) where T : class
        {
            var list = entities as IList<T> ?? entities.ToList();
            if (!force && list.Count >= BulkThreshold)
            {
                var inserts = list.Where(e => GetId(e) == 0).ToList();
                var updates = list.Where(e => GetId(e) != 0).ToList();
                if (inserts.Any()) BulkInsert(inserts);
                if (updates.Any()) BulkUpdate(updates);
            }
            else
            {
                foreach (var item in list)
                    Save(item, force);
            }
        }

        public void Delete<T>(T entity) where T : class
        {
            var connType = ResolveConnection<T>();
            if (connType == OrgTableAttribute.CONNECTION_TYPE.JASZ_MAIN)
                JaszMain.Delete(entity);
            else
                JaszOuter.Delete(entity);
        }

        public void BeginTransaction<T>() where T : class
        {
            var connType = ResolveConnection<T>();
            if (connType == OrgTableAttribute.CONNECTION_TYPE.JASZ_MAIN)
                JaszMain.BeginTransaction<T>();
            else
                JaszOuter.BeginTransaction<T>();
        }

        public void CommitTransaction<T>() where T : class
        {
            var connType = ResolveConnection<T>();
            if (connType == OrgTableAttribute.CONNECTION_TYPE.JASZ_MAIN)
                JaszMain.CommitTransaction<T>();
            else
                JaszOuter.CommitTransaction<T>();
        }

        public void BulkInsert<T>(IEnumerable<T> items) where T : class
        {
            var connType = ResolveConnection<T>();
            if (connType == OrgTableAttribute.CONNECTION_TYPE.JASZ_MAIN)
                JaszMain.BulkInsert(items);
            else
                JaszOuter.BulkInsert(items);
        }

        public void BulkUpdate<T>(IEnumerable<T> items) where T : class
        {
            var connType = ResolveConnection<T>();
            if (connType == OrgTableAttribute.CONNECTION_TYPE.JASZ_MAIN)
                JaszMain.BulkUpdate(items);
            else
                JaszOuter.BulkUpdate(items);
        }

        public void BulkDelete<T>(IEnumerable<T> items) where T : class
        {
            var connType = ResolveConnection<T>();
            if (connType == OrgTableAttribute.CONNECTION_TYPE.JASZ_MAIN)
                JaszMain.BulkDelete(items);
            else
                JaszOuter.BulkDelete(items);
        }

        private OrgTableAttribute.CONNECTION_TYPE ResolveConnection<T>()
        {
            var attr = typeof(T).GetCustomAttributes(typeof(OrgTableAttribute), false).Cast<OrgTableAttribute>().FirstOrDefault();
            return attr?.ConnectionType ?? OrgTableAttribute.CONNECTION_TYPE.JASZ_MAIN;
        }

        private int GetId<T>(T entity) where T : class
        {
            return (entity as IDataModel<T>)?.GetID() ?? 0;
        }
    }
}