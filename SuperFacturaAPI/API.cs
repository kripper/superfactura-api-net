using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Web.Script.Serialization;
using System.Web;

namespace SuperFactura
{
	public class API
	{
		private string user, password;
		string jsonOptions = "";
		string savePDF = null;
		string saveXML = null;

		public API(string user, string password)
		{
			this.user = user;
			this.password = password;
		}

		public void SetSavePDF(string outputFile)
		{
			savePDF = outputFile;
			AddJSONOption("getPDF", "1");
		}

		public void SetSaveXML(string outputFile)
		{
			saveXML = outputFile;
			AddJSONOption("getXML", "1");
		}

		private void AddJSONOption(string key, string val)
		{
			if (jsonOptions != "") jsonOptions += ",";
			jsonOptions += EscapeArgument(key) + ":" + EscapeArgument(val);
		}

		private string SendRequest(string jsonData, string jsonOptions)
		{
			using (WebClient client = new WebClient())
			{
				byte[] response = client.UploadValues("https://superfactura.cl?a=json", new NameValueCollection()
				{
					{ "user", user },
					{ "pass", password },
					{ "content", jsonData },
					{ "options", jsonOptions }
				});

				string rawOutput = System.Text.Encoding.UTF8.GetString(response);

				return System.Text.Encoding.UTF8.GetString(Decompress(response));
			}
		}

		static byte[] Decompress(byte[] gzip)
		{
			using (GZipStream stream = new GZipStream(new MemoryStream(gzip),
													  CompressionMode.Decompress))
			{
				const int size = 4096;
				byte[] buffer = new byte[size];
				using (MemoryStream memory = new MemoryStream())
				{
					int count = 0;
					do
					{
						count = stream.Read(buffer, 0, size);
						if (count > 0)
						{
							memory.Write(buffer, 0, count);
						}
					}
					while (count > 0);
					return memory.ToArray();
				}
			}
		}

		public APIResult SendDTE(string jsonData, string ambiente)
		{
			AddJSONOption("ambiente", ambiente);
			AddJSONOption("encoding", "UTF-8");

			APIResult apiResult = new APIResult();

			string output = null;
			dynamic obj = null;

			try
			{
				output = SendRequest(jsonData, "{" + jsonOptions + "}");

				var serializer = new JavaScriptSerializer();
				serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

				obj = serializer.Deserialize(output, typeof(object));
				if (obj.ack != "ok")
				{
					string text = obj.response.title != "" ? obj.response.title + " - " : "" + obj.response.message;
					throw new Exception("ERROR: " + text);
				}

			}
			catch (Exception e)
			{
				throw new Exception("API Error: " + e);

			}

			dynamic appRes = obj.response;

			int folio = Convert.ToInt32(appRes.folio);
			if (appRes.ok == "1")
			{
				apiResult.ok = true;
				apiResult.folio = folio;

				if (savePDF != null)
				{
					WriteFile(savePDF + ".pdf", DecodeBase64(appRes.pdf));

					if (appRes.pdfCedible != null)
					{
						WriteFile(savePDF + "-cedible.pdf", DecodeBase64(appRes.pdfCedible));
					}
				}

				if (saveXML != null)
				{
					WriteFile(saveXML + ".xml", Encoding.Conver‌​t(Encoding.UTF8, Encoding.GetEncoding("ISO-8859-1"), Encoding.UTF8.GetBytes(appRes.xml)));
				}

			}
			else
			{
				throw new Exception(output);
			}
			return apiResult;
		}

		private byte[] DecodeBase64(string base64)
		{
			return System.Convert.FromBase64String(base64);
		}

		private void WriteFile(string filename, byte[] content)
		{
			using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
			{
				writer.Write(content);
			}
		}

		private string EscapeArgument(string arg)
		{
			arg = arg.Replace("\\", "\\\\");
			arg = arg.Replace("\"", "\\\"");
			return "\"" + arg + "\"";
		}
	}

	public class APIResult
	{
		public int folio; // Entrega el folio asignado al DTE.
		public bool ok = false; // Indica que el DTE se generó correctamente.
	}
}
