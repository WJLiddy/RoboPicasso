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
            sender.ReceiveTimeout = 10; // 100 ms
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
        sender.Send(Encoding.ASCII.GetBytes(json + "\n"));
    }

    //Blocking Receive Call.
    public SimpleJSON.JSONNode Recv()
    {
        SimpleJSON.JSONNode n;
        while (true)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024 * 1024];
            // Receive the response from the remote device.  
            int bytesRec = 0;
            try
            {
                bytesRec = sender.Receive(bytes);
            }
            catch
            {
                //Server crashed do something.
                continue;
            }
            buf += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            n = SimpleJSON.JSON.Parse(buf);
            if (n != null)
            {
                break;
            }
        }
        return n;
    }



    //Non blocking recieve call
    // Tries to get response from server as a JSON.
    // Returns null if did not receieve anything yet
    public SimpleJSON.JSONNode TryRecv()
    {
        SimpleJSON.JSONNode n;
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024 * 1024];
        // Receive the response from the remote device.  
        int bytesRec = 0;
        try
        {
            bytesRec = sender.Receive(bytes);
        }
        catch
        {
            //TODO Probably alert that server crashed.
            return null;
        }
        buf += Encoding.ASCII.GetString(bytes, 0, bytesRec);
        n = SimpleJSON.JSON.Parse(buf);
        if (n != null)
        {
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