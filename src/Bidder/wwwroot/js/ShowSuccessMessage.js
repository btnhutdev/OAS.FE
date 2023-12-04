var successMessage = document.getElementById('SuccessMessage');

if (successMessage != null) {
    successMessage.style.display = 'block';
    setTimeout(function () {
        successMessage.style.display = 'none';
    }, 2500);
}