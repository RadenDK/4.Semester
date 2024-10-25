// Script for the login page to toggle the password visibility and change the icon color accordingly
document.addEventListener('DOMContentLoaded', function() {
    var passwordField = document.getElementById('password');
    var togglePasswordIcon = document.getElementById('togglePassword');

    // Set initial state of the icon based on the password field type, to enforce the correct icon color upon page reload
    if (passwordField.type === 'password') {
        togglePasswordIcon.classList.add('icon-black');
    } else {
        togglePasswordIcon.classList.add('icon-white');
    }

    togglePasswordIcon.addEventListener('click', function() {
        var type = passwordField.type === 'password' ? 'text' : 'password';
        passwordField.type = type;

        // Toggle icon color depending on the password field type
        if (type === 'text') {
            this.classList.remove('icon-black');
            this.classList.add('icon-white');
        } else {
            this.classList.remove('icon-white');
            this.classList.add('icon-black');
        }
    });
});