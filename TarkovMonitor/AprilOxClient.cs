using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace TarkovMonitor
{
    /// <summary>
    /// Sends quest and position events to Tarkov Quest Planner (protocol v1).
    /// Nothing here throws to callers; failures are reported once in a while on the message board.
    /// The pairing code is a secret and is never written to the message board.
    /// </summary>
    internal static class AprilOxClient
    {
        public const string DefaultSiteUrl = "https://tarkov.aprilox.fr";
        private static readonly Regex CodePattern = new("^[A-Za-z0-9]{16}$", RegexOptions.Compiled);
        private static readonly string AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0";
        private static readonly HttpClient client = CreateClient();
        private static DateTime skipUntilUtc = DateTime.MinValue;
        private static DateTime lastErrorUtc = DateTime.MinValue;

        /// <summary>Message board used to report failures; set once by the main window.</summary>
        public static MessageLog? Log { get; set; }

        private static HttpClient CreateClient()
        {
            var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TarkovAprilOxMonitor", AppVersion));
            return http;
        }

        public static bool IsValidCode(string? code) => code != null && CodePattern.IsMatch(code);

        /// <summary>Returns the normalized site URL (no trailing slash) or null when it is not an absolute http/https URL.</summary>
        public static string? NormalizeSiteUrl(string? url)
        {
            if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return null;
            }
            return uri.ToString().TrimEnd('/');
        }

        /// <summary>Generates the pairing code on first run (or when the stored one is invalid).</summary>
        public static void EnsureCode()
        {
            if (IsValidCode(Properties.Settings.Default.aprilOxCode))
            {
                return;
            }
            Properties.Settings.Default.aprilOxCode = RemoteCode.Generate(16);
            Properties.Settings.Default.Save();
        }

        /// <summary>"regular" or "pve"; null when the mode is not known (the event must not be sent).</summary>
        public static string? GameMode(EftSessionMode mode) => mode switch
        {
            EftSessionMode.PVE => "pve",
            EftSessionMode.Regular => "regular",
            _ => null,
        };

        public static Task SendHelloAsync() => SendAsync(new JsonObject
        {
            ["type"] = "hello",
            ["app"] = "TarkovAprilOxMonitor",
            ["version"] = AppVersion,
        });

        public static Task SendTaskAsync(string status, string taskId, EftSessionMode mode)
            => SendWithModeAsync(new JsonObject { ["type"] = "task", ["status"] = status, ["taskId"] = taskId }, mode);

        public static Task SendMapAsync(string map, EftSessionMode mode)
            => SendWithModeAsync(new JsonObject { ["type"] = "map", ["map"] = map }, mode);

        public static Task SendPositionAsync(string map, float x, float y, float z, float rotation, EftSessionMode mode)
            => SendWithModeAsync(new JsonObject { ["type"] = "position", ["map"] = map, ["x"] = x, ["y"] = y, ["z"] = z, ["rotation"] = rotation }, mode);

        private static Task SendWithModeAsync(JsonObject evt, EftSessionMode mode)
        {
            var gameMode = GameMode(mode);
            if (gameMode == null)
            {
                return Task.CompletedTask;
            }
            evt["gameMode"] = gameMode;
            return SendAsync(evt);
        }

        public static async Task SendAsync(JsonObject evt)
        {
            try
            {
                var settings = Properties.Settings.Default;
                var siteUrl = NormalizeSiteUrl(settings.aprilOxSiteUrl);
                var code = settings.aprilOxCode;
                if (!settings.aprilOxEnabled || siteUrl == null || !IsValidCode(code) || DateTime.UtcNow < skipUntilUtc)
                {
                    return;
                }
                evt["v"] = 1;
                using var content = new StringContent(evt.ToJsonString(), Encoding.UTF8, "application/json");
                using var response = await client.PostAsync($"{siteUrl}/api/monitor/{code}", content).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var wait = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
                    skipUntilUtc = DateTime.UtcNow + (wait > TimeSpan.FromMinutes(5) ? TimeSpan.FromMinutes(5) : wait);
                }
                ReportError($"Tarkov Quest Planner refused an event (HTTP {(int)response.StatusCode}).");
            }
            catch (Exception ex)
            {
                // Exception messages can contain the request URL, which holds the code: report the type only.
                ReportError($"Tarkov Quest Planner is unreachable ({ex.GetType().Name}).");
            }
        }

        private static void ReportError(string message)
        {
            // At most one board line per minute so a dead site does not flood the board with position events.
            if (DateTime.UtcNow - lastErrorUtc < TimeSpan.FromMinutes(1))
            {
                return;
            }
            lastErrorUtc = DateTime.UtcNow;
            try { Log?.AddMessage(message, "warning"); } catch { }
        }
    }
}
