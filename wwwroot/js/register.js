$(document).ready(function () {

    $("#togglePassword").click(function () {

        var password = $("#password");
        var type = password.attr("type") === "password" ? "text" : "password";

        password.attr("type", type);

        $(this).toggleClass("bi-eye bi-eye-slash");
    });

});