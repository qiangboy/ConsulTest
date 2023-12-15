using Consul.AspNetCore;

namespace ConsulTest;

public static class ConsulServiceCollectionExtensions
{
    /// <summary>
    /// 向容器中添加Consul必要的依赖注入
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置consul服务注册信息
        var consulOptions = configuration.GetSection(ConsulOptions.Consul).Get<ConsulOptions>();

        ArgumentNullException.ThrowIfNull(consulOptions);

        // 通过consul提供的注入方式注册consulClient
        services.AddConsul(options => options.Address = new Uri(consulOptions.ServerAddresses));

        // 注册服务到Consul
        services.AddConsulServiceRegistration(options =>
        {
            configuration.GetSection("Consul:AgentServiceRegistration").Bind(options);

            options.ID ??= Guid.NewGuid().ToString();
            options.Address = UriTool.GetCurrentIp();

            foreach (var agentServiceCheck in options.Checks)
            {
                agentServiceCheck.HTTP = $"http://{options.Address}:{options.Port}{Consts.HealthAddress}";
            }
            
        });

        services.AddSingleton<IConsulServerManager, DefaultConsulServerManager>();

        return services;
    }
}
