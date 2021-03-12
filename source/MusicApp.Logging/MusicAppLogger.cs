using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using FrameworkLogger = Microsoft.Extensions.Logging.ILogger;

namespace MusicApp.Logging
{
    public class MusicAppLogger : FrameworkLogger
    {
        private static readonly MessageTemplateParser MessageTemplateParser = new();
        private readonly Logger _logger;
        private readonly MusicAppLoggerProvider _provider;

        public MusicAppLogger(
            MusicAppLoggerProvider provider,
            Logger logger,
            string name = null)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

            _logger = logger.ForContext(new[] {provider}) as Logger;

            if (name != null) _logger = _logger?.ForContext(Constants.SourceContextPropertyName, name) as Logger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _provider.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            LogEventLevel? eventLevel = logLevel switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                _ => null
            };

            return _logger.IsEnabled(eventLevel.GetValueOrDefault()) && eventLevel.HasValue;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel)) return;
            LogEventLevel? eventLevel = logLevel switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                _ => null
            };
            var logger = _logger;
            string messageTemplate = null;

            var properties = new List<LogEventProperty>();

            if (state is IEnumerable<KeyValuePair<string, object>> structure)
            {
                foreach (var (key, value) in structure)
                    if (key == MusicAppLoggerProvider.OriginalFormatPropertyName && value is string s)
                    {
                        messageTemplate = s;
                    }
                    else if (key.StartsWith("@"))
                    {
                        if (logger.BindProperty(key.Substring(1), value, true, out var destructured))
                            properties.Add(destructured);
                    }
                    else
                    {
                        if (logger.BindProperty(key, value, false, out var bound))
                            properties.Add(bound);
                    }

                var stateType = state.GetType();
                var stateTypeInfo = stateType.GetTypeInfo();
                // Imperfect, but at least eliminates `1 names
                if (messageTemplate == null && !stateTypeInfo.IsGenericType)
                {
                    messageTemplate = "{" + stateType.Name + ":l}";
                    if (logger.BindProperty(stateType.Name, AsLoggableValue(state, formatter), false,
                        out var stateTypeProperty))
                        properties.Add(stateTypeProperty);
                }
            }

            if (messageTemplate == null)
            {
                string propertyName = null;
                if (state != null)
                {
                    propertyName = "State";
                    messageTemplate = "{State:l}";
                }
                else if (formatter != null)
                {
                    propertyName = "Message";
                    messageTemplate = "{Message:l}";
                }

                if (propertyName != null)
                    if (logger.BindProperty(propertyName, AsLoggableValue(state, formatter), false, out var property))
                        properties.Add(property);
            }

            if (eventId.Id != 0 || eventId.Name != null)
                properties.Add(CreateEventIdProperty(eventId));

            var parsedTemplate = MessageTemplateParser.Parse(messageTemplate ?? "");
            var evt = new LogEvent(DateTimeOffset.Now, eventLevel.GetValueOrDefault(), exception.Demystify(),
                parsedTemplate, properties);
            logger.Write(evt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object AsLoggableValue<TState>(TState state, Func<TState, Exception, string> formatter)
        {
            object sob = state;
            if (formatter != null)
                sob = formatter(state, null);
            return sob;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static LogEventProperty CreateEventIdProperty(EventId eventId)
        {
            var properties = new List<LogEventProperty>(2);

            if (eventId.Id != 0) properties.Add(new LogEventProperty("Id", new ScalarValue(eventId.Id)));

            if (eventId.Name != null) properties.Add(new LogEventProperty("Name", new ScalarValue(eventId.Name)));

            return new LogEventProperty("EventId", new StructureValue(properties));
        }
    }
}