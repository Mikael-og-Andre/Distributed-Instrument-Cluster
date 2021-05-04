var canvas;
var locked = false;

//Setup function called when dom is loaded.
function setup() {
    canvas = document.getElementById("canvas");

    //Add pointer lock event listeners.
    document.addEventListener("pointerlockchange", lockChange, false);
    document.addEventListener("mozpointerlockchange", lockChange, false);
}


function click() {
    canvas.requestPointerLock();
    return isLocked();
}

function isLocked() {
    return locked;
}

function lockChange() {
    if (document.pointerLockElement === canvas || document.mozPointerLockElement === canvas) {
        document.addEventListener("mousemove", updatePosition, false);
        locked = true;
    } else {
        document.removeEventListener("mousemove", updatePosition, false);
        locked = false;
    }
}


var x = 0;
var y = 0;
function updatePosition(e) {
    x += e.movementX;
    y += e.movementY;
}

function getPositionChange() {
    var tempX = x;
    var tempY = y;

    x = 0;
    y = 0;
    
    return [tempX, tempY];
}