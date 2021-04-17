using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TehGM.Telecord.Utilities
{
    class Logging
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();
        private const string _sectionName = "Logging";

        public static void ConfigureLogging(IConfiguration configuration)
        {
            lock (_lock)
            {
                LoggerConfiguration config = new LoggerConfiguration();

                // load from config if section exists
                if (configuration.GetSection(_sectionName).Exists())
                {
                    config.ReadFrom.Configuration(configuration, _sectionName);
                    // configure datadog
                    DatadogOptions ddOptions = configuration.GetSection(_sectionName).GetSection("DataDog").Get<DatadogOptions>();
                    if (!string.IsNullOrWhiteSpace(ddOptions?.ApiKey))
                    {
                        config.WriteTo.DatadogLogs(
                            ddOptions.ApiKey,
                            source: ".NET",
                            service: ddOptions.ServiceName ?? "Telecord",
                            host: ddOptions.HostName ?? Environment.MachineName,
                            new string[] { },
                            ddOptions.ToDatadogConfiguration(),
                            // no need for debug logs in datadag
                            logLevel: ddOptions.OverrideLogLevel ?? LogEventLevel.Information
                        );
                    }
                }
                // otherwise use defaults
                else
                {
                    config.WriteTo.Console()
                        .MinimumLevel.Is(Debugger.IsAttached ? LogEventLevel.Verbose : LogEventLevel.Information)
                        .Enrich.FromLogContext();
                }


                // create the logger
                Serilog.Log.Logger = config.CreateLogger();

                // enable logging of unhandled exceptions, but only when initializing for the first time
                if (!_initialized)
                    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                _initialized = true;
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Serilog.Log.Fatal((Exception)e.ExceptionObject, "An exception was unhandled");
                Serilog.Log.CloseAndFlush();
            }
            catch { }
        }
    }
}
