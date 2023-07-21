//Jumping animation
window.addEventListener('load', function () {
    document.getElementById('burger').classList.add('drop');
});
//Img src change 
//when hover
document.getElementById('burger').addEventListener('mouseover', function () {
    this.src = '/images/cute-burger-blush.png';
});

document.getElementById('burger').addEventListener('mouseout', function () {
    this.src = '/images/cute-burger.png';
});
//when input
var passwordEntered = false;

document.getElementById('form3Example4c').addEventListener('input', function () {
    passwordEntered = true;
    document.getElementById('burger').src = '/images/cute-burger-eyecover.png';
});

var inputs = document.querySelectorAll('input');

inputs.forEach(function (input) {
    input.addEventListener('blur', function () {
        if (passwordEntered) {
            document.getElementById('burger').src = '/images/cute-burger.png';
            passwordEntered = false;
        }
    });
});