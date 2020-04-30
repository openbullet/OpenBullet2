function adjustTextAreas() {
    var areas = document.querySelectorAll('textarea.debugger-entry');
    for (var i = 0; i < areas.length; i++) {
        areas[i].style.height = "1px";
        areas[i].style.height = (25 + areas[i].scrollHeight) + "px";
        console.log("RESIZED");
    }
}