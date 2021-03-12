using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace MusicApp.Logging
{
    public static class MusicAppExtensions
    {
        public static ILoggerFactory AddMusicAppLogging(this ILoggerFactory loggerFactory,
            IConfigurationSection configuration)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (TryCreateProvider(configuration, LogEventLevel.Information, out var provider))
                loggerFactory.AddProvider(provider);

            return loggerFactory;
        }

        public static ILoggerFactory AddMusicAppLogging(
            this ILoggerFactory loggerFactory,
            string serverUrl,
            string apiKey = null,
            LogEventLevel minimumLevel = LogEventLevel.Information)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (serverUrl == null) throw new ArgumentNullException(nameof(serverUrl));

            var provider = CreateProvider(serverUrl, apiKey, minimumLevel);
            loggerFactory.AddProvider(provider);
            return loggerFactory;
        }

        public static ILoggingBuilder AddMusicAppLogging(
            this ILoggingBuilder loggingBuilder,
            string serverUrl,
            string apiKey = null)
        {
            if (loggingBuilder == null) throw new ArgumentNullException(nameof(loggingBuilder));
            if (serverUrl == null) throw new ArgumentNullException(nameof(serverUrl));

            loggingBuilder.Services.AddSingleton<ILoggerProvider>(s =>
            {
                var opts = s.GetService<ILoggerProviderConfiguration<MusicAppLoggerProvider>>();
                var provider = CreateProvider(opts?.Configuration, serverUrl, apiKey);
                return provider;
            });

            return loggingBuilder;
        }


        public static ILoggingBuilder AddMusicAppLogging(
            this ILoggingBuilder loggingBuilder,
            IConfigurationSection configuration)
        {
            if (loggingBuilder == null) throw new ArgumentNullException(nameof(loggingBuilder));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (TryCreateProvider(configuration, LevelAlias.Minimum, out var provider))
                loggingBuilder.Services.AddSingleton<ILoggerProvider>(_ => provider);

            return loggingBuilder;
        }

        private static bool TryCreateProvider(
            IConfiguration configuration,
            LogEventLevel defaultMinimumLevel,
            out MusicAppLoggerProvider provider)
        {
            var serverUrl = configuration["ServerUrl"];
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                SelfLog.WriteLine("Unable to add the Seq logger: no ServerUrl was present in the configuration");
                provider = null;
                return false;
            }

            var apiKey = configuration["ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                apiKey = null;

            var minimumLevel = defaultMinimumLevel;
            var levelSetting = configuration["MinimumLevel"];
            if (!string.IsNullOrWhiteSpace(levelSetting))
                if (!Enum.TryParse(levelSetting, out minimumLevel))
                {
                    SelfLog.WriteLine("The minimum level setting `{0}` is invalid", levelSetting);
                    minimumLevel = LogEventLevel.Information;
                }


            provider = CreateProvider(serverUrl, apiKey, minimumLevel);
            return true;
        }

        private static MusicAppLoggerProvider CreateProvider(
            IConfiguration configuration,
            string defaultServerUrl,
            string defaultApiKey)
        {
            string serverUrl = null, apiKey = null;
            if (configuration != null)
            {
                serverUrl = configuration["ServerUrl"];
                apiKey = configuration["ApiKey"];
            }

            if (string.IsNullOrWhiteSpace(serverUrl))
                serverUrl = defaultServerUrl;

            if (string.IsNullOrWhiteSpace(apiKey))
                apiKey = defaultApiKey;

            return CreateProvider(serverUrl, apiKey, LevelAlias.Minimum);
        }

        private static MusicAppLoggerProvider CreateProvider(
            string serverUrl,
            string apiKey,
            LogEventLevel minimumLevel)
        {
            var levelSwitch = new LoggingLevelSwitch(minimumLevel);

            var sink = new MusicAppLoggingApiSink(
                serverUrl,
                apiKey,
                new ControlledLevelSwitch(levelSwitch));

            var batchingSink = new PeriodicBatchingSink(sink, new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = 1000,
                Period = TimeSpan.FromSeconds(2)
            });

            var logger = new LoggerConfiguration().WriteTo.Sink(batchingSink).CreateLogger();
            var provider = new MusicAppLoggerProvider(logger);
            return provider;
        }
    }
}