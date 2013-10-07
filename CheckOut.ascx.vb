' --- Copyright (c) notice NevoWeb ---
'  Copyright (c) 2008 SARL NevoWeb.  www.nevoweb.com. BSD License.
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


Imports DotNetNuke
Imports DotNetNuke.Common
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Services.Localization
Imports NEvoWeb.Modules.NB_Store.SharedFunctions
Imports DotNetNuke.Security.Membership

Namespace NEvoWeb.Modules.NB_Store

    Partial Public Class CheckOut
        Inherits BaseModule
        Implements Entities.Modules.IPortable

        Private Stage As String = ""
        Private objGateway As New GatewayWrapper

        Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
            Dim TemplateText As String = GetStoreSettingText(PortalId, "gateway.template", GetCurrentCulture)


            Dim blnLockOnCart As Boolean = GetStoreSettingBoolean(PortalId, "lockstockoncart", GetCurrentCulture)

            Dim QtyLimit As Integer = 999999
            Dim tmp As String = GetStoreSetting(PortalId, "productqty.limit", GetCurrentCulture)
            If IsNumeric(tmp) Then
                QtyLimit = CInt(tmp)
            End If
            
            dlGateway2.ItemTemplate = New ProductTemplate(TabId, ModuleId, StoreInstallPath, "50", Server.HtmlDecode(TemplateText), False, "", CType(Settings("txtCssBuyButton"), String), 1, -1, "50", "", TabId, UserId, UserInfo, "", "", blnLockOnCart, QtyLimit)

        End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

            Try

                If Not Page.IsPostBack Then
                    If Not (Request.QueryString("codeid") Is Nothing) Then
                        'link order and build cart for client.
                        Dim codeid As String = Request.QueryString("codeid")
                        Dim objCCtrl As New CartController
                        objCCtrl.BuildFromSeedOrder(PortalId, codeid)
                    End If
                End If


                Dim objCInfo As NB_Store_CartInfo

                If Page.IsPostBack Then

                    If objGateway.GetBankClick(PortalId, Request) Then
                        GatewayRedirect()
                    End If

                End If

                Stage = "1"
                If Not (Request.QueryString("stg") Is Nothing) Then
                    Stage = Request.QueryString("stg")
                End If

                If CType(Settings("chkHideExtraInfo"), Boolean) Then
                    plhNoteMsg.Visible = False
                    txtNoteMsg.Visible = False
                End If

                If CType(Settings("chkSkipCart"), Boolean) Then
                    'jump to address input
                    If Stage = "1" Then Stage = "2"
                    'hide back button of address page
                    cmdBack1.Visible = False
                End If

                If CType(Settings("chkShowStageHeader"), Boolean) Then
                    phHeader.Controls.Add(New LiteralControl(Server.HtmlDecode(GetStoreSettingText(PortalId, "stgheader" & Stage & ".template", GetCurrentCulture))))
                End If

                If CType(Settings("chkHideShipInCart"), Boolean) Then
                    cartlist1.ShippingHidden = True
                    cartlist2.ShippingHidden = True
                End If


                pnlAddressDetails.Visible = False
                pnlCart.Visible = False
                pnlLogin.Visible = False
                pnlPayRtn.Visible = False
                pnlPromptPay.Visible = False
                pnlEmptyCart.Visible = False

                Select Case Stage
                    Case "1" ' cart view (Default)
                        If CurrentCart.IsCartEmpty(PortalId) Then
                            DisplayEmptyCart()
                        Else
                            HideCartOptions()
                            If Not Page.IsPostBack Then
                                objCInfo = CurrentCart.GetCurrentCart(PortalId)
                                txtVATCode.Text = objCInfo.VATNumber
                                txtPromoCode.Text = objCInfo.PromoCode
                                If objCInfo.CountryCode = "" Then
                                    'get special countrycode if set.
                                    objCInfo.CountryCode = GetStoreSetting(PortalId, "shipcountrycode.default", GetCurrentCulture)
                                    If objCInfo.CountryCode = "" Then
                                        'else use merchant country code as default.
                                        objCInfo.CountryCode = GetMerchantCountryCode(PortalId)
                                    End If
                                    CurrentCart.Save(objCInfo)
                                End If
                                populateCountryList(PortalId, ddlCountry, objCInfo.CountryCode)
                            End If
                            SetUpCartList()
                        End If
                    Case "2" ' Address
                        If CurrentCart.IsCartEmpty(PortalId) Then
                            DisplayEmptyCart()
                        ElseIf Not CurrentCart.IsCartAboveMinimum(PortalId) Then
                            DisplayMinimumCart()
                        Else
                            If DisplayLogin() Then
                                DisplayLoginMsg()
                            Else
                                HideCartOptions()
                                If Not Page.IsPostBack Then
                                    objCInfo = CurrentCart.GetCurrentCart(PortalId)
                                    txtVATCode2.Text = objCInfo.VATNumber
                                    populateAddress()
                                End If
                                DisplayExtraDetailMsg()
                            End If
                        End If
                    Case "3" ' Payment Gateway
                        CurrentCart.ValidateCart(PortalId, UserInfo)
                        If CurrentCart.IsCartEmpty(PortalId) Then
                            DisplayEmptyCart()
                        Else
                            If Not Page.IsPostBack Then
                                HideCartOptions()
                                SetUpOrderList()

                                'display gateway2
                                Dim objOInfo As New NB_Store_OrdersInfo
                                Dim aryList As New ArrayList
                                Dim objOCtrl As New OrderController
                                objOInfo = objOCtrl.GetOrder(CurrentCart.GetCurrentCart(PortalId).OrderID)
                                If Not objOInfo Is Nothing Then
                                    If objOInfo.AlreadyPaid <> 0 Then
                                        cmdCancelOrder.Visible = False
                                    End If
                                End If
                                aryList.Add(objOInfo)
                                dlGateway2.DataSource = aryList
                                dlGateway2.DataBind()

                            End If

                            AddChqGateway()
                            AddBankGateway()
                            End If
                    Case "4" ' AUTO run return for bank

                        objGateway.AutoResponse(PortalId, Request)

                        If Not InternalUpdateInterface.Instance() Is Nothing Then
                            InternalUpdateInterface.Instance.AutoResponse(PortalId, Request)
                        End If

                        If Not EventInterface.Instance() Is Nothing Then
                            EventInterface.Instance.AutoResponse(PortalId, Request)
                        End If

                        'support for merchant integrated CC input.
                            If Not Session("BankHtmlRedirect") Is Nothing Then
                                If Session("BankHtmlRedirect") <> "" Then
                                    Response.Redirect(EditUrl("RemotePost"))
                                End If
                            End If

                    Case "5" ' completed return
                            If CurrentCart.IsCartEmpty(PortalId) Then
                                If Request.QueryString("chq") Is Nothing Then
                                    objGateway.AutoResponse(PortalId, Request) ' put auto response here, for payment providers that return notify on same url as return.
                                End If
                                DisplayEmptyCart()
                            Else
                                If Not InternalUpdateInterface.Instance() Is Nothing Then
                                    InternalUpdateInterface.Instance.ReturnToStore(PortalId, CurrentCart.GetCurrentCart(PortalId), GetCurrentCulture, Request)
                                End If

                                If Not (Request.QueryString("chq") Is Nothing) Then
                                    CompletedChqPayment()
                                Else
                                    CompletedBankPayment()
                                End If
                            End If
                End Select
            Catch exc As Exception
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

