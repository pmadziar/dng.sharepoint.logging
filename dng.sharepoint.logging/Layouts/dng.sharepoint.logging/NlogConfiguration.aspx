<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NlogConfiguration.aspx.cs" Inherits="dng.sharepoint.logging.Layouts.dng.sharepoint.logging.NlogConfiguration" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <script src="js/jquery-2.2.4.js" type="text/javascript"></script>
    <script src="js/base64js.min.js" type="text/javascript"></script>
    <script src="js/text-encoder-lite.js" type="text/javascript"></script>

    <script src="js/cm/lib/codemirror.js" type="text/javascript"></script>
    <link rel="stylesheet" href="js/cm/lib/codemirror.css" />
    <script src="js/cm/mode/xml/xml.js" type="text/javascript"></script>

    <style type="text/css">
        div.errorLbl {
            display: block;
            height: 25px;
        }
        div.errorLbl > span {
            color: red;
            font-weight: 700;
        }

        .CodeMirror {
          border: 1px solid darkgray;
          height: auto;
          padding-top: 5px;
          padding-bottom: 5px;
        }

        #buttonsContainer {
            margin-top: 20px;
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <div id="xmlEditor"></div>
    <div id="buttonsContainer">
        <asp:Button ID="saveConfig" runat="server" Text="Save Changes" OnClientClick="return saveConfig();" />
        <asp:Button ID="cancel" runat="server" Text="Cancel" OnClientClick="return getConfig();" />
    </div>
    <div class="errorLbl">
        <asp:Label ID="lblMessage" runat="server" Text="" CssClass="errorLbl"></asp:Label>
    </div>
    <asp:Literal ID="liGeneratedScript" runat="server"></asp:Literal>

    <script type="text/javascript">
        var cm;

        function Base64Encode(str) {
            var encoding = 'utf-8';
            var bytes = new TextEncoderLite(encoding).encode(str);        
            return base64js.fromByteArray(bytes);
        }

        function Base64Decode(str) {
            var encoding = 'utf-8';
            var bytes = base64js.toByteArray(str);
            return new TextDecoderLite(encoding).decode(bytes);
        }
                
        function saveConfig() {
            $("div.errorLbl > span").empty();
            //var xmlStr = $("#" + controlIds["tbEditor"]).val();
            var xmlStr = cm.getValue();
            var datastr = Base64Encode(xmlStr);
            $.ajax({
                type: "POST",
                url: "NlogConfiguration.aspx/SaveConfig",
                data: JSON.stringify({ input: datastr }),
                contentType: "application/json;charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    if (!msg.d.Valid) {
                        $("div.errorLbl > span").empty().append(msg.d.Message);
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $("div.errorLbl > span").empty().append(textStatus);
                }
            });
            return false;
        }

        function getConfig() {
            $("div.errorLbl > span").empty();
            $.ajax({
                type: "POST",
                url: "NlogConfiguration.aspx/GetConfig",
                contentType: "application/json;charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    if (msg.d.Valid) {
                        var msg = Base64Decode(msg.d.Message);
                        //$("#" + controlIds["tbEditor"]).val(msg);
                        cm.setValue(msg);
                    } else {
                        $("div.errorLbl > span").empty().append(msg.d.Message);
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $("div.errorLbl > span").empty().append(textStatus);
                }
            });
            return false;
        }

        var tec = $("#xmlEditor")[0];
        cm = CodeMirror(tec, {
            mode: 'xml',
            lineNumbers: true
        });
        getConfig();

        (function () {
            //_spBodyOnLoadFunctionNames.push("delayPlevDisplay");
        })();
    </script>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
NLog Configuration Manager
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
NLog Configuration Editor
</asp:Content>
