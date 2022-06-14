# Razer Chroma WLED (and Lenovo Legion 5 Pro keyboard) Connect App
We're all looking for the holy grail app that connects all our RGB devices together and syncs the colors between them. Or we just buy into a specific brand for their ecosystem. Sometimes brands do something really crazy and they open up their software to other manufacturers.

Razer is one of those companies that opened up their ecosystem to other brands.. well sort of. They announced Razer Connect and a list of companies that would be able to connect directly with Razer Synapse. Well to this day most of the brands on that list haven't released apps that connect to the Razer Synapse software.

I found this .NET wrapper for the Chroma Broadcast SDK here [Chroma Broadcast SDK .NET on GitHub](https://github.com/ChromaControl/ChromaBroadcastSDK.NET) with a sample application. To make it work, I needed an official Razer App Id. Which Razer wasn't going to give me. To use the app I made, you still need this Id and I can't provide you with one. Perhaps OpenChromaConnect still has them in their header files.

As a sort of proof-of-concept I wanted to connect Razer Synapse to my WLED addressable RGB LED strip. This app is the result. Oh and I've added support for the Lenovo Legion 5 Pro RGB keyboard (the one with 4 LED segments from 2020 or 20201). Just for fun

## What do you need?

* A valid working Razer App Id. You'll probably end up using another brand's app id. Be aware that I don't know if you can use the same id multiple times. So to be safe, use one of a brand you're not actually using in Razer Synapse. I can’t provide you with a working one, but maybe you can borrow one from the OpenChromaConnect ChromaBroadcastAPI (inc/RzChromaBroadcastAPIDefines.h) header file (use google).
* A WLED device with at least version 0.11.0. We use JSON commands over UDP, which was added in that version
* Either:
  * Visual Studio 2022 Community Edition (The free one) to open this solution, download the packages via NuGet and build it for your computer
  * Install the required packages with NuGet
  * Don't forget to do this for the 32 and 64bit file:

  ![50458730-72e65780-0976-11e9-9d46-1d4874083586](https://user-images.githubusercontent.com/5355154/168314251-287d0484-bfd3-491e-b4a2-8645e1bf16f0.png)
* Or:
  * Download the release from the release tab


## What does it do?

* Connect to Razer Synapse as a connected device. Razer sends 4 colors to the connected devices
* The app can find WLED installations via mDNS and automatically add/update them
* This app uses the 4 Razer colors and sends them in real-time to (multiple) WLED devices
  * You can change the brightness per strip
  * You can sync, 1, 2, 3 or all 4 colors per strip or even per segment
  * Colors can blend or not
* You can set it to run on windows boot


![wled-razer-chroma](https://user-images.githubusercontent.com/5355154/164540937-87e77325-7673-4265-a8f1-117fd02ff635.jpg)

![razer-settings-preview](https://user-images.githubusercontent.com/5355154/166119165-9f2214ef-cb97-4236-befc-ec57644450c8.jpg)

![razer-chroma-connect](https://user-images.githubusercontent.com/5355154/163829792-68effe51-7432-4366-a314-ee82a7ab7b64.jpg)

## What's next?
Right now this is working for me and I don't need it to do anything else at the moment. But maybe you find this useful and have other ideas. Just let me know and I'll see what I can do. Or just fork this and send me a PR.

Could think of a few things.. like a better interface, more options for distribution of the lights. Maybe a websocket server so that we can hook more stuff up without having to write little apps like this for everything.

Would be really awesome if we can get Razer to give WLED an official Razer App Id so we can make this an official Razer Chroma Connect App thing. Then the possibilities would be endless.

## Copyright stuff
* Do whatever you want with this app. Would be awesome if you dropped a message if you're using it
* This solution uses some packages found on NuGet
* Icon: [Led lighting icons created by Smashicons - Flaticon](https://www.flaticon.com/free-icons/led-lighting "led lighting icons")

## Thanks
* Thanks Jinx for testing the app
* Aircoookie for WLED and help in the discourse group
* Blazoncek for help pushing me down the UDP rabbit hole
* nvchernov for HID API Adapter and accepting my pull request [nvchernov/hidapiadapter](https://github.com/nvchernov/hidapiadapter)