#Region "Cart View"

#Region "Cart View Events"

        Private Sub ddlCountry_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlCountry.SelectedIndexChanged, rblShipMethod.SelectedIndexChanged, rblShipMethod2.SelectedIndexChanged
            ReDisplayCart()
        End Sub

        Private Sub cmdOrder_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdOrder.Click
            cartlist1.UpdateQty()
            Dim ShipMethodID As Integer
            Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalId)
            objCInfo.VATNumber = txtVATCode.Text
            objCInfo.PromoCode = txtPromoCode.Text
            objCInfo.CountryCode = ddlCountry.SelectedValue
            If rblShipMethod.SelectedIndex >= 0 Then
                ShipMethodID = rblShipMethod.SelectedValue
            Else
                ShipMethodID = GetDefaultShipMethod(PortalId)
            End If
            objCInfo.ShipMethodID = ShipMethodID
            CurrentCart.Save(objCInfo)
            If CType(Settings("chkSmoothLogin"), Boolean) And UserId = -1 Then
                Dim rTabID As Integer = CType(Settings("ddlSmoothLoginTab"), Integer)
                Dim rtnUrl As String = "?ReturnURL=/tabid/" & TabId & "/stg/2/Default.aspx"
                If rTabID = TabId Then
                    Response.Redirect(NavigateURL(rTabID, "", "ctl=login", "stg=2") & rtnUrl)
                Else
                    Response.Redirect(NavigateURL(rTabID, "") & rtnUrl)
                End If
            Else
                Response.Redirect(NavigateURL(TabId, "", "stg=2"))
            End If
        End Sub

        Private Sub cartlist1_CartIsEmpty() Handles cartlist1.CartIsEmpty
            Response.Redirect(NavigateURL(TabId))
        End Sub

        Private Sub cartlist1_RecalculateCart() Handles cartlist1.RecalculateCart, cmdPromo.Click, cmdVAT.Click
            If txtPromoCode.Visible Or txtVATCode.Visible Then
                'save promocode to cart
                Dim objCartInfo As NB_Store_CartInfo
                objCartInfo = CurrentCart.GetCurrentCart(PortalId)
                objCartInfo.PromoCode = txtPromoCode.Text
                objCartInfo.VATNumber = txtVATCode.Text
                CurrentCart.Save(objCartInfo)
                cartlist1.PopulateCartList()
            End If
            populateShipMethod()
        End Sub

        Private Sub cartlist1_ValidateCart() Handles cartlist1.ValidateCart
            CurrentCart.ValidateCart(PortalId, UserInfo)
        End Sub


        Private Sub ReDisplayCart()
            Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalId)
            Dim ShipMethodID As Integer
            cmdOrder.Visible = True
            If Stage = "1" Then
                objCInfo.VATNumber = txtVATCode.Text
                objCInfo.PromoCode = txtPromoCode.Text
                objCInfo.CountryCode = ddlCountry.SelectedValue
                If rblShipMethod.SelectedIndex >= 0 Then
                    ShipMethodID = rblShipMethod.SelectedValue
                Else
                    ShipMethodID = GetDefaultShipMethod(PortalId)
                End If
            End If

            If Stage = "3" Then
                If rblShipMethod2.SelectedIndex >= 0 Then
                    ShipMethodID = rblShipMethod2.SelectedValue
                Else
                    ShipMethodID = GetDefaultShipMethod(PortalId)
                End If
            End If

            objCInfo.ShipMethodID = ShipMethodID
            CurrentCart.Save(objCInfo)

            If Stage = "1" Then
                SetUpCartList()
                cartlist1.PopulateCartList()
            End If

            If Stage = "3" Then
                SetUpOrderList()
                cartlist2.PopulateCartList()
            End If

            populateShipMethod()
        End Sub

        Private Sub cmdContShop_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdContShop.Click
            If IsNumeric(CType(Settings("lstTabContShop"), String)) Then
                Response.Redirect(NavigateURL(CType(Settings("lstTabContShop"), Integer)))
            Else
                Response.Redirect(NavigateURL())
            End If
        End Sub

#End Region

