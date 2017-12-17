using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Log4Flock {
    /// <summary>
    /// Simple client for Flock using incoming webhooks.
    /// </summary>
    public class FlockClient {
        private readonly Uri _uri;
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly ArrayList _requests = ArrayList.Synchronized(new ArrayList(4));

        /// <summary>
        /// Creates a new instance of FlockClient.
        /// </summary>
        /// <param name="urlWithAccessToken">The incoming webhook URL with token.</param>
        public FlockClient(string urlWithAccessToken) {
            _uri = new Uri(urlWithAccessToken);
        }

        /// <summary>
        /// Post a message to Flock.
        /// </summary>
        /// <param name="text">The text of the message.</param>
        /// <param name="proxyAddress">If provided, uses this proxy address when posting payloads.</param>
        /// <param name="attachments">Optional collection of attachments.</param>
        /// <param name="linknames">Whether or not to link names in the Flock message.</param>
        /// <param name="buttons"></param>
        /// <param name="downloads"></param>
        public void PostMessageAsync(string text, string proxyAddress, List<Attachment> attachments = null) {
            var payload = BuildPayload(text, attachments);
            PostPayloadAsync(payload, proxyAddress);
        }

        /// <summary>
        /// Builds a payload for Flock.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="attachments"></param>
        /// <param name="linknames"></param>
        /// <param name="buttons"></param>
        /// <param name="downloads"></param>
        /// <returns></returns>
        private static Payload BuildPayload(string text, List<Attachment> attachments = null) {

            var payload = new Payload {
                Text = text,
                Attachments = attachments
            };

            return payload;
        }

        /// <summary>
        /// Posts a payload to Flock.
        /// </summary>
        private void PostPayloadAsync(Payload payload, string proxyAddress) {
            var data = JsonSerializeObject(payload);
            PostPayloadAsync(data, proxyAddress);
        }

        protected virtual void PostPayloadAsync(string json, string proxyAddress) {
            HttpWebRequest request = null;

            try {
                request = (HttpWebRequest)WebRequest.Create(_uri);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                if (!string.IsNullOrEmpty(proxyAddress))
                {
                    Uri uri = new Uri(proxyAddress);
                    request.Proxy = new WebProxy(uri);
                }

                var data = _encoding.GetBytes(json);
                request.ContentLength = data.Length;
                request.Method = "POST";
                request.ContentType = "application/json";

                _requests.Add(request);

                request.BeginGetRequestStream(OnGetRequestStreamCompleted, AsyncArgs(request, data));
            }
            catch (Exception localException) {
                OnWebPostError(request, localException);
            }
        }

        private void OnWebPostError(WebRequest request, Exception e) {
            if (request != null) _requests.Remove(request);
        }

        private static object[] AsyncArgs(params object[] args) {
            return args;
        }

        private void OnGetRequestStreamCompleted(IAsyncResult ar) {
            if (ar == null) throw new ArgumentNullException(nameof(ar));
            var args = (object[])ar.AsyncState;
            OnGetRequestStreamCompleted(ar, (WebRequest)args[0], (byte[])args[1]);
        }

        private void OnGetRequestStreamCompleted(IAsyncResult ar, WebRequest request, byte[] data)
        {
            try {
                using (var output = request.EndGetRequestStream(ar)) {
                    output.Write(data, 0, data.Length);
                }
                request.BeginGetResponse(OnGetResponseCompleted, request);
            }
            catch (Exception e) {
                OnWebPostError(request, e);
            }
        }

        private void OnGetResponseCompleted(IAsyncResult ar) {
            if (ar == null) throw new ArgumentNullException(nameof(ar));
            OnGetResponseCompleted(ar, (WebRequest)ar.AsyncState);
        }

        private void OnGetResponseCompleted(IAsyncResult ar, WebRequest request) {
            try {
                request.EndGetResponse(ar).Close(); // Not interested; assume OK
                _requests.Remove(request);
            }
            catch (Exception e) {
                OnWebPostError(request, e);
            }
        }

        private static string JsonSerializeObject(object obj) {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
