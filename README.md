# TCP C# Server
This is a C# Windows console application. The server called locationserver and it uses TCP sockets to communicate with a TCP client.
The client and the server used for a simple student locating facility. The server simply usees the Microsoft Command Prompt interface to run. 
The client and server communicate using simplified forms of the internet whois (RFC3912), HTTP/0.9 (w3.org), HTTP/1.0 (RFC1945) and HTTP/1.1 (RFC2616) protocols.

# PROTOCOL 
1. The server waits for clients to contact it. 
2. Clients should connect and communicate with the server via sockets and TCP packets. 
3. Our protocol will operate over port 43, normally used by the internet whois protocol. 
4. The protocol has four styles of commands representing the whois notation, and the HTTP 0.9, 1.0 and 1.1 styles. 
5. The protocol contains two types of requests from the client to the server. These are lookup and update. 
