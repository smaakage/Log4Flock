using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using log4net.Appender;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO.Compression;
using log4net.Core;
using log4net.Layout;

namespace Log4Flock
{
    public class FlockAppender : AppenderSkeleton
    {
        private readonly Process _currentProcess = Process.GetCurrentProcess();

        /// <summary>
        /// Flock webhook URL, with token.
        /// </summary>
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Indicates whether or not to include additional details in message attachments.
        /// </summary>
        public bool AddAttachment { get; set; }

        /// <summary>
        /// Indicates whether or not to make it possible to forward the message
        /// </summary>
        public bool AllowForward { get; set; }

        /// <summary>
        /// Indicates whether or not to open up the stack trace in a modal
        /// </summary>
        public bool OpenStackTraceInModal { get; set; }

        /// <summary>
        /// Stack trace view url for iframe in the modal popup window
        /// </summary>
        public string StackTraceModalUrl { get; set; }

        /// <summary>
        /// Indicates whether or not add download button for stack trace
        /// </summary>
        public bool DownloadStackTrace { get; set; }

        /// <summary>
        /// Indicates whether or not to include the exception traces as fields on message attachments.
        /// Requires AddAttachment be true.
        /// </summary>
        public bool AddExceptionTraceField { get; set; }

        /// <summary>
        /// Indicates whether or not to append the logger name to the Flock username.
        /// </summary>
        public bool UsernameAppendLoggerName { get; set; }

        /// <summary>
        /// The optional proxy configuration for outgoing flock posts
        /// </summary>
        public string Proxy { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            // Initialze the Flock client
            var flockClient = new FlockClient(WebhookUrl.Expand());
            var attachments = new List<Attachment>();

            if (AddAttachment)
                attachments.Add(BuildAttachment(loggingEvent));

            var formattedMessage = Layout != null ? Layout.FormatString(loggingEvent) : loggingEvent.RenderedMessage;
            flockClient.PostMessageAsync(formattedMessage, Proxy, attachments);
        }

        protected Attachment BuildAttachment(LoggingEvent loggingEvent)
        {
            var exception = loggingEvent.ExceptionObject;
            var stackTrace = string.Empty;

            // Set fallback string
            var attachment = new Attachment(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss.fff"));

            // Determine attachment color
            switch (loggingEvent.Level.DisplayName.ToLowerInvariant())
            {
                case "debug":
                case "info":
                    attachment.Color = "#E4E4E4";
                    break;
                case "warn":
                    attachment.Color = "#FFC300";
                    break;
                case "error":
                case "fatal":
                    attachment.Color = "#D00000";
                    break;
            }

            attachment.Forward = AllowForward;

            var flockMlString = $"<strong>Machine:</strong> <em>{Environment.MachineName}</em><br />";
            flockMlString += $"<strong>Process Name:</strong> <em>{_currentProcess.ProcessName}</em><br />";

            if (!UsernameAppendLoggerName)
                flockMlString += $"<strong>Logger: </strong> <em>{loggingEvent.LoggerName}</em><br />";

            //// Add exception fields if exception occurred
            if (exception != null)
            {
                flockMlString += $"<strong>Exception Type:</strong> <em>{exception.GetType().Name}</em><br />";
                if (AddExceptionTraceField && !string.IsNullOrWhiteSpace(exception.StackTrace))
                {
                    var parts = exception.StackTrace.SplitOn(1990)
                        .ToArray(); // Split call stack into consecutive fields of ~2k characters
                    for (var idx = parts.Length - 1; idx >= 0; idx--)
                    {
                        var name = "Exception Trace" + (idx > 0 ? $" ({idx + 1})" : null);
                        stackTrace += $"{parts[idx].Replace("```", "'''")}";

                        if (OpenStackTraceInModal && !string.IsNullOrWhiteSpace(StackTraceModalUrl)) continue;
                        flockMlString += $"<br /><strong>{name}</strong><br />";
                        flockMlString += $"{stackTrace}<br />";
                    }
                }

                flockMlString = flockMlString.Insert(0, $"<strong>Exception Message</strong><br />{exception.Message}<br />{exception.InnerException}<br />");
            }

            attachment.Views = new View(flockMl: flockMlString);

            if (exception == null) return attachment;

            if (string.IsNullOrWhiteSpace(StackTraceModalUrl) || string.IsNullOrWhiteSpace(stackTrace)) return attachment;


            if (OpenStackTraceInModal)
                attachment.Buttons = new List<Button> { BuildButton(stackTrace) };

            if (DownloadStackTrace)
                attachment.Downloads = new List<Download> { BuildDownload(_currentProcess.ProcessName, stackTrace) };

            return attachment;
        }

        protected Download BuildDownload(string processName, string stackTrace, string mime = "text/plain", byte size = 0)
        {
            var fileName = $"StackTrace-{_currentProcess.ProcessName}-{DateTime.Now:MM-dd-yyyy_HH-mm-ss}.txt";
            return new Download
            {
                Filename = fileName,
                Mime = mime,
                Size = size,
                Src = $"{StackTraceModalUrl}/StackTrace/Download?fileName={fileName}&stacktrace={Extensions.Base64EncodeAndCompress(stackTrace)}"
            };
        }

        protected Button BuildButton(string stackTrace, string type = "openWidget", string desktopType = "modal", string mobileType = "modal", string icon = "")
            => new Button {
                Name = "Stack trace",
                Action = new ButtonAction
                {
                    Type = "openWidget",
                    DesktopType = "modal",
                    MobileType = "modal",
                    Url =
                        $"{StackTraceModalUrl}/StackTrace/Index?stacktrace={Extensions.Base64EncodeAndCompress(stackTrace)}"
                },
                Icon = "",
                Id = 1
            };
    }

    internal static class Extensions
    {
        public static string Expand(this string text) => text != null
            ? Environment.ExpandEnvironmentVariables(text)
            : null;

        public static IEnumerable<string> SplitOn(this string text, int numChars)
        {
            var splitOnPattern = new Regex($@"(?<line>.{{1,{numChars}}})([\r\n]|$)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return splitOnPattern.Matches(text).OfType<Match>().Select(m => m.Groups["line"].Value);
        }

        public static string FormatString(this ILayout layout, LoggingEvent loggingEvent)
        {
            using (var writer = new StringWriter())
            {
                layout.Format(writer, loggingEvent);
                return writer.ToString();
            }
        }

        public static string Base64EncodeAndCompress(string text) => Base64Encode(Compress(text));

        public static string Base64Encode(byte[] data) => Convert.ToBase64String(data);

        public static byte[] Compress(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }
    }
}