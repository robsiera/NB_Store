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


Imports System
Imports System.Configuration
Imports System.Data
Imports System.Xml
Imports System.Web
Imports System.Collections.Generic

Imports DotNetNuke
Imports DotNetNuke.Common
Imports DotNetNuke.Common.Utilities.XmlUtils
Imports DotNetNuke.Common.Utilities
Imports NEvoWeb.Modules.NB_Store.SharedFunctions

Namespace NEvoWeb.Modules.NB_Store

    Public Class CategoryController

#Region "Private Methods/Function"


#End Region



#Region "Categories DB Methods"

        Public Sub ClearCategory(ByVal CategoryID As Integer)
            DataProvider.Instance().ClearCategory(CategoryID)
        End Sub

        Public Sub DeleteCategories(ByVal CategoryID As Integer)
            DataProvider.Instance().DeleteNB_Store_Categories(CategoryID)
        End Sub

        Public Sub CopyToLanguages(ByVal objInfo As NB_Store_CategoriesInfo, Optional ByVal ForceOverwrite As Boolean = True)
            Dim supportedLanguages As LocaleCollection = GetValidLocales()
            For Each L As String In supportedLanguages
                CopyToLanguages(objInfo, L, ForceOverwrite)
            Next
        End Sub

        Public Sub CopyToLanguages(ByVal objInfo As NB_Store_CategoriesInfo, ByVal Lang As String, Optional ByVal ForceOverwrite As Boolean = True)
            Dim blnDoCopy As Boolean = True
            Dim objDummy As NB_Store_CategoriesInfo

            'check if Language exists
            If Not ForceOverwrite Then
                objDummy = GetCategory(objInfo.CategoryID, Lang)
                If objDummy Is Nothing Then
                    blnDoCopy = True
                Else
                    If objDummy.CategoryName = "" Then
                        blnDoCopy = True
                    Else
                        blnDoCopy = False
                    End If
                End If
            End If

            If blnDoCopy Then
                objInfo.Lang = Lang
                UpdateObjCategories(objInfo)
            End If
        End Sub

        Public Function GetCategory(ByVal CategoryID As Integer, ByVal Lang As String) As NB_Store_CategoriesInfo
            Return CType(CBO.FillObject(DataProvider.Instance().GetNB_Store_Categories(CategoryID, Lang), GetType(NB_Store_CategoriesInfo)), NB_Store_CategoriesInfo)
        End Function

        Public Function GetCategories(ByVal PortalID As Integer, ByVal Lang As String) As ArrayList
            Return GetCategories(PortalID, Lang, -1, False, True)
        End Function

        Public Function GetCategories(ByVal PortalID As Integer, ByVal Lang As String, ByVal ParentID As Integer) As ArrayList
            Return GetCategories(PortalID, Lang, ParentID, False, True)
        End Function

        ''' <summary>
        ''' Gets list of Categories
        ''' </summary>
        ''' <param name="PortalID">PortalID</param>
        ''' <param name="Lang">Language Culture code</param>
        ''' <param name="ParentID">Returns Sub-categories of ParentID, use -1 for all.</param>
        ''' <param name="Archived">Returns match on Archived flag to True or False</param>
        ''' <param name="IncludeArchived">Always Included the Archived Categories</param>
        ''' <returns>ArrayList of NB_Store_CategoriesInfo</returns>
        ''' <remarks></remarks>
        Public Function GetCategories(ByVal PortalID As Integer, ByVal Lang As String, ByVal ParentID As Integer, ByVal Archived As Boolean, ByVal IncludeArchived As Boolean) As ArrayList
            Return CBO.FillCollection(DataProvider.Instance().GetNB_Store_CategoriesList(PortalID, Lang, ParentID, Archived, IncludeArchived), GetType(NB_Store_CategoriesInfo))
        End Function

        Public Function GetCategoriesTable(ByVal PortalID As Integer, ByVal Lang As String) As Hashtable
            Dim aryList As ArrayList
            aryList = GetCategories(PortalID, Lang)
            Return GetCategoriesTable(aryList)
        End Function


        Public Function GetCategoriesTable(ByVal PortalID As Integer, ByVal Lang As String, ByVal ParentID As Integer) As Hashtable
            Dim aryList As ArrayList
            aryList = GetCategories(PortalID, Lang, ParentID)
            Return GetCategoriesTable(aryList)
        End Function


        Public Function GetCategoriesTable(ByVal aryList As ArrayList) As Hashtable
            Dim hTable As New Hashtable
            Dim objInfo As NB_Store_CategoriesInfo
            For Each objInfo In arylist
                hTable.Add(objInfo.CategoryID, objInfo.CategoryName)
            Next
            Return hTable
        End Function

        Public Function UpdateObjCategories(ByVal objInfo As NB_Store_CategoriesInfo) As NB_Store_CategoriesInfo
            Return CType(CBO.FillObject(DataProvider.Instance().UpdateNB_Store_Categories(objInfo.CategoryID, objInfo.Lang, objInfo.CategoryName, objInfo.CategoryDesc, objInfo.Message, objInfo.PortalID, objInfo.Archived, objInfo.CreatedByUser, objInfo.CreatedDate, objInfo.ParentCategoryID, objInfo.ListOrder, objInfo.ProductTemplate, objInfo.ListItemTemplate, objInfo.ListAltItemTemplate, objInfo.Hide, objInfo.ImageURL, objInfo.SEOPageTitle, objInfo.SEOName, objInfo.MetaDescription, objInfo.MetaKeywords), GetType(NB_Store_CategoriesInfo)), NB_Store_CategoriesInfo)
        End Function

        Public Function DependantExists(ByVal PortalID As Integer, ByVal Lang As String, ByVal CategoryID As Integer) As Boolean
            Dim rtnValue As Boolean = False

            Dim aryList As ArrayList
            Dim objInfo As NB_Store_CategoriesInfo

            'check category dependants
            aryList = GetCategories(PortalID, Lang)
            For Each objInfo In aryList
                If objInfo.ParentCategoryID = CategoryID Then
                    rtnValue = True
                End If
            Next

            'check products
            Dim objPCtrl As New ProductController

            aryList = objPCtrl.GetProductList(PortalID, CategoryID, Lang, 1, 1, True)
            If aryList.Count > 0 Then
                rtnValue = True
            End If

            Return rtnValue
        End Function

        Public Function GetSubCategoryCollection(ByVal PortalID As Integer, ByVal ParentID As Integer) As Collection
            Dim aryList As ArrayList
            Dim SubCatCollection As New Collection

            aryList = GetCategories(PortalID, GetCurrentCulture)
            SubCatCollection = GetSubCategoryCollection(aryList, ParentID, SubCatCollection)

            Return SubCatCollection
        End Function

        Public Function GetSubCategoryCollection(ByVal aryList As ArrayList, ByVal ParentID As Integer, ByVal SubCatCollection As Collection) As Collection
            Dim objCInfo As NB_Store_CategoriesInfo

            For Each objCInfo In aryList
                If objCInfo.ParentCategoryID = ParentID And objCInfo.Archived = False Then

                    If Not SubCatCollection.Contains(objCInfo.CategoryID) Then ' check if processed before, to stop infinite loop.
                        SubCatCollection.Add(objCInfo.CategoryID, objCInfo.CategoryID)
                        SubCatCollection = GetSubCategoryCollection(aryList, objCInfo.CategoryID, SubCatCollection)
                    Else
                        'the parentID must have a recursive loop to a sub category.
                        'clear the parentcategory of the ParentID category to fix.
                        Dim objC As NB_Store_CategoriesInfo
                        objC = GetCategory(ParentID, GetCurrentCulture)
                        If Not objC Is Nothing Then
                            UpdateLog("FIXED RECURSIVE CATEGORY: " & objC.CategoryName & " CatID: " & objC.CategoryID & "ParentID: " & objC.ParentCategoryID)
                            objC.ParentCategoryID = 0
                            UpdateObjCategories(objC)
                        End If
                    End If

                End If
            Next

            Return SubCatCollection

        End Function

        Public Function GetSubCategoryList(ByVal aryList As ArrayList, ByVal ParentID As Integer) As String
            Dim colCat As New Collection
            Dim htmlText As String = ""

            colCat = GetSubCategoryCollection(aryList, ParentID, colCat)

            For intX As Integer = 1 To colCat.Count
                htmlText &= colCat.Item(intX) & ","
            Next

            Return htmlText
        End Function


        ''' <summary>
        ''' Move all the products from one category to another.
        ''' </summary>
        ''' <param name="FromCatID">Move From</param>
        ''' <param name="ToCatID">Move To</param>
        ''' <param name="RemoveFromCat">Removes product from the FromCatID</param>
        ''' <remarks></remarks>
        Public Sub MoveCategoryProducts(ByVal FromCatID As Integer, ByVal ToCatID As Integer, ByVal RemoveFromCat As Boolean)
            Dim objPCtrl As New ProductController
            Dim objCCtrl As New CategoryController
            Dim objCatInfo As NB_Store_CategoriesInfo
            Dim objPInfo As NB_Store.ProductListInfo

            Dim aryList As ArrayList

            objCatInfo = objCCtrl.GetCategory(FromCatID, GetCurrentCulture)
            If objCatInfo Is Nothing Then
                Exit Sub
            End If

            objCatInfo = objCCtrl.GetCategory(ToCatID, GetCurrentCulture)
            If objCatInfo Is Nothing Then
                Exit Sub
            End If

            aryList = objPCtrl.GetProductList(objCatInfo.PortalID, FromCatID, GetCurrentCulture, "", True, False, True)

            For Each objPInfo In aryList
                If RemoveFromCat Then
                    objPCtrl.DeleteProductCategory(objPInfo.ProductID, FromCatID)
                End If
                objPCtrl.UpdateProductCategory(objPInfo.ProductID, ToCatID)
            Next

        End Sub



#End Region

    End Class

End Namespace
