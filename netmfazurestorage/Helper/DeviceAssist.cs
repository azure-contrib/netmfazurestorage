using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using netmfazurestorage.NTP;
using Microsoft.SPOT.Hardware;

namespace netmfazurestorage.Helper
{
    public class DeviceAssist
    {
        public static void SetupDefault() {
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()[0];

            if (!networkInterface.IsDhcpEnabled || !networkInterface.IsDynamicDnsEnabled)
            {
                networkInterface.EnableDhcp();
                networkInterface.EnableDynamicDns();
                networkInterface.RenewDhcpLease();

                Debug.Print("Interface set to " + networkInterface.IPAddress);
            }

            if (DateTime.Now < new DateTime(2012, 01, 01))
            {
                var networkTime = NtpClient.GetNetworkTime();
                Utility.SetLocalTime(networkTime);
            }
        }
    }
}
