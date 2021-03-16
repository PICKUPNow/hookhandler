using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;

namespace HookHandler.Api.Services
{
    /// <summary>
    /// Recreates the signature input string that the sender used for signing
    /// </summary>
    /// <remarks>
    /// NOTE: This code makes way more sense if you peruse the document linked below.
    /// The algorithm for creating the header is based on the following IETF draft:
    /// https://tools.ietf.org/id/draft-richanna-http-message-signatures-00.html
    /// </remarks>
    public class SignatureStringBuilder: ISignatureStringBuilder
    {
        private ILogger _logger;

        ///
        public SignatureStringBuilder(ILogger<SignatureStringBuilder> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Recreates the signature input string that the sender used for signing
        /// </summary>
        public string BuildSignatureInput(HttpRequest request, Dictionary<string, string> signatureDictionary)
        {
            // The headers key/value pair contains a list of headers (or computed values) whose values were used to build the string that was hashed
            var headers = signatureDictionary["headers"].Split(' ');

            // We'll iterate through the list of headers and generate the same string that the caller did
            // We can deal with regular headers generically (in the default case), but we have to have special treatment for the non-header ones.
            var inputLines = new List<string>();
            foreach (var header in headers)
            {
                switch (header)
                {
                    case "(request-target)":
                        inputLines.Add(BuildRequestTargetLine(request));
                        break;
                    case "(created)":
                    case "(expires)":
                        inputLines.Add(BuildSpecialLine(header, signatureDictionary));
                        break;
                    default:
                        inputLines.Add(BuildHeaderLine(header, request));
                        break;
                }
            }

            // The algorithm calls for each of the built lines to be concatenated together with a newline character
            var signatureInput = string.Join('\n', inputLines);
            _logger.LogInformation($"signatureInput: {signatureInput}");

            return signatureInput;
        }

        private string BuildRequestTargetLine(HttpRequest request)
        {
            var pathAndQuery = request.Path + request.QueryString.ToString();
            return $"(request-target): {request.Method.ToLower()} {pathAndQuery}";
        }

        private string BuildSpecialLine(string header, Dictionary<string, string> signatureDictionary)
        {
            string key = header.Trim('(', ')');

            var value = signatureDictionary.GetValueOrDefault(key) ?? "";

            return $"{header}: {value}";
        }

        private string BuildHeaderLine(string headerName, HttpRequest request)
        {
            StringValues values;
            if (!request.Headers.TryGetValue(headerName, out values))
            {
                _logger.LogWarning($"Missing {headerName} Header");
            }
            return $"{headerName}: {values.ToString()}";
        }
    }
}
