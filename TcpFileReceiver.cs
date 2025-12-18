using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

// C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe .\TcpFileReceiver.cs

class TcpFileReceiver
{
    static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: trecv <bind_ip> <port> <output_file_path>");
            return;
        }

        string bindIp = args[0];
        int port = int.Parse(args[1]);
        string outputFilePath = args[2];

        IPEndPoint bindEndPoint = new IPEndPoint(IPAddress.Parse(bindIp), port);
        TcpListener listener = null;
        TcpClient client = null;
        NetworkStream ns = null;
        FileStream fs = null;

        try
        {
            listener = new TcpListener(bindEndPoint);
            listener.Start();
            Console.WriteLine("Listening on {0}", bindEndPoint);

            client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected: {0}", client.Client.RemoteEndPoint);

            ns = client.GetStream();
            fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

            byte[] buffer = new byte[64 * 1024];
            long total = 0;

            while (true)
            {
                int read = ns.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    break; // 接続終了 = ファイル完了

                fs.Write(buffer, 0, read);
                total += read;
            }

            fs.Flush();
            Console.WriteLine("Saved to '{0}' ({1} bytes)", outputFilePath, total);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: {0}", ex);
        }
        finally
        {
            if (fs != null) fs.Close();
            if (ns != null) ns.Close();
            if (client != null) client.Close();
            if (listener != null) listener.Stop();
        }
    }
}
