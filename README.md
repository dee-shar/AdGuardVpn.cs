# AdGuardVpn.cs
Mobile-API for [AdGuard VPN](https://play.google.com/store/apps/details?id=com.adguard.vpn) an application that hides your IP and location from prying eyes, encrypts traffic and makes you anonymous

## Example
```cs
using System;
using AdGuardVpnApi;

namespace Application
{
    internal class Program
    {
        static async Task Main()
        {
            var api = new AdGuardVpn();
            string locations = await api.GetLocations();
            Console.WriteLine(locations);
        }
    }
}
```
