using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

//C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe .\TcpFileSender.cs

class TcpFileSender
{
    static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: tsend <dest_ip> <port> <input_file_path>");
            return;
        }

        string destIp = args[0];
        int port = int.Parse(args[1]);
        string inputFilePath = args[2];

        TcpClient client = null;
        NetworkStream ns = null;
        FileStream fs = null;

        try
        {
            client = new TcpClient();
            client.Connect(IPAddress.Parse(destIp), port);
            Console.WriteLine("Connected to {0}:{1}", destIp, port);

            ns = client.GetStream();
            fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            byte[] buffer = new byte[64 * 1024];
            long total = 0;

            while (true)
            {
                int read = fs.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    break; // EOF

                ns.Write(buffer, 0, read);
                total += read;
            }

            ns.Flush();
            Console.WriteLine("Sent '{0}' ({1} bytes)", inputFilePath, total);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: {0}", ex);
        }
        finally
        {
            if (fs != null) fs.Close();
            if (ns != null) ns.Close();
            if (client != null) client.Close(); // ここで切断 = 受信側が完了
        }
    }
}