#Region "Cart View Methods"

        Private Sub HideCartOptions()
            If CType(Settings("chkHideVAT"), Boolean) Then
                txtVATCode.Visible = False
                txtVATCode2.Visible = False
                cmdVAT.Visible = False
                plVATCode.Visible = False
                plVATCode2.Visible = False
                divVATCode.Visible = False
            End If

            If CType(Settings("chkHidePromo"), Boolean) Then
                txtPromoCode.Visible = False
                cmdPromo.Visible = False
                plPromoCode.Visible = False
                divPromoCode.Visible = False
            End If

            If CType(Settings("chkHideCountry"), Boolean) Then
                ddlCountry.Visible = False
                plShipCountry1.Visible = False
                divShipCountry.Visible = False
            End If

            If Not CType(Settings("chkShowShipMethod"), Boolean) Then
                rblShipMethod.Visible = False
                divShipMethod.Visible = False
                divShipMethod2.Visible = False
            End If

            Select Case CType(Settings("rblGatewayDisplay"), String)
                Case "1"
                    pnlGateway1.Visible = True
                    pnlGateway2.Visible = False
                Case "2"
                    pnlGateway1.Visible = False
                    pnlGateway2.Visible = True
                Case "3"
                    pnlGateway1.Visible = True
                    pnlGateway2.Visible = True
                Case Else
                    pnlGateway1.Visible = True
                    pnlGateway2.Visible = False
            End Select


        End Sub

        Private Sub SetUpCartList()
            Dim objTaxCalc As New TaxCalcController(PortalId)
            Dim objCInfo As NB_Store_CartInfo
            objCInfo = CurrentCart.GetCurrentCart(PortalId)
            Dim ShipMethodID As Integer = objCInfo.ShipMethodID

            objCInfo.ShipType = ""
            CurrentCart.Save(objCInfo)

            If ShipMethodID <= 0 Then
                ShipMethodID = GetDefaultShipMethod(PortalId)
            End If

            pnlCart.Visible = True
            If Not Page.IsPostBack Then populateShipMethod()
            cartlist1.CartID = objCInfo.CartID
            cartlist1.PortalID = PortalId
            cartlist1.ShipMethodID = ShipMethodID
            cartlist1.TaxOption = objTaxCalc.getTaxOption
            cartlist1.ResourceFile = LocalResourceFile
            cartlist1.ShowDiscountCol = CBool(Settings("chkShowDiscountCol"))
            cartlist1.NoUpdates = False
        End Sub

        Private Sub DisplayEmptyCart()
            Dim objSCtrl As New NB_Store.SettingsController
            Dim objInfo As NB_Store_SettingsTextInfo
            Dim EmptyCartText As String = Localization.GetString("EmptyCart", LocalResourceFile)

            pnlEmptyCart.Visible = True

            objInfo = objSCtrl.GetSettingsText(PortalId, "emptycart.text", GetCurrentCulture)
            If Not objInfo Is Nothing Then
                If objInfo.SettingText <> "" Then
                    EmptyCartText = System.Web.HttpUtility.HtmlDecode(objInfo.SettingText)

                    'get order details and change tokens
                    Dim objTR As New TokenStoreReplace(PortalId, GetCurrentCulture)
                    EmptyCartText = objTR.DoTokenReplace(EmptyCartText)
                End If
            End If
            phEmptyCart.Controls.Add(New LiteralControl(EmptyCartText))
        End Sub

        Private Sub DisplayMinimumCart()
            Dim objSCtrl As New NB_Store.SettingsController
            Dim objInfo As NB_Store_SettingsTextInfo
            Dim objVInfo As NB_Store_SettingsInfo
            Dim EmptyCartText As String = Localization.GetString("EmptyCart", LocalResourceFile)

            pnlEmptyCart.Visible = True

            objInfo = objSCtrl.GetSettingsText(PortalId, "minimumcarttotal.text", GetCurrentCulture)
            If Not objInfo Is Nothing Then
                If objInfo.SettingText <> "" Then
                    EmptyCartText = System.Web.HttpUtility.HtmlDecode(objInfo.SettingText)
                End If
            End If

            Dim MinimumTotal As Double = -1
            objVInfo = objSCtrl.GetSetting(PortalId, "minimumcarttotal.limit", GetCurrentCulture)
            If Not objVInfo Is Nothing Then
                EmptyCartText = Replace(EmptyCartText, "[TAG:CARTMINIMUM]", objVInfo.SettingValue)
            End If
            EmptyCartText = Replace(EmptyCartText, "[TAG:CARTTOTAL]", FormatToStoreCurrency(PortalId, CurrentCart.GetCartItemTotal(PortalId)))

            phEmptyCart.Controls.Add(New LiteralControl(EmptyCartText))
        End Sub

        Private Sub populateShipMethod()
            If rblShipMethod.Visible Or rblShipMethod2.Visible Then
                Dim objCtrl As New ShipController
                Dim objSCtrl As New SettingsController
                Dim objSTInfo As NB_Store_SettingsTextInfo
                Dim objInfo As NB_Store_ShippingMethodInfo
                Dim aryList As ArrayList
                Dim li As ListItem
                Dim strText As String = ""
                Dim ShipCost As Decimal
                Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalId)
                Dim ShipMethodID As Integer

                ShipMethodID = objCInfo.ShipMethodID

                aryList = objCtrl.GetShippingMethodEnabledList(PortalId)

                rblShipMethod.Items.Clear()
                rblShipMethod2.Items.Clear()
                For Each objInfo In aryList
                    Dim cartTot As CartTotals
                    cartTot = CurrentCart.GetCalulatedTotals(PortalId, objCInfo.VATNumber, objCInfo.CountryCode, objCInfo.ShipType, objInfo.ShipMethodID)
                    ShipCost = cartTot.ShipAmt

                    li = New ListItem
                    li.Value = objInfo.ShipMethodID

                    If objInfo.TemplateName = "NONE" Then
                        li.Text = objInfo.MethodDesc
                    Else
                        objSTInfo = objSCtrl.GetSettingsText(PortalId, objInfo.TemplateName, GetCurrentCulture)
                        If objSTInfo Is Nothing Then
                            li.Text = objInfo.MethodDesc
                        Else
                            strText = Replace(System.Web.HttpUtility.HtmlDecode(objSTInfo.SettingText), "[TAG:SHIPPINGCOST]", FormatToStoreCurrency(PortalId, ShipCost))
                            li.Text = strText
                        End If
                    End If

                    If objInfo.ShipMethodID = ShipMethodID Then
                        li.Selected = True
                    End If

                    'check if we need to hide for dealer or non-dealer
                    Dim hideShipMethod As Boolean
                    hideShipMethod = False
                    If IsDealer(PortalId, UserInfo, GetCurrentCulture) Then
                        If objInfo.Dealer = False Then hideShipMethod = True
                    End If
                    If Not IsDealer(PortalId, UserInfo, GetCurrentCulture) Then
                        If objInfo.NonDealer = False Then hideShipMethod = True
                    End If

                    If ShipCost >= 0 And Not (hideShipMethod) Then
                        rblShipMethod.Items.Add(li)
                        rblShipMethod2.Items.Add(li)
                    End If
                Next

                'check shipping method select is still visible, if not reset to first method in list
                If rblShipMethod.Items.FindByValue(ShipMethodID) Is Nothing Then
                    If rblShipMethod.Items.Count > 0 Then
                        ShipMethodID = rblShipMethod.Items(0).Value
                        rblShipMethod.Items(0).Selected = True
                        rblShipMethod2.Items(0).Selected = True
                        ReDisplayCart()
                    Else
                        'unable to calcualte a cost for shipping so display special shipping needed message.
                        DisplaySpecialShippingMsg()
                    End If
                End If
            Else
                'multiple shipping turned off, but check for valid shipping.
                Dim cartTot As CartTotals
                Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalId)
                Dim ShipMethodID As Integer

                ShipMethodID = GetDefaultShipMethod(PortalId)

                cartTot = CurrentCart.GetCalulatedTotals(PortalId, objCInfo.VATNumber, objCInfo.CountryCode, objCInfo.ShipType, ShipMethodID)

                If cartTot.ShipAmt < 0 Then
                    'unable to calcualte a cost for shipping so display special shipping needed message.
                    DisplaySpecialShippingMsg()
                End If

            End If
        End Sub

        Private Sub DisplaySpecialShippingMsg()
            'unable to calcualte a cost for shipping so display special shipping needed message.
            pnlPayRtn.Visible = True
            cmdOrder.Visible = False
            cartlist1.ShippingHidden = True
            cartlist1.PopulateCartList()
            DisplayMsgText(PortalId, plhPayRtn, "specialshipping.text", "Special Shipping Required")
        End Sub

