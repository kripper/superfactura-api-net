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

        public API(string user, string password)
        {
            this.user = user;
            this.password = password;
        }

        public APIResult SendDTE(string data, string ambiente, string jsonOptions = "")
        {
            APIResult apiResult = new APIResult();
            apiResult.folio = 123; // Se debe obtener de la salida del comando

            string fileName = System.IO.Path.GetTempFileName();

            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
            file.WriteLine(data);
            file.Close();

            // Ejecutar comando
            Process p = new Process();

            p.StartInfo.UseShellExecute = false;
            // p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            string args = EscapeArgument(user) + " " + EscapeArgument(password) + " " + EscapeArgument(ambiente) + " " + EscapeArgument(fileName);
            if (jsonOptions != "")
            {
                args += " " + EscapeArgument(jsonOptions);
            }
            p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "libs\\superfactura.exe";
            p.StartInfo.Arguments = args;

            string output;

            try
            {
                p.Start();
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

            } catch(Exception e)
            {
                Console.WriteLine("ERROR: No se pudo ejecutar el comando: " + p.StartInfo.FileName);
                return apiResult;

            } finally
            {
                File.Delete(fileName);
            }

            Console.Out.WriteLine("API OUTPUT:\n" + output);

            int folio = ParseFolio(output);
            if(folio != 0)
            {
                apiResult.ok = true;
                apiResult.folio = folio;
            }
            return apiResult;
        }

        private string EscapeArgument(string arg)
        {
            arg = arg.Replace("\"", "\\\"");
            return "\"" + arg + "\"";
        }

        private int ParseFolio(string output)
        {
            string startToken = "<folio>";
            string endToken = "</folio>";
            int startIndex = output.IndexOf(startToken);
            if (startIndex == -1) return 0; // ERROR:
            var endIndex = output.IndexOf(endToken, startIndex);
            int valueIndex = startIndex + startToken.Length;
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
