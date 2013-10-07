﻿' --- Copyright (c) notice NevoWeb ---
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

Imports System.Web.UI

Imports DotNetNuke
Imports DotNetNuke.Services.Exceptions
Imports NEvoWeb.Modules.NB_Store.SharedFunctions

Namespace NEvoWeb.Modules.NB_Store

    Partial Public Class MiniCartSettings
        Inherits BaseModuleSettings

        Public Overrides Sub LoadSettings()

            Try

                If (Page.IsPostBack = False) Then

                    If ModuleSettings.Count = 0 Then
                        DoReset()
                    End If

                    populateTemplateList(PortalId, ddlEmptyTemplate, ".template")
                    populateTemplateList(PortalId, ddlFullTemplate, ".template")
                    populateDropDownList(ddlMiniPosition, Localization.GetString("MiniPositionName", LocalResourceFile))

                    Dim li As ListItem
                    li = New ListItem
                    li.Value = ""
                    li.Text = ""
                    ddlEmptyTemplate.Items.Insert(0, li)

                    li = New ListItem
                    li.Value = ""
                    li.Text = ""
                    ddlFullTemplate.Items.Insert(0, li)

                    LoadBaseSettings()

                End If

            Catch exc As Exception           'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try

        End Sub

        Public Overrides Sub UpdateSettings()
            Try

                UpdateBaseSettings()

            Catch exc As Exception           'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub


    End Class

End Namespace
