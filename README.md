![bpm banner](http://overflo.me/random/bpmbanner.png)

bpm is an app that runs when you start BeatSaber, and keeps all of your mods up to date without you having to think about it. It works entirely independently, as a replacement for [BeatSaberModInstaller](https://github.com/Umbranoxio/BeatSaberModInstaller).

[![build status](https://travis-ci.com/Adybo123/bpm.svg?branch=master)](https://travis-ci.com/Adybo123/bpm)
[![Known Vulnerabilities](https://snyk.io/test/github/Adybo123/bpm/badge.svg?targetFile=client%2Fpackage.json)](https://snyk.io/test/github/Adybo123/bpm?targetFile=client%2Fpackage.json)
[![JavaScript Style Guide](https://img.shields.io/badge/code_style-standard-brightgreen.svg)](https://standardjs.com)

## Installation

Download the latest release .zip file from the [releases page](https://github.com/Adybo123/BeatSaberFullAuto/releases) and unzip it. Run ```bpmInstaller.exe``` and press one button to install the loader. 

**To uninstall,** run the installer program again and press the uninstall button in the bottom left.

## Adding mods

Want to add a mod to be maintained by bpm? No problem. Head to [ModSaber.org](https://www.modsaber.org/) and pick a mod, you can use the long name (Song Loader Plugin) *or* the short name (song-loader), and paste its name into bpmPlugins.txt in your Beat Saber folder.

**E.G**, Add Camera Plus to ```steamapps\common\Beat Saber\bpmPlugins.txt``` to install & maintain it with bpm

On a new line like this:
```
Song Loader Plugin
scoresaber
Camera Plus
```

## Platform support

The installer will take its best guess at whether you're using the Steam or Oculus store version of the game, but if it's wrong you can use the ```platform``` field in bpm.json to pick a custom ModSaber platform, E.G ```steam``` or ```oculus```. Please note that some mods are not available on all platforms.


### Thanks

This project relys on the [ModSaber](https://github.com/lolPants/ModSaber) API by [lolPants](https://github.com/lolPants). It is heavily inspired by [BeatSaberModInstaller](https://github.com/Umbranoxio/BeatSaberModInstaller) by [Umbranoxio](https://github.com/Umbranoxio)
