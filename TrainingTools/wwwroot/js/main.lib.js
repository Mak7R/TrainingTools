function setCookie(cookieString) {
    document.cookie = cookieString;
}

function getCookies(){
    return document.cookie.split(';');
}

function clearModalsOnClose(){
    const modals = document.querySelectorAll('.modal');

    modals.forEach(modal => {
        modal.addEventListener('hide.bs.modal', function (event) {
            const inputs = modal.querySelectorAll('input');
            inputs.forEach(input => {
                input.value = '';
            });
        });
    });
}