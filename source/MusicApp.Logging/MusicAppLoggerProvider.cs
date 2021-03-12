using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace MusicApp.Logging
{
    [ProviderAlias("MusicAppLogger")]
    public class MusicAppLoggerProvider : ILoggerProvider, ILogEventEnricher, ISupportExternalScope
    {
        internal const string OriginalFormatPropertyName = "{OriginalFormat}";
        private const string ScopePropertyName = "Scope";
        private readonly Action _dispose;

        private readonly Logger _logger;

        private IExternalScopeProvider _scopeProvider;

        public MusicAppLoggerProvider(Logger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _dispose = logger.Dispose;
            _logger = logger.ForContext(new[] {this}) as Logger;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var scopeItems = new List<LogEventPropertyValue>();
            _scopeProvider?.ForEachScope((scopeState, state) =>
            {
                var (logEvener, logEventPropertyFactory, logEventPropertyValues) = state;
                MusicAppLoggerScope.EnrichAndCreateScopeItem(scopeState, logEvener, logEventPropertyFactory,
                    out var scopeItem);

                if (scopeItem != null) logEventPropertyValues.Add(scopeItem);
            }, (logEvent, propertyFactory, scopeItems));

            if (scopeItems.Count > 0)
                logEvent.AddPropertyIfAbsent(new LogEventProperty(ScopePropertyName, new SequenceValue(scopeItems)));
        }

        public void Dispose()
        {
            _dispose();
            GC.SuppressFinalize(this);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MusicAppLogger(this, _logger, categoryName);
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }


        public IDisposable BeginScope<T>(T state)
        {
            return _scopeProvider?.Push(state);
        }
    }
}