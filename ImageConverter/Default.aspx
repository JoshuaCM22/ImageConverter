<%@ Page Title="Main Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ImageConverter._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <br />
        <br />
        <asp:FileUpload ID="imgFileUpload" runat="server" onchange="validateFileSize()" />
        <br />
        <label>Select Image Format:</label>
        <asp:DropDownList ID="FormatDropDown" runat="server">
            <asp:ListItem Text="ICO" Value="ico"></asp:ListItem>
            <asp:ListItem Text="PNG" Value="png"></asp:ListItem>
            <asp:ListItem Text="JPEG" Value="jpeg"></asp:ListItem>
            <asp:ListItem Text="JPG" Value="jpg"></asp:ListItem>
            <asp:ListItem Text="BMP" Value="bmp"></asp:ListItem>
            <asp:ListItem Text="GIF" Value="gif"></asp:ListItem>
            <asp:ListItem Text="TIFF" Value="tiff"></asp:ListItem>
        </asp:DropDownList>
        <br />
        <br />
        <asp:Button ID="btnConvertImage" Text="Convert Image" Style="background-color: #2e86c1; color: white; border: none; padding: 10px 20px; font-size: 16px; cursor: pointer;"
            runat="server" OnClick="btnConvertImage_Click"
            OnClientClick="return checkFile();" />
        <br />
        <asp:Label ID="lblResult" runat="server" Text="" ForeColor="Red"></asp:Label>
        <br />
        <br />
    </div>

    <script type="text/javascript">
        function deleteCookie(cookieName) {
            document.cookie = `${cookieName}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
        }

        function checkCookieExists(cookieName) {
            // Get all cookies as a single string
            const allCookies = document.cookie;

            // Create a search pattern for the cookie name followed by an equals sign
            const cookiePattern = new RegExp(`(^|; )${cookieName}=`);

            // Test if the cookie pattern is found in the allCookies string
            return cookiePattern.test(allCookies);
        }

        function getCookieValue(cookieName) {
            // Get all cookies as a single string
            const allCookies = document.cookie;

            // Create a regular expression to find the specific cookie name and capture its value
            const match = allCookies.match(new RegExp(`(^|; )${cookieName}=([^;]*)`));

            // If match is found, return the value (the second captured group), otherwise return null
            return match ? decodeURIComponent(match[2]) : null;
        }

        function checkFile() {

            setTimeout(function () {
                const fileInput = document.getElementById('<%= imgFileUpload.ClientID %>');
                const labelResult = document.getElementById('<%= lblResult.ClientID %>');

                let laman = checkCookieExists("ConversionResult");
                let asd =  getCookieValue("ConversionResult");

                if (fileInput.files.length > 0 && checkCookieExists("ConversionResult") && getCookieValue("ConversionResult") === "Success") {
                    labelResult.innerText = "Successfully converted the image.";
                    labelResult.style.color = "green";
                }
                else labelResult.style.color = "red";

                deleteCookie("ConversionResult");
            }, 1000);
         
        }

        function validateFileSize() {
            const fileInput = document.getElementById('<%= imgFileUpload.ClientID %>');
              const file = fileInput.files[0];
              const maxSizeInMB = 10; // Set max size in MB
              const maxSizeInBytes = maxSizeInMB * 1024 * 1024;

              if (file && file.size > maxSizeInBytes) {
                  alert(`File size exceeds ${maxSizeInMB} MB. Please select a smaller file.`);
                  fileInput.value = ""; // Clear the file input
              }
        }

</script>

</asp:Content>

