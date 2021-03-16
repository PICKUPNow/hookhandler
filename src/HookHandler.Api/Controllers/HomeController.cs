using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using HookHandler.Api.Services;

namespace HookHandler.Api.Controllers
{
    /// api for webhook registrations
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{v:apiVersion}")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly IMessageSink _sink;

        ///
        public HomeController(
            ILogger<HomeController> logger,
            ISignatureVerifier signatureVerifier,
            IMessageSink sink)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
            _sink = sink;
        }

        /// <summary>
        /// Accepts a webhook request, validates the signature and sends the body of the message to the message sink.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> HandleHook()
        {
            LogHeaders(Request.Headers);

            var verified = _signatureVerifier.Verify(Request, DateTime.UtcNow);
            if (!verified)
            {
                _logger.LogWarning("Bad Signature!");
                return this.Unauthorized();
            }

            // the body is json, but we aren't trying to deserialize it here--we're just dumping it to the message sink
            using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
            var content = await reader.ReadToEndAsync();

            _sink.HandleMessage(content);

            return Ok();
        }

        private void LogHeaders(IHeaderDictionary headers)
        {
            var headerLog = string.Join(" | ", headers.Select(h => $"{h.Key}:{h.Value}"));
            _logger.LogInformation($"Headers: {headerLog}");
        }
    }
}
