function copyToClipboard(text) {
    var tempInput = document.createElement("input");
    tempInput.style = "position: absolute; left: -1000px; top: -1000px";
    tempInput.value = text;
    document.body.appendChild(tempInput);
    tempInput.select();
    document.execCommand("copy");
    document.body.removeChild(tempInput);
    alert("Link copied to clipboard: " + text);
}
function OpenPopup() {

    window.open("CreateEventView.aspx", "List", "toolbar=no, location=no,status=yes,menubar=no,scrollbars=yes,resizable=no, width=900,height=500,left=430,top=100");
    return false;
}