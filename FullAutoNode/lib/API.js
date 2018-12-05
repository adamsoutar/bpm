const request = require('request')
class API {
  constructor (apiBase, apiVersion) {
    this.apiURL = `${apiBase}/api/v${apiVersion}/`
  }
  getJSON (subURL, callback) {
    request(`${this.apiURL}${subURL}`, (error, response, body) => {
      if (error) {
        callback(error, null)
      } else {
        callback(null, JSON.parse(body))
      }
    })
  }
}
module.exports = API
