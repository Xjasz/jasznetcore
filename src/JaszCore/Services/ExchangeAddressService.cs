using JaszCore.Common;
using JaszCore.Models;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace JaszCore.Services
{
    [Service(typeof(ExchangeAddressService))]
    public interface IExchangeAddressService
    {
        void SendTestEmail();
        IEnumerable<AddressList> GetGlobalAddressLists();
        IEnumerable<AddressList> GetAllAddressLists();
        IEnumerable<AddressList> GetSystemAddressLists();
    }
    public class ExchangeAddressService : IExchangeAddressService
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        private static ExchangeService ExchangeService;

        private readonly ActiveDirectoryConnection _ActiveDirectoryConnection;

        internal ExchangeAddressService()
        {
            Log.Debug($"ExchangeAddressService starting....");
        }

        internal ExchangeAddressService(ActiveDirectoryConnection connection)
        {
            Log.Debug($"ExchangeAddressService params starting....");
            if (connection == null) throw new ArgumentNullException("connection");
            _ActiveDirectoryConnection = connection;
        }

        public ExchangeService getExchangeService()
        {
            Log.Debug($"getExchangeService....");
            ExchangeService = new ExchangeService(ExchangeVersion.Exchange2013_SP1);
            return ExchangeService;
        }

        public void SendTestEmail()
        {
            Log.Debug($"SendTestEmail email....");
        }

        public IEnumerable<AddressList> GetGlobalAddressLists()
        {
            return GetAddressLists("CN=All Global Address Lists");
        }

        public IEnumerable<AddressList> GetAllAddressLists()
        {
            return GetAddressLists("CN=All Address Lists");
        }
        public IEnumerable<AddressList> GetSystemAddressLists()
        {
            return GetAddressLists("CN=All System Address Lists");
        }

        private IEnumerable<AddressList> GetAddressLists(string containerName)
        {
            string exchangeRootPath;
            using (var root = _ActiveDirectoryConnection.GetLdapDirectoryEntry("RootDSE"))
            {
                exchangeRootPath = string.Format("CN=Microsoft Exchange, CN=Services, {0}", root.Properties["configurationNamingContext"].Value);
            }
            string companyRoot;
            using (var exchangeRoot = _ActiveDirectoryConnection.GetLdapDirectoryEntry(exchangeRootPath))
            using (var searcher = new DirectorySearcher(exchangeRoot, "(objectclass=msExchOrganizationContainer)"))
            {
                companyRoot = (string)searcher.FindOne().Properties["distinguishedName"][0];
            }

            var globalAddressListPath = string.Format(containerName + ",CN=Address Lists Container, {0}", companyRoot);
            var addressListContainer = _ActiveDirectoryConnection.GetLdapDirectoryEntry(globalAddressListPath);

            using (var searcher = new DirectorySearcher(addressListContainer, "(objectClass=addressBookContainer)"))
            {
                searcher.SearchScope = SearchScope.OneLevel;
                using (var searchResultCollection = searcher.FindAll())
                {
                    foreach (SearchResult addressBook in searchResultCollection)
                    {
                        yield return
                            new AddressList((string)addressBook.Properties["distinguishedName"][0], _ActiveDirectoryConnection);
                    }
                }
            }
        }
    }

    public class ActiveDirectoryConnection
    {
        public DirectoryEntry GetLdapDirectoryEntry(string path)
        {
            return GetDirectoryEntry(path, "LDAP");
        }

        public DirectoryEntry GetGCDirectoryEntry(string path)
        {
            return GetDirectoryEntry(path, "GC");
        }

        private DirectoryEntry GetDirectoryEntry(string path, string protocol)
        {
            var ldapPath = string.IsNullOrEmpty(path) ? string.Format("{0}:", protocol) : string.Format("{0}://{1}", protocol, path);
            return new DirectoryEntry(ldapPath);
        }
    }

    public class AddressList
    {
        private readonly ActiveDirectoryConnection _ActiveDirectoryConnection;
        private readonly string _Path;

        private DirectoryEntry _DirectoryEntry;

        internal AddressList(string path, ActiveDirectoryConnection connection)
        {
            _Path = path;
            _ActiveDirectoryConnection = connection;
        }

        private DirectoryEntry DirectoryEntry
        {
            get
            {
                if (_DirectoryEntry == null)
                {
                    _DirectoryEntry = _ActiveDirectoryConnection.GetLdapDirectoryEntry(_Path);
                }
                return _DirectoryEntry;
            }
        }

        public string Name
        {
            get { return (string)DirectoryEntry.Properties["name"].Value; }
        }

        public IEnumerable<SearchResult> GetMembers(params string[] propertiesToLoad)
        {
            var rootDse = _ActiveDirectoryConnection.GetGCDirectoryEntry(string.Empty);
            var searchRoot = rootDse.Children.Cast<DirectoryEntry>().First();
            using (var searcher = new DirectorySearcher(searchRoot, string.Format("(showInAddressBook={0})", _Path)))
            {
                if (propertiesToLoad != null)
                {
                    searcher.PropertiesToLoad.AddRange(propertiesToLoad);
                }
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PageSize = 512;
                do
                {
                    using (var result = searcher.FindAll())
                    {
                        foreach (SearchResult searchResult in result)
                        {
                            yield return searchResult;
                        }
                        if (result.Count < 512) break;
                    }
                } while (true);
            }
        }
    }
}