using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SecretsSample
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var environmentName = "Production";
            var host = CreateHostBuilder(args, environmentName).Build();
            var config = host.Services.GetService<IConfiguration>();
            var mySettingValue = config?["Hello"];
            Console.WriteLine(string.IsNullOrEmpty(mySettingValue)
                ? "This value only for Development mode"
                : $"Hello, {mySettingValue}");

            Console.WriteLine("Application is running...");
            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args, string environmentName) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    ApplyDefaultAppConfiguration(hostingContext, config, args, environmentName);
                });

        private static void ApplyDefaultAppConfiguration(HostBuilderContext hostingContext, IConfigurationBuilder appConfigBuilder, string[]? args, string environmentName)
        {
            IHostEnvironment env = hostingContext.HostingEnvironment;
            env.EnvironmentName = environmentName;

            bool reloadOnChange = GetReloadConfigOnChangeValue(hostingContext);
            appConfigBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);
            if (env.IsDevelopment() && env.ApplicationName is { Length: > 0 })
            {
                var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                appConfigBuilder.AddUserSecrets(appAssembly, optional: true, reloadOnChange: reloadOnChange);
            }
            appConfigBuilder.AddEnvironmentVariables();
            if (args is { Length: > 0 })
            {
                appConfigBuilder.AddCommandLine(args);
            }
        }

        static bool GetReloadConfigOnChangeValue(HostBuilderContext hostingContext) => hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);
    }
}
