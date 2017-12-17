using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Log4Flock.Web.Controllers
{
    public class StackTraceController : Controller
    {
        public ActionResult Index(string stacktrace)
        {
            ViewBag.Title = "Stack Trace";

            return View("Index", model: Base64DecodeAndDeCompress(stacktrace));
        }

        public ActionResult Download(string stacktrace, string fileName)
        {
            var decodedData = Base64DecodeAndDeCompress(stacktrace);
            return File(Encoding.ASCII.GetBytes(decodedData), System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public static string Base64DecodeAndDeCompress(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return null;
            data = data.Replace(' ', '+');

            var byteData = Convert.FromBase64String(data);
            using (var compressedStream = new MemoryStream(byteData))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                var buffer = new byte[4096];
                int read;

                while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    resultStream.Write(buffer, 0, read);
                }

                return System.Text.Encoding.UTF8.GetString(resultStream.ToArray());
            }
        }
    }
}
