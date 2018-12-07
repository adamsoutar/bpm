const request = require('request')
const autoupdate = require('./autoupdate')
class API {
  constructor (apiBase, apiVersion) {
    this.apiURL = `${apiBase}/api/v${apiVersion}/`
  }
  getJSON (subURL, callback) {
    request({
      url: `${this.apiURL}${subURL}`,
      headers: {
        'User-Agent': `bpm-checker / ${autoupdate.version}`
      }
    }, (error, response, body) => {
      if (error) {
        callback(error, null)
      } else {
        callback(null, JSON.parse(body))
      }
    })
  }
}
module.exports = API
