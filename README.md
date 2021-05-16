# Distributed-Instrument-Cluster
Virtual lab enviornment



## Blazor Server

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


