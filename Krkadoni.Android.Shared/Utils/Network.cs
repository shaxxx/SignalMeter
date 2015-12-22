using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Com.Krkadoni.Utils
{
    public static class Network
    {
        public static async Task<bool> CheckPortOpen(string hostname, int port)
        {
            if (string.IsNullOrEmpty(hostname))
                return false;
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    var task = tcpClient.ConnectAsync(hostname, port);
                    if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(3))) == task)
                    {
                        if (!tcpClient.Connected)
                            System.Diagnostics.Debug.WriteLine(string.Format("{0}:{1} is NOT available", hostname, port));
                        return tcpClient.Connected;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("TcpClient on {0}:{1} timed out.", hostname, port));
                        try
                        {
                            tcpClient.Close();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("TcpClient closing exception: {0}", ex.Message));
                        }
                        return false;
                    }
                }   
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("TcpClient exception: {0}", ex.Message));
                return false;
            }
        }

        public const string ValidIpAddressRegex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

        public const string ValidHostnameRegex = @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$";

        public const string PrivateIpAddressRegex = @"(^127\.)|(^192\.168\.)|(^10\.)|(^172\.1[6-9]\.)|(^172\.2[0-9]\.)|(^172\.3[0-1]\.)|(^::1$)|(^[fF][cCdD])";

        public static bool IsValidHostname(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
                return false;
            
            var regex = new Regex(ValidHostnameRegex, RegexOptions.Compiled);
            return regex.Match(hostname).Success;
        }

        public static bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;
            
            var regex = new Regex(ValidIpAddressRegex, RegexOptions.Compiled);
            return regex.Match(ipAddress).Success;
        }

        public static bool IsValidHostnameOrIpAddress(string address)
        {
            if (IsValidHostname(address))
                return true;

            return IsValidIpAddress(address);
        }

        public static bool IsValidPort(string port)
        {
            int result;
            if (int.TryParse(port, out result))
            {
                return (result > 0 && result <= 65535);
            }
            return false;
        }

        public static bool IsPrivateIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;
            
            var regex = new Regex(PrivateIpAddressRegex, RegexOptions.Compiled);
            return regex.Match(ipAddress).Success;
        }

        public static bool IsPrivateAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                return false;
            
            if (IsPrivateIpAddress(address))
                return true;
            
            return address.IndexOf('.') == -1;
        }

    }
}

