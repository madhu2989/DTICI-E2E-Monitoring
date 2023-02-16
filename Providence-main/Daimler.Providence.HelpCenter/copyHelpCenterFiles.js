; (function () {
  'use strict';
  /**
   * Modules
   */
  const fs = require('fs'),
  path = require('path'),
  argv = require('minimist')(process.argv.slice(2)),
  rimraf = require('rimraf');

  let argvInputDir = argv['input-path'],
  argvOutputDir = argv['output-path'],
  argvAssetsPath = argv['assets-path'];
  
  console.log('InputPath: ' + argvInputDir);
  console.log('OutputPath: ' + argvOutputDir);
  
  
  rimraf.sync(argvOutputDir);
  fs.mkdirSync(argvOutputDir);

    
  function copyRecursiveSync(src, dest) {
    var exists = fs.existsSync(src);   
    var stats = exists && fs.statSync(src);
    var isDirectory = exists && stats.isDirectory();
    if (exists && isDirectory) {
      fs.mkdirSync(dest);
      fs.readdirSync(src).forEach(function(childItemName) {
        copyRecursiveSync(path.join(src, childItemName),
                          path.join(dest, childItemName));
      });
    } else {
      fs.linkSync(src, dest);
    }
  };


  


  copyRecursiveSync(path.join(argvInputDir, 'Help-Center'), path.join(argvOutputDir, 'Help-Center'));
  copyRecursiveSync(argvAssetsPath, path.join(argvOutputDir, '.attachments'));
  fs.createReadStream(path.join(argvInputDir, 'Help-Center.md')).pipe(fs.createWriteStream(path.join(argvOutputDir, 'Help-Center.md')));

})();