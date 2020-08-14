/**
 * Este ejemplo genera un DTE a través del servicio online de SuperFactura.
 * Además, obtiene el XML firmado y el PDF.
 * Se recomienda deshabilitar las funcionalidades no requeridas para
 * reduir tiempo de respuesta y el volumen de transferencia de datos.
 */

using System;
using SuperFactura;

namespace Ejemplos
{
    class Ejemplo
    {
        static void Main(string[] args)
        {
            API api = new API("usuario@cliente.cl", "password");

			// Enviar documentID (importante para evitar documentos duplicados en caso de falla de red y reenvío):
			// Si se envía un ID ya utilizado, se retornará el mismo documento, en vez de crear uno nuevo.
			string documentID = "F123";
			api.SetOption("documentID", documentID);

			// Solicitar descarga del PDF
			api.SetSavePDF(@"C:\Users\kripp\Desktop\dte-" + documentID);

			// Solicitar descarga del XML firmado. Generalmente no será necesario.
			api.SetSaveXML(@"C:\Users\kripp\Desktop\dte-" + documentID);

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
			}
			catch(Exception e)
			{
				// IMPORTANTE: Este mensaje se debe mostrar al usuario para poder darle soporte.
				Console.WriteLine(e.Message);
			}
		}
    }
}
