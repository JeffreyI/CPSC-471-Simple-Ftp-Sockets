using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace _471_FTP_Client
{
    class ftp
    {
        public TcpClient control = new TcpClient(); //client is on port 2221
        public int bufferSize = 500;

        public void sendFile(string fileName)
        {
            TcpClient data = new TcpClient();
            IPAddress hostIPAddress1 = IPAddress.Parse("127.0.0.1");
            var port = 2222;
            if (control == null)
            {
                control = new TcpClient(hostIPAddress1.ToString(), port);
            }
            var endp = new IPEndPoint(hostIPAddress1, port);
            data.Connect(endp);
            
            byte[] bytebuffer = new byte[bufferSize];
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                                        + "\\" + fileName;
            if (File.Exists(path))
            {
                try
                {
                    Stream localFile = File.OpenRead(path);
                    int byteSent = localFile.Read(bytebuffer, 0, bufferSize);
                    int fileSize = 0;

                    NetworkStream fileStream = data.GetStream();
                    while (byteSent != 0)
                    {
                        fileSize += byteSent;
                        fileStream.Write(bytebuffer, 0, byteSent);
                        byteSent = localFile.Read(bytebuffer, 0, bufferSize);
                    }
                    localFile.Close();
                    fileStream.Close();
                    Console.WriteLine("File Sent: " + fileName + " " + fileSize);
                }
                catch(IOException ie)
                {
                    Console.WriteLine("IO Error: " + ie.Message);
                }
                catch(Exception e)
                {
                    Console.WriteLine("General Error: " + e.Message);
                }
            }
            else
            {
                Console.WriteLine("Path Does Not Exist!");
            }
            data.Close();
        }

        public void getFile(string fileName)
        {
            TcpClient data = new TcpClient();
            IPAddress hostIPAddress1 = IPAddress.Parse("127.0.0.1");
            var port = 2222;
            if (control == null)
            {
                control = new TcpClient(hostIPAddress1.ToString(), port);
            }
            var endp = new IPEndPoint(hostIPAddress1, port);
            data.Connect(endp);

            byte[] bytebuffer = new byte[bufferSize];
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)) + "\\FTP_Server_Dir\\" + fileName;
            string path_desk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                                + "\\" + fileName;

            if (File.Exists(path))
            {
                try
                {

                    NetworkStream fileStream = data.GetStream();
                    FileStream localFile = File.Open(path_desk, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
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
                Console.WriteLine("Path Does Not Exist!");
            }
            data.Close();
        }

        public void listLs()
        {
            TcpClient data = new TcpClient();
            IPAddress hostIPAddress1 = IPAddress.Parse("127.0.0.1");
            var port = 2222;
            if (control == null)
            {
                control = new TcpClient(hostIPAddress1.ToString(), port);
            }
            var endp = new IPEndPoint(hostIPAddress1, port);
            data.Connect(endp);

            byte[] bytebuffer = new byte[bufferSize];
            NetworkStream fileStream = data.GetStream();
            int byteRecv;
            string dataLs = null;
            byte[] msg = null;
            while ((byteRecv = fileStream.Read(bytebuffer, 0, bufferSize)) != 0)
            {
                dataLs = System.Text.Encoding.ASCII.GetString(bytebuffer, 0, byteRecv);
                Console.WriteLine("List Received: \t{0}", dataLs);
                msg = System.Text.Encoding.ASCII.GetBytes(dataLs);
            }
            fileStream.Close();
            data.Close();
        }

        static void Main(string[] args)
        {
            ftp data = new ftp();
            TcpClient control = new TcpClient();
            IPAddress hostIPAddress1 = IPAddress.Parse("127.0.0.1");
            var port = 2221;
            if (control == null)
            {
                control = new TcpClient(hostIPAddress1.ToString(), port);
            }
            var endp = new IPEndPoint(hostIPAddress1, port);
            control.Connect(endp);

            Console.WriteLine("Creating a Server Directory on Desktop!: FTP_Server_Dir");
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FTP_Server_Dir");
            if(!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
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

            string opt = null;
            string fileName = null;
            bool done = false;
            Console.WriteLine("Commands: put, get, ls and quit ");
            while (!done)
            {
                Console.Write("ftp> ");
                string[] str = Console.ReadLine().Split(new[] { ' '},2);
                opt = str[0];
                if (str.Length > 1)
                {
                    fileName = str[1];
                    opt = str[0] + " " + str[1];
                }
                else
                {
                    opt = str[0];
                }
                if (str[0].CompareTo("put") == 0)
                {
                    byte[] put = Encoding.ASCII.GetBytes(opt);
                    NetworkStream nwStream = control.GetStream();
                    Console.WriteLine("Sending: " + Encoding.ASCII.GetString(put, 0, put.Length));
                    nwStream.Write(put, 0, put.Length);

                    data.sendFile(fileName);
                }
                else if (str[0].CompareTo("get") == 0)
                {
                    byte[] get = Encoding.ASCII.GetBytes(opt);
                    NetworkStream nwStream = control.GetStream();
                    Console.WriteLine("Sending: " + Encoding.ASCII.GetString(get, 0, get.Length));
                    nwStream.Write(get, 0, get.Length);

                    data.getFile(fileName);
                }
                else if (str[0].CompareTo("ls") == 0)
                {
                    byte[] get = Encoding.ASCII.GetBytes(opt);
                    NetworkStream nwStream = control.GetStream();
                    Console.WriteLine("Sending: " + Encoding.ASCII.GetString(get, 0, get.Length));
                    nwStream.Write(get, 0, get.Length);

                    data.listLs();
                }
                else if (str[0].CompareTo("quit") == 0)
                {
                    done = true;

                    byte[] get = Encoding.ASCII.GetBytes(opt);
                    NetworkStream nwStream = control.GetStream();
                    Console.WriteLine("Sending: " + Encoding.ASCII.GetString(get, 0, get.Length));
                    nwStream.Write(get, 0, get.Length);

                    control.Close();
                }
                else
                {
                    Console.WriteLine("No command matched!");
                }
            }
            control.Close();
            Console.WriteLine("Client closed");
        }
    }
}
