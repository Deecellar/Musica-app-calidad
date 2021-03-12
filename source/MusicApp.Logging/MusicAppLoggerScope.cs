using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace MusicApp.Logging
{
    public static class MusicAppLoggerScope
    {
        private const string NoName = "None";

        public static void EnrichAndCreateScopeItem(object state, LogEvent logEvent,
            ILogEventPropertyFactory propertyFactory, out LogEventPropertyValue scopeItem)
        {
            if (state == null) scopeItem = null;

            if (state is IEnumerable<KeyValuePair<string, object>> stateProperties)
            {
                scopeItem = null; // Unless it's `FormattedLogValues`, these are treated as property bags rather than scope items.

                foreach (var (s, value) in stateProperties)
                {
                    if (s == MusicAppLoggerProvider.OriginalFormatPropertyName && value is string)
                    {
                        scopeItem = new ScalarValue(state.ToString());
                        continue;
                    }

                    var key = s;
                    var destructureObject = false;

                    if (key.StartsWith("@"))
                    {
                        key = key.Substring(1);
                        destructureObject = true;
                    }

                    var property = propertyFactory.CreateProperty(key, value, destructureObject);
                    logEvent.AddOrUpdateProperty(property);
                }
            }
            else
            {
                scopeItem = propertyFactory.CreateProperty(NoName, state).Value;
            }
        }
    }
}