#End Region

#End Region

#Region "Address"

#Region "Address Events"

        Private Sub radShipping_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles radShipping.CheckedChanged
            If radShipping.Checked Then
                saveAddrCookie()
                CurrentCart.SaveShipType(PortalId, "SHIP")
                populateAddress()
                Response.Redirect(NavigateURL(TabId, "", "stg=2"))
            End If
        End Sub

        Private Sub radNone_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles radNone.CheckedChanged
            If radNone.Checked Then
                saveAddrCookie()
                CurrentCart.SaveShipType(PortalId, "NONE")
                populateAddress()
                Response.Redirect(NavigateURL(TabId, "", "stg=2"))
            End If
        End Sub

        Private Sub radBilling_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles radBilling.CheckedChanged
            If radBilling.Checked Then
                saveAddrCookie()
                CurrentCart.SaveShipType(PortalId, "BILL")
                populateAddress()
                Response.Redirect(NavigateURL(TabId, "", "stg=2"))
            End If
        End Sub

        Private Sub cmdNext1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdNext1.Click
            'address has been entered, so Save order
            If Page.IsValid Then
                If chkSaveAddrCookie.Checked Then
                    saveAddrCookie()
                Else
                    removeAddrCookie()
                End If
                SaveOrder()
                Response.Redirect(NavigateURL(TabId, "", "stg=3"))
            End If
        End Sub

        Private Sub cmdBack1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdBack1.Click
            Response.Redirect(NavigateURL(TabId, "", "stg=1"))
        End Sub

        Private Sub cmdBack2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdBack2.Click
            Response.Redirect(NavigateURL(TabId, "", "stg=2"))
        End Sub

        Private Sub cmdCancelOrder_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdCancelOrder.Click
            CurrentCart.CancelOrder(PortalId)
            If IsNumeric(CType(Settings("lstTabs"), String)) Then
                Response.Redirect(NavigateURL(CType(Settings("lstTabs"), Integer)))
            Else
                Response.Redirect(NavigateURL())
            End If
        End Sub

#End Region

