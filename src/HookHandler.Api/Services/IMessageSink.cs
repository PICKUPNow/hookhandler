namespace HookHandler.Api.Services
{
    /// <summary>
    /// Provides a service that handles messages from the webhook request
    /// </summary>
    public interface IMessageSink
    {
        /// <summary>
        /// Handles the message that came in from the webhook request
        /// </summary>
        void HandleMessage(string message);
    }
}
