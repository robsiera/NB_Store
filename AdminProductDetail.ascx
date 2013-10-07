<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="AdminProductDetail.ascx.vb" Inherits="NEvoWeb.Modules.NB_Store.AdminProductDetail" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="TextEditor" Src="~/controls/TextEditor.ascx"%>
<%@ Register TagPrefix="nwb" TagName="ShowSelectLang" Src="controls/ShowSelectLang.ascx"%>

<table style="width:100%" cellpadding="8" cellspacing="0">
	<tr>
		<td><dnn:label id="labelManufacturer" runat="server" controlname="labelManufacturer"></dnn:label></td>
		<td><asp:TextBox ID="txtManufacturer" Runat="server" Width="300" MaxLength="50" CssClass="NormalTextBox"></asp:TextBox><nwb:ShowSelectLang id="ShowSelectLang2" runat="server"></nwb:ShowSelectLang></td>
		<td valign="top" rowspan="3"><nwb:ShowSelectLang id="ShowSelectLang1" runat="server"></nwb:ShowSelectLang><dnn:label id="labelSummary" runat="server" controlname="labelSummary"></dnn:label></td>
		<td valign="top" rowspan="3"><asp:TextBox ID="txtSummary" Runat="server" Width="350" Height="99" MaxLength="1000" TextMode="MultiLine" CssClass="NormalTextBox"></asp:TextBox></td>
	</tr>
	<tr>
		<td><dnn:label id="labelProductName" runat="server" controlname="labelProductName"></dnn:label></td>
		<td><asp:TextBox ID="txtProductName" Runat="server" Width="300" MaxLength="150" CssClass="NormalTextBox"></asp:TextBox><nwb:ShowSelectLang id="ShowSelectLang" runat="server"></nwb:ShowSelectLang></td>
	</tr>
	<tr>
		<td><dnn:label id="labelSEOName" runat="server" controlname="labelSEOName" suffix=" : "></dnn:label></td>
		<td><asp:TextBox ID="txtSEOName" runat="server" Width="300" MaxLength="256" CssClass="NormalTextBox"></asp:TextBox><nwb:ShowSelectLang ID="ShowSelectLang4" runat="server"></nwb:ShowSelectLang></td>
	</tr>
	<tr>
		<td><dnn:label id="labelProductRef" runat="server" controlname="labelProductRef"></dnn:label></td>
		<td><asp:TextBox ID="txtProductRef" Runat="server" Width="300" MaxLength="50" CssClass="NormalTextBox"></asp:TextBox></td>
		<td valign="top" rowspan="2"><nwb:ShowSelectLang id="ShowSelectLang5" runat="server"></nwb:ShowSelectLang><dnn:label id="plTagWords" runat="server" controlname="plTagWords"></dnn:label></td>
		<td valign="top" rowspan="2"><asp:TextBox ID="txtTagWords" Runat="server" Width="350" Height="52" MaxLength="255" TextMode="MultiLine" CssClass="NormalTextBox"></asp:TextBox></td>
	</tr>
	<tr>
		<td><dnn:label id="plTaxCategory" runat="server" controlname="plTaxCategory"></dnn:label></td>
		<td><asp:DropDownList ID="cmbCategory" Runat="server" Width="318" DataTextField="CategoryPathName" DataValueField="CategoryID"></asp:DropDownList></td>
	</tr>
	<tr>
		<td colspan="4"><asp:DataList ID="dlXMLData" runat="server"></asp:DataList></td>
	</tr>
	<tr>
		<td style="padding:0" colspan="4">
		<table class="ProductStatus" cellspacing="0">
		<tr>
		<td><dnn:label id="labelArchived" runat="server" controlname="labelArchived"></dnn:label><asp:CheckBox CssClass="normalCheckBox" ID="chkArchived" Runat="server"></asp:CheckBox></td>
		<td><dnn:label id="labelFeatured" runat="server" controlname="labelFeatured"></dnn:label><asp:CheckBox CssClass="normalCheckBox" ID="chkFeatured" Runat="server"></asp:CheckBox></td>
		<td><dnn:label id="labelDeleted" runat="server" controlname="labelDeleted"></dnn:label><asp:CheckBox CssClass="normalCheckBox" ID="chkDeleted" Runat="server"></asp:CheckBox></td>
		<td><dnn:label id="plIsHidden" runat="server" controlname="plIsHidden"></dnn:label><asp:CheckBox CssClass="normalCheckBox" ID="chkIsHidden" Runat="server"></asp:CheckBox></td>
		<td>&nbsp;</td>
		</tr>
		</table>
		</td>
	</tr>
	<tr>
		<td colspan="4">&nbsp;</td>
	</tr>
	<tr>
		<td colspan="4"><nwb:ShowSelectLang id="ShowSelectLang3" runat="server"></nwb:ShowSelectLang><dnn:label id="labelDescription" runat="server" controlname="labelDescription"></dnn:label></td>
	</tr>
	<tr>
		<td colspan="4"><dnn:TextEditor id="txtDescription" runat="server" width="100%" height="450"></dnn:TextEditor></td>
	</tr>
</table>