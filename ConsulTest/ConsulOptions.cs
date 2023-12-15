namespace ConsulTest;

public class ConsulOptions
{
    /// <summary>
    /// 配置Key
    /// </summary>
    public const string Consul = nameof(Consul);

    /// <summary>
    /// Consul服务器地址
    /// </summary>
    public string ServerAddresses { get; set; } = string.Empty;

    /// <summary>
    /// 负载均衡策略
    /// </summary>
    public string LBStrategy { get; set; } = string.Empty;

    /// <summary>
    /// 环境名称
    /// </summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>
    /// 配置中心
    /// </summary>
    public ConfigurationOptions Configuration { get; set; } = new();
}

public class ConfigurationOptions
{
    /// <summary>
    /// 配置Key
    /// </summary>
    public const string Configuration = nameof(Configuration);

    /// <summary>
    /// 监听配置列表
    /// </summary>
    public string[] Listeners { get; set; } = [];
}