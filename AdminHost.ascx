<%@ Control Language="vb" Inherits="NEvoWeb.Modules.NB_Store.AdminHost" AutoEventWireup="false"
    Explicit="True" CodeBehind="AdminHost.ascx.vb" %>
<table class="NBright_ContentDiv"><tr><td>
<asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
<asp:Panel ID="pnlHost" runat="server">
    <div id="tabs" class="NBright_tabs">
            <a id="buttontabs-1" href="#"><asp:Label ID="lblGeneral" runat="server" Text="General." resourcekey="lblGeneral"></asp:Label></a>
            <a id="buttontabs-2" href="#"><asp:Label ID="lblSettings" runat="server" Text="Settings." resourcekey="lblSettings"></asp:Label></a>
            <a id="buttontabs-3" href="#"><asp:Label ID="lblReports" runat="server" Text="Reports." resourcekey="lblReports"></asp:Label></a>
     </div>
        <div id="tabs-1">
            <div class="NBright_ActionDiv">
                <asp:LinkButton ID="cmdCalcSale" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdCalcSale">Calculate Sale Prices</asp:LinkButton>
                <asp:LinkButton ID="cmdCreateThumbs" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdCreateThumbs">Create Product Thumbnails</asp:LinkButton>                    
                <asp:LinkButton ID="cmdValidation" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdValidation">LinkButton</asp:LinkButton>
                &nbsp;<asp:CheckBox CssClass="normalCheckBox" ID="chkValidation" resourcekey="chkValidation" runat="server" />
                <asp:Label ID="lblValidationMsg" runat="server" Visible="false"></asp:Label>
            </div>
            <hr />
            <div class="NBright_ActionDiv">
                <asp:LinkButton ID="cmdClearTempUpload" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdClearTempUpload">Clear temp</asp:LinkButton>
                <asp:LinkButton ID="cmdPurgeFiles" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdPurgeFiles">Purge File system</asp:LinkButton>                    
                <asp:LinkButton ID="cmdRestart" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdRestart">Rerstart App</asp:LinkButton>
            </div>
            <hr />
            <div class="NBright_ActionDiv">
                <asp:LinkButton ID="cmdClearStore" CssClass="dnnTertiaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdClearStore">LinkButton</asp:LinkButton>
                Password:
                <asp:TextBox ID="txtPass" runat="server" TextMode="Password"></asp:TextBox>
                <asp:Label ID="lblInvalidPass" runat="server" resourcekey="InvalidPass" Visible="false"></asp:Label>
            </div>
        </div>
        <div id="tabs-2">
            <div class="NBright_ActionDiv">
                <asp:LinkButton ID="cmdSaveSettings" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdSaveSettings">LinkButton</asp:LinkButton>
            </div>
            <div class="NBright_ActionDiv">
                <asp:FileUpload ID="fupTemplate" runat="server" />
                <asp:LinkButton ID="cmdImportDefault" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdImportDefault">LinkButton</asp:LinkButton>
                &nbsp;<asp:CheckBox CssClass="normalCheckBox" ID="chkOverwriteSettings" resourcekey="chkOverwriteSettings"
                    runat="server" />
            </div>
            <div class="NBright_ActionDiv">
                <asp:LinkButton ID="cmdClearSettings" CssClass="dnnTertiaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdClearSettings">LinkButton</asp:LinkButton>
            </div>
            <asp:Panel ID="pnlCheckBoxLists" runat="server">
            <br />
            <asp:Label ID="lblLang" runat="server" CssClass="NBright_BackOfficeHeading" ></asp:Label>
            <asp:CheckBoxList ID="chkLlang" runat="server"  RepeatColumns="8" CssClass="normalCheckBox NBright_CheckBoxListItem">
            </asp:CheckBoxList>
            <asp:CheckBox ID="chkSelectAll" runat="server" Text="Select All" AutoPostBack="true"  CssClass="normalCheckBox NBright_BackOfficeHeading" />                
            <asp:Label ID="lblchkSettings" runat="server" CssClass="NBright_BackOfficeHeading" ></asp:Label>
            <asp:CheckBoxList ID="chkLsettings" runat="server" RepeatColumns="4"  CssClass="normalCheckBox NBright_CheckBoxListItem" >
            </asp:CheckBoxList>
            <asp:CheckBox ID="chkSelectAllT" runat="server" Text="Select All" AutoPostBack="true"  CssClass="normalCheckBox NBright_BackOfficeHeading" />                
            <asp:Label ID="lblTemplates" runat="server"  CssClass="NBright_BackOfficeHeading"></asp:Label>
            <asp:CheckBoxList ID="chkLtemplates" runat="server"  RepeatColumns="4"  CssClass="normalCheckBox NBright_CheckBoxListItem">
            </asp:CheckBoxList>
            </asp:Panel>
            <br />
                            <asp:LinkButton ID="cmdUpgradeAll" CssClass="dnnPrimaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdUpgradeAll">LinkButton</asp:LinkButton><br /><br />                    
                <asp:DataGrid ID="dgPortals" runat="server" AutoGenerateColumns="False" Width="100%"
                    CellPadding="1" GridLines="None" AllowPaging="false">
                    <HeaderStyle CssClass="NBright_HeaderStyle" />
                    <FooterStyle CssClass="NBright_FooterStyle" />
                    <EditItemStyle CssClass="NBright_EditItemStyle" />
                    <SelectedItemStyle CssClass="NBright_SelectedItemStyle" />
                    <PagerStyle CssClass="NBright_PagerStyle" Mode="NumericPages" />
                    <AlternatingItemStyle CssClass="NBright_AlternatingItemStyle" />
                    <ItemStyle CssClass="NBright_ItemStyle" />
                    <Columns>
                        <asp:BoundColumn DataField="PortalID" HeaderText="ID" Visible="true"></asp:BoundColumn>
                        <asp:BoundColumn DataField="PortalName" HeaderText="Portal Name" Visible="true"></asp:BoundColumn>
                        <asp:BoundColumn DataField="Version" HeaderText="Version" Visible="true"></asp:BoundColumn>
                        <asp:TemplateColumn HeaderText="">
                            <ItemTemplate>
                                <asp:Label  ID="lblMsg" runat="server" Text=''></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                    </Columns>
                </asp:DataGrid>
        </div>
        <div id="tabs-3">
            <div class="NBright_ActionDiv">
                <asp:LinkButton ID="cmdExportReport" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdExportReports">LinkButton</asp:LinkButton>
            </div>
            <div class="NBright_ActionDiv">
                <asp:FileUpload ID="fupReports" runat="server" />
                <asp:LinkButton ID="cmdImportReports" CssClass="dnnSecondaryAction NBright_CommandButton" runat="server"
                    resourcekey="cmdImportReports">LinkButton</asp:LinkButton>
                &nbsp;<asp:CheckBox CssClass="normalCheckBox" ID="chkOverwriteReport" resourcekey="chkOverwriteReport" runat="server" />
            </div>
        </div>
</asp:Panel>
</td></tr>
</table>
