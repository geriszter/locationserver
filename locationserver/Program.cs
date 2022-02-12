using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
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
            listener = new TcpListener(IPAddress.Any, 43);
            listener.Start();
            Console.WriteLine("Server started");
            while (true)
            {
                connection = listener.AcceptSocket();
                socketStream = new NetworkStream(connection);
                doRequest(socketStream);
                socketStream.Close();
                connection.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exeption: " + e.ToString());
        }
    }
    static void doRequest(NetworkStream socketStream)
    {
        try
        {

            StreamWriter sw = new StreamWriter(socketStream);
            StreamReader sr = new StreamReader(socketStream);
            //Console.WriteLine(sr.ReadToEnd());

            string line = sr.ReadLine().Trim();
            string[] commands = line.Split(" ");

            if (commands.Length == 1)
            {

            }
            else if(commands.Length == 2)
            {

            }
        }
        catch
        {
            Console.WriteLine("Connection faild");
        }
    }
}

