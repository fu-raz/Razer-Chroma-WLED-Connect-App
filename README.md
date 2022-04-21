# Razer Chroma WLED Connect App
We're all looking for the holy grail app that connects all our RGB devices together and syncs the colors between them. Or we just buy into a specific brand for their ecosystem. Sometimes brands do something really crazy and they open up their software to other manufacturers.

Razer is one of those companies that opened up their ecosystem to other brands.. well sort of. They announced Razer Connect and a list of companies that would be able to connect directly with Razer Synapse. Well to this day most of the brands on that list haven't released apps that connect to the Razer Synapse software.

I found this .NET wrapper for the Chroma Broadcast SDK here [Chroma Broadcast SDK .NET on GitHub](https://github.com/ChromaControl/ChromaBroadcastSDK.NET) with a sample application. To make it work, I needed an official Razer App Id. Which Razer wasn't going to give me. To use the app I made, you still need this Id and I can't provide you with one. Perhaps OpenChromaConnect still has them in their header files.

As a sort of proof-of-concept I wanted to connect Razer Synapse to my WLED addressable RGB LED strip. This app is the result.

## What do you need?

* A valid working Razer App Id. You'll probably end up using another brand's app id. Be aware that I don't know if you can use the same id multiple times. So to be safe, use one of a brand you're not actually using in Razer Synapse
* A WLED device with at least version 0.11.0. We use JSON commands over UDP, which was added in that version
* Visual Studio 2022 Community Edition (The free one) to open this solution, download the packages via NuGet and build it for your computer

## What does it do?

* Connect to Razer Synapse as a connected device. Razer sends 4 colors to the connected devices
* This app uses these colors to fill the led strip with these colors and sends these color in real-time to a WLED device
* You can set it to run on windows boot
* You can change the brightness of the strip

![wled-razer-chroma](https://user-images.githubusercontent.com/5355154/163828798-4d0ccfc2-5e74-451b-be52-a640c6376014.jpg)

![wled-razer-chroma-settings](https://user-images.githubusercontent.com/5355154/163828807-f4d44851-6e88-4eec-b30b-89f480123f93.jpg)

![razer-chroma-connect](https://user-images.githubusercontent.com/5355154/163829792-68effe51-7432-4366-a314-ee82a7ab7b64.jpg)

![razer-studio-connected](https://user-images.githubusercontent.com/5355154/163833292-43d16d60-6489-4506-a27b-0bf22bbb87ed.jpg)

## What's next?

Right now this is working for me and I don't need it to do anything else at the moment. But maybe you find this useful and have other ideas. Just let me know and I'll see what I can do. Or just fork this and send me a PR.

Could think of a few things.. like a better interface, more options for distribution of the lights. Maybe a websocket server so that we can hook more stuff up without having to write little apps like this for everything.

Would be really awesome if we can get Razer to give WLED an official Razer App Id so we can make this an official Razer Chroma Connect App thing. Then the possibilities would be endless.

## Copyright stuff
* Do whatever you want with this app. Would be awesome if you dropped a message if you're using it
* This solution uses some packages found on NuGet
* Icon: [Led lighting icons created by Smashicons - Flaticon](https://www.flaticon.com/free-icons/led-lighting "led lighting icons")
