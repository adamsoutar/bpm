// Interface with Beat Saber and IPA using this file

const childProcess = require('child_process')
const config = require('./config')
const path = require('path')
const installDir = config.getInstallDir()
const log = require('./log')
const IPAPath = path.join(installDir, 'IPA.exe')
const GamePath = path.join(installDir, 'Game.exe')

exports.IPAPatch = (callback) => {
  log.say('INFO', `Spawning ${IPAPath}`)
  try {
    const IPA = childProcess.spawn(IPAPath, [GamePath])

    IPA.stderr.on('data', (data) => {
      log.say('ERROR', `[IPA] ${data}`)
    })

    if (config.config.logIPA) {
      IPA.stdout.on('data', (data) => {
        log.say('INFO', `[IPA] ${data}`)
      })
    }

    IPA.on('close', (exitCode) => {
      log.say('INFO', `IPA finished with exit code ${exitCode}`)
      if (exitCode !== 0) log.say('WARNING', 'IPA exited with non-zero exit code. Game might not be patched.')
      callback()
    })
  } catch (err) {
    log.say('ERROR', 'Failed to spawn IPA')
    log.err(err)
    callback()
  }
}

exports.startBeatSaber = (args) => {
  log.say('INFO', 'Thanks for using bpm! - deeBo')
  log.say('INFO', 'Starting Beat Saber...')
  // Pass args on to Beat Saber
  var argString = ''
  if (args) {
    argString = (args.length > 0) ? args.join(' ') : ''
    log.say('INFO', `Launch args string:${argString}`)
  }
  childProcess.exec(`"${GamePath}" ${argString}`)
}
