using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

public class locationserver
{
    static void Main(string[] args)
    {
        runServer();
    }
    static void runServer()
    {
        TcpListener listener;
        Socket connection;
        NetworkStream socketStream;
        try
        {
            Dictionary<string, string> personLocation = new Dictionary<string, string>();
            listener = new TcpListener(IPAddress.Any, 43);
            listener.Start();
            Console.WriteLine("Server started");
            while (true)
            {
                connection = listener.AcceptSocket();
                socketStream = new NetworkStream(connection);
                Console.WriteLine("New Connection");
                doRequest(socketStream, personLocation);
                socketStream.Close();
                connection.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exeption: " + e.ToString());
        }
    }
    static void doRequest(NetworkStream socketStream, Dictionary<string, string> personLocation)
    {
        try
        {
            socketStream.ReadTimeout = 1000;
            socketStream.WriteTimeout = 1000;

            StreamWriter sw = new StreamWriter(socketStream);
            StreamReader sr = new StreamReader(socketStream);
            //Console.WriteLine(sr.ReadToEnd());

            string line = sr.ReadLine();
            string[] commands = line.Split(" ");
            commands[0] = commands[0].Trim();
            if (commands.Length == 1)
            {
                if (personLocation.ContainsKey(commands[0]))
                {
                    string location = personLocation[commands[0]];
                    sw.WriteLine(location);
                    sw.Flush();
                    Console.WriteLine($"[{DateTime.Now}] \"GET {commands[0]}\" OK");
                }
                else 
                {
                    sw.WriteLine("ERROR: no entries found");
                    sw.Flush();
                    Console.WriteLine($"[{DateTime.Now}] \"GET {commands[0]}\" UNKNOWN");
                }
            }
            else if(commands.Length > 1)
            {
                string locationstring = commands[1];
                for (int i = 2; i < commands.Length; i++)
                {
                    locationstring += " " + commands[i];
                }
                if (personLocation.ContainsKey(commands[0]))
                {
                    personLocation[commands[0]] = locationstring;
                }
                else
                {
                    personLocation.Add(commands[0], locationstring);
                }
                sw.WriteLine("OK");
                sw.Flush();
                Console.WriteLine($"[{DateTime.Now}] \"PUT {commands[0]} {locationstring}\" OK");
            }
        }
        catch
        {
            Console.WriteLine("Connection faild");
        }
    }
}

