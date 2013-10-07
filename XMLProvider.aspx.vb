Option Strict On

Imports System
Imports System.Web
Imports System.Web.Services
Imports System.IO
Imports NEvoWeb.Modules.NB_Store.SharedFunctions

Namespace NEvoWeb.Modules.NB_Store

    Public MustInherit Class XMLProvider
        Implements System.Web.IHttpHandler

#Region "Event Handlers"
        Public ReadOnly Property IsReusable() As Boolean Implements System.Web.IHttpHandler.IsReusable
            Get
                Return True
            End Get
        End Property

        Public Sub ProcessRequest(ByVal context As System.Web.HttpContext) Implements System.Web.IHttpHandler.ProcessRequest
            '24 June 2013 by Sergey Velichko: Added functionality to handle ajax calls for add related products
            'Begin of changes
            If (context.Request.QueryString("action") <> Nothing) Then
                If (context.Request.QueryString("action").ToLower() = "addtorelated") Then
                    Try
                        Dim ProductID As String = context.Request.QueryString("prodID")
                        Dim RelatedID As String = context.Request.QueryString("relID")
                        RelatedProducts.AddProduct(DotNetNuke.Entities.Portals.PortalController.GetCurrentPortalSettings().PortalId, ProductID, RelatedID)
                        context.Response.Write("done")
                    Catch ex As Exception
                        context.Response.Write(ex.Message)
                    End Try
                    Return
                End If
            End If
            'End of changes

            Dim strXML As String
            Dim key As String = getURLParam(context, "key")
            Dim ProdID As String = getURLParam(context, "ProdID")
            Dim ProdRef As String = getURLParam(context, "ProdRef")
            Dim CatID As String = getURLParam(context, "CatID")
            Dim PortalID As String = getURLParam(context, "PortalID")
            Dim Lang As String = getURLParam(context, "Lang")

            Dim objCtrl As New ProductController
            Dim objExp As New Export

            strXML = GetFeederReportText(context, True)

            If strXML <> "" Then

                Dim xmlDoc As New Xml.XmlDataDocument
                xmlDoc.LoadXml(strXML)

                context.Response.ContentType = "text/xml"
                context.Response.ContentEncoding = System.Text.Encoding.UTF8
                context.Response.Cache.SetAllowResponseInBrowserHistory(True)

                xmlDoc.Save(context.Response.OutputStream)

            End If

        End Sub
#End Region

#Region "Private Methods"




#End Region

    End Class

End Namespace
