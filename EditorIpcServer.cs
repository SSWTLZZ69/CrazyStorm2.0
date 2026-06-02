/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrazyStorm.Core;
using IoFile = System.IO.File;

namespace CrazyStorm
{
    public sealed class EditorIpcServer : IDisposable
    {
        public const string PipeName = "CrazyStorm2.0.Mcp";

        readonly Main mainWindow;
        CancellationTokenSource cancellation;
        Task listenTask;

        public EditorIpcServer(Main mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void Start()
        {
            if (listenTask != null) return;

            cancellation = new CancellationTokenSource();
            listenTask = Task.Factory.StartNew(
                () => ListenLoop(cancellation.Token),
                cancellation.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void Dispose()
        {
            if (cancellation == null) return;

            cancellation.Cancel();
            WakeListener();

            try
            {
                listenTask?.Wait(500);
            }
            catch (AggregateException)
            {
            }

            cancellation.Dispose();
            cancellation = null;
            listenTask = null;
        }

        void ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None))
                    {
                        pipe.WaitForConnection();
                        if (token.IsCancellationRequested) return;

                        using (var reader = new StreamReader(pipe, Encoding.UTF8))
                        using (var writer = new StreamWriter(pipe, new UTF8Encoding(false)) { AutoFlush = true })
                        {
                            string command = reader.ReadLine();
                            writer.WriteLine(HandleCommand(command));
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (token.IsCancellationRequested) return;
                    LogHelper.Error("Editor IPC error: " + ex);
                }
            }
        }

        string HandleCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return "ERROR\tEmpty command";

            string[] parts = command.Split(new[] { '\t' }, 2);
            string name = parts[0].Trim().ToLowerInvariant();
            string argument = parts.Length > 1 ? parts[1] : string.Empty;

            switch (name)
            {
                case "ping":
                    return "OK\tpong";

                case "open":
                    return OpenProject(argument);

                default:
                    return "ERROR\tUnknown command: " + parts[0];
            }
        }

        string OpenProject(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "ERROR\tProject path is required";
            if (!IoFile.Exists(path)) return "ERROR\tProject file not found: " + path;

            bool opened = false;
            string error = null;
            mainWindow.Dispatcher.Invoke(() =>
            {
                if (mainWindow.HasUnsavedChanges)
                {
                    error = "Editor has unsaved changes. Save or discard them in the editor before reloading from MCP.";
                    return;
                }

                opened = mainWindow.TryOpenFile(path);
                if (opened)
                {
                    mainWindow.Activate();
                    mainWindow.Focus();
                }
            });

            if (!string.IsNullOrEmpty(error)) return "ERROR\t" + error;
            return opened ? "OK\topened" : "ERROR\tEditor rejected the project";
        }

        void WakeListener()
        {
            try
            {
                using (var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut))
                    pipe.Connect(50);
            }
            catch
            {
            }
        }
    }
}
