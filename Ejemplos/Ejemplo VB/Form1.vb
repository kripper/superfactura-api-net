Public Class Form1
	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		' Debe usar su cuenta y contraseña de SuperFactura. Ver: https://blog.superfactura.cl/pasos-para-comenzar
		Dim api As New SuperFactura.API("cuenta@miempresa.cl", "mypassword")

		' Enviar documentID (importante para evitar documentos duplicados en caso de falla de red y reenvío)
		' Si se envía un ID ya utilizado, se retornará el mismo documento, en vez de crear uno nuevo.
		Dim documentID As String = tbDocumentID.Text
		api.SetOption("documentID", documentID)

		' Solicitar descarga del PDF
		api.SetSavePDF("C:\Users\kripp\Desktop\dte-" + documentID)

		' Solicitar descarga del XML firmado. Generalmente no será necesario.
		api.SetSaveXML("C:\Users\kripp\Desktop\dte-" + documentID)

		Dim json As String
		json = TextBox1.Text

		Try
			Dim res As SuperFactura.APIResult
			res = api.SendDTE(json, "cer")
			MsgBox("Se generó el folio " + res.folio.ToString())
		Catch ex As Exception
			' IMPORTANTE: Este mensaje se debe mostrar al usuario para poder darle soporte.
			MsgBox(ex.ToString())
		End Try
	End Sub
End Class
