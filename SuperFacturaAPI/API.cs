using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public APIResult SendDTE(string data, string ambiente)
        {
            APIResult apiResult = new APIResult();
            apiResult.folio = 123; // Se debe obtener de la salida del comando

            // TODO: Gardar data en archivo temporal en file
            string file = "temp.txt";

            // Ejecutar comando
            Process p = new Process();

            p.StartInfo.UseShellExecute = false;
            // p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            string args = EscapeArgument(user) + " " + EscapeArgument(password) + " " + EscapeArgument(ambiente) + " " + EscapeArgument(file);
            /* TODO: Soportar opciones adicionales
            if (options)
            {
                args += " " + EscapeArgument(options);
            }
            */
            p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "libs\\superfactura.exe";
            p.StartInfo.Arguments = args;
            try
            {
                p.Start();
            } catch(Exception e)
            {
                Console.WriteLine("ERROR: No se pudo ejecutar el comando: " + p.StartInfo.FileName);
                return apiResult;
            }

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            Console.Out.WriteLine("OUTPUT:\n" + output);

            return apiResult;
        }

        public string EscapeArgument(string arg)
        {
            // TODO: Escapar
            return "\"" + arg + "\"";
        }
    }

    public class APIResult
    {
        public int folio; // Entrega el folio asignado al DTE.
        public bool ok = false; // Indica que el DTE se generó correctamente.
    }
}
