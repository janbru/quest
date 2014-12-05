﻿Imports CefSharp
Imports System.IO
Imports System.Text

Public Class CefSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Public Sub New(parent As PlayerHTML)
        Me.Parent = parent
    End Sub

    Public Property Parent As PlayerHTML

    Public Function Create() As ISchemeHandler Implements ISchemeHandlerFactory.Create
        Return New CefSchemeHandler(Me)
    End Function

End Class

Public Class CefSchemeHandlerResponse
    Implements ISchemeHandlerResponse

    Public Property CloseStream As Boolean Implements ISchemeHandlerResponse.CloseStream
    Public Property ContentLength As Integer Implements ISchemeHandlerResponse.ContentLength
    Public Property MimeType As String Implements ISchemeHandlerResponse.MimeType
    Public Property RedirectUrl As String Implements ISchemeHandlerResponse.RedirectUrl
    Public Property ResponseHeaders As System.Collections.Specialized.NameValueCollection Implements ISchemeHandlerResponse.ResponseHeaders
    Public Property ResponseStream As Stream Implements ISchemeHandlerResponse.ResponseStream
    Public Property StatusCode As Integer Implements ISchemeHandlerResponse.StatusCode
End Class

Public Class CefSchemeHandler
    Implements ISchemeHandler

    Private _parent As CefSchemeHandlerFactory

    Public Sub New(parent As CefSchemeHandlerFactory)
        _parent = parent
    End Sub

    Public Function ProcessRequestAsync(request As IRequest, response As ISchemeHandlerResponse, requestCompletedCallback As OnRequestCompletedHandler) As Boolean Implements ISchemeHandler.ProcessRequestAsync
        Dim uri = New Uri(request.Url)
        Dim filename = uri.UnescapeDataString(uri.AbsolutePath.Substring(1))

        response = New CefSchemeHandlerResponse

        response.ResponseStream = _parent.Parent.CurrentGame.GetResource(filename)

        If (response.ResponseStream IsNot Nothing) Then
            response.MimeType = PlayerHelper.GetContentType(filename)
            Return True
        End If

        Return False
    End Function
End Class

Public Class CefResourceSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Public Function Create() As ISchemeHandler Implements ISchemeHandlerFactory.Create
        Return New CefResourceSchemeHandler(Me)
    End Function

    Public Property HTML As String
End Class

Public Class CefResourceSchemeHandler
    Implements ISchemeHandler

    Private _parent As CefResourceSchemeHandlerFactory

    Public Sub New(parent As CefResourceSchemeHandlerFactory)
        _parent = parent
    End Sub

    Public Function ProcessRequestAsync(request As IRequest, response As ISchemeHandlerResponse, requestCompletedCallback As OnRequestCompletedHandler) As Boolean Implements ISchemeHandler.ProcessRequestAsync
        Dim uri = New Uri(request.Url)

        response = New CefSchemeHandlerResponse

        If uri.AbsolutePath = "/ui" Then
            response.MimeType = "text/html"
            Dim bytes = Encoding.UTF8.GetBytes(_parent.HTML)
            response.ResponseStream = New MemoryStream(bytes)
            Return True
        End If

        Dim filepath = Path.Combine(My.Application.Info.DirectoryPath(), uri.AbsolutePath.Substring(1))

        If File.Exists(filepath) Then
            System.Diagnostics.Debug.WriteLine("Served {0} from {1}", request.Url, filepath)

            response.ResponseStream = New System.IO.FileStream(filepath, FileMode.Open, FileAccess.Read)

            Select Case Path.GetExtension(filepath)
                Case ".js"
                    response.MimeType = "text/javascript"
                Case ".css"
                    response.MimeType = "text/css"
                Case ".png"
                    response.MimeType = "image/png"
                Case Else
                    Throw New Exception("Unknown MIME type")
            End Select

            Return True
        Else
            System.Diagnostics.Debug.WriteLine("Not found " + filepath)
            Return False
        End If
    End Function
End Class