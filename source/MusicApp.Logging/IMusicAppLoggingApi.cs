using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestEase;
using Serilog.Events;

namespace MusicApp.Logging
{
    public interface IMusicAppLoggingApi : IDisposable
    {
        [Header("X-API-Key")] string ApiKey { get; set; }

        [Post("logging/bulk")]
        Task<Response<LogEventLevel>> BulkUploadResource([Body] IEnumerable<LogEvent> events);
    }
}