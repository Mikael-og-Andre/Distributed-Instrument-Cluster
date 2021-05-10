# Remote Server #



## Firewall Details ##
Firewall must allow ICMP for the other servers to recognise it as running
If on windows 10 run the command: netsh advfirewall firewall add rule name="ICMP Allow incoming V4 echo request" protocol=icmpv4:8,any dir=in action=allow


## Config ##

Example with comments:

{
  //Server settings for remote server.
  "serverSettings": {
    "ip": "0.0.0.0",
    "crestronPort": 6981,   //Port crestron traffic will go on.
    "videoPort": 8080       //Port for video traffic (if more than 1 video device is specified 2nd video device will use port number +1 e.g. 8081 and 3rd 8082).
  },
  //Settings for crestron cable.
  "crestronCable": {
    //What serial port the crestron cable is connected to.
    "portName": "com5",
    //Movement delta required to ingade large magnitude movements on crestron cable.
    "largeMagnitude": 20,
    //Movement delta required to ingade small magnitude movements on crestron cable.
    "smallMagnitude": 1,
    //Max accumulated mouse movement before movement algorithm starts rejecting additional movement to prevent lag.
    "maxDelta": 1000
  },
  //List of video devices
  "videoDevices": [
    {
      //Index of video device (typicaly on laptops with webcams, webcam is index 0 and any video device connected will be 1).
      "deviceIndex": 0,
      //700=dshow (windows video device api), see openCV docks to find api idexes: https://docs.opencv.org/3.4/d4/d15/group__videoio__flags__base.html.
      "apiIndex": 700,
      //Device video resolution (number higher than what the device is capable of outputting will be reduce the resolution to match that of the device).
      "width": 1920,
      "height": 1080,
      //Quality of jpeg compression, higher=better image and larger file.
      "quality": 40,
      //Frame rate of video device (number higher than what the device is capable of outputting will be reduced to match the max frame rate of the device).
      "fps": 30
    },
    //Example of a 2nd video device.
    {
      "deviceIndex": 1,
      "apiIndex": 700,
      "width": 1920,
      "height": 1080,
      "quality": 20,
      "fps": 15
    }
  ]
}