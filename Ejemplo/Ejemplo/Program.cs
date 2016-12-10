using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperFactura;

namespace Ejemplo
{
    class Program
    {
        static void Main(string[] args)
        {
            API api = new API("usuario@cliente.cl", "contraseña");

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

            APIResult res = api.SendDTE(json, "cer", "{\"getPDF\": 1}");
            if (res.ok)
            {
                Console.WriteLine("Se creó el DTE con folio " + res.folio);
            }
        }
    }
}
