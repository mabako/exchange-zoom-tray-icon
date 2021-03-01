# Simple tray icon app to show join scheduled Zoom meetings

At the very core, this app does exactly the following things:

* Fetch upcoming calendar entries from your exchange/EWS server
* Extract meetings containing Zoom links
* Allows you to join either the next upcoming Zoom meeting when double-clicking 
  the tray icon
* Displays all upcoming Zoom meetings when right-clicking on the tray icon
* Displays "sticky" Zoom meetings, for example team rooms, for events 
  explicitly scheduled prior to 06:30

## Configuration

Initially, you need to set up your EWS configuration once. This requires you to
be able to connect to your EWS instance, either through a publicly accessible 
URL or through an established VPN connection.

### Exchange URL

There's (unfortunately) no auto-discovery for your EWS url implemented as of
right now - which means you must determine the EWS url (typically ending in 
`/EWS/Exchange.asmx`) manually.

* If you have access to Outlook web mail under an address like
  `https://mail.example.org/owa/`, the EWS url is typically under 
  `https://mail.example.org/EWS/Exchange.asmx`.
* Alternatively, testing the connection settings in Outlook shows the EWS
  url in the log - an example of how this might look can be found
  [here](https://support.neuxpower.com/hc/en-us/articles/202482832-Determining-the-Exchange-Web-Services-EWS-URL).

Optionally, you can open the EWS url in a browser, which should show a 
webservice description of some kind. You *might* need to login with user
name & password. 

### User name & Password
* If your computer is part of a domain, only the EWS url itself need to be
  specified.
* If you're remoting into your work computer from your personal computer, and
  run Zoom on your personal computer (such as I do), then you also need to
  specify a username (including a domain, possibly) and a password.

## Motivation

Outlook's user interface for joining Skype meetings was alright, since you could
do it from the client itself. Zoom, however, is not afforded the same level of 
integration, and thus you'd need to visit the website for every meeting and go
through all that nonsense - and this saves me a click or two for every single 
meeting.

This app was tested to be compatible with Exchange 2016, as that is essentially
(a) what my company uses, and (b) thus the only Exchange version I actively 
have to work with. There's no particular reason why it shouldn't work with 
either older or newer versions, since the used feature scope is very limited -
after all, it only queries for a handful properties in your calendar entries.
