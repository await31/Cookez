//Password reveal
document.querySelector('.password-field i').addEventListener('click', function () {
    var passwordInput = document.getElementById('form3Example4c');
    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        this.classList.remove('fa-eye-slash');
        this.classList.add('fa-eye');
        document.getElementById('burger').src = '/images/cute-burger-1-eyecover.png';
    } else {
        passwordInput.type = 'password';
        this.classList.remove('fa-eye');
        this.classList.add('fa-eye-slash');
        document.getElementById('burger').src = '/images/cute-burger-eyecover.png';
    }
});

