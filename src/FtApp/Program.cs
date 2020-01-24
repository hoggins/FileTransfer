using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using InteractiveConsole;

namespace FtApp
{
  internal class Program
  {
    // https://github.com/OctopusDeploy/Octodiff for file sync
    public static void Main(string[] args)
    {
      FtConsole.AddCommand("s.start")
        .Param("port", "")
        .Method(c => FtServer.Start(c.Optional("port", 9001)));
      
      FtConsole.AddCommand("c.connect")
        .Param("ip", "")
        .Param("port", "")
        .Method(c => FtClient.Connect(c.Optional("ip", "127.0.0.1"), c.Optional("port", 9001)));
      
      FtConsole.Instance.Start();
    }
  }

  internal class FtClient
  {
    private static Client Inst = new Client();


    public static void Connect(string ip, int port)
    {
      Inst.Connect(ip, port);
    }

    private class Client
    {
      public async void Connect(string ip, int port)
      {
        var client = new TcpClient(ip, port);
        using (SslStream sslStream = new SslStream(client.GetStream(), false, ValidateServerCertificate, null))
        {
          var certificate = new X509Certificate("public_privatekey_client.pfx", "qwe", X509KeyStorageFlags.UserKeySet);
          await sslStream.AuthenticateAsClientAsync("ip", new X509CertificateCollection(new []{certificate}), SslProtocols.Tls12, false).ConfigureAwait(false);

          var bytes = Encoding.UTF8.GetBytes("Hello world");
          await sslStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
          await sslStream.FlushAsync().ConfigureAwait(false);
        }
        client.Close();
        FtConsole.WriteLine("exit client");
      }

      private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
      {
        FtConsole.WriteLine("cert on client");
        // todo compare with hardcoded cert 
        return true;
      }
    }
  }

  internal class FtServer
  {
    private static Server Inst = new Server();

    public static void Start(int port)
    {
      Inst.Start(port);
    }

    class Server
    {
      private TcpListener _listener;
      private bool _isRunning;

      public async Task Start(int port)
      {
        if (_isRunning)
          return;

        _isRunning = true;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        
        while (_isRunning)
        {
          var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
          HandleClient(client);
        }
        FtConsole.WriteLine("exit server");
      }

      private async void HandleClient(TcpClient client)
      {
        var cert = new X509Certificate("public_privatekey.pfx", "qwe", X509KeyStorageFlags.UserKeySet);
        using (var stream = new SslStream(client.GetStream(), false, UserCertificateValidationCallback))
        {
          await stream.AuthenticateAsServerAsync(cert, true, SslProtocols.Tls12, false).ConfigureAwait(false);

          var buf = new byte[1048 * 16];
          while (true)
          {
            var len = await stream.ReadAsync(buf, 0, 1048 * 16).ConfigureAwait(false);
            if (len == 0)
              return;

            var str = Encoding.UTF8.GetString(buf, 0, len);
            FtConsole.WriteLine("Rec: " + str);
          }
        }
      }

      private bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
      {
        FtConsole.WriteLine("cert on server");
        // todo compare with hardcoded certificate
        return true;
      }
    }
  }
}