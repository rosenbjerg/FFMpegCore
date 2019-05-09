const fs = require('fs');
const path = require('path');

const CONFIG_PATH = path.resolve('./FFMpegCore.Test/ffmpeg.config.json');

console.log('--- Configuring ffmpeg binary path for:', process.env.TRAVIS_OS_NAME);

let data = require(CONFIG_PATH);

data.RootDirectory = process.env.TRAVIS_OS_NAME === 'linux' ?
    '/usr/bin' :
    '/usr/local/bin';

fs.writeFileSync(CONFIG_PATH, JSON.stringify(data, null, 4));
