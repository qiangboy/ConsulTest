using Consul;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace ConsulTest;

public class DefaultConsulServerManager(IConsulClient consulClient, IOptions<ConsulOptions> options)
    : IConsulServerManager
{
    private readonly ConsulOptions _consulOptions = options.Value;

    private readonly ConcurrentDictionary<string, int> ServerCalls = new();

    /// <summary>
    /// 根据负载均衡策略获取服务地址
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public async Task<string> GetServerAsync(string serviceName)
    {
        var service = await GetServerInfoAsync(serviceName);
        return $"http://{service.Address}:{service.Port}";
    }

    /// <summary>
    /// 根据负载均衡策略获取服务信息
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public async Task<AgentService> GetServerInfoAsync(string serviceName)
    {
        var services = await GetServerListAsync(serviceName);

        // 安装负载均衡策略进行返回服务地址
        return BalancingRoute(services, serviceName);
    }

    /// <summary>
    /// 获取服务列表
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public async Task<IList<AgentService>> GetServerListAsync(string serviceName)
    {
        var result = await consulClient.Health.Service(serviceName);

        if (result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new ConsulRequestException("获取服务信息失败！", result.StatusCode);
        }

        return result.Response.Select(s => s.Service).ToList();
    }


    private AgentService BalancingRoute(IList<AgentService> services, string key)
    {
        if (services == null || !services.Any())
        {
            throw new ArgumentNullException(nameof(services), $"当前未找到{key}可用服务！");
        }

        return _consulOptions.LBStrategy switch
        {
            "First" => services.First(),
            "Random" => Random(services),
            "RoundRobin" => RoundRobin(services, key),
            "WeightRandom" => WeightRandom(services),
            "WeightRoundRobin" => WeightRoundRobin(services, key),
            _ => RoundRobin(services, key)
        };
    }

    /// <summary>
    /// 随机
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static AgentService Random(ICollection<AgentService> services)
    {
        return services.ElementAt(new Random().Next(0, services.Count));
    }

    /// <summary>
    /// 轮询
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private AgentService RoundRobin(ICollection<AgentService> services, string key)
    {
        var count = ServerCalls.GetOrAdd(key, 0);
        var service = services.ElementAt(count++ % services.Count);
        ServerCalls[key] = count;

        return service;
    }

    /// <summary>
    /// 加权随机
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public AgentService WeightRandom(IList<AgentService> services)
    {
        var pairs = services.SelectMany(s =>
        {
            var weight = 1;

            if (s.Meta.TryGetValue("Weight", out var value) && int.TryParse(value, out var w))
            {
                weight = w;
            }
            var result = new List<AgentService>();

            for (var i = 0; i < weight; i++)
            {
                result.Add(s);
            }

            return result;
        }).ToList();

        return Random(pairs);
    }

    /// <summary>
    /// 加权轮询
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public AgentService WeightRoundRobin(IList<AgentService> services, string key)
    {
        var pairs = services.SelectMany(s =>
        {
            var weight = 1;

            if (s.Meta.TryGetValue("Weight", out var value) && int.TryParse(value, out var w))
            {
                weight = w;
            }

            var result = new List<AgentService>();

            for (var i = 0; i < weight; i++)
            {
                result.Add(s);
            }

            return result;
        }).ToList();

        return RoundRobin(pairs, key);
    }
}
