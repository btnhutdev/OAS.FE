var errorMessage = document.getElementById('ErrorMessage');

if (errorMessage != null) {
    errorMessage.style.display = 'block';
    setTimeout(function () {
        errorMessage.style.display = 'none';
    }, 2500);
}