<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="ProductSearch.ascx.vb" Inherits="NEvoWeb.Modules.NB_Store.ProductSearch" %>
<table class="StoreSearch" cellspacing="0" cellpadding="0" summary="Search Input Table" border="0">
	<tr>
		<td valign="top"><asp:label id="plSearch" runat="server" controlname="cboModule" suffix=":"></asp:label><asp:image CssClass="SearchImage" id="imgSearch" runat="server" AlternateText="Search"></asp:image></td>
		<td valign="top"><asp:textbox id="txtSearch" runat="server" Wrap="False" maxlength="200"	cssclass="NBright_NormalTextBox NormalTextBox"></asp:textbox></td>
		<td valign="top"><asp:imagebutton CssClass="GoImage" id="imgGo" runat="server" AlternateText="OK"></asp:imagebutton><asp:Button id="cmdGo" CssClass="dnnSecondaryAction GoButton" runat="server" Text="Go"></asp:Button></td>
	</tr>
</table>