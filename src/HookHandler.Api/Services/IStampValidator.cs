using System;
using System.Collections.Generic;

namespace HookHandler.Api.Services
{
    /// <summary>
    /// Validates the created and expires datestamps in the Signature header are valid if they exist
    /// </summary>
    public interface IStampValidator
    {
        /// <summary>
        /// Validates the created and expires datestamps in the Signature header are valid if they exist
        /// </summary>
        bool Validate(DateTime currentUtc, Dictionary<string, string> signatureDictionary);
    }
}
