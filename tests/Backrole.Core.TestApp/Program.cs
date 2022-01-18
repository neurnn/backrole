using Backrole.Core.Abstractions;
using Backrole.Core.Abstractions.Defaults;
using Backrole.Core.Builders;
using Backrole.Core.Configurations.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Core.TestApp
{
    class Program
    {
        public enum Fruits
        {
            Apple,
            Banana,
            Watermelon,
        }

        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureConfigurations(Configurations =>
                {
                    Configurations.AddEnvironmentVariables(Options =>
                    {
                        Options.AsLowerCase = true;
                    });

                    Configurations.AddJsonFile("appsettings.json", true, Options =>
                    {
                        Options.Prefix = "app";
                        Options.AsLowerCase = true;
                    });
                })
                .ConfigureLoggings(Loggings =>
                {
                    Loggings.AddConsole(Logger =>
                    {
                        Logger.DebugLogsOnlyWithDebugger = true;
                        Logger.LogLevels.Add(LogLevel.Trace);
                    });

                })
                .ConfigureServices(Services =>
                {
                    Services
                        .AddSingleton<Test>();
                })
                .Configure<TestContainerBuilder>()
                .Build()
                .RunAsync();
        }

        class TestContainerBuilder : ContainerBuilder<TestContainer>
        {
            public TestContainerBuilder(IServiceProvider HostServices) : base(HostServices)
            {
            }

            protected override void OnConfigureServices(IServiceCollection Services)
            {
                Services
                    .AddHostedService<TestService>();
            }
        }

        class TestContainer : IContainer
        {
            [ServiceInjection]
            private IServiceScope m_Scope = null;

            /// <summary> 
            /// Service Provider.
            /// </summary>
            public IServiceProvider Services => m_Scope.ServiceProvider;


            public void Dispose()
            {
                m_Scope.Dispose();
            }

            public ValueTask DisposeAsync()
            {
                return m_Scope.DisposeAsync();
            }
        }


        class Test
        {
            [ServiceInjection]
            private IServiceProvider m_Services = null;

            [ServiceInjection]
            private IConfiguration m_Configuration = null;

            /// <summary>
            /// Called after <see cref="IServiceProvider"/> injected.
            /// </summary>
            private void OnServiceInjected()
            {
                var Logger = m_Services
                    .GetRequiredService<ILogger<Test>>()
                    .Warn("OnServiceInjected.");

                foreach (var Each in m_Configuration.Keys)
                    Logger.Info($"Configuration, {Each} = \"{m_Configuration[Each]}\"");


                Logger.Warn($"Configuration, env:windir = \"{m_Configuration["env:windir"]}\"");
            }

            public Task Hello()
            {
                m_Services
                    .GetRequiredService<ILogger<TestService>>()
                    .Debug("Hello.");

                return Task.CompletedTask;
            }
        }

        class TestService : IHostedService
        {
            [ServiceInjection]
            private Test m_Test = null;

            [ServiceInjection]
            private Test m_Test2 = null;

            public Task StartAsync(CancellationToken Token = default)
            {
                return m_Test.Hello();
            }

            public Task StopAsync()
            {
                if (m_Test == m_Test2)
                    return m_Test2.Hello();

                return Task.CompletedTask;
            }
        }
    }
}
