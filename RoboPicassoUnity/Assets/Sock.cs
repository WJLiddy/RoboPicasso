using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

//General Purpose non-blocking soccket
//TODO redfactor as JSON sock and IMAGE_GET sock
public class Sock
{
    Socket sender;
    string buf = "";
    List<byte> bytebuf = new List<byte>();

    //Open a socket to this ip and port. By default, talks to master monsterstop server.

    public Sock(string ip = "neutralspacestudios.com", int port = 45432)
    {
        // Connect to a remote device.  
        try
        {
            // Create a TCP/IP  socket.  
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(ip, port);
            //sender.ReceiveTimeout = 10000; // 10=000 ms
        }
        catch (Exception e)
        {
            Debug.Log(e);
            //("Unexpected exception : {0}", e.ToString());
        }
    }

    //Send JSON request.
    public void Submit(string json)
    {
        Debug.Log(json);
        sender.Send(Encoding.ASCII.GetBytes(json + "\n"));
    }

    //Non blocking recieve call
    // Tries to get response from server as a JSON.
    // Returns null if did not receieve anything yet
    public SimpleJSON.JSONNode TryRecv()
    {
        SimpleJSON.JSONNode n;
        // Data buffer for incoming data.  

        // Receive the response from the remote device.  

        int to_recv = sender.Available;
        if (to_recv == 0)
            return null;
        byte[] bytes = new byte[to_recv];
        // Data buffer for incoming data. Optimize?

        // Receive the response from the remote device.  
        sender.Receive(bytes);

        bytebuf.AddRange(bytes);
        if (bytebuf[bytebuf.Count - 1] == (byte)'\n')
        {
            n = SimpleJSON.JSON.Parse(Encoding.UTF8.GetString(bytebuf.ToArray()));
            bytebuf = new List<byte>();
            return n;
        }
        return null;
    }


    public void Close()
    {
        // Release the socket.
        try
        {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
        catch (SocketException e)
        {

        }
    }
}