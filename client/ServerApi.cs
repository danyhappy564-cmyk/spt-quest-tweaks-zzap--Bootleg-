using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace QuestTweaksLive
{
    /// <summary>
    /// Minimal HTTP client for the server mod's live-settings routes.
    /// Sends uncompressed JSON (requestcompressed/responsecompressed = 0), which the SPT server supports.
    /// </summary>
    internal static class ServerApi
    {
        public const string GetRoute = "/sgtlaggy-questtweaks/settings/get";
        public const string SetRoute = "/sgtlaggy-questtweaks/settings/set";

        private static string _backendUrl;

        public static string BackendUrl => _backendUrl ?? (_backendUrl = FindBackendUrl());

        public static JObject Post(string route, string body)
        {
            var request = (HttpWebRequest)WebRequest.Create(BackendUrl.TrimEnd('/') + route);
            request.Method = "POST";
            request.Timeout = 15000;
            request.ContentType = "application/json";
            request.Headers["requestcompressed"] = "0";
            request.Headers["responsecompressed"] = "0";
            // the local SPT server uses a self-signed certificate
            request.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

            var bytes = Encoding.UTF8.GetBytes(body ?? "{}");
            request.ContentLength = bytes.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return JObject.Parse(reader.ReadToEnd());
            }
        }

        // The launcher starts the game with -config={"BackendUrl":"https://127.0.0.1:6969",...}
        private static string FindBackendUrl()
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (!arg.StartsWith("-config=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var url = (string)JObject.Parse(arg.Substring("-config=".Length))["BackendUrl"];
                    if (!string.IsNullOrEmpty(url))
                    {
                        return url;
                    }
                }
                catch
                {
                    // fall through to the default
                }
            }

            return "https://127.0.0.1:6969";
        }
    }
}
