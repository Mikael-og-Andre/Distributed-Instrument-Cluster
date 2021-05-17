# Distributed-Instrument-Cluster
This project is meant to create a system that can control any device regardles of DRM protection, and have them controlled from a web user interface with your mouse and keyboard. The system was designed to be used on maritime systems for the purposes of learning in a classrom environment where real data from remote instrument could provide value for students.
The use of hardware called a Crestron cabel allows us to send keyboard and mouse commands to the connected device even if we can not install any software on the machine itself.


## Development Env
Visual Studio 2019. All .NET Extensions and Database extensions installed.

## Blazor Server

### Launching Blazor Server

#### Visual Studio

1.  Download Visual Studio 2019
2.  Install all extensions related to .NET - ASP.NET, Blazor
3.  Install Database features
4.  Edit RemoteDevices in the Remote Devices.json
5.  Open the Distributed Insturment Cluster Solution File - .SLN
6.  Build
7.  Launch the project called Blazor_Instrument_Cluster.Server
8.  Connect to localhost 5001 in your browser (This might not be the same for you, you can change it in the launch settings or launch profile in visual studio).

#### Docker
If you want to use the docker image of the project, you can get it at https://hub.docker.com/repository/docker/zekael/distributed_instrument_cluster.
Using a docker container with HTTPS Requires you to import your local dev certificate into the contianer. Or alternatively you can use Let's Encrypt and certbot.
Dev certification doc: https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide
ASP.NET with https in docker: https://docs.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-5.0

!There is a bug with kestrel where it expects the name of the .PFX file to match the Assembly .DLL file name of the asp.net project.

### Remote Device Json
Remote Devices are defined on the ASP.NET Server and sent to all connecting clients when they go to the Remote Device selection page. Remote Devices stored and loaded from a Json file, where Ip, and other metadata is defined about the remote device, including what the video port for the remote device is. The ports of the video streams are incrementally generated from what we call the base port, if the base port is 8080, and there are 5 devices, the 5th device will be on port 8084, because there are 4 additional devices, $8080+4=8084$. If the device does not have a Crestron connection it can be defined here, and if it does have a Crestron the port must also be specified.

Example:
"RemoteDevices": [
    {
      "ip": "DnsLookupblabla.com",
      "name": "andre",
      "location": "location 1 andre",
      "type": "type 1 andre",
      "VideoDevices": 1,
      "VideoBasePort": 8080,
      "hasCrestron": true,
      "CrestronBasePort": 6981
    },
    {
      "ip": "IamAwebsite.com",
      "name": "andre",
      "location": "location 2 andre",
      "type": "type 2 andre",
      "VideoDevices": 1,
      "VideoBasePort": 8080,
      "hasCrestron": true,
      "CrestronBasePort": 6981
    }]}


## Remote Server

### Launching Remote Server

#### Visual Studio

1.  Download Visual Studio 2019
2.  Install all extensions related to .NET - ASP.NET, Blazor
3.  Install Database features
4.  Edit Config.json with port information, and video information, and crestron cable information
5.  Open the Distributed Insturment Cluster Solution File - .SLN
6.  Build as either release or debug
7.  Launch the project called Remote_Server from visual studio. or navigate to the folder and launch from the .exe file directly.
8.  Confirm that all usb connections were setup correctly from the command line- if not retry.

### Third Party Devices hardware
#### Crestron Cable
For the remote Server to control a device you need to connect a Crestron cable to a usb port on the remote server, and connect the other end to the device you wish to control.
The crestron cable is not required, you can specify that a device does not have a crestron cable on the ASP.NET server.
#### Video Devices
You can connect both video cameras, caputure cards, or other video devices to the system.

### Remote Server Config.json

The remote server requires you to specify how many video devices you have, and what port you want the incremntation to start at. If you define the videoPort to be 8080 like in the example, each video device will increment in port number from this port, so device number 2 will have port 8081. When launching the remote server you will get information about if the com port of the crestron is detected, in red text if there are any errors. The videoDevices can have their fps and quality Adjusted Individually in the json file.

{
  "serverSettings": {
    "ip": "0.0.0.0",
    "crestronPort": 6981,
    "videoPort": 8080
  },
  "crestronCable": {
    "portName": "com5",
    "largeMagnitude": 20,
    "smallMagnitude": 1,
    "maxDelta": 1000
  },
  "videoDevices": [
    {
      "deviceIndex": 0,
      "apiIndex": 700,
      "width": 1920,
      "height": 1080,
      "quality": 40,
      "fps": 30
    }
  ]
}
