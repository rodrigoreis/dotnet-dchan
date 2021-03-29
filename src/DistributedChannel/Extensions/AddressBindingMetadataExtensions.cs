using System.Net;
using DistributedChannel.Internals;

namespace DistributedChannel.Extensions
{
    public static class AddressBindingMetadataExtensions
    {
        public static IPAddress ToIpAddress(this BindingAddress bindingAddress)
        {
            return IPAddress.Parse(bindingAddress.IpV4);
        }
    }
}