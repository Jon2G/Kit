using Java.Net;
using Java.Util;
using Kit.Droid.Services;
using Kit.Services.Interfaces;

[assembly: Dependency(typeof(IpAddressService))]
namespace Kit.Droid.Services
{
    class IpAddressService : IIpAddressService
    {
        public string GetIpAddress()
        {
            NetworkInterface[] AllNetworkInterfaces =
                Collections.List(NetworkInterface.NetworkInterfaces).OfType<NetworkInterface>()
                .Where(x => !x.IsVirtual && x.IsUp)
                .ToArray();

            string IPAddres = "";
            foreach (Java.Net.NetworkInterface network in AllNetworkInterfaces)
            {
                string network_name = network.Name;
                if (!network_name.ContainsAny(StringComparison.InvariantCultureIgnoreCase, "eth0", "wlan0")) continue;

                IList<Java.Net.InterfaceAddress> AddressInterface = network.InterfaceAddresses;
                foreach (Java.Net.InterfaceAddress AInterface in AddressInterface)
                {
                    if (AInterface.Broadcast != null)
                        IPAddres = AInterface.Address.HostAddress;
                }
            }
            return IPAddres;
        }
    }
}
