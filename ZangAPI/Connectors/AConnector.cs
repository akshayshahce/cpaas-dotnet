using Newtonsoft.Json;
using RestSharp;
using AvayaCPaaS.ConnectionManager;
using AvayaCPaaS.Exceptions;
using System.IO;
using System;
using System.Configuration;

namespace AvayaCPaaS.Connectors
{
    /// <summary>
    /// Abstract connector
    /// </summary>
    public abstract class AConnector
    {
        /// <summary>
        /// Gets or sets the HTTP provider.
        /// </summary>
        /// <value>
        /// The HTTP provider.
        /// </value>
        public IHttpProvider HttpProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AConnector"/> class.
        /// </summary>
        /// <param name="httpProvider">The HTTP provider.</param>
        protected AConnector(IHttpProvider httpProvider)
        {
            this.HttpProvider = httpProvider;
        }

        public static void VerifyDir(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
            catch { }
        }

        public static void Logger(string lines)
        {
            string path = ConfigurationManager.AppSettings["CPAAS_LOG_PATH"];
            VerifyDir(path);
            // MM-DD-YYYY-logs.txt
            string fileName = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Year.ToString() + "-Logs.txt";
            try
            {
                StreamWriter file = new StreamWriter(path + fileName, true);
                file.WriteLine(DateTime.Now.ToString() + ": " + lines);
                file.Close();
            }
            catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Returns the or throw exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">The response.</param>
        /// <returns>Returns instance of class T or throws exception</returns>
        public T ReturnOrThrowException<T>(IRestResponse response)
        {
            var status = (int)response.StatusCode;

            if (status >= 400) {
                Logger(response.Content);
                throw JsonConvert.DeserializeObject<CPaaSException>(response.Content);
            }
            return JsonConvert.DeserializeObject<T>(response.Content);
        }
    }
}
