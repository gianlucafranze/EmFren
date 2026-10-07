function togglePassword(inputId, button) {

    const input =
        document.getElementById(inputId);

    if (input.type === "password") {

        input.type = "text";

        button.textContent = "🙈";

    } else {

        input.type = "password";

        button.textContent = "👁";

    }

}


document.querySelector("form").addEventListener("submit", function (event) {

    const password =
        document.getElementById("password").value;

    const confirmPassword =
        document.getElementById("confirm_password").value;

    if (password !== confirmPassword) {

        event.preventDefault();

        alert("Las contraseñas no coinciden.");

    }

});