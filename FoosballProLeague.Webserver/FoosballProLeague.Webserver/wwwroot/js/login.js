document.addEventListener('DOMContentLoaded', function() {
    var passwordField = document.getElementById('password');
    var togglePasswordIcon = document.getElementById('togglePassword');

    // Set initial state of the icon based on the password field type
    if (passwordField.type === 'password') {
        togglePasswordIcon.classList.add('icon-black');
    } else {
        togglePasswordIcon.classList.add('icon-white');
    }

    togglePasswordIcon.addEventListener('click', function() {
        var type = passwordField.type === 'password' ? 'text' : 'password';
        passwordField.type = type;

        // Toggle icon color
        if (type === 'text') {
            this.classList.remove('icon-black');
            this.classList.add('icon-white');
        } else {
            this.classList.remove('icon-white');
            this.classList.add('icon-black');
        }
    });
});