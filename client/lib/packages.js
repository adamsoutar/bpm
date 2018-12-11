const config = require('./config')
const fs = require('fs')
const path = require('path')
const JSZip = require('jszip')
const request = require('request')
const log = require('./log')

exports.downloadAndExtract = (zipURL, callback) => {
  request({
    method: 'GET',
    url: zipURL,
    encoding: null // <- this one is important !
  }, function (error, response, body) {
    if (error || response.statusCode !== 200) {
      log.say('ERROR', 'Could not download package')
      log.err(error)
      // Don't return without calling the callback!
      callback(error)
      return
    }
    JSZip.loadAsync(body).then(function (zip) {
      const extractBasePath = config.getInstallDir()
      var filesInZip = 0
      var filesExtracted = 0
      var extractionError = false

      function fileExtracted (exErr) {
        filesExtracted++
        extractionError = exErr || extractionError
        if (filesExtracted === filesInZip) callback(extractionError)
      }

      // TODO: Better method than double-looping
      zip.forEach((relativePath, zipEntry) => {
        filesInZip++
      })

      // Loop and extract
      zip.forEach((relativePath, zipEntry) => {
        // Is this a JSZip folder entry?
        if (relativePath.slice(-1) === '/') {
          // Ignore folder entries
          fileExtracted()
          return
        }

        const dir = path.join(extractBasePath, path.dirname(relativePath))
        const fl = path.join(extractBasePath, relativePath)
        // Do we need to create a folder?
        var dirFunction = fs.existsSync(dir) ? (a, b, c) => { c(null) } : fs.mkdir
        dirFunction(dir, { recursive: true }, (err) => {
          if (err) {
            log.say('ERROR', 'Could not create directory when extracting package')
            log.err(err)
            // Don't return, there's a chance the error was a dir exists already
          }
          zipEntry.async('uint8array').then((contents) => {
            fs.writeFile(fl, contents, (err) => {
              fileExtracted()
              if (err) {
                log.say('ERROR', 'Could not read zip file entry in package')
                log.err(err)
                // Help, I'm drowning in callbacks!
              }
            })
          })
        })
      })
    })
  })
}
