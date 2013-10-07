<%@ Control language="vb" CodeBehind="~/admin/Skins/skin.vb" AutoEventWireup="false" Explicit="true" Inherits="DotNetNuke.UI.Skins.Skin" %>
<%@ Register TagPrefix="dnn" TagName="LANGUAGE" Src="~/Admin/Skins/Language.ascx" %>
<%@ Register TagPrefix="dnn" TagName="COPYRIGHT" Src="~/Admin/Skins/Copyright.ascx" %>
<div id="ControlPanel" runat="server" visible="false"></div>
<style type="text/css">#ControlBar_ControlPanel,.dnnDragHint,.actionMenu,.dnnActionMenuBorder,.dnnActionMenu{display:none !important}#Form.showControlBar{margin-top:0 !important}.dnnEditState .DnnModule,.DnnModule{opacity:1 !important}</style>
<!--[if gte IE 9]><style type="text/css">.gradient{filter:none}</style><![endif]-->
<div class="langpane"><dnn:LANGUAGE runat="server" id="dnnLANGUAGE" showMenu="False" showLinks="true" ItemTemplate="&lt;a href='[URL]' class='langfont' title='[CULTURE:NATIVENAME]'&gt;[CULTURE:NATIVENAME]&lt;/a&gt;" SelectedItemTemplate="&lt;a href='[URL]' class='langfont' title='[CULTURE:NATIVENAME]'&gt;[CULTURE:NATIVENAME]&lt;/a&gt;" SeparatorTemplate="&nbsp;|&nbsp;" /></div>
<table class="BackOffice" align="center" cellspacing="0" cellpadding="0">
  <tr><td id="ContentPane" class="" runat="server" valign="top" align="left" height="100%" ContainerType="G" ContainerName="_default" ContainerSrc="No Container.ascx"></td></tr>
  <tr><td class="copyright copypane" valign="top" align="left">Copyright &copy; Nevoweb Bright</td></tr>
</table>
