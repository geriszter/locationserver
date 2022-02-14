using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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

            StreamWriter sw = new StreamWriter(socketStream);
            StreamReader sr = new StreamReader(socketStream);
            //Console.WriteLine(sr.ReadToEnd());

            string line = sr.ReadLine().Trim();
            string[] commands = line.Split("\"");
            commands[0] = commands[0].Trim();
            if (commands.Length == 1)
            {
                if (personLocation.ContainsKey(commands[0]))
                {
                    sw.WriteLine(personLocation[commands[0]]);
                    sw.Flush();
                    Console.WriteLine($"\"GET {commands[0]}\" OK");
                }
                else 
                {
                    sw.WriteLine("Error: No data");
                    sw.Flush();
                    Console.WriteLine($"\"GET {commands[0]}\" UNKNOWN");
                }
            }
            else if(commands.Length > 1)
            {
                if (personLocation.ContainsKey(commands[0]))
                {

                    personLocation[commands[0]] = commands[1];

                }
                else
                {
                    personLocation.Add(commands[0],commands[1]);
                }
                sw.WriteLine("OK");
                sw.Flush();
                Console.WriteLine($"\"PUT {commands[0]} {commands[1]}\" OK");
            }
        }
        catch
        {
            Console.WriteLine("Connection faild");
        }
    }
}

