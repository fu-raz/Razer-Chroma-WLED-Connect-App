# Razer Chroma WLED Connect App
We're all looking for the holy grail app that connects all our RGB devices together and syncs the colors between them. Or we just buy into a specific brand for their ecosystem. Sometimes brands do something really crazy and they open up their software to other manufacturers.

Razer is one of those companies that opened up their ecosystem to other brands.. well sort of. They announced Razer Connect and a list of companies that would be able to connect directly with Razer Synapse. Well to this day most of the brands on that list haven't released apps that connect to the Razer Synapse software.

I found this .NET wrapper for the Chroma Broadcast SDK here https://github.com/ChromaControl/ChromaBroadcastSDK.NET with a sample application. To make it work, I needed an official Razer App Id. Which Razer wasn't going to give me. To use the app I made, you still need this Id and I can't provide you with one. Perhaps OpenChromaConnect still has them in their header files.

As a sort of proof-of-concept I wanted to connect Razer Synapse to my WLED addressable RGB LED strip. This app is the result.
