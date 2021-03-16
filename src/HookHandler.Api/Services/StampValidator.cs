using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace HookHandler.Api.Services
{
    /// <summary>
    /// Validates the Date stamp of the signature (not sent from the future or expired)
    /// </summary>
    /// <remarks>
    /// NOTE: This code makes way more sense if you peruse the document linked below.
    /// The algorithm for creating the header is based on the following IETF draft:
    /// https://tools.ietf.org/id/draft-richanna-http-message-signatures-00.html
    /// </remarks>
    public class StampValidator: IStampValidator
    {
        private ILogger _logger;

        /// <summary>
        /// Create the Validator
        /// </summary>
        public StampValidator(ILogger<StampValidator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates the created and expires datestamps in the Signature header are valid if they exist
        /// </summary>
        public bool Validate(DateTime currentUtc, Dictionary<string, string> signatureDictionary)
        {
            var currentUnix = new DateTimeOffset(currentUtc).ToUnixTimeSeconds();

            var created = signatureDictionary.GetValueOrDefault("created");
            if (!ValidateCurrentSignatureStamp(currentUnix, created))
                return false;

            var expires = signatureDictionary.GetValueOrDefault("expires");
            if (!ValidateExpiresSignatureStamp(currentUnix, expires))
                return false;

            return true;

        }

        private bool ValidateCurrentSignatureStamp(long currentUnix, string created)
        {
            // Validate Created time if the value was sent in the signature
            if (created is null)
                return true;

            long createdUnix;
            if (!long.TryParse(created, out createdUnix))
            {
                _logger.LogWarning($"Received 'created' Signature value '{created}', but it did not parse as a long. Failing Signature validation.");
                return false;
            }
            // allow for 20 seconds of clock drift (+ any processing time)
            if (currentUnix + 20 < createdUnix)
            {
                _logger.LogWarning($"Created time of this signature is in the future. Current is '{currentUnix}', Created is '{createdUnix}'. Failing Signature validation.");
                return false;
            }

            return true;
        }

        private bool ValidateExpiresSignatureStamp(long currentUnix, string expires)
        {
            // validate Expires time if the value was sent in the signature
            if (expires is null)
                return true;

            long expiresUnix;
            if (!long.TryParse(expires, out expiresUnix))
            {
                _logger.LogWarning($"Received 'expires' Signature value '{expires}', but it did not parse as a long. Failing Signature validation.");
                return false;
            }
            if (currentUnix > expiresUnix)
            {
                _logger.LogWarning($"Expires time of this signature is in the past. Current is '{currentUnix}', Expires is '{expiresUnix}'. Failing Signature validation.");
                return false;
            }

            return true;
        }
    }
}
