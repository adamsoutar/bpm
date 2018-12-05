const chalk = require('chalk')
const path = require('path')
const fs = require('fs')
const logTypes = {
  INFO: chalk.blue,
  WARNING: chalk.yellow,
  ERROR: chalk.red
}

function debugLog (type, logStr) {
  var extraNote = (type === 'ERROR') ? ' - See bpmLog.txt for details' : ''
  console.log(`[${logTypes[type](type)}] ${logStr}${extraNote}`)
}

function logException (errStr) {
  fs.writeFile(path.join(process.cwd(), 'bpmLog.txt'), errStr, (err) => {
    if (err) debugLog('WARNING', "Could not write to error log file. That's ironic.")
  })
}

exports.say = debugLog
exports.err = logException
