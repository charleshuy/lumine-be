using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Application.Helper
{
    public class VNPayHelper
    {
        private readonly SortedList<string, string> _requestData = new();
        private readonly SortedList<string, string> _responseData = new();

        public void AddRequestData(string key, string value)
        {
            _requestData[key] = value;
        }

        public void AddResponseData(string key, string value)
        {
            _responseData[key] = value;
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public static string BuildPaymentUrl(Dictionary<string, string> inputData, string baseUrl, string hashSecret)
        {
            var sortedParams = inputData
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .OrderBy(kv => kv.Key)
                .ToList();

            // 1. URL-encode for query string
            var queryString = string.Join("&", sortedParams.Select(kv =>
                $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

            // ✅ 2. URL-encode for rawData too (VERY IMPORTANT!)
            var rawData = string.Join("&", sortedParams.Select(kv =>
                $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

            var secureHash = HmacSHA512(hashSecret, rawData);
            var paymentUrl = $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";

            Console.WriteLine("🔐 RawData for HMAC: " + rawData);
            Console.WriteLine("🔐 SecureHash: " + secureHash);

            return paymentUrl;
        }


        public bool ValidateSignature(string receivedHash, string secretKey)
        {
            var filteredParams = _responseData
                .Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                .OrderBy(kv => kv.Key)
                .ToList();

            var rawData = string.Join("&", filteredParams.Select(kv =>
                $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));


            var calculatedHash = HmacSHA512(secretKey, rawData);
            Console.WriteLine($"🔍 RawData: {rawData}");
            Console.WriteLine($"🔍 CalculatedHash: {calculatedHash}");
            Console.WriteLine($"🔍 ReceivedHash: {receivedHash}");
            return string.Equals(receivedHash, calculatedHash, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string HmacSHA512(string key, string input)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}
