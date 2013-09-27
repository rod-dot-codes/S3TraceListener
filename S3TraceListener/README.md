﻿S3 C# Trace Listerner
=====

Apparently, what does it do?
----

Simply put this is a class library that uses a Trace Listener and writes all the Debug and Trace messages generated to a S3 bucket specified.

I did this because it was becoming impossible to keep track of where every log was being written to. It would be useful to analyse data generated by such a method to build a Dashboard of some sorts.

Use this at your own risk.

You are free to contribute.

Installation Instructions
-------
Import the S3 Trace Listener solution.
Add this to the *config.xml* of the Solution.

```xml

	<system.diagnostics>
		<trace autoflush="false" indentsize="4">
			<listeners>
				<add name="Listener" type="S3TraceListener.S3TraceListener,S3TraceListener" initializeData="logfile=<name_of_log>,bucket=<bucket_name>" />
			</listeners>
		</trace>
	</system.diagnostics>
	<appSettings>
		<add key="AWSAccessKey" value="<aws_access_key>" />
		<add key="AWSSecretKey" value="<aws_secret_key>" />
	</appSettings>
    ```
Call Trace.Close() in your main application once it has reached the end of execution. Of course, any exceptions raised and not caught, do not get written to the log file. You could flush on every message, but that would be costly performance wise. If you want to refactor it for me, I'ld be more than happy. :+1:

```cs

using System.Diagnostics;

        static void Main(string[] args)
        {
            using System.Diagnostics;

            Trace.Close();

        }
     
     
     ```
     
  And one last thing, it of course needs the AWS SDK for .NET. You can download that using NuGet.