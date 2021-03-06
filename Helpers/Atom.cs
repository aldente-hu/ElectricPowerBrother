﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers
{
	// Atomという名前空間を用意した方がいいかも？

	#region AtomFeedクラス
	public class AtomFeed
	{
		const string NAMESPACE = "http://www.w3.org/2005/Atom";
	
		/// <summary>
		/// タイトル．必須．
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// 更新日時．必須．
		/// </summary>
		public DateTime UpdatedAt { get; set; }

		/// <summary>
		/// IRI形式の識別子．必須．
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// 発行者(の名前)．必須(各Entryにauthorがあればその限りではないが，その実装はしていない)．
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// フィード自身のURL．あった方がよい．
		/// </summary>
		public string SelfLink { get; set; }

		/// <summary>
		/// サイトのURL．任意．
		/// </summary>
		public string AlternateLink { get; set; }

		public IList<AtomEntry> Entries { get; private set; }

		#region *コンストラクタ(AtomFeed)
		public AtomFeed()
		{
			Entries = new List<AtomEntry>();
		}
		#endregion

		// (1.1.10.1)feedのid要素を出力(今まで出力していなかった...)
		// ※このメソッド名ってGenerateDocumentの方がいいのでは？
		#region *Atomフィードを生成(OutputDocument)
		public XDocument OutputDocument()
		{
			XName feed = XName.Get("feed", NAMESPACE);

			XName link = XName.Get("link", NAMESPACE);

			//XName rel = XName.Get("rel", NAMESPACE);
			string rel = "rel";

			XElement root = new XElement(XName.Get("feed", NAMESPACE),
				new XElement(XName.Get("id", NAMESPACE), ID),
				new XElement(XName.Get("title", NAMESPACE), Title),
				new XElement(XName.Get("author", NAMESPACE), new XElement(XName.Get("name", NAMESPACE), Author)),
				new XElement(XName.Get("updated", NAMESPACE), UpdatedAt.ToString(@"yyyy-MM-dd\THH:mm:sszzz")),
				from entry in Entries select entry.OutputElement()
 			);
			if (!string.IsNullOrEmpty(SelfLink))
			{
				root.Add(new XElement(link, new XAttribute(rel, "self"), SelfLink));
			}
			if (!string.IsNullOrEmpty(AlternateLink))
			{
				root.Add(new XElement(link, new XAttribute(rel, "alternate"), AlternateLink));
			}


			return new XDocument(root);
		}
		#endregion

	}
	#endregion

	#region AtomEntryクラス
	public class AtomEntry
	{
		const string NAMESPACE = "http://www.w3.org/2005/Atom";

		/// <summary>
		/// タイトル．必須．
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// 発行日時．任意．
		/// </summary>
		public DateTime PublishedAt { get; set; }

		/// <summary>
		/// IRI形式の識別子．必須．
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// テキスト形式の内容．
		/// これがあれば，summaryやlinkは不要．
		/// </summary>
		public string Content { get; set; }

		// このメソッド名はGenerateElementの方がいいのでは？
		#region *Atomエントリ要素を生成(OutputElement)
		public XElement OutputElement()
		{
			XElement element = new XElement(XName.Get("entry", NAMESPACE),
				new XElement(XName.Get("title", NAMESPACE), Title),
				new XElement(XName.Get("published", NAMESPACE), PublishedAt.ToString(@"yyyy-MM-dd\THH:mm:sszzz")),
				new XElement(XName.Get("id", NAMESPACE), ID),
				new XElement(XName.Get("content", NAMESPACE), new XAttribute("type", "text"), Content)
			);
			return element;
		}
		#endregion

	}
	#endregion


}
