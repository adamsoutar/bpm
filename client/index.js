/*
  Beatsaber Package Manager - BPM
  Originally by deeBo
  MIT License
*/
// Note: Relys on song-loader containing IPA

const apiBase = 'https://www.modsaber.org'
const apiVersion = '1.1'
const log = require('./lib/log')
const API = require('./lib/API')
const ModSaber = new API(apiBase, apiVersion)
const config = require('./lib/config')
const packages = require('./lib/packages.js')
const gameLib = require('./lib/game')
const logo = require('./lib/logo')
const path = require('path')
const fs = require('fs')
const autoupdate = require('./lib/autoupdate')

var modPages = 1
var pageNumber = 0
var modsUpdated = false
var thisPage = {}
var uncheckedPlugins = config.plugins

function lastVersion (pluginName) {
  if (config.config.versions) {
    return config.config.versions[pluginName] || 0
  } else return 0
}

function installPlugin (plugin, callback) {
  modsUpdated = true
  log.say('INFO', `Updating ${plugin.name} to ${plugin.version}...`)
  packages.downloadAndExtract(plugin.files[config.getPlatform()].url, (err) => {
    if (err) {
      log.say('WARNING', 'Plugin installer detected a failure in the package engine. Continuing.')
    } else {
      // Successfully updated, tell bpm.json
      if (!config.config.versions) config.config.versions = {}
      config.config.versions[plugin.name] = plugin.version
      config.save()
    }
    callback()
  })
}

// These handle functions are my solution to synchronous loops using async functions
function handleMod (modNumber) {
  if (modNumber === thisPage.mods.length) {
    doneWithPage()
    return
  }
  var plugin = thisPage.mods[modNumber]
  // ALlows you to use 'song-loader' or 'Song Loader Plugin'
  if (
    uncheckedPlugins.includes(plugin.name) ||
    uncheckedPlugins.includes(plugin.details.title)
  ) {
    uncheckedPlugins = uncheckedPlugins.filter((x) => (x !== plugin.name && x !== plugin.details.title))
    // Do we need to update this one?
    let lastModVer = lastVersion(plugin.name)
    if (plugin.version !== lastModVer) {
      installPlugin(plugin, () => {
        handleMod(modNumber + 1)
      })
      return
    } else log.say('INFO', `${plugin.name} is up to date.`)
  }
  handleMod(modNumber + 1)
}
function doneWithPage () {
  var doneWithPages = (
    uncheckedPlugins.length === 0 ||
    pageNumber === modPages ||
    pageNumber > modPages
  )
  if (doneWithPages) {
    updatesDone()
  } else {
    pageNumber++
    handleModPage()
  }
}
function handleModPage () {
  ModSaber.getJSON(`mods/approved/newest-by-gameversion/${pageNumber}`, (err, pageData) => {
    if (err) {
      log.say('ERROR', `Couldn't get mod listings from ModSaber`)
      log.err(err)
    }
    log.say('INFO', `Got page ${pageNumber + 1} of mods from ModSaber`)

    thisPage = pageData
    if (pageNumber === 0) modPages = thisPage.lastPage

    handleMod(0)
  })
}

function updatesDone () {
  if (modsUpdated) {
    // Run IPA
    log.say('INFO', 'Repatching game with IPA...')
    gameLib.IPAPatch(() => {
      gameLib.startBeatSaber()
    })
  } else {
    gameLib.startBeatSaber()
  }
}

// Are we performing an auto-update?
var updateMode = false
process.argv.forEach((val, index) => {
  if (val === '--update') {
    updateMode = true
  }
})

if (config.config !== null) {
  logo.printLogo()

  // bpm auto-update code
  if (updateMode) {
    const updateExePath = path.join(config.getInstallDir(), 'bpmUpdate.exe')
    const bpmExePath = path.join(config.getInstallDir(), 'Beat Saber.exe')
    fs.copyFile(updateExePath, bpmExePath, (err) => {
      if (err) {
        log.say('ERROR', 'Failed to copy updated bpm')
        log.err(err)
        return
      }
      log.say('INFO', 'bpm has finished updating successfully!')
    })
  } else {
    autoupdate.checkForUpdates((updated) => {
      if (!updated) {
        log.say('INFO', `Checking ${apiBase} for plugin updates...`)
        handleModPage(0)
      }
    })
  }
}
