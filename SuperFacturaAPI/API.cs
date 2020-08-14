using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.IO.Compression;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace SuperFactura
{
	public class API
	{
		private string user, password;
		string jsonOptions = "";
		string additionalJsonOptions = null;
		string savePDF = null;
		string saveXML = null;

		string url;

		// Conexión a la nube
		public API(string user, string password)
		{
			this.user = user;
			this.password = password;
			url = "https://superfactura.cl";
		}

		// Conexión al Servidor Local
		public API(string host, int port)
		{
			url = "http://" + host + ":" + port;
		}

		// Conexión a un seridor específico
		public API(string url, string user, string password)
		{
			this.url = url;
			this.user = user;
			this.password = password;
		}

		public void SetSavePDF(string outputFile)
		{
			savePDF = outputFile;
			SetOption("getPDF", "1");
		}

		public void SetSaveXML(string outputFile)
		{
			saveXML = outputFile;
			SetOption("getXML", "1");
		}

		public void AddOptions(string json)
		{
			additionalJsonOptions = json;
		}

		public void SetOption(string key, string val)
		{
			if (jsonOptions != "") jsonOptions += ",";
			jsonOptions += EscapeArgument(key) + ":" + EscapeArgument(val);
		}

		public void SetOption(string key, bool val)
		{
			SetOption(key, val ? "1" : "");
		}

		private string SendRequest(string jsonData, string jsonOptions)
		{
			using (WebClient client = new WebClient())
			{
				byte[] response = client.UploadValues(url + "?a=json", new NameValueCollection()
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
			SetOption("ambiente", ambiente);
			SetOption("encoding", "UTF-8");
			SetOption("getXML", true);

			APIResult apiResult = new APIResult();

			string output = null;
			dynamic obj = null;

			try
			{
				if (additionalJsonOptions != null)
				{
					additionalJsonOptions = additionalJsonOptions.Trim();
					if (additionalJsonOptions != "")
					{
						jsonOptions += ", " + additionalJsonOptions.Trim().TrimStart('{').TrimEnd('}');
					}
				}

				jsonOptions = "{" + jsonOptions + "}";

				output = SendRequest(jsonData, jsonOptions);

				// var serializer = new JavaScriptSerializer();
				// serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
				// obj = serializer.Deserialize(output, typeof(object));
				obj = JsonConvert.DeserializeObject(output);

				if (obj.ack != "ok")
				{
					string text = (obj.response.title != "" ? obj.response.title + " - " : "") + obj.response.message;
					throw new Exception("ERROR: " + text);
				}

			}
			catch (Exception e)
			{
				throw new Exception("API Error: " + e);

			}

			dynamic appRes = obj.response;

			if ((string)appRes.ok == "1")
			{
				int folio = Convert.ToInt32(appRes.folio);

				apiResult.ok = true;
				apiResult.folio = folio;
				apiResult.xml = (string)appRes.xml;
				apiResult.escpos = (string)appRes.escpos;

				if (savePDF != null)
				{
					WriteFile(savePDF + ".pdf", Convert.FromBase64String((string)appRes.pdf));

					if ((string)appRes.pdfCedible != null)
					{
						WriteFile(savePDF + "-cedible.pdf", Convert.FromBase64String((string)appRes.pdfCedible));
					}
				}

				if (saveXML != null)
				{
					WriteFile(saveXML + ".xml", Encoding.Conver‌​t(Encoding.UTF8, Encoding.GetEncoding("ISO-8859-1"), Encoding.UTF8.GetBytes((string)appRes.xml)));
				}

			}
			else
			{
				throw new Exception("WRONG OUTPUT: " + output);
			}
			return apiResult;
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

		public void TestPrinter(string port)
		{
			SendRequest("", "{\"cmd\":\"test-escpos\"}");
		}
	}

	public class APIResult
	{
		public int folio; // Entrega el folio asignado al DTE.
		public bool ok = false; // Indica que el DTE se generó correctamente.
		public string xml;
		public string escpos;

		public bool PrintEscPos(String port)
		{
			Print2LPT.Print(port, Convert.FromBase64String(escpos));
			return true;
		}
	}

	public static class Print2LPT
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

		public static void Print(String port, byte[] data)
		{
			try
			{
				SafeFileHandle fh = CreateFile(port, FileAccess.Write, 0, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);
				if (!fh.IsInvalid)
				{
					FileStream fs = new FileStream(fh, FileAccess.ReadWrite);
					fs.Write(data, 0, data.Length);
					fs.Close();
				}

			}
			catch (Exception ex)
			{
				string message = ex.Message;
			}
		}
	}
}