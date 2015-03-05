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

		public Jappajil(string server, int port = 25)
		{
			this.Server = server;
			this.Port = port;
			this._client = new SmtpClient(server, port);
		}

		public void Post(MailMessage message)
		{
			if (string.IsNullOrEmpty(this.UserName))
			{
				_client.EnableSsl = false;
			}
			else
			{
				_client.EnableSsl = true;
				_client.Credentials = new NetworkCredential(this.UserName, this.Password);
			}

			_client.Send(message);
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
