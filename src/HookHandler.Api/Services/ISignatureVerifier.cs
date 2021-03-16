using System;
using Microsoft.AspNetCore.Http;

namespace HookHandler.Api.Services
{
    /// <summary>
    /// Validates a Signature header on an HttpRequest using a shared secret
    /// </summary>
    public interface ISignatureVerifier
    {
        /// <summary>
        /// Verify the Signature header. Return true if valid, false otherwise.
        /// </summary>
        bool Verify(HttpRequest request, DateTime currentUtc);
    }
}
