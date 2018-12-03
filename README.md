![FullAuto](http://overflo.me/random/FullAutoLogo.png)

FullAuto is an app that runs when you start BeatSaber, and keeps all of your mods up to date without you having to think about it. It works entirely independently, as a replacement for [BeatSaberModInstaller](https://github.com/Umbranoxio/BeatSaberModInstaller).

## Installation

Download the latest release .zip file from the [releases page](https://github.com/Adybo123/BeatSaberFullAuto/releases), and unzip it. Run "FullAutoInstaller.exe", and press one button to install the loader. 

## Adding mods

Want to add a mod to be maintained by FullAuto? No problem. Head to [ModSaber.org](https://www.modsaber.org/), and find the short name for the mod. For example, for 'Song Loader Plugin', it's 'song-loader' (listed before the @ symbol in the version column). Then, add it to the packages section of the fullAuto.json file in your Beat Saber folder.

**E.G**, My fullAuto.json is at ```C:\Program Files (x86)\Steam\steamapps\common\Beat Saber\fullAuto.json```, and I want to install Camera Plus. I just change the line

```json
"packages": ["song-loader","scoresaber"],
```

to

```json
"packages": ["song-loader","scoresaber","camera-plus"],
```

## Platform support

The installer will take its best guess at whether you're using the Steam or Oculus store version of the game, but if it's wrong you can use the ```platform``` field in fullAuto.json to pick a custom ModSaber platform, E.G ```steam``` or ```oculus```. Please note that some mods are not available on all platforms.


### Thanks

This project relys on the [ModSaber](https://github.com/lolPants/ModSaber) API by [lolPants](https://github.com/lolPants). It is heavily inspired by [BeatSaberModInstaller](https://github.com/Umbranoxio/BeatSaberModInstaller) by [Umbranoxio](https://github.com/Umbranoxio)
