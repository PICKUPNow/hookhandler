using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;

namespace HookHandler.Api.Services
{
    /// <summary>
    /// Validates the SHA-512 Signature based on the Headers of an HttpRequest
    /// </summary>
    /// <remarks>
    /// NOTE: This code makes way more sense if you peruse the document linked below.
    /// The algorithm for creating the header is based on the following IETF draft:
    /// https://tools.ietf.org/id/draft-richanna-http-message-signatures-00.html
    /// </remarks>
    public class RsaSignatureVerifier: ISignatureVerifier
    {
        private RSA _rsa;
        private IStampValidator _stampValidator;
        private ISignatureStringBuilder _signatureStringBuilder;
        private ILogger _logger;

        /// <summary>
        /// Create the Validator
        /// </summary>
        public RsaSignatureVerifier(
            RSA rsaAlgorithm,
            IStampValidator stampValidator,
            ISignatureStringBuilder signatureStringBuilder,
            ILogger<RsaSignatureVerifier> logger)
        {
            _rsa = rsaAlgorithm;
            _stampValidator = stampValidator;
            _signatureStringBuilder = signatureStringBuilder;
            _logger = logger;
        }

        /// <summary>
        /// Validate the Signature header. Return true if valid, false otherwise.
        /// </summary>
        public bool Verify(HttpRequest request, DateTime currentUtc)
        {
            // The Signature header contains several key/value pairs. Extract them into a dictionary to make them easy to work with
            // If the Signature header doesn't exist, then we fail validation
            var signatureDictionary = BuildSignatureDictionary(request);
            if (signatureDictionary is null)
                return false;

            // if created or expired timestamps were sent in the Signature header, validate them
            if (!_stampValidator.Validate(currentUtc, signatureDictionary))
                return false;

            // Build the same string that was signed by the client
            var signatureInput = _signatureStringBuilder.BuildSignatureInput(request, signatureDictionary);

            // pull the signature string from the content in the signature header
            var signedContent = signatureDictionary.GetValueOrDefault("signature") ?? "";

            // let rsa verify that the string we built matches the string that was signed by the client
            var valid = VerifySignature(signatureInput, signedContent);

            return valid;
        }

        private Dictionary<string, string> BuildSignatureDictionary(HttpRequest request)
        {
            // The Signature header contains several key/value pairs. Extract them into a dictionary to make them easy to work with

            StringValues signatureHeaderValues;
            if (!request.Headers.TryGetValue("Signature", out signatureHeaderValues))
            {
                _logger.LogWarning("Missing Signature Header");
                return null;
            }

            var signatureHeader = signatureHeaderValues.ToString();

            var sigPairs = signatureHeader.Split(", ").Select(pair =>
            {
                // The values can have equals signs in them, so we limit the split to 2 array elements to get a key and a value
                var parts = pair.Split('=', 2);
                return new KeyValuePair<string, string>(parts[0], parts[1].Trim('"'));
            });

            return new System.Collections.Generic.Dictionary<string, string>(sigPairs);
        }

        private bool VerifySignature(string message, string signature)
        {
            var messageBytes = new ASCIIEncoding().GetBytes(message);
            var signatureBytes = Convert.FromBase64String(signature);

            return _rsa.VerifyData(messageBytes, signatureBytes, HashAlgorithmName.SHA512, RSASignaturePadding.Pss);
        }

        /// <summary>
        /// Creates and initializes the rsa algorithm with a public key.
        /// Use to generate the RSA algorithm to inject into the RsaSignatureVerifier constructor
        /// </summary>
        public static RSA InitializeRsa(string publicKeyPath)
        {
            // helpful reading: https://www.scottbrady91.com/C-Sharp/PEM-Loading-in-dotnet-core-and-dotnet
            var pemContent = File.ReadAllText(publicKeyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(pemContent);
            return rsa;
        }
    }
}
