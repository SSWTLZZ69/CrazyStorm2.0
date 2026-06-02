/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.IO;
using System.Text;

namespace CrazyStorm.McpServer
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                if (args.Length > 0)
                {
                    string command = args[0].TrimStart('-', '/').ToLowerInvariant();
                    switch (command)
                    {
                        case "help":
                        case "h":
                        case "?":
                            PrintHelp();
                            return 0;

                        case "version":
                            Console.WriteLine("CrazyStorm MCP Server 0.1.0");
                            return 0;

                        case "self-test":
                        case "selftest":
                            return RunSelfTest();

                        case "stdio":
                            RunStdio();
                            return 0;

                        default:
                            Console.Error.WriteLine("Unknown option: " + args[0]);
                            Console.Error.WriteLine("Run with --help for usage.");
                            return 2;
                    }
                }

                if (!Console.IsInputRedirected)
                {
                    PrintInteractiveHelp();
                    return 0;
                }

                RunStdio();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static void RunStdio()
        {
            new McpProtocol(new CrazyStormTools()).Run(Console.In, Console.Out, Console.Error);
        }

        private static int RunSelfTest()
        {
            var input = string.Join(Environment.NewLine, new[]
            {
                "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{\"protocolVersion\":\"2025-06-18\",\"capabilities\":{},\"clientInfo\":{\"name\":\"self-test\",\"version\":\"0\"}}}",
                "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"tools/list\",\"params\":{}}",
                string.Empty
            });
            var output = new StringWriter();
            var error = new StringWriter();

            new McpProtocol(new CrazyStormTools()).Run(new StringReader(input), output, error);

            string text = output.ToString();
            if (!text.Contains("crazy_storm_project_summary") ||
                !text.Contains("crazy_storm_create_project") ||
                !text.Contains("crazy_storm_add_multi_emitter"))
            {
                Console.Error.WriteLine("Self-test failed. MCP tool list did not include expected tools.");
                Console.Error.WriteLine(text);
                string err = error.ToString();
                if (!string.IsNullOrWhiteSpace(err)) Console.Error.WriteLine(err);
                return 1;
            }

            Console.WriteLine("CrazyStorm MCP self-test passed.");
            Console.WriteLine("The server can initialize and list inspection/editing tools.");
            return 0;
        }

        private static void PrintInteractiveHelp()
        {
            PrintHelp();
            Console.WriteLine();
            Console.WriteLine("This window is only for help/testing. MCP clients start this executable in the background.");
            Console.WriteLine("Press any key to close...");
            Console.ReadKey(true);
        }

        private static void PrintHelp()
        {
            string exe = AppDomain.CurrentDomain.FriendlyName;
            Console.WriteLine("CrazyStorm MCP Server");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  " + exe + " --stdio       Run as a stdio MCP server for MCP clients.");
            Console.WriteLine("  " + exe + " --self-test   Verify initialize/tools-list locally.");
            Console.WriteLine("  " + exe + " --version     Print version.");
            Console.WriteLine("  " + exe + " --help        Show this help.");
            Console.WriteLine();
            Console.WriteLine("Normal users do not manually keep this window open.");
            Console.WriteLine("Configure your MCP client with this executable path; the client will launch it when needed.");
            Console.WriteLine();
            Console.WriteLine("Codex install helper:");
            Console.WriteLine("  powershell -ExecutionPolicy Bypass -File .\\Install-CodexMcp.ps1");
            Console.WriteLine();
            Console.WriteLine("Example client command:");
            Console.WriteLine("  " + System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }
}
