using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;

class Server
{
    public int bufferSize = 500;
    public void put(string fileName)
    {
        TcpListener dataServer = null;
        // Set the TcpListener on port 2221.
        Int32 port = 2222;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        // TcpListener server = new TcpListener(port);
        dataServer = new TcpListener(localAddr, port);

        // Start listening for client requests.
        dataServer.Start();
        Console.Write("Waiting for a connection... ");
        TcpClient server = dataServer.AcceptTcpClient();
        Console.WriteLine("Connected!");

        byte[] bytebuffer = new byte[bufferSize];

        string path_desk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                                        + "\\" + fileName;

        if (File.Exists(path_desk))
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                                              + "\\FTP_Server_Dir\\" + fileName;
                Stream localFile = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                NetworkStream fileStream = server.GetStream();

                int byteRecv;
                int fileSize = 0;
                while ((byteRecv = fileStream.Read(bytebuffer, 0, bufferSize)) > 0)
                {
                    fileSize += byteRecv;
                    if (byteRecv == 0) break;
                    localFile.Write(bytebuffer, 0, byteRecv);

                }
                Console.WriteLine("File Received: " + fileName + " " + fileSize);
                fileStream.Close();
                localFile.Close();
            }
            catch (IOException ie)
            {
                Console.WriteLine("IO Error: " + ie.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("General Error: " + e.Message);
            }
        }
        else
        {
            Console.WriteLine("Path Does Not Exists!");
        }
        dataServer.Stop();
        server.Close();
    }

    public void get(string fileName)
    {
        TcpListener dataServer = null;
        // Set the TcpListener on port 2221.
        Int32 port = 2222;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        // TcpListener server = new TcpListener(port);
        dataServer = new TcpListener(localAddr, port);

        // Start listening for client requests.
        dataServer.Start();
        Console.Write("Waiting for a connection... ");
        TcpClient server = dataServer.AcceptTcpClient();
        Console.WriteLine("Connected!");

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                                        + "\\FTP_Server_Dir\\" + fileName;
        if (File.Exists(path))
        {
            try
            {
                Stream localFile = File.OpenRead(path);
                byte[] bytebuffer = new byte[bufferSize];
                int byteSent = localFile.Read(bytebuffer, 0, bufferSize);
                int fileSize = 0;

                NetworkStream fileStream = server.GetStream();
                while (byteSent != 0)
                {
                    fileSize += byteSent;
                    fileStream.Write(bytebuffer, 0, byteSent);
                    byteSent = localFile.Read(bytebuffer, 0, bufferSize);
                }

                Console.WriteLine("File Sent: " + fileName + " " + fileSize);
                fileStream.Close();
                localFile.Close();
            }
            catch (IOException ie)
            {
                Console.WriteLine("IO Error: " + ie.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("General Error: " + e.Message);
            }
        }
        else
        {
            Console.WriteLine("Path Does Not Exists!");
        }
        dataServer.Stop();
        server.Close();
    }

    public static void ProcessFile(string path)
    {
        Console.WriteLine("Processed file '{0}'.", path);
    }
    public void ls()
    {
        TcpListener dataServer = null;
        // Set the TcpListener on port 2221.
        Int32 port = 2222;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        // TcpListener server = new TcpListener(port);
        dataServer = new TcpListener(localAddr, port);

        // Start listening for client requests.
        dataServer.Start();
        Console.Write("Waiting for a connection... ");
        TcpClient server = dataServer.AcceptTcpClient();
        Console.WriteLine("Connected!");

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                                        + "\\FTP_Server_Dir\\";
        if (Directory.Exists(path))
        {
            try
            {
                //create a list of items in the Directory
                string[] fileEntries = Directory.GetFiles(path);
                string fileName = null;
                for (int i = 0; i < fileEntries.Length; i++)
                {
                    fileName += Path.GetFileName(fileEntries[i]) + " " +'\t';
                }

                byte[] ls = Encoding.ASCII.GetBytes(fileName);
                NetworkStream nwStream = server.GetStream();
                Console.WriteLine("Sending: \t" + Encoding.ASCII.GetString(ls, 0, ls.Length));
                nwStream.Write(ls, 0, ls.Length);

            }
            catch (IOException ie)
            {
                Console.WriteLine("IO Error: " + ie.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("General Error: " + e.Message);
            }
        }
        else
        {
            Console.WriteLine("Path Does Not Exists!");
        }
        dataServer.Stop();
        server.Close();
    }

    public static void Main()
    {
        Server tcpServer = new Server();
        TcpListener server = null;
        try
        {
            // Set the TcpListener on port 2221.
            Int32 port = 2221;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);
            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;

            // Enter the listening loop.
            while (true)
            {
                Console.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);//.Split(new[] { ' '},2));
                    
                    string[] str = Encoding.UTF8.GetString(msg, 0, msg.Length).Split(new[] { ' '},2);

                    if (str[0].CompareTo("put") == 0)
                    {
                        //put function
                        tcpServer.put(str[1]);
                    }
                    else if (str[0].CompareTo("get") == 0)
                    {
                        //get function
                        tcpServer.get(str[1]);
                    }
                    else if (str[0].CompareTo("ls") == 0)
                    {
                        //ls function
                        tcpServer.ls();
                    }
                    else if(str[0].CompareTo("quit") == 0)
                    {
                        //quit
                        server.Stop();
                        break;
                    }
                }
                // Shutdown and end connection
                client.Close();
                break;
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }
        
        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }
}