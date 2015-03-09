using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Mail;
using System.Configuration;

// (1.1.4.1) 名前空間を修正．
namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers
{
	// (1.1.4.0)メール送信用
	public class Jappajil
	{
		// この2つはコンストラクタで設定する．
		//public string Server { get; set; }
		public readonly string Server;
		public readonly int Port;
 		//public int Port {get;}
		readonly SmtpClient _client;

		public string UserName {get; set;}
		public string Password {get; set;}

		public string POPServer { get; set; }

		public Jappajil(string server, int port = 25)
		{
			this.Server = server;
			this.Port = port;
			this._client = new SmtpClient(server, port);

			this.POPServer = string.Empty;
		}

		// (1.1.5.0)POP before SMTPに対応．
		public void Post(MailMessage message)
		{
			if (string.IsNullOrEmpty(this.UserName))
			{
				_client.EnableSsl = false;
			}
			else
			{
				// (1.1.5.1)条件文を修正．
				if (!string.IsNullOrEmpty(POPServer))
				{
					_client.EnableSsl = false;	// (1.1.5.2)追加．
					// POP before SMTP
					POPAuthentication(POPServer);
				}
				else
				{
					// SMTP認証
					Console.WriteLine("Try SMTP Authentication.");
					_client.EnableSsl = true;
					_client.Credentials = new NetworkCredential(this.UserName, this.Password);
				}
			}

			_client.Send(message);
			
		}

		// (1.1.5.0)POP before SMTPの認証を行います．
		protected void POPAuthentication(string server)
		{
			using (System.Net.Sockets.TcpClient pop = new System.Net.Sockets.TcpClient())
			{
				pop.SendTimeout = 10000;
				pop.ReceiveTimeout = 10000;
				pop.Connect(server, 110);

				using (System.Net.Sockets.NetworkStream nws = pop.GetStream())
				{
					System.IO.StreamWriter sw = new System.IO.StreamWriter(nws);
					var sr = new System.IO.StreamReader(nws);
					String res = string.Empty;

					sw.NewLine = "\r\n";
					res += sr.ReadLine();

					sw.AutoFlush = true;
					sw.WriteLine("USER {0}", UserName);
					res += sr.ReadLine();
					sw.WriteLine("PASS {0}", Password);
					res += sr.ReadLine();
					sw.WriteLine("QUIT");
					res += sr.ReadLine();

					nws.Close();
				}
				pop.Close();
			}
		}
		/*
					string tanuki = "*****@hirosaki-u.ac.jp";
			SmtpClient client = new SmtpClient("smtp.*****.*****", 587);
			client.EnableSsl = true;
			client.Credentials = new NetworkCredential("jm####@hirosaki-u.ac.jp", "**********");
		*/

		/*
			MailMessage message = new MailMessage();
			message.From = new MailAddress("*****@hirosaki-u.ac.jp");
			message.To.Add(new MailAddress(tanuki));
			message.ReplyToList.Add(new MailAddress(tanuki));
			message.Sender = new MailAddress("jm####@hirosaki-u.ac.jp");
			message.Subject = "test";

			message.Body = "これもテストです？\r\n次は本番かな？";
		*/
		//client.Send(message);

	}



}
