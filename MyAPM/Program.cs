using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MyAPM
{
    public class Program
    {
        static string file = Environment.CurrentDirectory + "/download/";
        static byte[] buffer = new byte[1024];

        static string url = ConfigurationManager.AppSettings["urlPath"];
        private static FileStream fileStream;
        private static void Init()
        {
            string[] filesName = url.Split('/');
            string fileName = filesName[filesName.Length - 1];
            file += fileName;

            fileStream = new FileStream(file, FileMode.OpenOrCreate);
        }
        static void Main(string[] args)
        {
            Init();
            //DownloadFileAsync(url);

            DownLoadFilesAsync(url);
            Console.ReadKey();
        }

        #region APM


        public static void DownloadFileAsync(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.BeginGetResponse(ResponseCallBack, httpWebRequest);

            Console.ReadLine();
        }

        private static void ResponseCallBack(IAsyncResult ar)
        {
            HttpWebRequest tempRequest = ar.AsyncState as HttpWebRequest;

            if (tempRequest == null)
            {
                throw new ArgumentNullException("ar 为空！");
            }

            var tempResponse = tempRequest.EndGetResponse(ar);

            Stream responseStream = tempResponse.GetResponseStream();
            responseStream.BeginRead(buffer, 0, buffer.Length, ReadCallBack, responseStream);

        }

        private static void ReadCallBack(IAsyncResult asyncResult)
        {
            try
            {
                Stream httpWebRequest = asyncResult.AsyncState as Stream;

                int size = httpWebRequest.EndRead(asyncResult);

                if (size > 0)
                {
                    fileStream.Write(buffer, 0, size);
                    httpWebRequest.BeginRead(buffer, 0, buffer.Length, ReadCallBack, httpWebRequest);


                }
                else
                {
                    Console.WriteLine("完毕！");
                    fileStream.Close();
                }
            }
            catch (Exception e)
            {

            }
        }

        #endregion


        #region Async Await  

        public static async void DownLoadFilesAsync(string url)
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            HttpClient httpClient = new HttpClient();
            var stream = await httpClient.GetStreamAsync(url);

            while (true)
            {
                int size = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (size > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, size);
                }
                else
                {
                    fileStream.Close();
                    Console.WriteLine("完成 await");
                    stopwatch.Stop();

                    Console.WriteLine(stopwatch.ElapsedMilliseconds);
                    return;
                }
            }



        }
        #endregion
    }
}
