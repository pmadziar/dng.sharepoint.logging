<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LogTester.ascx.cs" Inherits="dng.sharepoint.logging.LogTester.LogTester" %>
<table style="width:100%;">
    <tr>
        <td>
            <asp:TextBox ID="tbNum01" runat="server"></asp:TextBox>
        </td>
        <td>
            <asp:TextBox ID="tbNum02" runat="server"></asp:TextBox>
        </td>
        <td>
            <asp:Button ID="btnDivide" runat="server" OnClick="btnDivide_Click" Text="Divide" Width="200px" />
        </td>
    </tr>
    <tr>
        <td>&nbsp;</td>
        <td>&nbsp;</td>
        <td>
            <asp:Button ID="btnError" runat="server" OnClick="btnError_Click" Text="Write To Error" Width="200px" />
        </td>
    </tr>
    <tr>
        <td>&nbsp;</td>
        <td>&nbsp;</td>
        <td>
            <asp:Button ID="btnDebug" runat="server" OnClick="btnDebug_Click" Text="Write to Debug" Width="200px" />
        </td>
    </tr>
    <tr>
        <td colspan="3">
            <asp:Label ID="lblErr" runat="server" ForeColor="Red"></asp:Label>
        </td>
    </tr>
</table>