#Region "Address Methods"

        Private Function AllowNonUserOrder() As Boolean
            Return CType(Settings("chkNonUserOrder"), Boolean)
        End Function

        Private Function IsNonUserOrder() As Boolean
            If UserId >= 0 Then
                Return False
            Else
                Return True
            End If
        End Function



        Private Sub populateAddress()
            Dim objCtrl As New OrderController
            Dim objInfo As NB_Store_OrdersInfo
            Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalId)
            Dim objAInfo As NB_Store_AddressInfo

            pnlAddressDetails.Visible = True

            valEmail.ErrorMessage = Localization.GetString("valEmail", LocalResourceFile)
            revEmail.ErrorMessage = Localization.GetString("revEmail", LocalResourceFile)

            'disable address validate
            If CType(Settings("chkMinimumValidate"), Boolean) Then
                billaddress.NoValidate = True
                shipaddress.NoValidate = True
            Else
                billaddress.NoValidate = False
                shipaddress.NoValidate = False
            End If

            'hide no shipping radio
            If CType(Settings("chkHideShip"), Boolean) Then
                radNone.Visible = False
                lblNone.Visible = False
            End If

            'hide set default email and address options if generic user
            If IsNonUserOrder() Then
                chkDefaultAddress.Visible = False
                chkDefaultEmail.Visible = False
                plDefaultAddress.Visible = False
                plDefaultEmail.Visible = False
            End If

            'set display of radio shipping type (from currentcart)
            pnlShipAddress.Visible = False
            Select Case objCInfo.ShipType
                Case "NONE"
                    radNone.Checked = True
                Case "BILL"
                    radBilling.Checked = True
                Case "SHIP"
                    radShipping.Checked = True
                    pnlShipAddress.Visible = True
                Case Else
                    radBilling.Checked = True
                    objCInfo.ShipType = "BILL"
                    CurrentCart.Save(objCInfo)
            End Select

            objInfo = objCtrl.GetOrder(CurrentCart.GetCurrentCart(PortalId).OrderID)

            'initialize  the address controls so that the templates get displayed.
            billaddress.AddressDataInfo = New NB_Store_AddressInfo
            shipaddress.AddressDataInfo = New NB_Store_AddressInfo


            If objInfo Is Nothing Then

                If getStoreCookieValue(PortalId, CurrentCart.getCartID(PortalId), "BAddressName") <> "" Then
                    'get address cookie
                    Dim AddrCookie As HttpCookie = getStoreCookie(PortalId, CurrentCart.getCartID(PortalId))
                    txtEmail.Text = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "Email").ToString)

                    billaddress.CountryCode = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BCountryCode").ToString)
                    shipaddress.CountryCode = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SCountryCode").ToString)


                    objAInfo = New NB_Store_AddressInfo

                    objAInfo.AddressDescription = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BAddressDescription").ToString)
                    objAInfo.AddressName = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BAddressName").ToString)
                    objAInfo.AddressName2 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BAddressName2").ToString)
                    objAInfo.Address1 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BAddress1").ToString)
                    objAInfo.Address2 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BAddress2").ToString)
                    objAInfo.City = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BCity").ToString)
                    objAInfo.CountryCode = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BCountryCode").ToString)
                    objAInfo.RegionCode = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BRegion").ToString)
                    objAInfo.Phone1 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BPhone1").ToString)
                    objAInfo.Phone2 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BPhone2").ToString)
                    objAInfo.PostalCode = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BPostalCode").ToString)

                    objAInfo.CompanyName = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BCompanyName").ToString)
                    objAInfo.Extra1 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BExtra1").ToString)
                    objAInfo.Extra2 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BExtra2").ToString)
                    objAInfo.Extra3 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BExtra3").ToString)
                    objAInfo.Extra4 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "BExtra4").ToString)

                    billaddress.AddressDataInfo = objAInfo

                    objAInfo = New NB_Store_AddressInfo

                    objAInfo.AddressDescription = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SAddressDescription").ToString)
                    objAInfo.AddressName = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SAddressName").ToString)
                    objAInfo.AddressName2 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SAddressName2").ToString)
                    objAInfo.Address1 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SAddress1").ToString)
                    objAInfo.Address2 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SAddress2").ToString)
                    objAInfo.City = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SCity").ToString)
                    objAInfo.CountryCode = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SCountryCode").ToString)
                    objAInfo.RegionCode = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SRegion").ToString)
                    objAInfo.Phone1 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SPhone1").ToString)
                    objAInfo.Phone2 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SPhone2").ToString)
                    objAInfo.PostalCode = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SPostalCode").ToString)

                    objAInfo.CompanyName = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SCompanyName").ToString)
                    objAInfo.Extra1 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SExtra1").ToString)
                    objAInfo.Extra2 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SExtra2").ToString)
                    objAInfo.Extra3 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SExtra3").ToString)
                    objAInfo.Extra4 = Server.UrlDecode(getStoreCookieValue(PortalId, AddrCookie, "SExtra4").ToString)


                    shipaddress.AddressDataInfo = objAInfo

                Else
                    shipaddress.CountryCode = objCInfo.CountryCode

                    objAInfo = New NB_Store_AddressInfo

                    If IsNonUserOrder() Then
                        objAInfo.AddressName = ""
                        txtEmail.Text = ""
                    Else
                        objAInfo.AddressName = UserInfo.FirstName
                        objAInfo.AddressName2 = UserInfo.LastName
                        txtEmail.Text = UserInfo.Email
                    End If

                    objAInfo.Address1 = UserInfo.Profile.Unit
                    objAInfo.Address2 = UserInfo.Profile.Street
                    objAInfo.City = UserInfo.Profile.City
                    Dim c As String = GetCountryCodeByName(UserInfo.Profile.Country)
                    If c = "" Then
                        objAInfo.CountryCode = objCInfo.CountryCode
                    Else
                        objAInfo.CountryCode = c
                    End If
                    objAInfo.RegionCode = UserInfo.Profile.Region
                    objAInfo.Phone1 = UserInfo.Profile.Telephone
                    objAInfo.Phone2 = UserInfo.Profile.Cell
                    objAInfo.PostalCode = UserInfo.Profile.PostalCode
                    If Not IsNothing(UserInfo.Profile.GetPropertyValue("CompanyName")) Then
                        objAInfo.CompanyName = UserInfo.Profile.GetPropertyValue("CompanyName")
                    End If

                    billaddress.AddressDataInfo = objAInfo


                End If
            Else

                If objInfo.Email <> "" Then
                    txtEmail.Text = objInfo.Email
                Else
                    If IsNonUserOrder() Then
                        txtEmail.Text = ""
                    Else
                        txtEmail.Text = UserInfo.Email
                    End If
                End If

                objAInfo = objCtrl.GetOrderAddress(objInfo.BillingAddressID)
                If Not objAInfo Is Nothing Then
                    billaddress.AddressDataInfo = objAInfo
                End If

                'get shipping address, if different from billing address
                If objInfo.ShippingAddressID = -1 Then
                    radNone.Checked = True
                Else
                    If objInfo.BillingAddressID <> objInfo.ShippingAddressID Then
                        objAInfo = objCtrl.GetOrderAddress(objInfo.ShippingAddressID)
                        If Not objAInfo Is Nothing Then
                            shipaddress.AddressDataInfo = objAInfo
                        End If
                    End If
                End If

            End If

        End Sub


        Private Sub DisplayExtraDetailMsg()
            DisplayMsgText(PortalId, plhNoteMsg, "extradetail.text", Localization.GetString("ExtraDetail", LocalResourceFile))
        End Sub

        Private Sub DisplayLoginMsg()
            pnlLogin.Visible = True
            DisplayMsgText(PortalId, phLogin, "logintext.text", Localization.GetString("LoginText", LocalResourceFile))
        End Sub


        Private Function DisplayLogin() As Boolean

            If UserId >= 0 Or IsNumeric(Request.QueryString("logmsg")) Then
                Return False
            End If

            If AllowNonUserOrder() Then
                If CType(Settings("chkDisableLoginMsg"), Boolean) Then
                    Return False
                End If
            End If
            Return True
        End Function

        Private Sub SaveOrder()
            Dim objOCtrl As New OrderController
            Dim objInfo As NB_Store_OrdersInfo
            Dim CountryCode As String = "XX"
            Dim BillID As Integer
            Dim ShipID As Integer
            Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalId)
            Dim VATNumberAddr As String = ""

            'update currentcart with selected shipping country & Shipping type
            If radNone.Checked Then
                objCInfo.ShipType = "NONE"
                objCInfo.CountryCode = billaddress.AddressDataInfo.CountryCode
            End If
            If radBilling.Checked Then
                objCInfo.ShipType = "BILL"
                objCInfo.CountryCode = billaddress.AddressDataInfo.CountryCode
            End If
            If radShipping.Checked Then
                objCInfo.ShipType = "SHIP"
                objCInfo.CountryCode = shipaddress.AddressDataInfo.CountryCode
            End If

            objCInfo.VATNumber = txtVATCode2.Text

            CurrentCart.Save(objCInfo)

            'Does email enterd need to be save onto order or updated to account? 
            Dim OrderEmail As String = UpdateOrderEmail(txtEmail.Text, billaddress)

            'create the order from the currentcart info
            objInfo = CurrentCart.CreateOrder(UserId, PortalId, txtNoteMsg.Text, OrderEmail, UserInfo)

            'and form xml data
            objInfo.Stg2FormXML = Stg2Form.XMLData

            'update address info
            BillID = SaveAddress(billaddress, objInfo.OrderID)
            If radNone.Checked Then ShipID = -1
            If radBilling.Checked Then ShipID = BillID
            If radShipping.Checked Then ShipID = SaveAddress(shipaddress, objInfo.OrderID)

            'update new order with address id's
            objInfo.BillingAddressID = BillID
            objInfo.ShippingAddressID = ShipID
            objOCtrl.UpdateObjOrder(objInfo)



        End Sub

        Private Function UpdateOrderEmail(ByVal OrderEmail As String, ByVal AddCtrl As NB_Store.Address) As String
            Dim rtnEmail As String = ""

            If IsNonUserOrder() Then
                Return OrderEmail
            Else
                Dim blnUpdate As Boolean = False
                If UserInfo.Email <> OrderEmail Then
                    rtnEmail = OrderEmail
                    If chkDefaultEmail.Checked Then
                        UserInfo.Email = OrderEmail
                        UserInfo.Membership.Email = OrderEmail
                        rtnEmail = ""
                        blnUpdate = True
                    End If
                End If

                If chkDefaultAddress.Checked Then
                    Dim objAInfo As New NB_Store_AddressInfo

                    objAInfo = AddCtrl.AddressDataInfo

                    UserInfo.Profile.Unit = objAInfo.Address1
                    UserInfo.Profile.Street = objAInfo.Address2
                    UserInfo.Profile.Region = objAInfo.RegionCode
                    UserInfo.Profile.PostalCode = objAInfo.PostalCode
                    UserInfo.Profile.City = objAInfo.City
                    'remove this, the DNN country uses country name, and it's not always compatible.
                    'UserInfo.Profile.Country = GetCountryByCode(objAInfo.CountryCode)
                    UserInfo.Profile.Telephone = objAInfo.Phone1
                    UserInfo.Profile.Cell = objAInfo.Phone2
                    If Not IsNothing(UserInfo.Profile.GetPropertyValue("CompanyName")) Then
                        UserInfo.Profile.SetProfileProperty("CompanyName", objAInfo.CompanyName)
                    End If

                    blnUpdate = True
                End If

                If blnUpdate Then
                    Users.UserController.UpdateUser(PortalId, UserInfo)
                End If
            End If
            Return rtnEmail
        End Function


        Private Function SaveAddress(ByVal AddCtrl As NB_Store.Address, ByVal OrderID As Integer) As Integer
            'create new address for all 
            Dim objOCtrl As New OrderController
            Dim objAInfo As New NB_Store_AddressInfo

            objAInfo = AddCtrl.AddressDataInfo

            objAInfo.PortalID = PortalId
            objAInfo.CreatedByUser = UserId
            objAInfo.CreatedDate = Now
            objAInfo.OrderID = OrderID
            objAInfo.PrimaryAddress = False
            objAInfo.AddressID = -1
            objAInfo.UserID = UserId

            objAInfo = objOCtrl.UpdateObjOrderAddress(objAInfo)

            Return objAInfo.AddressID
        End Function

        Private Sub saveAddrCookie()
            Dim FullCookieName As String = STOREPREFIX & CurrentCart.getCartID(PortalId) & "_Portal" & PortalId.ToString
            Dim AddrCookie As New HttpCookie(FullCookieName)

            Dim objAInfo As NB_Store_AddressInfo

            objAInfo = billaddress.AddressDataInfo()

            AddrCookie("Email") = StoreEncrypt(PortalId, Server.UrlEncode(txtEmail.Text))
            AddrCookie("BAddressDescription") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.AddressDescription))
            AddrCookie("BAddressName") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.AddressName))
            AddrCookie("BAddressName2") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.AddressName2))
            AddrCookie("BAddress1") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Address1))
            AddrCookie("BAddress2") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Address2))
            AddrCookie("BCity") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.City))
            AddrCookie("BCountryCode") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.CountryCode))
            AddrCookie("BRegion") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.RegionCode))
            AddrCookie("BPhone1") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Phone1))
            AddrCookie("BPhone2") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Phone2))
            AddrCookie("BPostalCode") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.PostalCode))

            AddrCookie("BCompanyName") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.CompanyName))
            AddrCookie("BExtra1") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Extra1))
            AddrCookie("BExtra2") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Extra2))
            AddrCookie("BExtra3") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Extra3))
            AddrCookie("BExtra4") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Extra4))

            objAInfo = shipaddress.AddressDataInfo()

            AddrCookie("SAddressDescription") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.AddressDescription))
            AddrCookie("SAddressName") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.AddressName))
            AddrCookie("SAddressName2") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.AddressName2))
            AddrCookie("SAddress1") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Address1))
            AddrCookie("SAddress2") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Address2))
            AddrCookie("SCity") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.City))
            AddrCookie("SCountryCode") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.CountryCode))
            AddrCookie("SRegion") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.RegionCode))
            AddrCookie("SPhone1") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Phone1))
            AddrCookie("SPhone2") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Phone2))
            AddrCookie("SPostalCode") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.PostalCode))

            AddrCookie("SCompanyName") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.CompanyName))
            AddrCookie("SExtra1") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Extra1))
            AddrCookie("SExtra2") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Extra2))
            AddrCookie("SExtra3") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Extra3))
            AddrCookie("SExtra4") = StoreEncrypt(PortalId, Server.UrlEncode(objAInfo.Extra4))

            AddrCookie.Expires = DateAdd(DateInterval.Day, 30, Today)

            HttpContext.Current.Response.Cookies.Add(AddrCookie)

        End Sub

        Private Sub removeAddrCookie()
            Dim FullCookieName As String = STOREPREFIX & CurrentCart.getCartID(PortalId) & "_Portal" & PortalId.ToString
            Dim AddrCookie As New HttpCookie(FullCookieName)
            AddrCookie("Email") = ""
            AddrCookie("BAddressDescription") = ""
            AddrCookie("BAddressName") = ""
            AddrCookie("BAddressName2") = ""
            AddrCookie("BAddress1") = ""
            AddrCookie("BAddress2") = ""
            AddrCookie("BCity") = ""
            AddrCookie("BCountryCode") = ""
            AddrCookie("BRegion") = ""
            AddrCookie("BPhone1") = ""
            AddrCookie("BPhone2") = ""
            AddrCookie("BPostalCode") = ""
            AddrCookie("BCompanyName") = ""
            AddrCookie("BExtra1") = ""
            AddrCookie("BExtra2") = ""
            AddrCookie("BExtra3") = ""
            AddrCookie("BExtra4") = ""
            AddrCookie("SAddressDescription") = ""
            AddrCookie("SAddressName") = ""
            AddrCookie("SAddressName2") = ""
            AddrCookie("SAddress1") = ""
            AddrCookie("SAddress2") = ""
            AddrCookie("SCity") = ""
            AddrCookie("SCountryCode") = ""
            AddrCookie("SRegion") = ""
            AddrCookie("SPhone1") = ""
            AddrCookie("SPhone2") = ""
            AddrCookie("SPostalCode") = ""
            AddrCookie("SCompanyName") = ""
            AddrCookie("SExtra1") = ""
            AddrCookie("SExtra2") = ""
            AddrCookie("SExtra3") = ""
            AddrCookie("SExtra4") = ""
            AddrCookie.Expires = DateAdd(DateInterval.Day, 30, Today)
            HttpContext.Current.Response.Cookies.Add(AddrCookie)
        End Sub


