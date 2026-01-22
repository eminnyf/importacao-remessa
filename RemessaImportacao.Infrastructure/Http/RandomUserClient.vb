Imports System.Net.Http
Imports Newtonsoft.Json.Linq
Imports RemessaImportacao.Domain.Entities

Namespace Http
    Public Class RandomUserClient
        Private Shared ReadOnly _http As New HttpClient() With {
            .Timeout = TimeSpan.FromSeconds(15)
        }

        Public Async Function ObterEnderecoAsync() As Task(Of Endereco)
            Try
                Dim json = Await _http.GetStringAsync("https://randomuser.me/api/")
                Dim root = JObject.Parse(json)

                Dim results = root("results")
                If results Is Nothing OrElse results.Type <> JTokenType.Array OrElse results.Count() = 0 Then
                    Return EnderecoFallback()
                End If

                Dim loc = results(0)("location")
                If loc Is Nothing Then Return EnderecoFallback()

                Dim street = loc("street")
                Dim streetName = street?("name")?.ToString()
                Dim streetNumber = street?("number")?.ToString()

                Dim logradouro As String = ""
                If Not String.IsNullOrWhiteSpace(streetName) AndAlso Not String.IsNullOrWhiteSpace(streetNumber) Then
                    logradouro = $"{streetName}, {streetNumber}"
                ElseIf Not String.IsNullOrWhiteSpace(streetName) Then
                    logradouro = streetName
                ElseIf Not String.IsNullOrWhiteSpace(streetNumber) Then
                    logradouro = streetNumber
                End If

                Dim uf = loc("state")?.ToString()

                Dim endereco As New Endereco() With {
                    .Logradouro = logradouro,
                    .Cidade = loc("city")?.ToString(),
                    .Uf = uf,
                    .Bairro = ""
                }

                If Not String.IsNullOrWhiteSpace(endereco.Uf) AndAlso endereco.Uf.Length > 2 Then
                    endereco.Uf = endereco.Uf.Substring(0, 2).ToUpperInvariant()
                ElseIf Not String.IsNullOrWhiteSpace(endereco.Uf) Then
                    endereco.Uf = endereco.Uf.ToUpperInvariant()
                End If

                If String.IsNullOrWhiteSpace(endereco.Logradouro) Then endereco.Logradouro = "NÃO INFORMADO"
                If String.IsNullOrWhiteSpace(endereco.Cidade) Then endereco.Cidade = "NÃO INFORMADO"
                If String.IsNullOrWhiteSpace(endereco.Uf) Then endereco.Uf = "NA"

                Return endereco

            Catch
                Return EnderecoFallback()
            End Try
        End Function

        Private Function EnderecoFallback() As Endereco
            Return New Endereco() With {
                .Logradouro = "NÃO INFORMADO",
                .Bairro = "NÃO INFORMADO",
                .Cidade = "NÃO INFORMADO",
                .Uf = "NA"
            }
        End Function
    End Class
End Namespace
