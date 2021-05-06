using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimpleHttpServer
{
	class Program
	{
		private static HttpListener _listener;

		static void Main(string[] args)
		{
            int port = 8000;
            if(args.Length == 1 && int.TryParse(args[0], out int portT))
			{
                port = portT;
			}
			else
			{
                Console.WriteLine("ENTER PORT FOR HTTP SERVER: ");
                port = int.Parse(Console.ReadLine());
			}
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] STARTING HTTP SERVER ON PORT {port}");

            _listener = new HttpListener();
            try
            {
                _listener.Prefixes.Add($"http://*:{port}/");
                _listener.Start();
            }
            catch
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{port}/");
                _listener.Start();
            }
            _listener.BeginGetContext(OnContext, null);

            Console.ReadLine();
        }

        private static void OnContext(IAsyncResult ar)
        {
            var ctx = _listener.EndGetContext(ar);
            _listener.BeginGetContext(OnContext, null);

            Console.Out.WriteLineAsync($"[{DateTime.Now:HH:mm:ss.fff}] Handling request {ctx.Request.RawUrl}");

            byte[] buf = GetPageContent(ctx, out string contentType);

            ctx.Response.ContentType = contentType;
            ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
            ctx.Response.OutputStream.Write(buf, 0, buf.Length);
            ctx.Response.OutputStream.Close();
        }

        public static byte[] GetPageContent(HttpListenerContext ctx, out string contentType)
        {
            string rawUrl = ctx.Request.RawUrl;
            rawUrl = rawUrl.Split('?')[0].Split('#')[0];
            string[] pathName = rawUrl.Split('/');

            string FilePath = Path.Combine(pathName);
            if (!File.Exists(FilePath)) FilePath = Path.Combine(FilePath, "index.html");
            contentType = MimeTypes.MimeTypeMap.GetMimeType(FilePath);

            if (File.Exists(FilePath))
			{
                return File.ReadAllBytes(FilePath);
            }
			else
			{
                contentType = "text/html";
                return Encoding.UTF8.GetBytes("<h1>404</h1>");
            }
        }
    }
}
