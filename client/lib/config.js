const path = require('path')
const fs = require('fs')
const log = require('./log')
const childProcess = require('child_process')
const CONFIG_PATH = path.join(process.cwd(), 'bpm.json')
const PLUGINS_PATH = path.join(process.cwd(), 'bpmPlugins.txt')
const REQUIRED_PLUGINS = ['song-loader']

function guessPlatform (directory) {
  // TODO: bpm should be smarter at this
  let d = directory.toLowerCase()
  // All libraries include 'steamapps' in their path
  if (d.includes('steam')) return 'steam'
  return 'oculus'
}

function getConfig () {
  if (fs.existsSync(CONFIG_PATH)) {
    // bpm.json is... JSON
    return JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8'))
  } else {
    log.say('ERROR', `Couldn't find bpm.json, making one up.`)
    log.say('WARNING', `Platform assumed as 'steam'`)
    return {
      platform: guessPlatform(process.cwd()),
      installDir: process.cwd(),
      logIPA: false
    }
  }
}

function getPlugins () {
  if (fs.existsSync(PLUGINS_PATH)) {
    // bpmPlugins.txt is split by newline
    var p = fs.readFileSync(PLUGINS_PATH, 'utf8').split(/[\r\n]+/)
    p = p.filter((x) => x !== '')
    return REQUIRED_PLUGINS.concat(p)
  } else {
    log.say('WARNING', `Couldn't find bpmPlugins.txt, bpm will only update song-loader.`)
    // Why do we need song-loader? It contains IPA.
    return REQUIRED_PLUGINS
  }
}

module.exports.config = getConfig()
module.exports.plugins = getPlugins()

module.exports.save = () => {
  // Beautify using 2 space tabs. Easier for humans to edit.
  var configString = JSON.stringify(module.exports.config, null, 2)
  fs.writeFile(CONFIG_PATH, configString, (err) => {
    if (err) {
      log.say('ERROR', `Couldn't save bpm.json`)
      log.err(err)
    }
  })
}

module.exports.getInstallDir = () => {
  if (!module.exports.config.installDir) {
    module.exports.config.installDir = process.cwd()
    module.exports.save()
  }
  return module.exports.config.installDir
}

module.exports.getPlatform = () => {
  return module.exports.config.platform || 'steam'
}

module.exports.checkSetup = () => {
  // A symlink is required for the game to work. Is it there?
  const symLinkPath = path.join(module.exports.config.installDir, 'Game_Data')
  if (!fs.existsSync(symLinkPath)) {
    log.say('INFO', `Didn't find the Game_Data symlink, creating...`)
    try {
      childProcess.exec(`mklink /J "${symLinkPath}" "${path.join(module.exports.config.installDir, 'Beat Saber_Data')}"`)
      log.say('INFO', 'Symlink created!')
    } catch (ex) {
      log.say('ERROR', 'Failed to create symlink!')
      log.err(ex)
    }
  }
}
