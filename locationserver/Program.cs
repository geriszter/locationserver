using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Text;

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
            //socketStream.ReadTimeout = 1000;
            //socketStream.WriteTimeout = 1000;

            StreamWriter sw = new StreamWriter(socketStream);
            StreamReader sr = new StreamReader(socketStream);
            string name = null;
            string location = null;
            string response = null;
            bool ched = true;


            string line = "";

            byte[] myReadBuffer = new byte[1024];
            int numberOfBytesRead = 0;
            StringBuilder myCompleteMessage = new StringBuilder();
            do
            {
                numberOfBytesRead = socketStream.Read(myReadBuffer);

                myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
            }
            while (socketStream.DataAvailable);
            
            line = myCompleteMessage.ToString();
            //Console.WriteLine(line);
            string[] commands = line.Split(" ");
            //GET commands
            if (commands[0] == "GET")
            {
                //Console.WriteLine("Get part");
                if (commands.Length > 2)
                {

                    if (commands[2].Contains("HTTP/1.0"))
                    {
                        ched = false;
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
                    else if (commands[2].Contains("HTTP/1.1"))
                    {
                        ched = false;
                        name = commands[1].Remove(0, 7);
                        location = GetLocation(name, personLocation);

                        if (location != null)
                        {
                            response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n";
                        }
                        else
                        {
                            response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
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
                    }
                    else
                    {
                        response = "HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                    }
                }
            }
            //PUT HTTP/0.9 
            else if (commands[0] == "PUT" && commands[1].Contains("/"))
            {
                ched = false;
                string[] array = line.Split("\r\n");
                name = array[0].Remove(0, 5);
                location = array[array.Length - 2];
                Console.WriteLine("AddLocation " + location);

                UpdateAndAdd(name, location, personLocation);
                response = $"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n";
            }
            else if (commands[0] == "POST")
            {
                Console.WriteLine("POST");

                if (commands[2].Contains("HTTP/1.0"))
                {
                    ched = false;

                    name = commands[1].Remove(0, 1);
                    //location = sr.ReadLine();
                    string[] array = line.Split("\r\n");
                    location = array[array.Length - 1];

                    UpdateAndAdd(name, location, personLocation);
                    response = "HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                }
                //"HTTP/1.1"
                else if (commands[2].Contains("HTTP/1.1"))
                {
                    ched = false;
                    //sr.ReadLine(); //Host
                    //sr.ReadLine(); //Content-Length
                    //sr.ReadLine(); //optional
                    //line = sr.ReadLine();

                    int locationIndex = line.IndexOf("&location=");
                    name = line.Substring(4, locationIndex);
                    UpdateAndAdd(name, location, personLocation);
                    response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n";
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
                }
                else
                {
                    response = "ERROR: no entries found";
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
            }

            //if(ched)
            //{
            //    name = commands[0];
            //    if (commands.Length > 1)
            //    {
            //        location = commands[1];
            //        for (int i = 2; i < commands.Length; i++)
            //        {
            //            location += " " + commands[i];
            //        }
            //        location = location.Remove(location.Length-2);
            //        UpdateAndAdd(name, location, personLocation);
            //        response = "OK";
            //    }
            //    else
            //    {
            //        name = name.Remove(name.Length - 2);
            //        location = GetLocation(name, personLocation);
            //        if (location != null)
            //        {
            //            response = location;
            //        }
            //        else
            //        {
            //            response = "ERROR: no entries found";
            //        }
            //    }
            //}
            
            sw.WriteLine(response);
            sw.Flush();
        }
        catch(Exception e)
        {
            Console.WriteLine("Connection faild");
            Console.WriteLine(e);
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
            Console.WriteLine($"[{DateTime.Now}] \"GET {name}\" UNKNOWN");
            return null;
        }
    }

}

