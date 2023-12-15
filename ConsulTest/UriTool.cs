using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ConsulTest;

internal static class UriTool
{
    public static string GetCurrentIp()
    {
        return "192.168.0.184";

        // 获取可用网卡
        var networkInterfaces = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(network => network.OperationalStatus == OperationalStatus.Up);

        // 获取所有可用网卡IP信息
        var ipCollection = networkInterfaces
            .Select(x => x.GetIPProperties())
            .SelectMany(x => x.UnicastAddresses);

        var firstIPAddress = ipCollection
            .FirstOrDefault(ip => !IPAddress.IsLoopback(ip.Address) && ip.Address.AddressFamily == AddressFamily.InterNetwork);

        var instanceIp = "127.0.0.1";
        if (ipCollection is not null)
        {
            foreach (var ipadd in ipCollection)
            {
                if (!IPAddress.IsLoopback(ipadd.Address) && ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (string.IsNullOrEmpty(""))
                    {
                        instanceIp = ipadd.Address.ToString();
                        break;
                    }

                    if (!ipadd.Address.ToString().StartsWith("")) continue;
                    instanceIp = ipadd.Address.ToString();
                    break;
                }
            }
        }

        return firstIPAddress is null ? "127.0.0.1" : firstIPAddress.Address.ToString();
    }

    public static string BuildRunUri(string address, int port, string? scheme = "http") => $"{scheme}://{address}:{port}";

    public static string BuildRunUri(int port, string? scheme = "http") => BuildRunUri(GetCurrentIp(), port, scheme);

    public static string BuildRunUri(IConfiguration configuration, string? scheme = "http") =>
        BuildRunUri(Convert.ToInt32(configuration["Consul:AgentServiceRegistration:Port"]), scheme);

    public static string BuildRunUri(IConfiguration configuration, string address, string? scheme) =>
        BuildRunUri(address, Convert.ToInt32(configuration["Consul:AgentServiceRegistration:Port"]), scheme);

    public static string BuildRunLocalUri(IConfiguration configuration, string? scheme = "http") => BuildRunUri(configuration, "localhost", scheme);
}