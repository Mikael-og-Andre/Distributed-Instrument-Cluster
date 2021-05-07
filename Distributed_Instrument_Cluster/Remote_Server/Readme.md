#Remote Server



##Firewall Details
Firewall must allow ICMP for the other servers to recognise it as running
If on windows 10 run the command: netsh advfirewall firewall add rule name="ICMP Allow incoming V4 echo request" protocol=icmpv4:8,any dir=in action=allow
