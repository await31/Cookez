var darkmodeactive = localStorage.getItem("darkmode");

window.addEventListener('DOMContentLoaded', function () {
    if (!darkmodeactive || darkmodeactive === "") {
        localStorage.setItem('darkmode', false);
    }
});


function labelLight() {
    $(".toggle-switch").attr("alt", "Go dark");
    $(".toggle-switch").attr("title", "Go dark");
}
function goLight() {
    labelLight();
    $("body").removeClass("dark");
}
function stayLight() {
    goLight();
    localStorage.setItem("darkmode", false);
    darkmodeactive = localStorage.getItem("darkmode");
}

function labelDark() {
    $(".toggle-switch").attr("alt", "Go light");
    $(".toggle-switch").attr("title", "Go light");
}
function goDark() {
    labelDark();
    $("body").addClass("dark");
    $("table").removeClass("table-striped");
}
function stayDark() {
    goDark();
    localStorage.setItem("darkmode", true);
    darkmodeactive = localStorage.getItem("darkmode");
}
window.matchMedia("(prefers-color-scheme: light)").addListener(e => e.matches && stayLight());
window.matchMedia("(prefers-color-scheme: dark)").addListener(e => e.matches && stayDark());

$(".toggle-switch").click(function () {
    if ($("body").hasClass("dark")) {
        stayLight();
    } else {
        stayDark();
    }
});
$(".label-light").click(function () {
    if ($("body").hasClass("dark")) {
        stayLight();
    }
});
$(".label-dark").click(function () {
    if (!$("body").hasClass("dark")) {
        stayDark();
    }
});
$(document).ready(function () {
    if (localStorage.darkmode == "true") {
        goDark();
    } else if (localStorage.darkmode == "false") {
        goLight();
    } else {
        if ($("body").hasClass("dark")) {
            labelDark();
        } else {
            labelLight();
        }
    }
});

// disAnime sau 15s
function tempDisableAnim() {
    $("*").addClass("disableEasingTemporarily");
    setTimeout(function () {
        $("*").removeClass("disableEasingTemporarily");
    }, 2000);
}
// disLoadFlash sau 15s
setTimeout(function () {
    $(".load-flash").css("display", "none");
    $(".load-flash").css("visibility", "hidden");
    tempDisableAnim();
}, 2000);
// resize window thi disAnime sau 15s lien tuc
$(window).resize(function () {
    tempDisableAnim();
    setTimeout(function () {
        tempDisableAnim();
    }, 0);
});

