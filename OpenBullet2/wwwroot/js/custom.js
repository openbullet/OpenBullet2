function adjustTextAreas() {
    var areas = document.querySelectorAll('.debugger-log textarea');
    for (var i = 0; i < areas.length; i++) {
        areas[i].style.height = "1px";
        areas[i].style.height = (12 + areas[i].scrollHeight) + "px";
        console.log("RESIZED");
    }
}

function debuggerScrollToBottom() {
    var elements = document.getElementsByClassName("debugger-log");
    for (var i = 0; i < elements.length; i++) {
        elements[i].scrollTop = elements[i].scrollHeight;
    }
}

function setSidebarWidth(width) {
    var element = document.getElementsByClassName("sidebar")[0];
    element.style.width = width;
}

function setSidebarMargin(margin) {
    var element = document.getElementsByClassName("sidebar")[0];
    element.style.marginLeft = margin;
}