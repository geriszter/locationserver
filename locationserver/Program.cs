﻿using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class locationserver
{
    //https://stackoverflow.com/questions/4715896/web-server-reading-http-request-from-stream
    static void Main(string[] args)
    {
        runServer();
    }
    static void runServer()
    {
        TcpListener listener;
        Socket connection;
        Handler RequestHandler;
        try
        {
            Dictionary<string, string> personLocation = new Dictionary<string, string>();
            listener = new TcpListener(IPAddress.Any, 43);
            listener.Start(); 
            Console.WriteLine("Server started");
            while (true)
            {
                connection = listener.AcceptSocket();
                RequestHandler = new Handler();
                new Thread(() => RequestHandler.doRequest(connection,personLocation)).Start();
                new Thread(() => RequestHandler.doRequest(connection,personLocation)).Start();

            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exeption: " + e.ToString());
        }
    }
    class Handler
    {
        public void doRequest(Socket connection, Dictionary<string, string> personLocation)
        {
            NetworkStream socketStream;
            socketStream = new NetworkStream(connection);
            string ip = ((IPEndPoint)(connection.RemoteEndPoint)).Address.ToString();
            Console.WriteLine("New Connection "+ip);
            try
            {

                int timeOut = 1000;
                socketStream.ReadTimeout = timeOut;
                socketStream.WriteTimeout = timeOut;

                StreamWriter sw = new StreamWriter(socketStream);
                StreamReader sr = new StreamReader(socketStream);
                string name = null;
                string location = null;
                string response = null;
                bool ched = true;


                string line = null;
                byte[] myReadBuffer = new byte[1048576];
                int numberOfBytesRead = 0;
                StringBuilder myCompleteMessage = new StringBuilder();
                do
                {
                    numberOfBytesRead = socketStream.Read(myReadBuffer);
                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                }
                while (socketStream.DataAvailable);

                line = myCompleteMessage.ToString();

                //while (line==null)
                //{
                //    try
                //    {
                //        int num;
                //        while ((num = sr.Read()) > 0)
                //        {
                //            line += ((char)num);
                //        }
                //    }
                //    catch
                //    {
                //    }
                //}

                string log = $"{ip} - - [{DateTime.Now}]";
                string[] commands = line.Split(" ");
                //GET commands
                if (commands[0] == "GET")
                {
                    if (commands.Length > 2)
                    {
                        //GET HTTP/1.0
                        if (commands[2].Contains("HTTP/1.0"))
                        {
                            ched = false;
                            name = commands[1].Remove(0, 2);
                            location = GetLocation(name, personLocation);

                            if (location != null)
                            {
                                response = $"HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n";
                                log += $" \" GET /{name} HTTP/1.0\" OK";
                            }
                            else
                            {
                                response = "HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                                log += $" \" GET /{name} HTTP/1.0\" UNKNOWN";

                            }
                        }
                        //GET HTTP/1.1
                        else if (commands[2].Contains("HTTP/1.1"))
                        {
                            ched = false;
                            name = commands[1].Remove(0, 7);
                            location = GetLocation(name, personLocation);

                            if (location != null)
                            {
                                response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n";
                                log += $" \" GET /name={name} HTTP/1.1\" OK";
                            }
                            else
                            {
                                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                                log += $" \" GET /name={name} HTTP/1.1\" UNKNOWN";
                            }
                        }
                    }
                    //GET HTTP/0.9
                    else if (commands.Length == 2)
                    {
                        ched = false;
                        //5th character is the name start
                        name = line.Remove(0, 5);
                        name = name.Remove(name.Length - 2);
                        location = GetLocation(name, personLocation);
                        if (location != null)
                        {
                            response = $"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n";
                            log += $" \" GET /{name}\" OK";
                        }
                        else
                        {
                            response = "HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                            log += $" \" GET /{name}\" UNKNOWN";
                        }
                    }
                }
                //PUT HTTP/0.9 
                else if (commands[0] == "PUT" && commands[1].IndexOf("/") == 0 && line.Contains("\r\n\r\n"))
                {
                    ched = false;
                    string[] array = line.Split("\r\n");
                    name = array[0].Remove(0, 5);
                    location = array[array.Length - 2];
                    Console.WriteLine("AddLocation " + location);
                    UpdateAndAdd(name, location, personLocation);

                    response = $"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                    log += $" \" PUT /{name}\" OK";

                }
                else if (commands[0] == "POST")
                {
                    Console.WriteLine("POST");
                    if (commands.Length > 2)
                    {
                        //"HTTP/1.0"
                        if (commands[2].Contains("HTTP/1.0"))
                        {
                            ched = false;
                            name = commands[1].Remove(0, 1);
                            string[] array = line.Split("\r\n");
                            location = array[array.Length - 1];

                            UpdateAndAdd(name, location, personLocation);
                            response = "HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                            log += $" \" POST /{name} HTTP/1.0\" OK";
                        }
                        //"HTTP/1.1"
                        else if (commands[2].Contains("HTTP/1.1"))
                        {
                            ched = false;
                            int locationIndex = line.IndexOf("&location=");
                            int nameIndex = line.IndexOf("name=");
                            name = line.Remove(locationIndex);
                            name = name.Remove(0, (5 + nameIndex));

                            location = line.Remove(0, (10 + locationIndex));
                            UpdateAndAdd(name, location, personLocation);
                            response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                            log += $" \" POST /name={name}&location={location}  HTTP/1.1\" OK";
                        }
                    }
                }

                if (commands.Length == 1)
                {
                    name = commands[0];
                    name = name.Remove(name.Length - 2);
                    location = GetLocation(name, personLocation);
                    if (location != null)
                    {
                        response = location;
                        log += $" \" GET {name}\" OK";
                    }
                    else
                    {
                        response = "ERROR: no entries found";
                        log += $" \" GET {name}\" UNKNOWN";
                    }
                }
                else if (commands.Length > 1 && ched)
                {
                    name = commands[0];
                    location = commands[1];
                    for (int i = 2; i < commands.Length; i++)
                    {
                        location += " " + commands[i];
                    }
                    location = location.Remove(location.Length - 2);
                    UpdateAndAdd(name, location, personLocation);
                    response = "OK";
                    log += $"\"{name} {location} WHOIS\" OK";
                }
                Console.WriteLine(log);
                sw.WriteLine(response);
                sw.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection faild");
                Console.WriteLine(e);
            }
            finally 
            {
                socketStream.Close();
                connection.Close();
                Console.WriteLine("[Disconnected]");
            }
        }

        static void UpdateAndAdd(string name, string location, Dictionary<string, string> personLocation)
        {
            location = location.Trim(new Char[] { '\"', '\'', '`', '\\', '.' });
            if (personLocation.ContainsKey(name))
            {
                personLocation[name] = location;
            }
            else
            {
                personLocation.Add(name, location);
            }
            //Console.WriteLine($"[{DateTime.Now}] \"PUT {name} {location}\" OK");
        }

        static string GetLocation(string name, Dictionary<string, string> personLocation)
        {

            if (personLocation.ContainsKey(name))
            {
                string location = personLocation[name];
                //Console.WriteLine($"[{DateTime.Now}] \"GET {name}\" OK");
                return location;
            }
            else
            {
                //Console.WriteLine($"[{DateTime.Now}] \"GET {name}\" UNKNOWN");
                return null;
            }
        }
    }

}

