using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilityLib
{
    internal class MacAddr
    {
        private readonly byte[] address;

        public MacAddr(string macAddress)
        {
            if (!IsValidMacAddress(macAddress))
            {
                throw new ArgumentException("Invalid MAC address format");
            }

            address = ParseMacAddress(macAddress);
        }

        public MacAddr(byte[] macAddress)
        {
            if (macAddress.Length != 6)
            {
                throw new ArgumentException("MAC address must be 6 bytes long");
            }

            address = new byte[6];
            Array.Copy(macAddress, address, 6);
        }

        public MacAddr GetNextMacAddress()
        {
            byte[] nextAddress = new byte[6];
            Array.Copy(address, nextAddress, 6);

            for (int i = 5; i >= 0; i--)
            {
                if (nextAddress[i] < 255)
                {
                    nextAddress[i]++;
                    break;
                }
                nextAddress[i] = 0;
            }

            return new MacAddr(nextAddress);
        }

        public byte[] GetAddressBytes()
        {
            return address;
        }

        public override string ToString()
        {
            return BitConverter.ToString(address).Replace("-", ":");
        }

        public static bool IsValidMacAddress(string macAddress)
        {
            var regex = new Regex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
            return regex.IsMatch(macAddress);
        }

        private byte[] ParseMacAddress(string macAddress)
        {
            var sanitizedMac = macAddress.Replace(":", "").Replace("-", "");
            var macBytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                macBytes[i] = Convert.ToByte(sanitizedMac.Substring(i * 2, 2), 16);
            }
            return macBytes;
        }

        public static List<MacAddr> GetMacAddressesInRange(MacAddr start, MacAddr end)
        {
            List<MacAddr> macAddresses = new List<MacAddr>();
            MacAddr current = new MacAddr(start.GetAddressBytes());

            while (true)
            {
                macAddresses.Add(current);
                if (current.Equals(end))
                {
                    break;
                }
                current = current.GetNextMacAddress();
            }

            return macAddresses;
        }

        public override bool Equals(object obj)
        {
            if (obj is MacAddr other)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (address[i] != other.address[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(address, 0);
        }
    }
}
