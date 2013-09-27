using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3TraceListener
{
    /// <summary>
    /// The idea is to hack the normal log procedure for applications and instead of writing Logs to either databases or
    /// to files which are notorious for never ever being read, we write it to an S3 Bucket for Big Data analysis.
    /// You must call Trace.Close() in the main execution of your program else it never gets written.
    /// </summary>
    /// <remarks>Rodney Hawkins, New Media Labs, rodneyhawkins@gmail.com</remarks>
    public class S3TraceListener : System.Diagnostics.DefaultTraceListener
    {
        private EnvironmentAWSCredentials _amazonConfig = null;
        private AmazonS3Client _s3Client = null;
        private List<string> _stringFile = null;
        private string _bucketName = null;

        public void WriteToS3(string command)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Writing to S3 Log File Started, from a {0} command.", command));
            PutObjectRequest request = new PutObjectRequest();
            request.Key = LogFileName;
            var memoryStream = new MemoryStream();
            using (memoryStream)
            {
                using (var writer = new StreamWriter(memoryStream, UTF8Encoding.UTF8, 8192, true))
                {
                    // Various for loops etc as necessary that will ultimately do this:
                    foreach (var line in _stringFile)
                    {
                        writer.WriteLine(line);
                        writer.Flush();
                        //System.Diagnostics.Trace.WriteLine(line);
                    }

                }
                long size = memoryStream.Length;
                memoryStream.Position = 0;
                request.InputStream = memoryStream;
                request.WithMetaData("ContentLength", size.ToString());
                request.BucketName = _bucketName;

                _s3Client.PutObject(request);
            }
            System.Diagnostics.Trace.WriteLine(string.Format("Writing to S3 Log File Completed, from a {0} command.",command));
        }


        public override void Flush()
        {
            WriteToS3("flush");
        }

        public override void Close()
        {
            WriteToS3("close");
        }

        protected override void Dispose(bool disposing)
        {
            WriteToS3("disposal");
            base.Dispose(disposing);
        }

        public string GetUnixTime()
        {
            return ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();
        }
        public S3TraceListener(string initializeData)
        {
            string[] commands = initializeData.Split(',');
            foreach (var command in commands)
            {
                string[] subcommand = command.Split('=');
                switch (subcommand[0])
                {
                    case "logfile":
                        LogFileName = string.Format("{0}.{1}.log", GetUnixTime(), subcommand[1]);
                        break;
                    case "bucket":
                        _bucketName = subcommand[1];
                        break;
                }
            }

            if (LogFileName == null || _bucketName == null)
            {
                throw new Exception("Not valid parameters. Pass logfile=,bucketname=.");
            }

            if (_amazonConfig == null)
            {
                _amazonConfig = new EnvironmentAWSCredentials();
                _s3Client = new AmazonS3Client(_amazonConfig);
                _stringFile = new List<string>();

            }   
             
        }
        public override void WriteLine(string message, string category)
        {

            _stringFile.Add(string.Format("{0},{1},{2}", GetUnixTime(), category, message));
        }

        public override void WriteLine(string message)
        {
            _stringFile.Add(string.Format("{0},{1}", GetUnixTime(), message));
        }

        public override void WriteLine(object o)
        {
            _stringFile.Add(string.Format("{0},{1}", GetUnixTime(), o));
        }
    }
}
