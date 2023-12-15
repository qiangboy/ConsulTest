using Winton.Extensions.Configuration.Consul;

namespace ConsulTest;

public static class ConsulConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddConsul(this IConfigurationBuilder builder, Action<ConsulOptions>? setupAction)
    {
        var options = new ConsulOptions();

        setupAction?.Invoke(options);

        foreach (var consulOptionsListener in options.Configuration.Listeners)
        {
            builder.AddConsul($"{options.EnvironmentName}/{consulOptionsListener}", opt =>
            {
                opt.ConsulConfigurationOptions = co => co.Address = new Uri(options.ServerAddresses);
                opt.ReloadOnChange = true;
                opt.Optional = true;
            });
        }

        return builder;
    }
}