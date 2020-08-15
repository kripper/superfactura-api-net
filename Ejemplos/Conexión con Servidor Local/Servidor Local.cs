/**
 * Este ejemplo muestra como hacer una conexión con el servidor local de SuperFactura
 * para emitir documentos en forma offline.
 * 
 * Ver: https://blog.superfactura.cl/servicio-offline-para-puntos-de-venta/
 */

using System;
using SuperFactura;

namespace Ejemplos
{
    class Ejemplo
    {
        static void Main(string[] args)
        {
			// No se envía usuario ni contraseña, ya que estos datos serán centralizados en el servidor.
			API api = new API("http://127.0.0.1:9080");

			// Enviar documentID (importante para evitar documentos duplicados en caso de falla de red y reenvío):
			// Si se envía un ID ya utilizado, se retornará el mismo documento, en vez de crear uno nuevo.
			string documentID = "F123"; // El ID debe ser único por documento. Si se envía un ID ya usado, la API retornará el documento enviado anteriormente.
			api.SetOption("documentID", documentID);

			// Solicitar muestra impresa en formato EscPos para impresoras térmicas
			api.SetOption("getEscPos", true);

			// Solicitar descarga del PDF
			// api.SetSavePDF(@"C:\Users\kripp\Desktop\dte-" + documentID);

			// Solicitar descarga del XML firmado
			// api.SetSaveXML(@"C:\Users\kripp\Desktop\dte-" + documentID);

			// Aca se costruye el JSON con el contenido del documento.
			string json = @"
{
	""Encabezado"" : {
		""IdDoc"" : {
			""TipoDTE"" : ""33""
		},
		""Emisor"" : {
			""RUTEmisor"" : ""99581150-2""
		},
		""Receptor"" : {
			""RUTRecep"" : ""1-9"",
			""RznSocRecep"" : ""Test"",
			""GiroRecep"" : ""Giro"",
			""DirRecep"" : ""Dirección"",
			""CmnaRecep"" : ""Comuna"",
			""CiudadRecep"" : ""Ciudad""
		}
	},
	""Detalles"" : [
		{
			""NmbItem"" : ""Item 1"",
			""DscItem"" : ""Descripción del item 1"",
			""QtyItem"" : ""3"",
			""UnmdItem"" : ""KG"",
			""PrcItem"" : ""100""
		},
		{
			""NmbItem"" : ""Item 2"",
			""DscItem"" : ""Descripción del item 2"",
			""QtyItem"" : ""5"",
			""UnmdItem"" : ""KG"",
			""PrcItem"" : ""65""
		}
	]
}
";

			try
			{
				APIResult res = api.SendDTE(json, "cer");

				Console.WriteLine("Se creó el DTE con folio " + res.folio);

				// Imprimir el documento a una impresora Esc/Pos conectada en LPT1
				res.PrintEscPos("LPT1:");

				// Mostrar XML firmado
				// Console.WriteLine(res.xml);
			}
			catch(Exception e)
			{
				Console.WriteLine(System.AppDomain.CurrentDomain.BaseDirectory);

				// IMPORTANTE: Este mensaje se debe mostrar al usuario para poder darle soporte.
				Console.WriteLine(e.Message);
			}
		}
    }
}