#End Region

#End Region

#Region "Payment Gateway"


#Region "Payment Gateway Events"

        Private Sub imgBChq_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles imgBChq.Click
            PayedByChq()
            Response.Redirect(NavigateURL(TabId, "", "stg=5", "chq=1"))
        End Sub

        Private Sub dlGateway2_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataListCommandEventArgs) Handles dlGateway2.ItemCommand
            Select Case e.CommandSource.CommandName
                Case "Purchase"
                    Dim item As DataListItem = e.Item
                    ContinueToPurchase(item)
            End Select

        End Sub


#End Region

#Region "Payment Gateway Methods"

        Private Sub ContinueToPurchase(ByVal LItem As DataListItem)
            Dim Ctrl As Control
            Dim SelectedGateway As String = ""

            Ctrl = LItem.FindControl("rblGateway")
            If Not Ctrl Is Nothing Then
                SelectedGateway = DirectCast(Ctrl, RadioButtonList).SelectedValue
            End If

            If SelectedGateway <> "" Then

                Dim objGInfo As NB_Store_GatewayInfo
                Dim hTable As Hashtable
                Dim OrderID As Integer = CurrentCart.GetCurrentCart(PortalId).OrderID

                hTable = GetAvailableGatewaysTable(PortalId)

                objGInfo = hTable.Item(SelectedGateway)

                If objGInfo.gatewaytype = "CHQ" Then
                    PayedByChq()
                    Response.Redirect(NavigateURL(TabId, "", "stg=5", "chq=1"))
                Else
                    Dim objCtrl As New OrderController
                    Dim objOInfo As NB_Store_OrdersInfo

                    objOInfo = objCtrl.GetOrder(OrderID)
                    If Not objOInfo Is Nothing Then
                        objOInfo.BankAuthCode = objGInfo.assembly

                        ' Clear cart redirect, so we make sure we've got correct one...or none. 
                        Dim objCInfo As NB_Store_CartInfo = CurrentCart.GetCurrentCart(PortalId)
                        objCInfo.BankHtmlRedirect = ""
                        CurrentCart.Save(objCInfo)

                        objCtrl.UpdateObjOrder(objOInfo)
                        GatewayRedirect()
                    End If

                End If
            Else
                'no gateway selected, run encapsulated.
                PayedByChq()
                Response.Redirect(NavigateURL(TabId, "", "stg=5", "chq=1"))
            End If


        End Sub

        Private Sub GatewayRedirect()
            'save any form data
            SaveForm3XML(CurrentCart.GetCurrentCart(PortalId).OrderID)

            objGateway.SetBankRemotePost(PortalId, CurrentCart.GetCurrentCart(PortalId).OrderID, GetCurrentCulture, Request)
            If Not InternalUpdateInterface.Instance() Is Nothing Then
                InternalUpdateInterface.Instance.BankRemotePost(PortalId, CurrentCart.GetCurrentCart(PortalId), GetCurrentCulture, Request)
            End If
            Response.Redirect(EditUrl("RemotePost"))

        End Sub

        Private Sub SetUpOrderList()
            Dim objTaxCalc As New TaxCalcController(PortalId)
            pnlPromptPay.Visible = True
            populateShipMethod()
            cartlist2.CartID = CurrentCart.GetCurrentCart(PortalId).CartID
            cartlist2.PortalID = PortalId
            cartlist2.TaxOption = objTaxCalc.getTaxOption
            cartlist2.ResourceFile = LocalResourceFile
            cartlist2.ShowDiscountCol = CBool(Settings("chkShowDiscountCol"))
            cartlist2.NoUpdates = True
            cartlist2.ShipMethodID = CurrentCart.GetCurrentCart(PortalId).ShipMethodID
            DisplayMsgText(PortalId, plhGatewayMsg, "gatewaymsg.text", "")
        End Sub

        Private Sub AddChqGateway()

            If CType(Settings("chkHideChq"), Boolean) Then
                imgBChq.Visible = False
                lblCheque.Visible = False
            Else
                imgBChq.Visible = True
                Dim imgPath As String = GetStoreSetting(PortalId, "encapsulated.image", GetCurrentCulture)
                Dim strText As String = GetStoreSetting(PortalId, "encapsulated.text", GetCurrentCulture)

                If imgPath = "" Then
                    imgBChq.ImageUrl = StoreInstallPath & "img/BANKCHEQUE.gif"
                Else
                    imgBChq.ImageUrl = PortalSettings.HomeDirectory & imgPath
                End If

                If strText = "" Then
                    lblCheque.Text = Localization.GetString("lblCheque.Text", LocalResourceFile)
                Else

                    lblCheque.Text = strText
                End If

            End If
        End Sub

        Private Sub PayedByChq()
            'save any form data
            SaveForm3XML(CurrentCart.GetCurrentCart(PortalId).OrderID)
            ChequeInterface.Instance().CompleteOrder(PortalId, CurrentCart.GetCurrentCart(PortalId).OrderID, GetCurrentCulture, CType(Settings("chkStockChq"), Boolean))

        End Sub

        Private Sub AddBankGateway()
            Dim ordID As Integer = CurrentCart.GetCurrentCart(PortalId).OrderID
            Dim strHTML As String = CheckForElaspedOrderTime(ordID)

            If strHTML = "" Then
                strHTML = objGateway.GetButtonHtml(PortalId, ordID, GetCurrentCulture)
            End If

            plhGateway.Controls.Add(New LiteralControl(strHTML))

        End Sub

        Private Function CheckForElaspedOrderTime(ByVal OrderID As Integer) As String
            'check for eleasped date
            Dim objCtrl As New OrderController
            Dim objOInfo As NB_Store_OrdersInfo
            Dim strHTML As String = ""

            objOInfo = objCtrl.GetOrder(OrderID)
            If Not objOInfo Is Nothing Then
                If Not objOInfo.ElapsedDate = Null.NullDate Then
                    If objOInfo.ElapsedDate < Now Then
                        strHTML = GetStoreSettingText(PortalId, "elapsedorder.text", GetCurrentCulture, False, True)
                    End If
                End If
            End If
            Return strHTML
        End Function
