# The CUBE
A VR application for building intuition for the electromagnetic fields of point charges. This is the version of the application as described in "The CUBE Virtual Reality Immersion" by Laura Estridge and Joel Franklin. To find the most recent version with new and/or experimental features, see [this repository](https://github.com/ReedPhysicsVR/TheCUBE-Experimental).

## To install and run the application on a Meta Quest headset:
Required materials/software:
- [Meta Horizon developer account](https://developers.meta.com/horizon/sign-up/)
- Meta Quest headset
- Mobile device with the Meta Horizon app
- A computer with [Meta Quest Developer Hub](https://developers.meta.com/horizon/downloads/package/oculus-developer-hub-win/) (MQDH)
- [TheCUBE.apk](TheCUBE.apk)
- USB-C to USB/USB-C cable (depending on if your machine has a USB-C port or not)

Make sure the primary account on the headset is the developer account and is the same account logged in on the Meta Horizons app. If the primary account is not a developer account, either [register it as a developer account](https://developers.meta.com/horizon/sign-up/) or factory reset the headset in order to set the new primary account. While the headset is connected to the Horizons app on the mobile device, navigate to 'Headset Settings' in the Horizons app and turn on 'Developer Mode'.

Connect the headset to the computer with the USB-C cable. Open Meta Quest Developer Hub and log in with the same developer account acting as the primary account on the headset. Navigate to the 'Device Manager' window. Locate TheCUBE.apk in the computer's file directory, drag it into the MQDH window, and drop in the area showing the headset when it appears. 

In the headset, open the app library. The application will only be listed in the "Unknown Source" section. Select it and you should be good to go!

## How to use the application:
This application uses the conventional controller mapping for VR games.
- **To move the player without moving in the real world**, use the left joystick to move the player in space and use the right joystick to turn the camera view. Or teleport by pointing at the ground and pressing forward with the right joystick, then releasing when pointed at the desired teleportation location. Teleportation is a better option for those prone to motion sickness.
- **To interact with the point charge (sphere)**, point a controller at the charge. When the controller vibrates slightly and the cursor turns blue, press and hold the grip (side) button on the selecting controller. While holding the grip button, moving the controller will also move the charge, and pressing the joystick forward/backward will push/pull the charge away from/towards the user. Pressing the B button on the controller will teleport the charge back to its origin position if the charge is moving too quickly or it escaped the CUBE somehow. The 'Retrieve Point' button on the menu has the same effect.
- **To interact with the menu**, point at the menu with the right controller. When the desired option is highlighted, press the trigger button on the right controller to select it. Pressing the A or X buttons on the controllers will toggle hiding/showing the menu.
- **To change the color palette**, press the Y button.

## To open the project files in Unity:
Download and install the [Unity Hub](https://unity.com/download). After launching the Hub, install the Unity editor version 2022.3.46f1, and make sure to include Android as a build platform. Download [TheCUBE.zip](TheCUBE.zip) and extract the files. In the "Projects" tab of the Hub, click the "Add" dropdown and select "Add project from disk". Select the folder containing the extracted project files. The required packages should download and the project should open. 

For a guide to the project structure and VR development in Unity, see the [experimental CUBE repository](https://github.com/ReedPhysicsVR/TheCUBE-Experimental).
