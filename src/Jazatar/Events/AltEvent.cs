using JaszCore.Common;
using JaszCore.Events;
using JaszCore.Services;
using System;

namespace Jazatar.Events
{
    public class AltEvent : IEvent
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();
        private static IExchangeAddressService ExchangeAddressService => ServiceLocator.Get<IExchangeAddressService>();

        public void StartEvent(string[] args = null)
        {
            Log.Debug($"AltEvent starting....");
            RunAltEvent();
            Log.Debug($"AltEvent finished....");
        }

        public void RunAltEvent()
        {
            Log.Debug($"RunAltEvent starting....");
            foreach (var addressList in ExchangeAddressService.GetGlobalAddressLists())
            {
                Console.Out.WriteLine("addressList.Name = {0}", addressList.Name);
                foreach (var searchResult in addressList.GetMembers())
                {
                    Console.Out.WriteLine("\t{0}", searchResult.Properties["name"][0]);
                }
            }

            foreach (var addressList in ExchangeAddressService.GetAllAddressLists())
            {
                Console.Out.WriteLine("addressList.Name = {0}", addressList.Name);
                foreach (var searchResult in addressList.GetMembers())
                {
                    Console.Out.WriteLine("\t{0}", searchResult.Properties["name"][0]);
                }
            }

            foreach (var addressList in ExchangeAddressService.GetSystemAddressLists())
            {
                Console.Out.WriteLine("addressList.Name = {0}", addressList.Name);
                foreach (var searchResult in addressList.GetMembers())
                {
                    Console.Out.WriteLine("\t{0}", searchResult.Properties["name"][0]);
                }
            }
            Log.Debug($"RunAltEvent finished....");
        }
    }
}
