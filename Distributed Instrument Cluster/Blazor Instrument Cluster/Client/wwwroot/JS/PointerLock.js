var canvas;

//Setup function called when dom is loaded.
function setup() {
    canvas = document.getElementById("canvas");

    //Add pointer lock event listeners.
    document.addEventListener("pointerlockchange", lockChange, false);
    document.addEventListener("mozpointerlockchange", lockChange, false);
}


function click() {
    canvas.requestPointerLock();
}

function lockChange() {
    if (document.pointerLockElement === canvas || document.mozPointerLockElement === canvas) {
        document.addEventListener("mousemove", updatePosition, false);
    } else {
        document.removeEventListener("mousemove", updatePosition, false);
    }
}

var x = 50;
var y = 50;

function updatePosition(e) {
    x += e.movementX;
    y += e.movementY;

    DotNet.invokeMethodAsync("Blazor_Instrument_Cluster.Client", "updatePosition", x, y);
}