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
            string name = null;
            string location = null;
            string response = "OK";


            string line = sr.ReadLine();
            string[] commands = line.Split(" ");

            //h9 HTTP/0.9
            if (line.Contains("GET \\"))
            {
                // 5th character is the name start
                name = line.Remove(0, 5);
                location = GetLocation(name, personLocation);
                if (location != null)
                {
                    response = $"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n";
                }
                else
                {
                    response = "HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                }
            }
            else if (line.Contains("PUT \\"))
            {
                name = line.Remove(0, 5);
                int iOfLoc = line.IndexOf("\n") + 2;
                location = line.Remove(0, iOfLoc);
                UpdateAndAdd(name, location, personLocation);
                response = $"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n";
            }
            //h0 HTTP/1.0
            else if (line.Contains("GET \\?") && line.Contains(" HTTP/1.0"))
            {
                name = commands[1].Remove(0, 2);
                location = GetLocation(name, personLocation);

                if (location != null)
                {
                    response = $"HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n";
                }
                else
                {
                    response = "HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                }
            }
            else if (line.Contains("POST /") && line.Contains(" HTTP/1.0"))
            {
                name = commands[1].Remove(0,1);
                string[] temp = line.Split("\n");
                location = temp[temp.Length - 1];
                UpdateAndAdd(name, location, personLocation);
                response = "HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n";
            }
            //h1 HTTP/1.1
            else if (line.Contains("GET \\?") && line.Contains(" HTTP/1.1"))
            {
                name = commands[1].Remove(0, 7);
                location = GetLocation(name, personLocation);

                if (location != null)
                {
                    response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n<optional header lines>\r\n{location}\r\n";
                }
                else
                {
                    response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n<optional header lines>\r\n";
                }
            }
            else if (line.Contains("POST /") && line.Contains(" HTTP/1.1"))
            {
                int nameStartIndex = line.IndexOf("name=") + 5;
                int locationStartIndex = line.IndexOf("&location=");

                name = line.Substring(nameStartIndex+5); //Substring does not work!!
                name = line.Remove(locationStartIndex);

                location = line.Substring(locationStartIndex + 10);
                UpdateAndAdd(name, location, personLocation);
                response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n<optional header lines>\r\n";
            }
            //whois
            else
            {
                name = commands[0];
                if (commands.Length > 1)
                {
                    for (int i = 2; i < commands.Length; i++)
                    {
                        location += " " + commands[i];
                    }
                }
                else
                {
                    location = GetLocation(name, personLocation);
                    if (location != null)
                    {
                        response = location;
                    }
                    else
                    {
                        response = "ERROR: no entries found";
                    }
                }
            }

            sw.WriteLine(response);
            sw.Flush();
        }
        catch
        {
            Console.WriteLine("Connection faild");
        }
    }

    static void UpdateAndAdd(string name, string location, Dictionary<string, string> personLocation) 
    {
        if (personLocation.ContainsKey(name))
        {
            personLocation[name] = location;
        }
        else
        {
            personLocation.Add(name, location);
        }
        Console.WriteLine($"[{DateTime.Now}] \"PUT {name} {location}\" OK");
    }

    static string GetLocation(string name, Dictionary<string, string> personLocation)
    {

        if (personLocation.ContainsKey(name))
        {
            string location = personLocation[name];
            Console.WriteLine($"[{DateTime.Now}] \"GET {name}\" OK");
            return location;
        }
        else
        {
            //sw.WriteLine("ERROR: no entries found");
            //sw.Flush();
            Console.WriteLine($"[{DateTime.Now}] \"GET {name}\" UNKNOWN");
            //return "ERROR: no entries found";
            return null;
        }
    }

}

