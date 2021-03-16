using Microsoft.Extensions.Logging;

namespace HookHandler.Api.Services
{
    /// <summary>
    /// Stub service that simply writes messages to the log.
    /// </summary>
    public class LoggingMessageSink: IMessageSink
    {
        private ILogger _logger;

        ///
        public LoggingMessageSink(ILogger<LoggingMessageSink> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the message that came in from the webhook request
        /// </summary>
        public void HandleMessage(string message)
        {
            _logger.LogInformation(message);
        }
    }
}
