function showLoading(buttonId, containerId) {
    var button = document.getElementById(buttonId);
    var container = document.getElementById(containerId);
    var overlay = document.createElement('div');
    overlay.classList.add('btn-overlay');
    var loader = document.createElement('div');
    loader.classList.add('loader');
    overlay.appendChild(loader);
    container.appendChild(overlay);

    button.disabled = true;
    setTimeout(function () {
        overlay.remove();
        button.disabled = false;
    }, 5000);
}