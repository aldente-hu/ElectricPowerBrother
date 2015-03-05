using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helper
{
	public class JappajilConfiguration : ConfigurationSection
	{

		const string KEY_SMTP_SERVER = "smtp_server";
		const string KEY_MESSAGE_HEADER = "message_header";

		[ConfigurationProperty(KEY_SMTP_SERVER)]
		public SmtpServerConfig SmtpServer
		{
			get
			{
				return (SmtpServerConfig)this["KEY_SMTP_SERVER"];
			}
		}

		[ConfigurationProperty(KEY_MESSAGE_HEADER)]
		public MessageConfig MessageHeader
		{
			get
			{
				return (MessageConfig)this["KEY_MESSAGE"];
			}
		}

	}

	#region SmtpServerConfigクラス
	public class SmtpServerConfig : ConfigurationElement
	{
		const string KEY_ADDRESS = "address";
		const string KEY_PORT = "port";
		const string KEY_CREDENTIAL = "credential";

		[ConfigurationProperty(KEY_ADDRESS, IsRequired = true)]
		public string Address
		{
			get { return (string)this[KEY_ADDRESS]; }
		}

		[ConfigurationProperty(KEY_PORT, DefaultValue = 25)]
		public int Port
		{
			get { return int.Parse((string)this[KEY_PORT]); }
		}

		[ConfigurationProperty(KEY_CREDENTIAL)]
		public CredentialConfig Credential
		{
			get { return (CredentialConfig)this[KEY_CREDENTIAL]; }
		}

	}
	#endregion

	#region CredentialConfigクラス
	public class CredentialConfig : ConfigurationElement
	{
		const string KEY_USER = "username";
		const string KEY_PASS = "password";

		[ConfigurationProperty(KEY_USER, IsRequired = true)]
		public string UserName
		{
			get
			{ return (string)this[KEY_USER]; }
		}

		[ConfigurationProperty(KEY_PASS, IsRequired = true)]
		public string Password
		{
			get
			{ return (string)this[KEY_PASS]; }
		}

	}
	#endregion


	// <destination>
	// 	<add address="********@****.ac.jp" />
	//  <add address="*5****@****.ac.jp" />
	// </destination>


	public class MessageConfig : ConfigurationElement
	{
		const string KEY_FROM = "from";
		const string KEY_SENDER = "sender";
		const string KEY_DESTINATIONS = "destinations";
		const string KEY_REPLY_TO = "reply_to";


		[ConfigurationProperty(KEY_FROM, IsRequired = true)]
		public string From
		{
			get { return (string)this[KEY_FROM]; }
			set { this[KEY_FROM] = value; }
		}

		[ConfigurationProperty(KEY_SENDER)]
		public string Sender
		{
			get { return (string)this[KEY_SENDER]; }
			set { this[KEY_SENDER] = value; }
		}

		[ConfigurationProperty(KEY_DESTINATIONS, IsDefaultCollection = true)]
		public AddressCollection Destinations
		{
			get { return (AddressCollection)this[KEY_DESTINATIONS]; }
		}

		[ConfigurationProperty(KEY_REPLY_TO)]
		public AddressCollection ReplyToCollection
		{
			get { return (AddressCollection)this[KEY_REPLY_TO]; }
		}

	}



	[ConfigurationCollection(typeof(AddressConfigItem))]
	public class AddressCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new AddressCollection();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			// elementがCollection(自身)の要素でないときの返り値は規定されていない？
			AddressConfigItem item = element as AddressConfigItem;
			if (item == null) { return string.Empty; }
			else
			{
				return item.Address;
			}
		}
	}


	public class AddressConfigItem : ConfigurationElement
	{
		const string KEY_ADDRESS = "address";

		[ConfigurationProperty(KEY_ADDRESS, IsRequired = true, IsKey = true)]
		public string Address
		{
			get { return (string)this[KEY_ADDRESS]; }
			set { this[KEY_ADDRESS] = value; }
		}

	}
}
