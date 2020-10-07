<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="CheckOut.ascx.vb" Inherits="NEvoWeb.Modules.NB_Store.CheckOut" %>
<%@ Register TagPrefix="nbs" TagName="CartList" Src="CartList.ascx" %>
<%@ Register TagPrefix="nbs" TagName="Address" Src="Address.ascx" %>
<%@ Register TagPrefix="nbs" TagName="CustomForm" Src="controls/CustomForm.ascx" %>
<div class="Checkout">
    <asp:PlaceHolder ID="phHeader" runat="server"></asp:PlaceHolder>
    <asp:Panel ID="pnlEmptyCart" runat="server">
        <asp:PlaceHolder ID="phEmptyCart" runat="server"></asp:PlaceHolder>
    </asp:Panel>
    <asp:Panel ID="pnlCart" runat="server">
        <nbs:CartList runat="server" id="cartlist1" />
        <div id="divTermsAndConditions" runat="server" class="NBright_CartOptDiv">
            <asp:CheckBox CssClass="normalCheckbox TermsAndConditions" RepeatLayout="Flow" ID="chkTermsAndConditions" runat="server" AutoPostBack="true"></asp:CheckBox>
            <asp:Label CssClass="Label" ID="plTermsAndConditions" runat="server" controlname="plTermsAndConditions" suffix=""></asp:Label>
        </div>
        <div id="divPromoCode" runat="server" class="NBright_CartOptDiv">
            <asp:Label CssClass="Label" ID="plPromoCode" resourcekey="plPromoCode" runat="server" controlname="plPromoCode" suffix=""></asp:Label>
            <asp:TextBox CssClass="NormalTextBox" Width="130" ID="txtPromoCode" runat="server"></asp:TextBox>&nbsp;&nbsp;<asp:LinkButton
                ID="cmdPromo" runat="server" resourcekey="cmdPromo">Apply</asp:LinkButton>
        </div>
        <div id="divVATCode" runat="server" class="NBright_CartOptDiv">
            <asp:Label CssClass="Label" ID="plVATCode" resourcekey="plVATCode" runat="server" controlname="plVATCode" suffix=""></asp:Label>
            <asp:TextBox CssClass="NormalTextBox" Width="130" ID="txtVATCode" runat="server"></asp:TextBox>&nbsp;&nbsp;<asp:LinkButton ID="cmdVAT"
                runat="server" resourcekey="cmdVAT">Apply</asp:LinkButton>
        </div>
        <div id="divShipCountry" runat="server" class="NBright_CartOptDiv">
            <asp:Label CssClass="Label" ID="plShipCountry1" resourcekey="plShipCountry1" runat="server" controlname="plShipCountry" suffix=""></asp:Label>
            <asp:DropDownList CssClass="NormalTextBox" Width="136" ID="ddlCountry" runat="server" AutoPostBack="true"></asp:DropDownList>
        </div>
        <div id="divShipMethod" runat="server" class="NBright_CartOptDiv">
            <asp:Label CssClass="Label" ID="plShippingMethods" resourcekey="plShippingMethods" runat="server" controlname="plShippingMethods" suffix=""></asp:Label>
            <asp:RadioButtonList CssClass="normalRadioButton ShippingMethods" RepeatLayout="Flow" ID="rblShipMethod" runat="server" AutoPostBack="true">
            </asp:RadioButtonList>
        </div>
        <div class="NBright_ClientButtonDiv">
            <table cellpadding="0" cellspacing="0" border="0" width="100%">
                <tr>
                    <td class="NBright_ClientButtonDivLeft">
                        <asp:LinkButton ID="cmdContShop" runat="server" CssClass="NBright_ClientButton Button ContinueShopping" resourcekey="cmdContShop">Continue Shopping</asp:LinkButton>
                    </td>
                    <td class="NBright_ClientButtonDivRight">
                        <asp:LinkButton ID="cmdOrder" runat="server" CssClass="NBright_ClientButton Button ContinueOrder" resourcekey="cmdOrder">Order</asp:LinkButton>
                    </td>
                </tr>
            </table>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlLogin" runat="server">
        <asp:PlaceHolder ID="phLogin" runat="server"></asp:PlaceHolder>
    </asp:Panel>
    <asp:Panel ID="pnlAddressDetails" runat="server">
        <asp:Panel ID="pnlAddress" runat="server">
            <fieldset class="EmailFieldset">
                <table class="AddressCollector" cellspacing="0" cellpadding="0" border="0">
                    <tr>
                        <td class="Label" style="white-space: nowrap">
                            <asp:Label ID="plEmail" resourcekey="plEmail" runat="server" controlname="txtEmail" Text="Email"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="NormalTextBox"
                                MaxLength="50"></asp:TextBox><span>&nbsp;*</span>
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" CssClass="NormalRed" Text="!"
                                ControlToValidate="txtEmail" ErrorMessage="Invalid Email Address" ValidationExpression='[^"\r\n]*\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*'
                                Display="Dynamic"></asp:RegularExpressionValidator>
                            <asp:RequiredFieldValidator ID="valEmail" runat="server" CssClass="NormalRed" ControlToValidate="txtEmail" Text="*"
                                ErrorMessage="Email Is Required." Display="Dynamic" resourcekey="valEmail"></asp:RequiredFieldValidator>
                        </td>
                    </tr>
                </table>
                <asp:Label ID="plDefaultEmail" resourcekey="plDefaultEmail" runat="server" controlname="plDefaultEmail" suffix="" Text=""></asp:Label>
                <asp:CheckBox CssClass="normalCheckBox" ID="chkDefaultEmail" resourcekey="chkDefaultEmail" runat="server" />
            </fieldset>
            <fieldset class="BillingAddressFieldset">
                <legend>
                    <asp:Label ID="lblBillingAddressTitle" resourcekey="lblBillingAddressTitle" runat="server" controlname="lblBillingAddressTitle"></asp:Label>
                </legend>
                <nbs:address id="billaddress" runat="server"></nbs:address>
                <div id="divVATCode2" runat="server">
                    <table border="0" cellpadding="0" cellspacing="0" class="AddressCollector">
                        <tr>
                            <td class="Label">
                                <asp:Label ID="plVATCode2" runat="server" controlname="plVATCode2" resourcekey="plVATCode" suffix=""></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtVATCode2" runat="server" CssClass="NormalTextBox"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                </div>
                <asp:Label ID="plDefaultAddress" resourcekey="plDefaultAddress" runat="server" controlname="plDefaultAddress" suffix="" Text=""></asp:Label>
                <asp:CheckBox CssClass="normalCheckBox" ID="chkDefaultAddress" resourcekey="chkDefaultAddress" runat="server" />
            </fieldset>
            <fieldset class="AddressSelectorFieldset">
                <table class="AddressCollector" cellspacing="0" cellpadding="0" border="0">
                    <tr>
                        <td>
                            <asp:RadioButton CssClass="normalRadioButton" ID="radNone" runat="server" GroupName="radShipAddress" AutoPostBack="True"></asp:RadioButton>
							<asp:Label ID="lblNone" resourcekey="lblNone" runat="server" controlname="radNone"></asp:Label>
                            <asp:RadioButton CssClass="normalRadioButton" ID="radBilling" runat="server" GroupName="radShipAddress" AutoPostBack="True"></asp:RadioButton>
							<asp:Label ID="lblUseBillingAddress" resourcekey="lblUseBillingAddress" runat="server" controlname="radBilling"></asp:Label>
                            <asp:RadioButton CssClass="normalRadioButton" ID="radShipping" runat="server" GroupName="radShipAddress" AutoPostBack="True"></asp:RadioButton>
							<asp:Label ID="lblUseShippingAddress" resourcekey="lblUseShippingAddress" runat="server" controlname="radShipping"></asp:Label>
                        </td>
                    </tr>
                </table>
            </fieldset>
        </asp:Panel>
        <asp:Panel ID="pnlShipAddress" runat="server">
            <fieldset class="ShippingAddressFieldset">
                <legend>
                    <asp:Label ID="lblShippingAddressTitle" resourcekey="lblShippingAddressTitle" runat="server" controlname="lblShippingAddressTitle"></asp:Label>
                </legend>
                <nbs:address id="shipaddress" runat="server"></nbs:address>
            </fieldset>
        </asp:Panel>
        <fieldset class="AddressOptionsFieldset">
            <table border="0" cellpadding="0" cellspacing="0" class="AddressOptions">
                <tr>
                    <td>
                        <nbs:CustomForm ID="Stg2Form" runat="server" DisplayTemplateName="stg2form.template"></nbs:CustomForm>
                        <asp:PlaceHolder ID="plhNoteMsg" runat="server"></asp:PlaceHolder>
                        <asp:TextBox ID="txtNoteMsg" runat="server" CssClass="NormalTextBox SpecialInstructions" MaxLength="500" TextMode="MultiLine"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:CheckBox CssClass="normalCheckBox" ID="chkSaveAddrCookie" runat="server" Checked="true" resourcekey="chkSaveAddrCookie" />
                        <asp:Label ID="plSaveAddrCookie" runat="server" controlname="plSaveAddrCookie" resourcekey="plSaveAddrCookie" suffix="" Text=""></asp:Label>
                    </td>
                </tr>
            </table>
        </fieldset>
        <asp:ValidationSummary CssClass="ValidationSummary" ID="ValidationSummary1" runat="server" />
        <div class="NBright_ClientButtonDiv">
            <table border="0" cellpadding="0" cellspacing="0" width="100%">
                <tr>
                    <td class="NBright_ClientButtonDivLeft">
                        <asp:LinkButton ID="cmdBack1" runat="server" CausesValidation="False"
                            CssClass="NBright_ClientButton Button ReturnPrevious" resourcekey="cmdBack">B</asp:LinkButton>
                    </td>
                    <td class="NBright_ClientButtonDivRight">
                        <asp:LinkButton ID="cmdNext1" runat="server"
                            CssClass="NBright_ClientButton Button ContinueOrder" resourcekey="cmdNext">N</asp:LinkButton>
                    </td>
                </tr>
            </table>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlPromptPay" runat="server">
        <nbs:CartList runat="server" id="cartlist2" />
        <div id="divShipMethod2" runat="server" class="NBright_CartOptDiv">
            <asp:Label CssClass="Label" ID="plShippingMethods2" resourcekey="plShippingMethods" runat="server" controlname="plShippingMethods" suffix=""></asp:Label>
            <asp:RadioButtonList CssClass="normalRadioButton ShippingMethods" ID="rblShipMethod2" runat="server" RepeatLayout="Flow" AutoPostBack="true">
            </asp:RadioButtonList>
        </div>
        <nbs:CustomForm ID="Stg3Form" runat="server" DisplayTemplateName="stg3form.template"></nbs:CustomForm>
        <asp:Panel ID="pnlGateway2" runat="server">
            <asp:DataList ID="dlGateway2" runat="server"></asp:DataList>
        </asp:Panel>
        <asp:Panel ID="pnlGateway1" runat="server">
            <fieldset class="GatewayFieldset">
                <legend>
                    <asp:Label ID="lblBankCard" runat="server" resourcekey="lblBankCard"></asp:Label>
                </legend>
                <table class="GateWays" cellspacing="0" cellpadding="4" border="0">
                    <tr>
                        <td valign="top">
                            <asp:PlaceHolder ID="plhGatewayMsg" runat="server"></asp:PlaceHolder>
                        </td>
                        <td>&nbsp;&nbsp;&nbsp;
                        </td>
                        <td valign="top">
                            <asp:Label ID="lblCheque" runat="server" CssClass="Cheque_Link"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td valign="middle" align="center" class="CardGatewayButton">
                            <asp:PlaceHolder ID="plhGateway" runat="server"></asp:PlaceHolder>
                        </td>
                        <td>&nbsp;&nbsp;&nbsp;
                        </td>
                        <td valign="middle" align="center" class="ManualGatewayButton">
                            <asp:ImageButton ID="imgBChq" runat="server" Visible="False"></asp:ImageButton>
                        </td>
                    </tr>
                </table>
            </fieldset>
        </asp:Panel>
        <div class="NBright_ClientButtonDiv">
            <table cellpadding="0" cellspacing="0" border="0" width="100%">
                <tr>
                    <td class="NBright_ClientButtonDivLeft">
                        <asp:LinkButton ID="cmdBack2" runat="server" CssClass="NBright_ClientButton Button ReturnPrevious" resourcekey="cmdBack">B</asp:LinkButton>
                        <asp:LinkButton ID="cmdCancelOrder" runat="server" CssClass="NBright_ClientButton Button CancelOrder"
                            resourcekey="cmdCancelOrder">C</asp:LinkButton>
                    </td>
                </tr>
            </table>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlPayRtn" runat="server">
        <asp:PlaceHolder ID="plhPayRtn" runat="server"></asp:PlaceHolder>
    </asp:Panel>
</div>
