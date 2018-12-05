const chalk = require('chalk')
var logo = [
  `   __br               `,
  `  / /  b  ___   r__ _ `,
  ` / _ \\b  / _ \\r /  ' \\`,
  `/_.__/b / .__\\r/_/_/_/`,
  `     b /_/r           `
]
exports.printLogo = () => {
  for (let l of logo) {
    var firstSplit = l.split('b')
    var secondSplit = firstSplit[1].split('r')
    var parts = [firstSplit[0], secondSplit[0], secondSplit[1]]
    console.log(chalk.red(parts[0]), chalk.blue(parts[1]), chalk.red(parts[2]))
  }
}