#End Region

#End Region

#Region "Completed Methods"

        Private Sub CompletedChqPayment()
            Dim ordID As Integer = CurrentCart.GetCurrentCart(PortalId).OrderID

            If Not EventInterface.Instance() Is Nothing Then
                EventInterface.Instance.CompletedOrder(PortalId, 1, ordID)
            End If

            'delete cart
            Dim objCCtrl As New CartController
            objCCtrl.DeleteCart(CurrentCart.GetCurrentCart(PortalId).CartID)

            DisplayCompletedMsg(ordID, "chqpayment.text")
        End Sub

        Private Sub CompletedBankPayment()
            Dim ordID As Integer = CurrentCart.GetCurrentCart(PortalId).OrderID
            Dim MsgText As String = ""

            MsgText = objGateway.GetCompletedHtml(PortalId, UserId, Request, ordID)

            If Not EventInterface.Instance() Is Nothing Then
                EventInterface.Instance.CompletedOrder(PortalId, 2, ordID)
            End If

            'delete cart, if not been told to keep it.
            If CurrentCart.GetCurrentCart(PortalId).BankHtmlRedirect <> "KEEPCART" Then
                Dim objCCtrl As New CartController
                objCCtrl.DeleteCart(CurrentCart.GetCurrentCart(PortalId).CartID)
            End If

            pnlPayRtn.Visible = True
            plhPayRtn.Controls.Add(New LiteralControl(Server.HtmlDecode(MsgText)))

        End Sub

        Private Sub DisplayCompletedMsg(ByVal OrderID As Integer, ByVal TemplateID As String)
            Dim objSCtrl As New NB_Store.SettingsController
            Dim objOCtrl As New OrderController
            Dim objInfo As NB_Store_SettingsTextInfo
            Dim objOInfo As NB_Store_OrdersInfo
            Dim MsgText As String = ""

            pnlPayRtn.Visible = True

            objInfo = objSCtrl.GetSettingsText(PortalId, TemplateID, GetCurrentCulture)
            If Not objInfo Is Nothing Then
                If objInfo.SettingText <> "" Then
                    MsgText = objInfo.SettingText
                    'get order details and change tokens
                    objOInfo = objOCtrl.GetOrder(OrderID)
                    Dim objTR As New TokenStoreReplace(objOInfo, GetCurrentCulture)
                    MsgText = objTR.DoTokenReplace(MsgText)
                End If
            End If

            plhPayRtn.Controls.Add(New LiteralControl(Server.HtmlDecode(MsgText)))
        End Sub

#End Region

#Region "General"

        Private Sub SaveForm3XML(ByVal OrderID As Integer)
            'save any form datav
            Dim objOCtrl As New OrderController
            Dim objOInfo As NB_Store_OrdersInfo
            objOInfo = objOCtrl.GetOrder(CurrentCart.GetCurrentCart(PortalId).OrderID)
            If Not objOInfo Is Nothing Then
                objOInfo.Stg3FormXML = Stg3Form.XMLData
                objOCtrl.UpdateObjOrder(objOInfo)
            End If
        End Sub

#End Region

#Region "Optional Interfaces"

        Public Function ExportModule(ByVal ModuleID As Integer) As String Implements Entities.Modules.IPortable.ExportModule
            ' included as a stub only so that the core knows this module Implements Entities.Modules.IPortable
        End Function

        Public Sub ImportModule(ByVal ModuleID As Integer, ByVal Content As String, ByVal Version As String, ByVal UserID As Integer) Implements Entities.Modules.IPortable.ImportModule
            ' included as a stub only so that the core knows this module Implements Entities.Modules.IPortable
        End Sub

#End Region


    End Class

End Namespace
