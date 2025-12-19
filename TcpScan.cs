using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe .\TcpScan.cs


class TcpPortScan
{
    static int Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: tscan <dest_ip> <start_port> <end_port> <timeout_ms>");
            Console.WriteLine("Example: tscan 192.168.1.10 1 1024 200");
            return 1;
        }

        string destIp = args[0];
        int startPort = int.Parse(args[1]);
        int endPort = int.Parse(args[2]);
        int timeoutMs = int.Parse(args[3]);

        if (startPort < 1 || startPort > 65535 || endPort < 1 || endPort > 65535 || startPort > endPort)
        {
            Console.WriteLine("Error: port range must be 1..65535 and start_port <= end_port");
            return 1;
        }
        if (timeoutMs < 1)
        {
            Console.WriteLine("Error: timeout_ms must be >= 1");
            return 1;
        }

        IPAddress ip;
        if (!IPAddress.TryParse(destIp, out ip))
        {
            Console.WriteLine("Error: invalid dest_ip");
            return 1;
        }

        Console.WriteLine("Scanning {0} ports {1}-{2} timeout={3}ms", destIp, startPort, endPort, timeoutMs);

        int openCount = 0;

        for (int port = startPort; port <= endPort; port++)
        {
            ScanResult r = CheckTcpPort(ip, port, timeoutMs);

            if (r == ScanResult.Open)
            {
                Console.WriteLine("OPEN  {0}", port);
                openCount++;
            }
            else if (r == ScanResult.Closed)
            {
                // 必要なら表示。多いので通常は黙る。
                // Console.WriteLine("CLOSED {0}", port);
            }
            else // Timeout
            {
                // フィルタ/無応答はタイムアウトになりがち
                // Console.WriteLine("TIMEOUT {0}", port);
            }
        }

        Console.WriteLine("Done. Open ports: {0}", openCount);
        return 0;
    }

    enum ScanResult { Open, Closed, Timeout }

    static ScanResult CheckTcpPort(IPAddress ip, int port, int timeoutMs)
    {
        TcpClient client = null;

        try
        {
            client = new TcpClient();
            IAsyncResult ar = client.BeginConnect(ip, port, null, null);

            bool ok = ar.AsyncWaitHandle.WaitOne(timeoutMs);
            if (!ok)
            {
                try { client.Close(); } catch { }
                return ScanResult.Timeout;
            }

            // 成否を確定させる（失敗だとここで例外）
            client.EndConnect(ar);

            return ScanResult.Open;
        }
        catch (SocketException)
        {
            // 接続拒否など
            return ScanResult.Closed;
        }
        catch
        {
            return ScanResult.Closed;
        }
        finally
        {
            if (client != null) client.Close();
        }
    }
}
