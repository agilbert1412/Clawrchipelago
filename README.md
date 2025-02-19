# Clawrchipelago
Archipelago Randomizer Implementation for Dungeon Clawler

# Install Instructions

1 - Navigate to your Dungeon Clawler install location. If you have the game through steam, it will generally be in `C:\Program Files (x86)\Steam\steamapps\common\Dungeon Clawler`. Make a copy of the game folder somewhere else on your computer. This step is optional, but will allow you to keep your vanilla game clean and playable. If you mod it directly, you will be unable to play vanilla without re-installing.

## A: First Time Install, Easy version

  2 - Download the latest file `Clawrchipelago Full x.x.x.zip` from [The Releases Page](https://github.com/agilbert1412/Clawrchipelago/releases)

  3 - Extract the content of the zip in your game folder, in the `Windows` folder. The mod files should live side by side with the game executable

![image](https://i.imgur.com/kAnCuNv.png)

  4 - Edit the file `ArchipelagoConnectionInfo.json` to set your desired connection information

  5 - Launch the modded game! If errors occured, they'll be displayed in the console, and be saved to the file `\BepInEx\LogOutput.log`

## B: If you already have the mod installed and just wish to update it

2: Download the latest file `Clawrchipelago Plugin x.x.x.zip` [The Releases Page](https://github.com/agilbert1412/Clawrchipelago/releases)

3: Navigate to `\BepInEx\plugins\` and overwrite the folder `Clawrchipelago` in there with the one from the downloaded zip file

4: This will maintain all your local configs and should cause no disruption. You are now ready to play!

## C: Manual Install

2: Download the latest release of [BepInEx](https://github.com/BepInEx/BepInEx/releases/latest). Pick the file that corresponds to your operating system

3: Extract it in your game folder, as seen on step A2

4: Launch the modded game once. This will allow BepInEx to generate critical files, like its own config and `\plugins` folder

5: Download the latest file `Clawrchipelago Plugin x.x.x.zip` [The Releases Page](https://github.com/agilbert1412/Clawrchipelago/releases)

6: Extract it as its own folder in `\BepInEx\plugins\`, usually naming the folder `Clawrchipelago`

7: Launch the modded game again. This will load Clawrchipelago and allow it to generate its own proper config files

8: Go to step A4
	
# How to host a multiworld game

Follow the guide on the [Archipelago Website](https://archipelago.gg/tutorial/Archipelago/setup/en), and use the `apworld` file available on [The Releases Page](https://github.com/agilbert1412/Clawrchipelago/releases)