<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PageNotFound.aspx.cs" Inherits="ImageConverter.PageNotFound" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Page Not Found - Image Converter App</title>
    <link href="Content/main.css" rel="stylesheet" />

    <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />

    <script type="text/javascript">
        function redirectToMainPage() {
            var redirectUrl = '<%= GetRedirectUrl() %>';
            window.location.href = redirectUrl;
        }
    </script>

</head>
<body>
    <form runat="server">
        <div class="pageNotFoundContainer">
            <img src="Images/page-not-found.png" class="center-block img-responsive pageNotFoundImage" alt="Page Not Found" />
            <h2>Page Not Found</h2>
            <input type="button" class="pageNotFoundHeading2 pageNotFoundButton" value="Go to Main Page" onclick="redirectToMainPage();" />
        </div>
    </form>
</body>
</html>

