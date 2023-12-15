using Consul;

namespace ConsulTest;

public interface IConsulServerManager
{
    Task<string> GetServerAsync(string serviceName);

    Task<AgentService> GetServerInfoAsync(string serviceName);

    Task<IList<AgentService>> GetServerListAsync(string serviceName);
}
