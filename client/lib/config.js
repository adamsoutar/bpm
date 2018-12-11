const path = require('path')
const fs = require('fs')
const log = require('./log')
const configLocation = path.join(process.cwd(), 'bpm.json')
const pluginsLocation = path.join(process.cwd(), 'bpmPlugins.txt')

function getConfig () {
  if (fs.existsSync(configLocation)) {
    // bpm.json is... JSON
    return JSON.parse(fs.readFileSync(configLocation, 'utf8'))
  } else {
    log.say('ERROR', `Couldn't find bpm.json, making one up.`)
    log.say('WARNING', `Platform assumed as 'steam'`)
    return {
      platform: 'steam',
      installDir: process.cwd(),
      logIPA: false
    }
  }
}

function getPlugins () {
  if (fs.existsSync(pluginsLocation)) {
    // bpmPlugins.txt is split by newline
    var p = fs.readFileSync(pluginsLocation, 'utf8').split(/[\r\n]+/)
    p = p.filter((x) => x !== '')
    return p
  } else {
    log.say('WARNING', `Couldn't find bpmPlugins.txt, bpm will only update song-loader.`)
    // Why do we need song-loader? It contains IPA.
    return ['song-loader']
  }
}

module.exports.config = getConfig()
module.exports.plugins = getPlugins()

module.exports.save = () => {
  // Beautify using 2 space tabs. Easier for humans to edit.
  var configString = JSON.stringify(module.exports.config, null, 2)
  fs.writeFile(configLocation, configString, (err) => {
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
