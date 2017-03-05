using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperFactura
{
    public class API
    {
        private string user, password;
        string jsonOptions = "";

        public API(string user, string password)
        {
            this.user = user;
            this.password = password;
        }

        public void SetSavePDF(string outputFile)
        {
            AddJSONOption("savePDF", outputFile);
        }

        private void AddJSONOption(string key, string val)
        {
            if (jsonOptions != "") jsonOptions += ",";
            jsonOptions += EscapeArgument(key) + ":" + EscapeArgument(val);
        }

        public APIResult SendDTE(string data, string ambiente)
        {
            AddJSONOption("encoding", "UTF-8");

            APIResult apiResult = new APIResult();

            string fileName = System.IO.Path.GetTempFileName();

            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
            file.WriteLine(data);
            file.Close();

            // Ejecutar comando
            Process p = new Process();

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            string args = EscapeArgument(user) + " " + EscapeArgument(password) + " " + EscapeArgument(ambiente) + " " + EscapeArgument(fileName);
            if (jsonOptions != "")
            {
                args += " " + EscapeArgument("{" + jsonOptions + "}");
            }

            p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "libs\\superfactura.exe";
            p.StartInfo.Arguments = args;

            string output;

            try
            {
                p.Start();
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

            } catch(Exception)
            {
				throw new Exception("ERROR: No se pudo ejecutar el comando: " + p.StartInfo.FileName);

            } finally
            {
                File.Delete(fileName);
            }

            int folio = ParseFolio(output);
            if(folio != 0)
            {
                apiResult.ok = true;
                apiResult.folio = folio;
            } else
            {
                throw new Exception(output);
            }
            return apiResult;
        }

        private string EscapeArgument(string arg)
        {
            arg = arg.Replace("\\", "\\\\");
            arg = arg.Replace("\"", "\\\"");
            return "\"" + arg + "\"";
        }

        private int ParseFolio(string output)
        {
            string startToken = "\"folio\":\"";
            string endToken = "\"";
            int startIndex = output.IndexOf(startToken);
            if (startIndex == -1) return 0; // ERROR:
            int valueIndex = startIndex + startToken.Length;
            var endIndex = output.IndexOf(endToken, valueIndex);
            string value = output.Substring(valueIndex, endIndex - valueIndex);
            return int.Parse(value);
        }
    }

    public class APIResult
    {
        public int folio; // Entrega el folio asignado al DTE.
        public bool ok = false; // Indica que el DTE se generó correctamente.
    }
}
