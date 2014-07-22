using System;
using NLog;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Radish.Utilities
{
    abstract public class ProcessInvoker
    {
        static private readonly Logger logger = LogManager.GetCurrentClassLogger();


        public string OutputString { get { return outputBuffer.ToString(); } }
        public string ErrorString { get { return errorBuffer.ToString(); } }

        protected ProcessInvoker()
        {
        }

        public void Run(string commandLine, params object[] args)
        {
            if (autoCheckProcessPath)
            {
                bool canRun = false;
                try
                {
                    canRun = CheckProcessPath(ProcessPath);
                }
                catch (Exception e)
                {
                    logger.Warn("Error invoking {0}: {1}", ProcessName, e);
                }

                if (!canRun)
                {
                    throw new InvalidOperationException("Unable to run " + ProcessName);
                }

                outputBuffer.Clear();
                errorBuffer.Clear();
            }

            Invoke(commandLine, args);
        }

        abstract protected string ProcessName { get; }
        abstract protected string ProcessPath { get; }
        abstract protected bool CheckProcessPath(string path);

        static private bool autoCheckProcessPath = true;
        private StringBuilder outputBuffer = new StringBuilder(1024);
        private StringBuilder errorBuffer = new StringBuilder(1024);

        protected void Invoke(string commandLine, params object[] args)
        {
            // Arbitrary timeout for now...
            const int timeout = 10 * 1000;

            var psi = new ProcessStartInfo 
            {
                FileName = ProcessPath,
                Arguments = String.Format(commandLine, args),
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            // Mac specific...
            psi.EnvironmentVariables["PATH"] = Environment.GetEnvironmentVariable("PATH") + ":/usr/local/bin";

            logger.Info("Running {0}: {1} {2}", ProcessName, psi.FileName, psi.Arguments);

            using (var outputWaitHandle = new AutoResetEvent(false))
            using (var errorWaitHandle = new AutoResetEvent(false))
            {
                using (var process = new Process())
                {
                    process.StartInfo = psi;
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            outputBuffer.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            errorBuffer.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        var exitCode = process.ExitCode;
                        if (exitCode != 0)
                        {
                            logger.Warn("{0} failed with message: {1}", ProcessName, ErrorString);
                            if (exitCode != 1)
                            {
                                logger.Warn("{0} exit code: {1}", ProcessName, exitCode);
                            }
                            throw new InvalidOperationException(ProcessName + " returned an error: " + exitCode + "; " + ErrorString);
                        }
                    }
                }
            }
        }
    }
}
