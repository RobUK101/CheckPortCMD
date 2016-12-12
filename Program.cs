using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace CheckPortCMD
{
    class Program
    {
        static void doHelp()
        {
            Console.WriteLine("No arguments specified");
            Console.WriteLine("  /IP and /PORT are Mandatory )");
            Console.WriteLine("  /OUTPUT can be 0, 1, 2 or 3 and represents:");
            Console.WriteLine("    Numerical, Boolean, Compliance Statement or");
            Console.WriteLine("    Silent with ERRORLEVEL set");
            Console.WriteLine("  /TIMEOUT in seconds");
            Console.WriteLine("  /SUPPRESS sets ERRORLEVEL to 0 no matter the result");
            Console.WriteLine(" ");
            Console.WriteLine(" EXAMPLE: CheckPortCMD.EXE /IP:192.168.1.1 /PORT:80 /OUTPUT:1 /TIMEOUT:5");
        }

        static void Main(string[] args)
        {
            bool suppressERRORLEVEL = false;
            
            // handle the parameters

            var myArguments = new Dictionary<string, string>();            

            foreach (var argument in Environment.GetCommandLineArgs())
            {
                try
                {
                    if (argument.Contains(@"/?"))
                    {
                        doHelp();

                        Environment.Exit(0);
                    }

                    if (argument.Contains(":"))
                    {
                        var elements = argument.ToLower().Split(':');

                        myArguments.Add(elements[0].ToLower().Replace("/", ""), elements[1]);
                    }
                }
                catch (Exception ee)
                {
                    /// Could not split
                    /// the command line
                }
            }

            if (myArguments.Count > 0)
            {
                string myipAddress = "";

                int myPort = 0;

                int myTimeout = 5000;

                bool iConnected = false;

                bool iSent = false;

                int outputFormat = 0; // 0 = Boolean, 1 = Integer (0 success, 1 failure), 2 = Compliance (Compliant\Noncompliant)                

                if (myArguments.Keys.Contains("ip"))
                {
                    myipAddress = myArguments["ip"];
                }

                if (myArguments.Keys.Contains("port"))
                {
                    myPort = Convert.ToInt32(myArguments["port"]);
                }

                if (myArguments.Keys.Contains("suppress"))
                {
                    suppressERRORLEVEL = true;
                }

                if (myArguments.Keys.Contains("output"))
                {
                    outputFormat = Convert.ToInt32(myArguments["output"]);
                }

                if (myArguments.Keys.Contains("timeout"))
                {
                    try
                    {
                        myTimeout = Convert.ToInt32(myArguments["timeout"]) * 1000;
                    }
                    catch (Exception ee)
                    {

                    }
                }

                if (myipAddress != "" && myPort > 0)
                {
                    // Perform the scan

                    System.Net.Sockets.Socket ipSocket;

                    ipSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                    ipSocket.SendTimeout = myTimeout; // // Set Port Send timeout

                    ipSocket.ReceiveTimeout = myTimeout; // Set Port Receive timeout

                    ipSocket.Blocking = false;

                    try
                    {

                        IAsyncResult result = ipSocket.BeginConnect(myipAddress, myPort, null, null);

                        bool success = result.AsyncWaitHandle.WaitOne(myTimeout, true);

                        if (success)
                        {
                            iConnected = true;

                            try
                            {
                                Byte[] sendBytes = Encoding.UTF8.GetBytes("?");

                                int totalbytesSent = ipSocket.Send(sendBytes);

                                iSent = true;

                                ipSocket.EndConnect(result);

                                ipSocket.Close();
                            }
                            catch (SocketException ee)
                            {
                                // Failed to send do nothing
                            }

                            // Return the results

                            if (iConnected && iSent)
                            {
                                if (outputFormat == 0)
                                {
                                    Console.WriteLine("0");
                                }

                                if (outputFormat == 1)
                                {
                                    Console.WriteLine("TRUE");
                                }

                                if (outputFormat == 2)
                                {
                                    Console.WriteLine("COMPLIANT");
                                }

                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            if (outputFormat == 0)
                            {
                                Console.WriteLine("1");
                            }

                            if (outputFormat == 1)
                            {
                                Console.WriteLine("FALSE");
                            }

                            if (outputFormat == 2)
                            {
                                Console.WriteLine("NONCOMPLIANT");
                            }

                            if (suppressERRORLEVEL)
                            {
                                Environment.Exit(0);
                            }
                            else
                            {
                                Environment.Exit(1);
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        if (suppressERRORLEVEL)
                        {
                            Environment.Exit(0);
                        }
                        else
                        {
                            Environment.Exit(1);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid arguments specified");

                    if (suppressERRORLEVEL)
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Environment.Exit(1);
                    }
                }
            }
            else
            {
                doHelp();

                if (suppressERRORLEVEL)
                {
                    Environment.Exit(0);
                }
                else
                {
                    Environment.Exit(1);
                }
            }
        }
    }
}
