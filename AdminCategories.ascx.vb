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

Namespace NEvoWeb.Modules.NB_Store

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The EditNB_Store class is used to manage content
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Partial Class AdminCategories
        Inherits BaseAdminModule

#Region "Private Members"

        'Protected _CatId As Integer = -1

        Private _CtlKey As String = ""
        Private _CurrentPage As Integer = 1
        Private _SkinSrc As String = ""
        Protected _Related As Boolean = False

        Protected WithEvents productlist As NEvoWeb.Modules.NB_Store.AdminProductList

#End Region

#Region "Event Handlers"

        Private Sub Page_Init1(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

            AddHandler productlist.AddRelated, AddressOf productlist_AddRelated
            AddHandler productlist.ReturnButton, AddressOf productlist_ReturnButton

            _CurrentPage = 1
            If Not (Request.QueryString("currentpage") Is Nothing) Then
                If IsNumeric(Request.QueryString("currentpage")) Then
                    _CurrentPage = Request.QueryString("currentpage")
                End If
            End If

            _SkinSrc = ""
            If Not (Request.QueryString("skinsrc") Is Nothing) Then
                _SkinSrc = Request.QueryString("skinsrc")
            End If

            _Related = False
            If Not (Request.QueryString("rel") Is Nothing) Then
                If Request.QueryString("rel") = "1" Then
                    _Related = True
                End If
            End If

            _CtlKey = ""
            If Not (Request.QueryString("ctl") Is Nothing) Then
                _CtlKey = Request.QueryString("ctl")
            End If

        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Page_Load runs when the control is loaded
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
            Try

                ' If this is the first visit to the page, bind the role data to the datalist
                If Not Page.IsPostBack Then

                    If _Related Then
                        populateList()
                    Else
                        pnlList.Visible = False

                        cmdDelete.Attributes.Add("onClick", "javascript:return confirm('" & Localization.GetString("DeleteItem", LocalResourceFile) & "');")
                        cmdClearProducts.Attributes.Add("onClick", "javascript:return confirm('" & Localization.GetString("cmdClearProductsMsg", LocalResourceFile) & "');")
                        cmdMoveProd.Attributes.Add("onClick", "javascript:return confirm('" & Localization.GetString("cmdMoveProdMsg", LocalResourceFile) & "');")
                        cmdCopyProd.Attributes.Add("onClick", "javascript:return confirm('" & Localization.GetString("cmdCopyProdMsg", LocalResourceFile) & "');")

                        divCatSelect.Visible = True
                        pnlEdit.Visible = False

                        Dim CatID As Integer = getCookieCatID()

                        populateCategoryTextList(PortalSettings.PortalId, lbCategory, "", "", CatID.ToString)
                        If CatID >= 0 Then
                            populateEdit()
                        End If
                    End If
                End If

            Catch exc As Exception    'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

        Private Sub lbCategory_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lbCategory.SelectedIndexChanged
            If lbCategory.SelectedValue <> "-1" And IsNumeric(lbCategory.SelectedValue) Then
                setAdminCookieValue(PortalId, "CatID", lbCategory.SelectedValue.ToString)
                Response.Redirect(EditUrl(_CtlKey), True)
            End If
        End Sub

        Private Sub cmdCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdCancel.Click
            Try
                setAdminCookieValue(PortalId, "CatID", "-1")
                Response.Redirect(EditUrl(_CtlKey), True)
            Catch exc As Exception    'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

        Private Sub cmdUpdate_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdUpdate.Click
            Try
                Dim objInfo As NB_Store_CategoriesInfo
                objInfo = updateCategory()
                populateCategoryTextList(PortalSettings.PortalId, lbCategory, "", "", -1)
                populateEdit()
            Catch exc As Exception    'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

        Private Sub cmdDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdDelete.Click
            Try
                Dim objCtrl As New CategoryController
                If objCtrl.DependantExists(PortalId, GetCurrentCulture, getCookieCatID) Then
                    lblErrorDelete.Visible = True
                Else
                    lblErrorDelete.Visible = False

                    objCtrl.DeleteCategories(getCookieCatID)
                    Response.Redirect(EditUrl(_CtlKey), True)
                End If
            Catch exc As Exception    'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

        Private Sub cmdAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdAdd.Click
            Try
                setAdminCookieValue(PortalId, "CatID", "0") ' new product
                Response.Redirect(EditUrl(_CtlKey), True)
            Catch exc As Exception    'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

        Private Sub cmdRemove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdRemove.Click
            Try

                Dim objCtrl As New CategoryController
                Dim objInfo As NB_Store_CategoriesInfo
                objInfo = objCtrl.GetCategory(getCookieCatID(), GetCurrentCulture)
                If Not objInfo Is Nothing Then
                    objInfo.ImageURL = ""
                    objCtrl.UpdateObjCategories(objInfo)
                End If

                populateEdit()

            Catch exc As Exception    'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

        Private Sub selectlang_AfterChange(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataListCommandEventArgs, ByVal PreviousEditLang As String) Handles selectlang.AfterChange
            Select Case e.CommandName
                Case "Change"
                    populateEdit()
            End Select
        End Sub

        Private Sub selectlang_BeforeChange(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataListCommandEventArgs, ByVal NewEditLang As String) Handles selectlang.BeforeChange
            Select Case e.CommandName
                Case "Change"
                    Dim objInfo As New NB_Store_CategoriesInfo
                    objInfo = updateCategory()
            End Select
        End Sub

        Private Sub cmdMoveProd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdMoveProd.Click
            Dim objCtrl As New CategoryController
            objCtrl.MoveCategoryProducts(getCookieCatID(), ddlMoveProd.SelectedValue, True)
            populateEdit()
        End Sub

        Private Sub cmdCopyProd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdCopyProd.Click
            Dim objCtrl As New CategoryController
            objCtrl.MoveCategoryProducts(getCookieCatID(), ddlMoveProd.SelectedValue, False)
            populateEdit()
        End Sub

#End Region

        Private Function updateCategory() As NB_Store_CategoriesInfo
            Dim objCtrl As New CategoryController
            Dim objInfo As New NB_Store_CategoriesInfo
            Dim CatID As Integer = getCookieCatID()

            If CatID > 0 And CatID = ddlParentCategory.SelectedValue Then
                lblRecursionWarning.Visible = True
                Return Nothing
            Else

                objInfo = objCtrl.GetCategory(CatID, GetCurrentCulture)
                If objInfo Is Nothing Then
                    objInfo = New NB_Store_CategoriesInfo
                End If

                'check for recursive sub categories,
                Dim subCatList As New Collection
                Dim updatePCatID As String = ddlParentCategory.SelectedValue
                If CatID > 0 Then 'If New record an infinate loop is impossible, so do not check.
                    subCatList = objCtrl.GetSubCategoryCollection(PortalId, CatID)
                    If subCatList.Contains(updatePCatID) Then
                        updatePCatID = objInfo.ParentCategoryID
                    End If
                End If

                lblRecursionWarning.Visible = False
                objInfo.CategoryID = CatID
                objInfo.CategoryName = txtCategoryName.Text
                objInfo.CategoryDesc = txtDescription.Text
                objInfo.Message = CType(txtMessage, DotNetNuke.UI.UserControls.TextEditor).Text
                objInfo.ListOrder = txtOrder.Text
                objInfo.CreatedByUser = UserId
                objInfo.CreatedDate = Now
                objInfo.Lang = selectlang.SelectedLang
                objInfo.ParentCategoryID = updatePCatID
                objInfo.ProductTemplate = ddlProductTemplate.SelectedValue
                objInfo.ListItemTemplate = ddlListItemTemplate.SelectedValue
                objInfo.ListAltItemTemplate = ddlListAltItemTemplate.SelectedValue
                objInfo.PortalID = PortalId
                objInfo.Archived = chkArchived.Checked
                objInfo.Hide = chkHide.Checked

                objInfo.SEOName = txtSEOName.Text
                objInfo.SEOPageTitle = txtSEOPageTitle.Text
                objInfo.MetaDescription = txtMetaDescription.Text
                objInfo.MetaKeywords = txtMetaKeywords.Text

                objInfo = objCtrl.UpdateObjCategories(objInfo)

                setAdminCookieValue(PortalId, "CatID", objInfo.CategoryID.ToString)

                objCtrl.CopyToLanguages(objInfo, False)

                Return objInfo
            End If

        End Function

        Private Sub populateList()
            pnlList.Visible = True
            pnlCategory.Visible = False
            productlist.populateList()
        End Sub

        Private Sub populateEdit()
            Dim objCtrl As New CategoryController
            Dim objInfo As NB_Store_CategoriesInfo
            Dim CatID As Integer = getCookieCatID()

            phCatID.Controls.Add(New LiteralControl("CatID : " & CatID.ToString))
            
            pnlEdit.Visible = True
            pnlCategory.Visible = True
            pnlList.Visible = False
            'divCatSelect.Visible = False

            objInfo = objCtrl.GetCategory(CatID, selectlang.SelectedLang)

            'parent category list load ---------------------------
            If Not objInfo Is Nothing Then
                populateCategoryList(PortalId, ddlParentCategory, "", "", objInfo.ParentCategoryID)
            Else
                populateCategoryList(PortalId, ddlParentCategory)
            End If

            populateCategoryList(PortalId, ddlMoveProd)

            Dim li As ListItem
            li = New ListItem
            li.Value = "0"
            li.Text = Localization.GetString("None", LocalResourceFile)
            ddlParentCategory.Items.Insert(0, li)
            '-------------------------------------------------

            populateTemplateList(PortalId, ddlProductTemplate, ".template")
            populateTemplateList(PortalId, ddlListItemTemplate, ".template")
            populateTemplateList(PortalId, ddlListAltItemTemplate, ".template")
            li = New ListItem
            li.Value = ""
            li.Text = Localization.GetString("Default", LocalResourceFile)
            ddlProductTemplate.Items.Insert(0, li)
            li = New ListItem
            li.Value = ""
            li.Text = Localization.GetString("Default", LocalResourceFile)
            ddlListItemTemplate.Items.Insert(0, li)
            li = New ListItem
            li.Value = ""
            li.Text = Localization.GetString("Default", LocalResourceFile)
            ddlListAltItemTemplate.Items.Insert(0, li)

            ShowSelectLang.Refresh()
            ShowSelectLang1.Refresh()
            ShowSelectLang2.Refresh()
            ShowSelectLang3.Refresh()
            ShowSelectLang4.Refresh()
            ShowSelectLang5.Refresh()
            ShowSelectLang6.Refresh()

            If Not objInfo Is Nothing Then
                If Not ddlParentCategory.Items.FindByValue(objInfo.ParentCategoryID) Is Nothing Then
                    ddlParentCategory.ClearSelection()
                    ddlParentCategory.Items.FindByValue(objInfo.ParentCategoryID).Selected = True
                End If
                If Not ddlProductTemplate.Items.FindByValue(objInfo.ProductTemplate) Is Nothing Then
                    ddlProductTemplate.ClearSelection()
                    ddlProductTemplate.Items.FindByValue(objInfo.ProductTemplate).Selected = True
                End If
                If Not ddlListItemTemplate.Items.FindByValue(objInfo.ListItemTemplate) Is Nothing Then
                    ddlListItemTemplate.ClearSelection()
                    ddlListItemTemplate.Items.FindByValue(objInfo.ListItemTemplate).Selected = True
                End If
                If Not ddlListAltItemTemplate.Items.FindByValue(objInfo.ListAltItemTemplate) Is Nothing Then
                    ddlListAltItemTemplate.ClearSelection()
                    ddlListAltItemTemplate.Items.FindByValue(objInfo.ListAltItemTemplate).Selected = True
                End If
                txtCategoryName.Text = objInfo.CategoryName
                txtDescription.Text = objInfo.CategoryDesc
                CType(txtMessage, DotNetNuke.UI.UserControls.TextEditor).Text = objInfo.Message
                txtOrder.Text = objInfo.ListOrder
                chkArchived.Checked = objInfo.Archived
                chkHide.Checked = objInfo.Hide

                txtSEOName.Text = objInfo.SEOName
                txtSEOPageTitle.Text = objInfo.SEOPageTitle
                txtMetaDescription.Text = objInfo.MetaDescription
                txtMetaKeywords.Text = objInfo.MetaKeywords

                If objInfo.ImageURL = "" Then
                    cmdAddImage.Visible = True
                    cmdBrowse.Visible = True
                    cmdRemove.Visible = False
                    imgCat.Visible = False
                Else
                    cmdAddImage.Visible = False
                    cmdBrowse.Visible = False
                    cmdRemove.Visible = True
                    imgCat.Visible = True
                    imgCat.ImageUrl = StoreInstallPath & "makethumbnail.ashx?Image=" & QueryStringEncode(PRODUCTIMAGESFOLDER & "\" & System.IO.Path.GetFileName(objInfo.ImageURL)) & "&w=100&tabid=" & TabId
                End If
            Else
                cmdAddImage.Visible = True
                cmdBrowse.Visible = True
                cmdRemove.Visible = False
                imgCat.Visible = False
            End If

            If txtOrder.Text = "" Then txtOrder.Text = "1"

            '----- Populate Product list -------------
            If CatID > 0 Then

                Dim aryList As ArrayList
                Dim ListSize As Integer
                Dim objPCtrl As New ProductController
                Dim PgSize As Integer = dgProducts.PageSize

                aryList = objPCtrl.GetProductList(PortalSettings.PortalId, CatID, GetCurrentCulture, "", False, _CurrentPage, PgSize, False, True)
                ListSize = objPCtrl.GetProductListSize(PortalSettings.PortalId, CatID, GetCurrentCulture, "", False, False, False, True, "", False)

                dgProducts.DataSource = aryList
                dgProducts.DataBind()


                dgProducts.DataSource = aryList
                dgProducts.DataBind()
                ctlPagingControl.QuerystringParams = "ctl=" & _CtlKey & "&mid=" & ModuleId

                ctlPagingControl.TotalRecords = ListSize
                ctlPagingControl.PageSize = PgSize
                ctlPagingControl.CurrentPage = _CurrentPage
                ctlPagingControl.TabID = PortalSettings.ActiveTab.TabID
                ctlPagingControl.BorderWidth = 0
                ctlPagingControl.SkinSrc = _SkinSrc

                If ListSize <= PgSize Or PgSize = -1 Then
                    ctlPagingControl.Visible = False
                Else
                    ctlPagingControl.Visible = True
                End If
            End If

        End Sub

        Private Sub cmdAddImage_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdAddImage.Click
            Dim ImageSize As Integer = 450
            Dim objSCtrl As New SettingsController
            Dim objSInfo As NB_Store_SettingsInfo

            objSInfo = objSCtrl.GetSetting(PortalId, "image.resize", "None")
            If Not objSInfo Is Nothing Then
                If IsNumeric(objSInfo.SettingValue) Then
                    If CInt(objSInfo.SettingValue) >= 50 Then
                        ImageSize = CInt(objSInfo.SettingValue)
                    Else
                        ImageSize = 0
                    End If
                End If
            End If

            Dim ImageQuality As Integer = 85
            objSInfo = objSCtrl.GetSetting(PortalId, "image.quality", "None")
            If Not objSInfo Is Nothing Then
                If IsNumeric(objSInfo.SettingValue) Then
                    ImageQuality = CInt(objSInfo.SettingValue)
                End If
            End If

            Dim InterpolationMode As Integer = 7
            Dim SmoothingMode As Integer = 2
            Dim PixelOffsetMode As Integer = 0
            Dim CompositingQuality As Integer = 0
            Dim ImageAdvanced() As String
            objSInfo = objSCtrl.GetSetting(PortalId, "image.advanced", "None")
            If Not objSInfo Is Nothing Then
                ImageAdvanced = objSInfo.SettingValue.Split(","c)
                If ImageAdvanced.GetUpperBound(0) = 3 Then
                    If IsNumeric(ImageAdvanced(0)) Then InterpolationMode = CInt(ImageAdvanced(0))
                    If IsNumeric(ImageAdvanced(1)) Then SmoothingMode = CInt(ImageAdvanced(1))
                    If IsNumeric(ImageAdvanced(2)) Then PixelOffsetMode = CInt(ImageAdvanced(2))
                    If IsNumeric(ImageAdvanced(3)) Then CompositingQuality = CInt(ImageAdvanced(3))
                End If
            End If

            Dim objRtn As NB_Store_CategoriesInfo
            objRtn = updateCategory()

            UploadImage(objRtn.CategoryID, selectlang.SelectedLang, ImageSize, ImageQuality, InterpolationMode, SmoothingMode, PixelOffsetMode, CompositingQuality)

            populateEdit()

        End Sub

        Public Sub UploadImage(ByVal CatID As Integer, ByVal Lang As String, ByVal ImageSize As Integer, Optional ByVal ImageQuality As Integer = 85, Optional ByVal InterpolationMode As Integer = 7, Optional ByVal SmoothingMode As Integer = 2, Optional ByVal PixelOffsetMode As Integer = 0, Optional ByVal CompositingQuality As Integer = 0)
            Dim strMsg As String = ""
            Dim fs As New FileSystemUtils
            Dim HideFlag As Boolean = False
            Dim objCtrl As New CategoryController
            Dim objInfo As NB_Store_CategoriesInfo

            If cmdBrowse.FileName <> "" Then

                'try and clear any file that may be in temp directory (IIS may lock them)
                RemoveFiles(PortalSettings, "temp")

                Dim NewFileName As String = ""
                NewFileName = "Cat" & CatID & "_Image" & System.IO.Path.GetExtension(cmdBrowse.FileName)

                CreateDir(PortalSettings, "temp")
                CreateDir(PortalSettings, PRODUCTIMAGESFOLDER)

                FileSystemUtils.SaveFile(PortalSettings.HomeDirectoryMapPath & "temp\" & NewFileName, cmdBrowse.FileBytes)

                Dim objImgResize As New ImgReSize
                objImgResize._ImageQuality = ImageQuality
                objImgResize._InterpolationMode = InterpolationMode
                objImgResize._CompositingQuality = CompositingQuality
                objImgResize._PixelOffsetMode = PixelOffsetMode
                objImgResize._SmoothingMode = SmoothingMode
                Dim strUploadFile As String = ""
                strUploadFile = objImgResize.ResizeImageFile(PortalSettings, PortalSettings.HomeDirectoryMapPath & PRODUCTIMAGESFOLDER, PortalSettings.HomeDirectoryMapPath & "temp\" & NewFileName, ImageSize)

                objInfo = objCtrl.GetCategory(CatID, GetCurrentCulture)
                If Not objInfo Is Nothing Then
                    objInfo.ImageURL = PortalSettings.HomeDirectory & PRODUCTIMAGESFOLDER & "/" & System.IO.Path.GetFileName(strUploadFile)
                    objCtrl.UpdateObjCategories(objInfo)

                    'clear any existing cache
                    Dim strCacheKey As String = ""
                    Dim strFilepath As String = ""
                    strFilepath = PortalSettings.HomeDirectoryMapPath & PRODUCTIMAGESFOLDER & "\" & System.IO.Path.GetFileName(objInfo.ImageURL)
                    strCacheKey = strFilepath.Substring(strFilepath.LastIndexOf("\Portals\")) & "100x100"
                    DataCache.RemoveCache(strCacheKey)

                End If

                ''clear down temp folder 
                FileSystemUtils.DeleteFile(PortalSettings.HomeDirectoryMapPath & "temp\" & NewFileName, PortalSettings, True)

            End If
        End Sub

        Private Function getCookieCatID() As Integer
            Dim strCatID As String = getAdminCookieValue(PortalId, "CatID")
            If IsNumeric(strCatID) Then
                Return CInt(strCatID)
            Else
                Return -1
            End If
        End Function

#Region "Related products events"

        Private Sub cmdClearProducts_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdClearProducts.Click
            Dim objCtrl As New CategoryController
            objCtrl.ClearCategory(getCookieCatID())
            populateEdit()
        End Sub


        Private Sub cmdSelectProducts_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdSelectProducts.Click
            updateCategory()
            setUrlCookieInfo(PortalId, Request)
            'store current outletid in cookie as prodid for return from adminprdiuctlist
            setAdminCookieValue(PortalId, "ProdID", getCookieCatID().ToString)
            Response.Redirect(EditUrl("rel", "1", _CtlKey), True)
        End Sub

        Private Sub cmdEditProducts_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdEditProducts.Click
            updateCategory()
            Response.Redirect(EditUrl("CatID", getCookieCatID().ToString, "AdminProduct"), True)
        End Sub


        Private Sub dgProducts_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles dgProducts.ItemDataBound
            Dim item As DataGridItem = e.Item

            If item.ItemType = ListItemType.Item Or _
                item.ItemType = ListItemType.AlternatingItem Or _
                item.ItemType = ListItemType.SelectedItem Then

                Dim imgColumnControl As Control = item.Controls(3).Controls(0)
                If TypeOf imgColumnControl Is ImageButton Then
                    Dim remImage As ImageButton = CType(imgColumnControl, ImageButton)
                    remImage.Attributes.Add("onClick", "javascript:return confirm('" & Localization.GetString("cmdRemoveFromCatMsg", LocalResourceFile) & "');")
                    remImage.ToolTip = Localization.GetString("cmdRemove", LocalResourceFile)
                End If

            End If

        End Sub

        Private Sub dgProducts_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles dgProducts.ItemCommand
            If e.CommandName = "Remove" Then
                Dim item As DataGridItem = e.Item
                Dim ItemId As Integer = Int32.Parse(e.CommandArgument.ToString)
                Dim objCtrl As New ProductController
                objCtrl.DeleteProductCategory(ItemId, getCookieCatID())
                populateEdit()
            End If
        End Sub

#End Region

#Region "productlist Events"

        Private Sub productlist_AddRelated(ByVal CategoryID As Integer, ByVal RelatedID As Integer)
            Dim objCtrl As New ProductController
            objCtrl.UpdateProductCategory(RelatedID, CategoryID)
        End Sub

        Private Sub productlist_ReturnButton(ByVal sender As Object, ByVal e As System.EventArgs)
            Response.Redirect(EditUrl(_CtlKey), True)
        End Sub


#End Region



    End Class

End Namespace