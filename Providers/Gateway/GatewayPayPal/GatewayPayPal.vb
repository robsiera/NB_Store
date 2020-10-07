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
Imports System.Text.RegularExpressions

Namespace NEvoWeb.Modules.NB_Store.Gateway

    Public Class GatewayPayPal
        Inherits GatewayInterface

        ' ------------------------------------------------------------------------
        ' Step One: Display payment buttons on website checkout.
        ' This function simply send back the html needed to display a payment button(s) on the checkout page.
        ' NOTE:
        ' - The html in an imput field with a NAME attribute, which will be used later to check on postback
        '   to see if the button has been pressed.
        ' - All parameters for the gateway are assigned by reading the gateway xml settings (In this case "PayPal.gateway")
        ' ------------------------------------------------------------------------
        Public Overrides Function GetButtonHtml(ByVal PortalID As Integer, ByVal OrderID As Integer, ByVal Lang As String) As String
            Dim strHTML As String = ""
            Dim objSCtrl As New SettingsController
            Dim objSInfo As NB_Store_SettingsInfo
            objSInfo = objSCtrl.GetSetting(PortalID, "PayPal.gateway", Lang)

            If Not objSInfo Is Nothing Then
                Dim setParams As Hashtable = createSettingsTable(objSInfo.SettingValue)

                strHTML = "<INPUT TYPE=IMAGE NAME=PAYPAL BORDER=0 SRC=""" & setParams("ButtonImageURL") & """/>"

            End If

            Return strHTML
        End Function

        ' ------------------------------------------------------------------------
        ' Step Two: Create the html to redirect the user, if the payment button is clicked.
        ' This function create the html needed to redirect the user to the banking gateway by using the "RemotePost.vb" in NB_Store.
        ' NOTE:
        ' - All parameters for the gateway are assigned by reading the gateway xml settings (In this case "PayPal.gateway")
        ' - On Checkout.ascx postback, the user will be redirected to "Remotepost.ascx", which will read the Html create
        '   in this function and write the html as a httpcontext response.
        ' ------------------------------------------------------------------------
        Public Overrides Sub SetBankRemotePost(ByVal PortalID As Integer, ByVal OrderID As Integer, ByVal Lang As String, ByVal Request As System.Web.HttpRequest)

            Dim RPost As New RemotePost

            Dim objSCtrl As New SettingsController
            Dim objSInfo As NB_Store_SettingsInfo
            objSInfo = objSCtrl.GetSetting(PortalID, "PayPal.gateway", Lang)
            If Not objSInfo Is Nothing Then

                Dim setParams As Hashtable = createSettingsTable(objSInfo.SettingValue)

                Dim returnURL As String = setParams("ReturnURL")
                Dim cancelURL As String = setParams("ReturnCancelURL")
                Dim notifyURL As String = setParams("ReturnNotifyURL")

                Dim payPalURL As String = setParams("paymentURL")

                Dim PayPalID As String = setParams("PayPalID")
                Dim PayPalCartName As String = setParams("CartName")
                Dim PayPalButtonURL As String = setParams("ButtonImageURL")
                Dim PayPalCurrency As String = setParams("Currency")
                Dim PayPalMerchantLanguage As String = setParams("MerchantLanguage")

                Dim objOCtrl As New OrderController
                Dim oInfo As NB_Store_OrdersInfo = objOCtrl.GetOrder(OrderID)

                If Not oInfo Is Nothing Then

                    returnURL = Replace(returnURL, "[ORDERID]", oInfo.OrderID.ToString)
                    cancelURL = Replace(cancelURL, "[ORDERID]", oInfo.OrderID.ToString)
                    notifyURL = Replace(notifyURL, "[ORDERID]", oInfo.OrderID.ToString)
                    PayPalCartName = Replace(PayPalCartName, "[CARTID]", CurrentCart.GetCurrentCart(PortalID).CartID)

                    payPalURL += "?business=" & PayPalEncode(PayPalID)
                    payPalURL += "&item_name=" & PayPalEncode(PayPalCartName)
                    payPalURL += "&item_number=" & HTTPPOSTEncode(oInfo.OrderID.ToString)
                    payPalURL += "&custom=" & GetCurrentCulture()
                    payPalURL += "&amount=" & HTTPPOSTEncode(Replace(oInfo.CartTotal.ToString("0.00"), ",", "."))
                    payPalURL += "&shipping=" & HTTPPOSTEncode(Replace(oInfo.ShippingCost.ToString("0.00"), ",", "."))
                    payPalURL += "&tax=" & HTTPPOSTEncode(Replace(oInfo.AppliedTax.ToString("0.00"), ",", "."))
                    payPalURL += "&currency_code=" & HTTPPOSTEncode(PayPalCurrency)
                    payPalURL += "&return=" & HTTPPOSTEncode(returnURL)
                    payPalURL += "&cancel_return=" & HTTPPOSTEncode(cancelURL)
                    payPalURL += "&notify_url=" & HTTPPOSTEncode(notifyURL)

                    If setParams("paypalurl") Is Nothing Then
                        payPalURL += "&bn=" & HTTPPOSTEncode("NB_Store")
                        payPalURL += "&quantity=1"
                        payPalURL += "&undefined_quantity=0"
                        payPalURL += "&no_note=1"
                        payPalURL += "&no_shipping=1"
                    Else
                        If Not oInfo Is Nothing Then
                            Dim xmlDoc As New Xml.XmlDataDocument
                            Dim xmlNod As Xml.XmlNode
                            Dim xmlNodList As Xml.XmlNodeList
                            Dim objTR As New TokenStoreReplace(oInfo, GetCurrentCulture)

                            xmlDoc.LoadXml(objSInfo.SettingValue)

                            xmlNodList = xmlDoc.SelectNodes("root/paypalurl/*")

                            For Each xmlNod In xmlNodList
                                payPalURL += "&" & xmlNod.Name & "=" & HTTPPOSTEncode(objTR.DoTokenReplace(xmlNod.InnerText))
                            Next

                        End If
                    End If


                    'update stock tranction in progress
                    Dim objPCtrl As New ProductController
                    objPCtrl.UpdateModelQtyTrans(oInfo.OrderID)

                    'set order status to redirect to bank
                    oInfo.OrderStatusID = 20
                    objOCtrl.UpdateObjOrder(oInfo)

                    objSInfo = objSCtrl.GetSetting(PortalID, "gateway.loadimage", "None")

                    RPost = New RemotePost
                    RPost.Url = payPalURL

                    Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalID)
                    'Save the report html into the cart, the user will be redirect to this 
                    objCInfo.BankHtmlRedirect = RPost.GetPostHtml(objSInfo.SettingValue)
                    CurrentCart.Save(objCInfo)

                    UpdateLog("PayPal URL = " & payPalURL)

                End If
            End If
        End Sub

        Private Function PayPalEncode(value As String) As String
            'a single accentuated/special character matches a single non acc/spec character:
            value = StringListReplace(value, "ŠŽŸÀÁÂÃÅÇÈÉÊËÌÍÎÏÐÑÒÓÔÕØÙÚÛÝÞØ", "SZYAAAAACEEEEIIIIDNOOOOOUUUYPO")
            value = StringListReplace(value, "šžÿàáâãåçèéêëìíîïðñòóôõøùúûýþµ", "szyaaaaaceeeeiiiidnooooouuuypu")

            'a single accentuated/special character matches a couple of non acc/spec character:
            value = value.Replace("Œ", "OE")
            value = value.Replace("Æ", "AE")
            value = value.Replace("œ", "oe")
            value = value.Replace("æ", "ae")

            Return HTTPPOSTEncode(value)
        End Function

        Private Function StringListReplace(value As String, searchfor As String, replacewith As String) As String
            For x = 1 To searchfor.Length
                value = value.Replace(searchfor.Substring(x - 1, 1), replacewith.Substring(x - 1, 1))
            Next
            Return value
        End Function

        ' ------------------------------------------------------------------------
        ' Step Three: Auto Response from banking gateway.
        ' If the banking gateway supports Automatic response (A url is send to the store just after the users make the payment, 
        '  but before they click to return to the store) then this function deals with what needs to be done in NB_Store.
        ' NOTE:
        ' - All parameters for the gateway are assigned by reading the gateway xml settings (In this case "PayPal.gateway")
        ' - Not all gateways support this, in which case the payment processing for NB_Store needs to be done in the "GetCompletedHtml" function.
        ' ------------------------------------------------------------------------
        Public Overrides Sub AutoResponse(ByVal PortalID As Integer, ByVal Request As System.Web.HttpRequest)
            Try

                Dim objSCtrl As New SettingsController
                Dim objOCtrl As New OrderController
                Dim objPCtrl As New ProductController
                Dim objOInfo As NB_Store_OrdersInfo
                Dim objSInfo As NB_Store_SettingsInfo
                Dim ordID As Integer = -1

                Dim ipn As PayPalIPNParameters = New PayPalIPNParameters(Request.Form)
                UpdateLog("IPN = " & ipn.ToString)

                'clear cart parameters 
                Dim objCCtrl As New CartController
                Dim objCInfo As NB_Store_CartInfo = objCCtrl.GetCart(ipn.CartID)
                If Not objCInfo Is Nothing Then
                    objCInfo.BankTransID = -1
                    objCInfo.BankHtmlRedirect = ""
                    objCCtrl.UpdateObjCart(objCInfo)
                End If

                objOInfo = objOCtrl.GetOrder(CInt(ipn.item_number))
                If Not objOInfo Is Nothing Then
                    'check that the order is valid. (Not yet been processed)
                    If Not objOInfo.OrderIsPlaced Then
                        If ipn.IsValid Then
                            ' check if order has already been processed
                            If Not objOInfo.OrderIsPlaced Then
                                'remove qty in trans
                                objPCtrl.UpdateStockLevel(objOInfo.OrderID)
                                'set order status to Payed
                                objOInfo.OrderStatusID = 40
                                Dim UsrID As Integer = objOInfo.UserID
                                If UsrID = -1 Then UsrID = 0
                                objOInfo.OrderNumber = Format(PortalID, "00") & "-" & UsrID.ToString("0000#") & "-" & objOInfo.OrderID.ToString("0000#") & "-" & objOInfo.OrderDate.ToString("yyyyMMdd")
                                objOInfo.OrderIsPlaced = True
                                objOCtrl.UpdateObjOrder(objOInfo)

                                'update in email
                                Dim merchant_language As String
                                Dim VerifyPay As Boolean
                                Dim language As String = ipn.custom
                                objSInfo = objSCtrl.GetSetting(PortalID, "PayPal.gateway", "None")
                                If Not objSInfo Is Nothing Then
                                    Dim setParams As Hashtable = createSettingsTable(objSInfo.SettingValue)
                                    VerifyPay = VerifyPayment(ipn, setParams("verifyURL"))
                                    merchant_language = setParams("MerchantLanguage")
                                End If

                                SendEmailToClient(PortalID, GetClientEmail(PortalID, objOInfo), objOInfo.OrderNumber, objOInfo, "paymentOK.email", language)
                                If VerifyPay Then
                                    SendEmailToManager(PortalID, objOInfo.OrderNumber, objOInfo, "paymentOK.email")
                                Else
                                    SendEmailToManager(PortalID, objOInfo.OrderNumber, objOInfo, "paymentunverified.email")
                                End If

                            End If
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
            Catch ex As Exception
                DotNetNuke.Services.Exceptions.LogException(ex) 'something went wrong, lets log the error...
                SendEmailToAdministrator(PortalID, "PayPal AutoReturn ERROR", ex.ToString)
            End Try
        End Sub

        ' ------------------------------------------------------------------------
        ' Step Four: User returned from banking gateway.
        ' Once the user has made a payment and clicked to return to the store, then this function deals with displaying
        '  the return message, either success of failed.
        ' It this case has the functionaity to process the order on the NB_Store side has been added, in case the AutoResponse (IPN in PayPal)
        '  has not been setup.
        ' NOTE:
        ' - All parameters for the gateway are assigned by reading the gateway xml settings (In this case "PayPal.gateway")
        ' - We need to test if the autoreponse has already processed the order, by using the order status and OrderIsPlaced properties.
        ' ------------------------------------------------------------------------
        Public Overrides Function GetCompletedHtml(ByVal PortalID As Integer, ByVal UserID As Integer, ByVal Request As System.Web.HttpRequest) As String
            Dim objSCtrl As New SettingsController
            Dim objOCtrl As New OrderController
            Dim objPCtrl As New ProductController
            Dim objOInfo As NB_Store_OrdersInfo
            Dim objSInfo As NB_Store_SettingsTextInfo
            Dim ordID As Integer = -1
            Dim rtnMsg As String = ""

            If IsNumeric(Request.QueryString("ordID")) Then
                'set rtnMsg to security error message
                objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentSECURITY.text", GetCurrentCulture)
                rtnMsg = objSInfo.SettingText
                ordID = Request.QueryString("ordID")
                objOInfo = objOCtrl.GetOrder(ordID)
                If Not objOInfo Is Nothing Then
                    If objOInfo.UserID = UserID Then

                        Select Case UCase(Request.QueryString("PayPalExit"))
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
                                If Not objOInfo.OrderIsPlaced And Not objOInfo.OrderStatusID = 30 Then
                                    'remove qty in trans
                                    objPCtrl.UpdateStockLevel(objOInfo.OrderID)
                                    'set order status to Payed
                                    objOInfo.OrderStatusID = 45
                                    Dim UsrID As Integer = objOInfo.UserID
                                    If UsrID = -1 Then UsrID = 0
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
            'test if Paypal button has been clicked
            If Not Request.Form.Item("PAYPAL.x") Is Nothing Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Function createSettingsTable(ByVal SettingsParams As String) As Hashtable
            Return ParseGateway(SettingsParams)
        End Function

        Private Function VerifyPayment(ByVal ipn As PayPalIPNParameters, ByVal verifyURL As String) As Boolean
            Try 

                Dim isVerified As Boolean = False
                
                If ipn.IsValid Then

                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

                    Dim PPrequest As HttpWebRequest = CType(WebRequest.Create(verifyURL), HttpWebRequest)
                    If Not (PPrequest Is Nothing) Then
                        PPrequest.Method = "POST"
                        PPrequest.ContentLength = ipn.PostString.Length
                        PPrequest.ContentType = "application/x-www-form-urlencoded"
                        Dim writer As StreamWriter = New StreamWriter(PPrequest.GetRequestStream)
                        writer.Write(ipn.PostString)
                        writer.Close()
                        Dim response As HttpWebResponse = CType(PPrequest.GetResponse, HttpWebResponse)
                        If Not (response Is Nothing) Then
                            Dim reader As StreamReader = New StreamReader(response.GetResponseStream)
                            Dim responseString As String = reader.ReadToEnd
                            reader.Close()
                            If String.Compare(responseString, "VERIFIED", True) = 0 Then
                                isVerified = True
                            End If
                        End If
                    End If
                End If
                Return isVerified

            Catch ex As Exception
                return False
            End Try
        End Function

    End Class



    Public Class PayPalIPNParameters
        Inherits RequestFormWrapper

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal requestForm As NameValueCollection)
            MyBase.New(requestForm)
            _postString = "cmd=_notify-validate"
            For Each paramName As String In requestForm
                _postString += String.Format("&{0}={1}", paramName, HTTPPOSTEncode(requestForm(paramName)))
                Select Case paramName
                    Case "payment_status"
                        _payment_status = requestForm(paramName)
                    Case "item_number"
                        _item_number = CInt(requestForm(paramName))
                    Case "custom"
                        _custom = requestForm(paramName)
                End Select
            Next
        End Sub
        Private _postString As String = String.Empty
        Private _payment_status As String = String.Empty
        Private _txn_id As String = String.Empty
        Private _receiver_email As String = String.Empty
        Private _email As String = String.Empty
        Private _custom As String = ""
        Private _item_number As Integer = -1
        Private _mc_gross As Decimal = -1
        Private _shipping As Decimal = -1
        Private _tax As Decimal = -1

        Public Property PostString() As String
            Get
                Return _postString
            End Get
            Set(ByVal Value As String)
                _postString = Value
            End Set
        End Property

        Public Property payment_status() As String
            Get
                Return _payment_status
            End Get
            Set(ByVal Value As String)
                _payment_status = Value
            End Set
        End Property

        Public Property txn_id() As String
            Get
                Return _txn_id
            End Get
            Set(ByVal Value As String)
                _txn_id = Value
            End Set
        End Property

        Public Property receiver_email() As String
            Get
                Return _receiver_email
            End Get
            Set(ByVal Value As String)
                _receiver_email = Value
            End Set
        End Property

        Public Property email() As String
            Get
                Return _email
            End Get
            Set(ByVal Value As String)
                _email = Value
            End Set
        End Property

        Public Property custom() As String
            Get
                Return _custom
            End Get
            Set(ByVal Value As String)
                _custom = Value
            End Set
        End Property

        Public Property item_number() As Integer
            Get
                Return _item_number
            End Get
            Set(ByVal Value As Integer)
                _item_number = Value
            End Set
        End Property

        Public Property mc_gross() As Decimal
            Get
                Return _mc_gross
            End Get
            Set(ByVal Value As Decimal)
                _mc_gross = Value
            End Set
        End Property

        Public Property shipping() As Decimal
            Get
                Return _shipping
            End Get
            Set(ByVal Value As Decimal)
                _shipping = Value
            End Set
        End Property

        Public Property tax() As Decimal
            Get
                Return _tax
            End Get
            Set(ByVal Value As Decimal)
                _tax = Value
            End Set
        End Property

        Public ReadOnly Property CartID() As Integer
            Get
                Return _item_number
            End Get
        End Property

        Public ReadOnly Property ShipToID() As Integer
            Get
                ShipToID = -1
                If _custom >= 0 Then
                    ShipToID = _custom
                Else
                    ShipToID = _item_number
                End If
                Return ShipToID
            End Get
        End Property

        Public ReadOnly Property IsValid() As Boolean
            Get
                If _payment_status <> "Completed" And _payment_status <> "Pending" Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

    End Class


End Namespace

