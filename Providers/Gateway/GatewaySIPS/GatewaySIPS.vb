' --- Copyright (c) notice NevoWeb ---
'  Copyright (c) 2008 SARL NevoWeb.  www.nevoweb.com. All rights are reserved.
' Author: D.C.Lee
' ------------------------------------------------------------------------
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' ------------------------------------------------------------------------
' This copyright notice may NOT be removed, obscured or modified without written consent from the author.
' --- End copyright notice --- 


Imports System
Imports System.Web
Imports System.Net
Imports System.IO
Imports NEvoWeb.Modules.NB_Store.SharedFunctions
Imports System.Collections.Specialized

Namespace NEvoWeb.Modules.NB_Store.Gateway

    Public Class GatewaySIPS
        Inherits GatewayInterface


        Public Overrides Function GetButtonHtml(ByVal PortalID As Integer, ByVal OrderID As Integer, ByVal Lang As String) As String
            'Do SIPS connection call
            Dim transaction As New SIPSExec.ExecuteClass
            Dim objOCtrl As New OrderController
            Dim objSCtrl As New SettingsController
            Dim objOInfo As NB_Store_OrdersInfo
            Dim objSInfo As NB_Store_SettingsInfo
            objOInfo = objOCtrl.GetOrder(OrderID)
            Dim parm As String = ""

            parm = GetGatewayParms(PortalID, Lang)
            If parm.Trim(" ") <> "" And objOInfo IsNot Nothing Then

                parm = Replace(parm, "[ORDERID]", objOInfo.OrderID.ToString)
                parm = Replace(parm, "[TRANSID]", objOInfo.OrderID.ToString("00000"))
                parm = Replace(parm, "[ORDERNUMBER]", objOInfo.OrderNumber.ToString)
                parm = Replace(parm, "[ORDERAMOUNT]", Replace(Replace(objOInfo.Total.ToString("0.00"), ".", ""), ",", ""))
                parm = Replace(parm, "[CUSTOMERID]", objOInfo.UserID.ToString)
                parm = Replace(parm, "[LANGUAGE]", Left(Lang, 2))
                parm = Replace(parm, "[PROMOCODE]", objOInfo.PromoCode)
                parm = Replace(parm, "[CARTID]", CurrentCart.GetCurrentCart(PortalID).CartID)

                objSInfo = objSCtrl.GetSetting(PortalID, "SIPS.exepath", Lang)
                If Not objSInfo Is Nothing Then
                    transaction.cmdLine = objSInfo.SettingValue & "request.exe"
                End If

                transaction.parameters = parm

                ' Appel de l'activeX pour executer request
                Dim result As String = transaction.ExecuteApp()

                ' Sortie de la fonction executeApp() : !code!error!buffer!
                ' 	- code=0	: la fonction génère une page html contenue dans la variable buffer
                ' 	- code=-1 	: La fonction retourne un message d'erreur dans la variable error
                ' Libération des ressources

                transaction = Nothing

                ' Exploitation des résultats
                ' Analyse du code retour

                Dim tableau As String() = Split(result, "!")

                Dim code As String = ""
                code = tableau(1)
                Dim error_msg As String = tableau(2)

                If code.Equals("") Or code.Equals("-1") Then
                    Return error_msg
                Else
                    Dim message As String = tableau(3)
                    Dim strSIPSAction As String = GetActionUrl(message)
                    message = Replace(message, "<FORM METHOD=POST ACTION=""" & strSIPSAction & """>", "")
                    message = Replace(message, "</FORM>", "")
                    'store actionurl in cart for use when client clicks paybutton
                    Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalID)
                    objCInfo.BankHtmlRedirect = strSIPSAction
                    CurrentCart.Save(objCInfo)

                    Return message
                End If
            Else
                Return "NO GATEWAY SETTINGS OR ORDERID FOUND!!"
            End If

        End Function


        Public Overrides Sub AutoResponse(ByVal PortalID As Integer, ByVal Request As System.Web.HttpRequest)
            Dim objSCtrl As New SettingsController
            Dim objOCtrl As New OrderController
            Dim objPCtrl As New ProductController
            Dim objOInfo As NB_Store_OrdersInfo
            Dim objSInfo As NB_Store_SettingsInfo
            Dim ordID As Integer = -1

            UpdateLog("SIPS Auto Return")

            Dim settingsTable As Hashtable = createSettingsTable(PortalID, "None")

            If settingsTable.Count > 0 Then
                'Do SIPS connection call
                Dim transaction As New SIPSExec.ExecuteClass

                Dim message As String = Request.Form("DATA")

                If message = "" Then
                    UpdateLog("No SIPS Data")
                Else
                    message = "message=" & message
                    Dim pathfile As String = "pathfile=" & settingsTable("pathfile")

                    objSInfo = objSCtrl.GetSetting(PortalID, "SIPS.exepath", "None")
                    If Not objSInfo Is Nothing Then
                        transaction.cmdLine = objSInfo.SettingValue & "response.exe"
                        transaction.parameters = pathfile & " " & message
                    End If

                    Dim result As String = transaction.ExecuteApp()

                    transaction = Nothing

                    Dim tableau As String() = Split(result, "!")

                    Dim code As String = tableau(1)
                    Dim error_msg As String = tableau(2)

                    If code = "" Or code = "-1" Then
                        UpdateLog("ERROR API message : " & error_msg)
                        'send email to administrator
                        SendEmailToAdministrator(PortalID, "ERROR API Portal:" & PortalID.ToString, "ERROR API message : " & error_msg & vbCrLf & vbCrLf & result)
                    Else

                        ' L'execution s'est bien deroulee
                        ' recuperation des donnees de la reponse

                        Dim merchant_id As String = tableau(3)
                        Dim merchant_country As String = tableau(4)
                        Dim amount As String = tableau(5)
                        Dim transaction_id As String = tableau(6)
                        Dim payment_means As String = tableau(7)
                        Dim transmission_date As String = tableau(8)
                        Dim payment_time As String = tableau(9)
                        Dim payment_date As String = tableau(10)
                        Dim response_code As String = tableau(11)
                        Dim payment_certificate As String = tableau(12)
                        Dim authorisation_id As String = tableau(13)
                        Dim currency_code As String = tableau(14)
                        Dim card_number As String = tableau(15)
                        Dim cvv_flag As String = tableau(16)
                        Dim cvv_response_code As String = tableau(17)
                        Dim bank_response_code As String = tableau(18)
                        Dim complementary_code As String = tableau(19)
                        Dim complementary_info As String = tableau(20)
                        Dim return_context As String = tableau(21)
                        Dim caddie As String = tableau(22)
                        Dim receipt_complement As String = tableau(23)
                        Dim merchant_language As String = tableau(24)
                        Dim language As String = tableau(25)
                        Dim customer_id As String = tableau(26)
                        Dim order_id As String = tableau(27)
                        Dim customer_email As String = tableau(28)
                        Dim customer_ip_address As String = tableau(29)
                        Dim capture_day As String = tableau(30)
                        Dim capture_mode As String = tableau(31)
                        Dim data As String = tableau(32)

                        ' Sauvegarde des champs de la reponse
                        Dim Lmsg As String

                        Lmsg = merchant_id & ","
                        Lmsg += merchant_country & ","
                        Lmsg += amount & ","
                        Lmsg += transaction_id & ","
                        Lmsg += transmission_date & ","
                        Lmsg += payment_means & ","
                        Lmsg += payment_time & ","
                        Lmsg += payment_date & ","
                        Lmsg += response_code & ","
                        Lmsg += payment_certificate & ","
                        Lmsg += authorisation_id & ","
                        Lmsg += currency_code & ","
                        Lmsg += card_number & ","
                        Lmsg += cvv_flag & ","
                        Lmsg += cvv_response_code & ","
                        Lmsg += bank_response_code & ","
                        Lmsg += complementary_code & ","
                        Lmsg += complementary_info & ","
                        Lmsg += return_context & ","
                        Lmsg += caddie & ","
                        Lmsg += receipt_complement & ","
                        Lmsg += merchant_language & ","
                        Lmsg += language & ","
                        Lmsg += customer_id & ","
                        Lmsg += order_id & ","
                        Lmsg += customer_email & ","
                        Lmsg += customer_ip_address & ","
                        Lmsg += capture_day & ","
                        Lmsg += capture_mode & ","
                        Lmsg += data & ","

                        UpdateLog(Lmsg)

                        'update database stuff
                        Dim oID As Integer
                        If order_id <> "" Then
                            oID = CInt(order_id)
                        Else
                            oID = CInt(transaction_id)
                        End If

                        'clear cart parameters 
                        Dim objCCtrl As New CartController
                        Dim objCInfo As NB_Store_CartInfo = objCCtrl.GetCart(caddie)
                        If Not objCInfo Is Nothing Then
                            objCInfo.BankTransID = -1
                            objCInfo.BankHtmlRedirect = ""
                            objCCtrl.UpdateObjCart(objCInfo)
                        End If

                        objOInfo = objOCtrl.GetOrder(oID)
                        If Not objOInfo Is Nothing Then
                            'check that the order is valid. (Not yet been processed)
                            If Not objOInfo.OrderIsPlaced Then
                                If response_code = "00" Then
                                    'remove qty in trans
                                    objPCtrl.UpdateStockLevel(objOInfo.OrderID)
                                    'set order status to Payed
                                    objOInfo.OrderStatusID = 40
                                    Dim UsrID As Integer = objOInfo.UserID
                                    If UsrID = -1 Then UsrID = 0
                                    objOInfo.OrderNumber = Format(PortalID, "00") & "-" & UsrID.ToString("0000#") & "-" & objOInfo.OrderID.ToString("0000#") & "-" & objOInfo.OrderDate.ToString("yyyyMMdd")
                                    objOInfo.OrderIsPlaced = True
                                    objOInfo.PayType = "SIPS"
                                    objOCtrl.UpdateObjOrder(objOInfo)

                                    SendEmailToClient(PortalID, GetClientEmail(PortalID, objOInfo), objOInfo.OrderNumber, objOInfo, "paymentOK.email", GetClientLang(PortalID, objOInfo))
                                    SendEmailToManager(PortalID, objOInfo.OrderNumber, objOInfo, "paymentOK.email")

                                Else
                                    'check if order has already been cancelled
                                    If Not objOInfo.OrderStatusID = 30 Then
                                        'remove qty in trans
                                        objPCtrl.RemoveModelQtyTrans(objOInfo.OrderID)
                                        'set order status to cancelled
                                        objOInfo.OrderStatusID = 30
                                        objOCtrl.UpdateObjOrder(objOInfo)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End Sub

        Public Overrides Function GetCompletedHtml(ByVal PortalID As Integer, ByVal UserID As Integer, ByVal Request As System.Web.HttpRequest) As String
            Dim objSCtrl As New SettingsController
            Dim objOCtrl As New OrderController
            Dim objPCtrl As New ProductController
            Dim objOInfo As NB_Store_OrdersInfo
            Dim objSInfo As NB_Store_SettingsTextInfo
            Dim ordID As Integer = -1
            Dim rtnMsg As String = ""
            Dim SIPSAction As String = ""

            If Not Request.QueryString("SIPSAction") Is Nothing Then
                SIPSAction = Request.QueryString("SIPSAction").ToUpper
            End If

            ordID = Request.QueryString("ordID")

            'set rtnMsg to security error message
            objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentSECURITY.text", GetCurrentCulture)
            rtnMsg = objSInfo.SettingText
            If IsNumeric(ordID) Then
                objOInfo = objOCtrl.GetOrder(ordID)
                If Not objOInfo Is Nothing Then
                    If objOInfo.UserID = UserID Then
                        Select Case SIPSAction
                            Case "CANCEL"
                                ' check if order has already been processed
                                If Not objOInfo.OrderIsPlaced Then
                                    'check if order has already been cancelled
                                    If Not objOInfo.OrderStatusID = 30 Then
                                        'remove qty in trans
                                        objPCtrl.RemoveModelQtyTrans(objOInfo.OrderID)
                                        'set order status to cancelled
                                        objOInfo.OrderStatusID = 30
                                        objOCtrl.UpdateObjOrder(objOInfo)
                                    End If
                                    'set Cancel order message
                                    objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentFAIL.text", GetCurrentCulture)
                                    rtnMsg = objSInfo.SettingText

                                End If
                            Case "RETURN"
                                ' check if order has already been processed
                                If Not objOInfo.OrderIsPlaced Then
                                    'remove qty in trans
                                    objPCtrl.UpdateStockLevel(objOInfo.OrderID)
                                    'set order status to Payed
                                    objOInfo.OrderStatusID = 45
                                    objOInfo.OrderNumber = Format(PortalID, "00") & "-" & UserID.ToString("0000#") & "-" & objOInfo.OrderID.ToString("0000#") & "-" & objOInfo.OrderDate.ToString("yyyyMMdd")
                                    objOInfo.OrderIsPlaced = True
                                    objOCtrl.UpdateObjOrder(objOInfo)
                                    SendEmailToManager(PortalID, objOInfo.OrderNumber, objOInfo, "paymentunverified.email")
                                End If
                                'set completed order message
                                objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentOK.text", GetCurrentCulture)
                                rtnMsg = objSInfo.SettingText

                            Case Else
                                ' break
                        End Select

                        'Do Token Replacement
                        Dim objTR As New TokenStoreReplace(objOInfo, GetMerchantCulture(PortalID))
                        rtnMsg = objTR.DoTokenReplace(rtnMsg)
                    End If
                End If
            End If

            Return rtnMsg
        End Function

        Public Overrides Function GetBankClick(ByVal PortalID As Integer, ByVal Request As System.Web.HttpRequest) As Boolean
            'Test if data from sips form has been found
            If GetImageKeyValueX(PortalID, Request) = "" Then
                Return False
            Else
                Return True
            End If
        End Function

        Public Overrides Sub SetBankRemotePost(ByVal PortalID As Integer, ByVal OrderID As Integer, ByVal Lang As String, ByVal Request As System.Web.HttpRequest)
            Dim RPost As New RemotePost
            'test if SIP button has been clicked
            If GetImageKeyValueX(PortalID, Request) <> "" Then

                Dim objSCtrl As New SettingsController
                Dim objSInfo As NB_Store_SettingsInfo

                Dim objOCtrl As New OrderController
                Dim oInfo As NB_Store_OrdersInfo = objOCtrl.GetOrder(OrderID)

                If Not oInfo Is Nothing Then

                    Dim SIPSTypeX As String = GetImageKeyValueX(PortalID, Request)
                    Dim SIPSTypeY As String = GetImageKeyValueY(PortalID, Request)

                    If SIPSTypeX <> "" And SIPSTypeY <> "" Then

                        'update stock tranction in progress
                        Dim objPCtrl As New ProductController
                        objPCtrl.UpdateModelQtyTrans(oInfo.OrderID)

                        'set order status to redirect to bank
                        oInfo.OrderStatusID = 20
                        objOCtrl.UpdateObjOrder(oInfo)

                        objSInfo = objSCtrl.GetSetting(PortalID, "gateway.loadimage", "None")

                        RPost = New RemotePost

                        'get back actionurl saved when button html fetched
                        RPost.Url = CurrentCart.GetCurrentCart(PortalID).BankHtmlRedirect
                        RPost.Add("DATA", Request.Form.Item("DATA").ToString)
                        RPost.Add(SIPSTypeX, Request.Form.Item(SIPSTypeX).ToString)
                        RPost.Add(SIPSTypeY, Request.Form.Item(SIPSTypeY).ToString)

                        Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalID)
                        objCInfo.BankHtmlRedirect = RPost.GetPostHtml(objSInfo.SettingValue)
                        CurrentCart.Save(objCInfo)

                        UpdateLog("SIPS RemotePost")
                    Else
                        UpdateLog("SIPS ERROR, Blank SIPSTypes.  Unable to redirect to bank.")
                    End If
                Else
                    UpdateLog("SIPS ERROR, No Order Found.  Unable to redirect to bank.")
                End If
            End If
        End Sub

        Public Function createSettingsTable(ByVal Portalid As Integer, ByVal Lang As String) As Hashtable
            Dim objSCtrl As New SettingsController
            Dim objSInfo As NB_Store_SettingsInfo

            objSInfo = objSCtrl.GetSetting(Portalid, "SIPS.gateway", Lang)
            If objSInfo IsNot Nothing Then
                Return ParseGateway(objSInfo.SettingValue)
            End If
            Return Nothing
        End Function

        Private Function GetGatewayParms(ByVal Portalid As Integer, ByVal Lang As String) As String
            Dim objSCtrl As New SettingsController
            Dim objSInfo As NB_Store_SettingsInfo
            Dim HashTab As New Hashtable
            Dim strRtn As String = " "
            Dim d As DictionaryEntry

            objSInfo = objSCtrl.GetSetting(Portalid, "SIPS.gateway", Lang)
            If objSInfo IsNot Nothing Then
                HashTab = ParseGateway(objSInfo.SettingValue)
                For Each d In HashTab
                    strRtn &= d.Key.ToString & "=" & d.Value.ToString & " "
                Next

            End If
            Return strRtn
        End Function

        Private Function GetImageKeyValueX(ByVal Portalid As Integer, ByVal Request As System.Web.HttpRequest) As String
            Return GetImageKeyValue(Portalid, Request, ".x")
        End Function

        Private Function GetImageKeyValueY(ByVal Portalid As Integer, ByVal Request As System.Web.HttpRequest) As String
            Return GetImageKeyValue(Portalid, Request, ".y")
        End Function

        Private Function GetImageKeyValue(ByVal Portalid As Integer, ByVal Request As System.Web.HttpRequest, ByVal KeyExt As String) As String
            Dim ImageKeyX As String = ""

            Dim settingsTable As Hashtable = createSettingsTable(Portalid, GetCurrentCulture)
            Dim SIPSTypeList As String = settingsTable.Item("payment_means")

            Dim lp As Integer
            Dim c As Integer = 0
            For lp = 0 To Request.Form.Count - 1
                If Request.Form.Keys(lp).ToLower.EndsWith(KeyExt.ToLower) Then
                    If SIPSTypeList.ToLower.Contains(Replace(Request.Form.Keys(lp).ToLower, KeyExt.ToLower, "")) Then
                        ImageKeyX = Request.Form.Keys(lp).ToString
                        c = c + 1
                    End If
                End If
            Next
            If c = 1 Then
                Return ImageKeyX
            Else
                Return ""
            End If
        End Function

        Private Function GetActionUrl(ByVal message As String) As String
            Dim StartPos As Integer
            Dim EndPos As Integer
            Dim SIPSAct As String

            If message = "" Then
                SIPSAct = "No SIPS message returned from API"
            Else
                StartPos = message.IndexOf("""")
                EndPos = message.IndexOf("""", StartPos + 1)
                SIPSAct = message.Substring(StartPos + 1, ((EndPos - StartPos) - 1))
            End If

            Return SIPSAct

        End Function


    End Class


End Namespace

