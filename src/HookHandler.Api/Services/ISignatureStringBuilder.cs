using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace HookHandler.Api.Services
{
    /// <summary>
    /// Recreates the signature input string that the sender used for signing
    /// </summary>
    public interface ISignatureStringBuilder
    {
        /// <summary>
        /// Recreates the signature input string that the sender used for signing
        /// </summary>
        string BuildSignatureInput(HttpRequest request, Dictionary<string, string> signatureDictionary);
    }
}
