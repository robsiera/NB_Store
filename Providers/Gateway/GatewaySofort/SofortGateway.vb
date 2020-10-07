Imports System
Imports NEvoWeb.Modules.NB_Store.SharedFunctions
Imports System.Web
Imports DotNetNuke.Security

Namespace NEvoWeb.Modules.NB_Store.Gateway

    Public Class GatewaySofort
        Inherits GatewayInterface
        Public Overrides Function GetButtonHtml(ByVal PortalID As Integer, ByVal OrderID As Integer, ByVal Lang As String) As String
            Dim strHTML As String = ""
            Dim objSCtrl As New SettingsController
            Dim objSInfo As NB_Store_SettingsInfo
            Dim objSTInfo As NB_Store_SettingsTextInfo

            'get the gateway settings from the NB_Store settings table
            objSInfo = objSCtrl.GetSetting(PortalID, "sofort.gateway", Lang)
            If Not objSInfo Is Nothing Then
                'Move the xml format settingn into the a hashtable so we can use it easily.
                Dim setParams As Hashtable = createSettingsTable(objSInfo.SettingValue)
                'Build Html to display the payment button.
                objSTInfo = objSCtrl.GetSettingsText(PortalID, "sofort.html", Lang)
                If objSTInfo Is Nothing Then
                    strHTML = "<input type=""image"" name=""sofortGateway"" border=""0"" src=""" & setParams("ButtonImageURL") & """/>"
                Else
                    strHTML = HttpUtility.HtmlDecode(objSTInfo.SettingText)
                End If
            End If
            Return strHTML
        End Function

        Public Overrides Sub AutoResponse(ByVal PortalID As Integer, ByVal Request As System.Web.HttpRequest)

        End Sub

        Public Overrides Function GetCompletedHtml(ByVal PortalID As Integer, ByVal UserID As Integer, ByVal Request As System.Web.HttpRequest) As String
            Dim objSCtrl As New SettingsController
            Dim objOCtrl As New OrderController
            Dim objPCtrl As New ProductController
            Dim objOInfo As NB_Store_OrdersInfo
            Dim objSInfo As NB_Store_SettingsTextInfo
            Dim ordID As Integer = -1
            Dim rtnMsg As String = ""


            'Get the order id back from the url.
            ordID = Request.QueryString("ordID")

            'set rtnMsg to security warning message.
            objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentSECURITY.text", GetCurrentCulture)
            rtnMsg = objSInfo.SettingText
            If IsNumeric(ordID) Then
                objOInfo = objOCtrl.GetOrder(ordID)
                If Not objOInfo Is Nothing Then
                    'Here we make sure that the user  trying to view the order is the same as the user that placed the order.
                    'If this is not the case, then we want to display the security warning.
                    If objOInfo.UserID = UserID Then

                        If Not objOInfo.OrderIsPlaced Then ' just check if order has already been processed.

                            'The user is OK, so continue to pocess the order.
                            Select Case UCase(Request.QueryString("status"))
                                Case "FAIL"
                                    ' check if order has already been processed
                                    'check if order has already been cancelled
                                    If Not objOInfo.OrderStatusID = Constants.OrderStatus.Cancelled Then
                                        'remove qty in trans
                                        objPCtrl.RemoveModelQtyTrans(objOInfo.OrderID)
                                        'set order status to cancelled
                                        objOInfo.OrderStatusID = Constants.OrderStatus.Cancelled '30
                                        objOCtrl.UpdateObjOrder(objOInfo)
                                    End If
                                    'set Cancel order message
                                    objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentFAIL.text", GetCurrentCulture)
                                    rtnMsg = objSInfo.SettingText

                                Case "OK"
                                    'remove qty in trans
                                    objPCtrl.UpdateStockLevel(objOInfo.OrderID)

                                    'set order status to Payed
                                    objOInfo.OrderStatusID = Constants.OrderStatus.PaymentOk
                                    objOInfo.OrderNumber = Format(PortalID, "00") & "-" & UserID.ToString("0000#") & "-" & objOInfo.OrderID.ToString("0000#") & "-" & objOInfo.OrderDate.ToString("yyyyMMdd")
                                    objOInfo.OrderIsPlaced = True
                                    objOCtrl.UpdateObjOrder(objOInfo)

                                    'Send email confirmation
                                    SendEmailToClient(PortalID, GetClientEmail(PortalID, objOInfo), objOInfo.OrderNumber, objOInfo, "paymentOK.email", GetCurrentCulture)
                                    SendEmailToManager(PortalID, objOInfo.OrderNumber, objOInfo, "paymentOK.email")

                                    'set completed order message
                                    objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentOK.text", GetCurrentCulture)
                                    rtnMsg = objSInfo.SettingText

                                Case Else
                                    ' break
                            End Select
                        Else
                            UpdateLog("Order has already been placed. display security warning. ORDERID=" & ordID.ToString)
                        End If

                        'Do Token Replacement
                        Dim objTR As New TokenStoreReplace(objOInfo, GetMerchantCulture(PortalID))
                        rtnMsg = objTR.DoTokenReplace(rtnMsg)
                    End If
                End If
            End If
            Return rtnMsg
        End Function
        Public Overrides Function GetBankClick(ByVal PortalID As Integer, ByVal Request As System.Web.HttpRequest) As Boolean
            'test if button has been clicked
            If Not Request.Form.Item("sofortGateway.x") Is Nothing Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Overrides Sub SetBankRemotePost(ByVal PortalID As Integer, ByVal OrderID As Integer, ByVal Lang As String, ByVal Request As System.Web.HttpRequest)
            Dim RPost As New RemotePost ' RemotePost class from NB_Store, this generates the re-direct html required.

            Dim GatewayParams As String = GetStoreSetting(PortalID, "sofort.gateway", "None")
            If GatewayParams <> "" Then

                'Parse the gateway setttings into a Hashtable for easy access.
                Dim setParams As Hashtable = createSettingsTable(GatewayParams)

                'get all sofort sendparams
                Dim strSendParams As String = "user_id|project_id|sender_holder|sender_account_number|sender_bank_code|sender_country_id|amount|currency_id|reason_1|reason_2|user_variable_0|user_variable_1|user_variable_2|user_variable_3|user_variable_4|user_variable_5"
                Dim strParamList() As String
                strParamList = strSendParams.Split("|"c)
                Dim Inputs As System.Collections.Specialized.NameValueCollection = New System.Collections.Specialized.NameValueCollection

                '-------------------------------------------------
                'Get the basic required data from the settings.
                Dim SofortURL As String = setParams("paymentURL") ' URL of the payment provider, where the client will be redirected so they can pay.
                Dim project_password As String = setParams("project_password")

                'Get the order we want to pay for.
                Dim objOCtrl As New OrderController
                Dim oInfo As NB_Store_OrdersInfo = objOCtrl.GetOrder(OrderID)
                If Not oInfo Is Nothing Then

                    'update stock transaction in progress
                    Dim objPCtrl As New ProductController
                    objPCtrl.UpdateModelQtyTrans(oInfo.OrderID)

                    'set order status to redirect to bank
                    oInfo.OrderStatusID = Constants.OrderStatus.WaitingforBank
                    objOCtrl.UpdateObjOrder(oInfo)

                    'get the gateway processing image we're going to display while connecting to the payment provider.
                    Dim gatewayImg As String = GetStoreSetting(PortalID, "gateway.loadimage", "None")

                    'Now assign the values we need to the RemotePost class, ready to build the re-direct html.
                    RPost = New RemotePost
                    RPost.Url = SofortURL

                    'set setting parmas to sofort send params
                    For lp As Integer = 0 To strParamList.GetUpperBound(0)
                        If Not setParams(strParamList(lp)) Is Nothing Then
                            If strParamList(lp) <> "" Then
                                Inputs.Add(strParamList(lp), setParams(strParamList(lp)))
                            End If
                        End If
                    Next

                    'add calculated fields.
                    'Inputs.Add("amount", HTTPPOSTEncode(oInfo.Total.ToString("0.00")))
                    'Inputs.Add("amount", HTTPPOSTEncode(oInfo.Total.ToString("F")))  'dotnet 3.5 and later
                    Inputs.Add("amount", oInfo.Total.ToString("F"))  'dotnet 3.5 and later
                    Inputs.Add("user_variable_1", oInfo.OrderID.ToString)
                    Inputs.Add("user_variable_2", CurrentCart.GetCurrentCart(PortalID).CartID)
                    Inputs.Add("user_variable_3", Lang)

                    'add fields to post form.
                    For lp As Integer = 0 To Inputs.Keys.Count - 1
                        RPost.Add(Inputs.Keys(lp), Inputs(Inputs.Keys(lp)))
                    Next

                    'calc and add the hash value to form fields
                    Dim strHash As String = calculateSendHash(Inputs, strParamList, project_password)
                    RPost.Add("hash", strHash)


                    '-------------------------------------------------
                    'We are now going to save the re-direct html into the cart data on the database.
                    '  This is because the actual re-direct is done by the checkout module, when it loads.

                    'Get the cart data from the database.
                    Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalID)
                    'Build and assign the re-direct html to the cart.
                    objCInfo.BankHtmlRedirect = RPost.GetPostHtml(gatewayImg)
                    'Save this data on the cart.
                    CurrentCart.Save(objCInfo)
                    '-------------------------------------------------

                    'Do a log statement of the html created, so we have a trace of what data has been passed.
                    UpdateLog("Sofort HTML = " & objCInfo.BankHtmlRedirect)

                End If
            End If
        End Sub

        Public Function createSettingsTable(ByVal SettingsParams As String) As Hashtable

            Return ParseGateway(SettingsParams)
        End Function

        Private Function calculateSendHash(ByVal Inputs As System.Collections.Specialized.NameValueCollection, ByVal strParamList As String(), ByVal projectpassword As String) As String
            Dim p As String = ""

            For lp As Integer = 0 To strParamList.GetUpperBound(0)
                If Not Inputs(strParamList(lp)) Is Nothing Then
                    p = p & Inputs(strParamList(lp)) & "|"
                Else
                    p = p & "|"
                End If
            Next

            p = p & projectpassword

            Return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(p, "sha1")
        End Function

        Private Function HashString(ByVal inputString As String, ByVal hashName As String) As String
            Dim algorithm As System.Security.Cryptography.HashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(hashName)
            If algorithm Is Nothing Then
                Throw New ArgumentException("Unrecognized hash name", "hashName")
            End If
            Dim hash As Byte() = algorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inputString))
            Return Convert.ToBase64String(hash)
        End Function

    End Class

End Namespace