
using System;
using System.Text;
using libutilscore.Logging;

namespace libutilscore.Common
{
    public static class Codec
    {
        /* encode plain string to base64 string */
        public static string Base64Encode(string plainText)
        {
            Log.logger.Debug("Entering Base64Encode function");
            var plainTextBytes = new byte[1024];
            try
            {
                Log.logger.Debug("Try to Eecode string: {0}", plainText);
                plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            }
            catch (ArgumentNullException e)
            {
                Log.logger.Error("Decode {0} failed, due to {1}", plainText, e.Message);
                throw e;
            }
            catch(EncoderFallbackException e)
            {
                Log.logger.Error("Decode {0} failed, due to {1}", plainText, e.Message);
                throw e;
            }
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /* decode base64 string to plain string */
        public static string Base64Decode(string base64EncodedData)
        {
            Log.logger.Debug("Entering Base64Decode function");
            var base64EncodedBytes = new byte[1024];
            try
            {
                Log.logger.Debug("Try to Decode string: {0}", base64EncodedData);
                base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            }
            catch (System.FormatException e)
            {
                Log.logger.Error("Decode {0} failed, due to {1}", base64EncodedData, e.Message);
                throw e;
            }
            catch(System.ArgumentNullException e)
            {
                Log.logger.Error("Decode {0} failed, due to {1}", base64EncodedData, e.Message);
                throw e;
            }
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
