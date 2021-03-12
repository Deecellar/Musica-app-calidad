using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Polly;
using RestEase;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace MusicApp.Logging
{
    public class MusicAppLoggingApiSink : IBatchedLogEventSink, IDisposable
    {
        private static readonly TimeSpan RequiredLevelCheckInterval = TimeSpan.FromMinutes(2);

        private readonly string _apiKey;
        private readonly ControlledLevelSwitch _controlledSwitch;
        private readonly IMusicAppLoggingApi _musicAppLoggingApi;

        private DateTime _nextRequiredLevelCheckUtc = DateTime.UtcNow.Add(RequiredLevelCheckInterval);

        public MusicAppLoggingApiSink(string serverUrl,
            string apiKey,
            ControlledLevelSwitch controlledSwitch)
        {
            if (serverUrl == null) throw new ArgumentNullException(nameof(serverUrl));
            _controlledSwitch = controlledSwitch ?? throw new ArgumentNullException(nameof(controlledSwitch));
            _apiKey = apiKey;
            var policy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound)
                .RetryAsync();
            _musicAppLoggingApi = RestClient.For<IMusicAppLoggingApi>(serverUrl, new PolicyHttpMessageHandler(policy));
            _musicAppLoggingApi.ApiKey = apiKey;
        }

        public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            _nextRequiredLevelCheckUtc = DateTime.UtcNow.Add(RequiredLevelCheckInterval);
            if (!string.IsNullOrWhiteSpace(_apiKey))
                _musicAppLoggingApi.ApiKey = _apiKey;

            var result = await _musicAppLoggingApi.BulkUploadResource(batch).ConfigureAwait(false);
            if (!result.ResponseMessage.IsSuccessStatusCode)
                throw new Exception(
                    $"Received failed result {result.ResponseMessage.IsSuccessStatusCode} when posting events to API.");
            _controlledSwitch.Update(result.GetContent());
        }

        public async Task OnEmptyBatchAsync()
        {
            if (_controlledSwitch.IsActive &&
                _nextRequiredLevelCheckUtc < DateTime.UtcNow)
                await EmitBatchAsync(Enumerable.Empty<LogEvent>());
        }

        public void Dispose()
        {
            _musicAppLoggingApi.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}