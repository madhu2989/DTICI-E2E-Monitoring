; (function () {
  'use strict';
  /**
   * Modules
   */

  const fs = require('fs'),
    path = require('path'),
    marked = require('./node_modules/marked/marked.min.js'),
    markdownpdf = require("markdown-pdf"),
    argv = require('minimist')(process.argv.slice(2)),
    rimraf = require('rimraf');

  let argvDir = argv['input-path'],
    argvOutputDir = argv['output-path'],
    argvImageDir = argv['image-path'],
    imgDir = argvOutputDir + '/assets',
    imgSrcPath = 'assets/help/assets',
    navigationContent = '',
    helpRoutes = [];

    var pdfOptions = {
      remarkable: {
          html: true,
          breaks: true,
          
      },
      paperFormat: 'A4',
      tables: true,
      cssPath: "helpcenter_pdf.css"
    }

  marked.setOptions({
    gfm: true,
    tables: true,
    breaks: true,
    smartLists: true,
    xhtml: true
  });


  var pdfContent = {};
  var pdfOrder = [];


  //first the old directory is removed, the create basic folder strukture again
  rimraf.sync(argvOutputDir);
  fs.mkdirSync(argvOutputDir);
  fs.mkdirSync(imgDir);

  console.log('readDirAndCreateHelpCenter:' + argvOutputDir);
  readDirAndCreateHelpCenter(argvDir, argvOutputDir, '', '');

  console.log('converting md to pdf');
  convertMdToPdf(pdfContent, argvOutputDir);

  console.log('writing HTML template');
  writeHtmlTemplate('navigation.component', navigationContent, argvOutputDir);
  
  console.log('writing AppRouting module');
  writeAppRoutingModule(helpRoutes, argvOutputDir);

  function writeAppRoutingModule(routingArray, outputDir) {
    let file,
      appRoutingModule = "import { NgModule } from '@angular/core';\r\nimport { Routes, RouterModule } from '@angular/router';\r\nimport { HelpCenterContentComponent } from '../../app/help-center-content/help-center-content.component';\r\n" +
        "import { LoggedInGuard } from '../../app/shared/logged-in.guard';\r\nimport { LoginComponent } from '../../app/shared/login/login.component';\r\nimport { LogoutComponent } from '../../app/shared/logout/logout.component';\r\n\r\n" +
        "const routes: Routes = [\r\n  { path: '', component: HelpCenterContentComponent, data: { path: '/help-center' }, canActivate: [LoggedInGuard]},\r\n{ path: 'help', component: HelpCenterContentComponent, data: { path: '/help-center' }, canActivate: [LoggedInGuard]}, \r\n { path: 'wwwroot/help', component: HelpCenterContentComponent, data: { path: '/help-center' }, canActivate: [LoggedInGuard]}, \r\n";

    for (let i = 0; i < routingArray.length; i++) {
      
      appRoutingModule += "  { path: '" + routingArray[i].toLowerCase() + "', component: HelpCenterContentComponent, data: { path: '/help-center/" + routingArray[i].toLowerCase() + "' }, canActivate: [LoggedInGuard] },\r\n";

    }
    appRoutingModule += "  { path: 'login', component: LoginComponent },\r\n  { path: 'logout', component: LogoutComponent }\r\n];"+
      "\r\n\r\n@NgModule({\r\n  imports: [RouterModule.forRoot(\r\n    routes,\r\n    { enableTracing: true }\r\n  )],\r\n  exports: [RouterModule]\r\n})\r\nexport class AppRoutingModule { }\r\n";

    try {
      file = outputDir + '/app-routing.module.ts';
      fs.writeFileSync(file, appRoutingModule);
    }
    catch (e) {
      console.log(e);
    }
  }

  function getListOfMDFiles(dir) {
    let list;

    try {
      list = fs
        .readdirSync(dir)
        .filter(function (file) {
          return path.extname(file) === '.md';
        });
    }
    catch (e) {
      console.log(e);
    }
    return list;
  }

  function writeHtmlTemplate(name, content, outputDir) {
    let file;
    try {
      file = outputDir + '/' + name.toLowerCase() + '.html';
      fs.writeFileSync(file, content);
    }
    catch (e) {
      console.log(e);
    }
  }

  function createNavigationContent(order, folderPath, folderName) {
    let list, newContent, mainReplaceContent, subReplaceContent;
    list = order;
    if (navigationContent.length === 0) {
      navigationContent = '<mat-nav-list color="primary">\n';
      navigationContent += '<a class="main-menu-item" mat-list-item href="https://spp-monitoringservice-#{EnvironmentName}#.azurewebsites.net/help/assets/help/assets/HelpCenter.pdf" target="_blank" type="appliction/octet-stream">Help as PDF</a>' 
      for (let i = 0; i < list.length; i++) {
        if (list[i].length > 0) {
          navigationContent += '<a style="cursor: pointer" class="main-menu-item" mat-list-item  [routerLink]="[\'/' + list[i].toLowerCase() + '\']">' + decodeURI(list[i].replace(/-/g, " ")) + '</a>\n';
          helpRoutes.push(list[i]);
        }
      }

      navigationContent += '</mat-nav-list>\n';
    }
    else {
      newContent = '<mat-expansion-panel>\n<mat-expansion-panel-header [expandedHeight]=" \'48px\'" [routerLink]="[\'/' + folderPath.toLowerCase() + '\']">\n<mat-panel-title>' +
      decodeURI(folderName.replace(/-/g, " ")) +
        '</mat-panel-title>\n</mat-expansion-panel-header>\n';
      for (let i = 0; i < list.length; i++) {
        newContent += '<a mat-list-item style="cursor: pointer" class="sub-menu-item" [routerLink]="[\'/' + folderPath.toLowerCase() + '/' + list[i].toLowerCase() + '\']">' + decodeURI(list[i].replace(/-/g, " ")) + '</a>\n';
        helpRoutes.push(folderPath + '/' + list[i]);
      }
      newContent += '</mat-expansion-panel>';
      decodeURI(folderName.replace(/-/g, " ")).toLowerCase()
      mainReplaceContent = '<a style="cursor: pointer" class="main-menu-item" mat-list-item  [routerLink]="[\'/' + folderPath.toLowerCase() + '\']">' + decodeURI(folderName.replace(/-/g, " ")) + '</a>';
      subReplaceContent = '<a mat-list-item style="cursor: pointer" class="sub-menu-item" [routerLink]="[\'/' + folderPath.toLowerCase() + '\']">' + decodeURI(folderName.replace(/-/g, " ")) + '</a>';
      navigationContent = navigationContent.replace(mainReplaceContent, newContent);
      navigationContent = navigationContent.replace(subReplaceContent, newContent);
      return navigationContent;
    }

  }



  function convertMdToHtml(dir, outputDir) {
    let list, content, text, file, name, order;
    
    if (!fs.existsSync(outputDir)) {
      fs.mkdirSync(outputDir.toLowerCase());
    }
    try {
      list = getListOfMDFiles(dir);
      for (let i = 0; i < list.length; i++) {
        name = list[i].slice(0, -3);
        file = path.join(dir, list[i]);
        content = fs.readFileSync(file, 'utf8');

        content = handleImageLinks(content);
       
        addContentToPdf(dir, name, file, content);

        text = marked(content);
        text = handleTemplateLinks(text);
        writeHtmlTemplate(name, text, outputDir);
      }

    } catch (e) {
      console.log(e);
    }
    
    
    return;
  }
  
  // adds one wiki page to the pdfContent 
  function addContentToPdf(dir, name, file, content) {
    
    let order = readOrderFile(dir);
    if (order) {
        
        if (pdfOrder.length == 1) {
            pdfOrder = pdfOrder.concat(order);
        } else {
            if (JSON.stringify(order) != JSON.stringify(pdfOrder)) {
                // we need to insert the new order in the placeholder of the old global pdfOrder 

                var menuToken = dir.replace(/wiki\\Help-Center/ , "");
                menuToken = menuToken.replace(/\\/, "");
                
                // check if menutoken is in global pdfOrder
                // if yes insert new order in globalOrder
                                
                if (pdfOrder.includes(menuToken)) {
                     let found = pdfOrder.findIndex(element => element == menuToken)
                     
                     if (found != -1) {
                                            
                        var beforeO = pdfOrder.slice(0,found);
                        var afterO = pdfOrder.slice(found+1, pdfOrder.length);

                      
                        pdfOrder = beforeO; // pages before the current apge
                        pdfOrder = pdfOrder.concat(menuToken + "_"); // we need to keep the main page
                        pdfOrder =  pdfOrder.concat(order); // the sub pages
                        
                        pdfOrder = pdfOrder.concat(afterO); // the pages after the current page
                        
                     }
                     

                }

            }
        }


    } else {
      // add page without ordering
      pdfOrder = pdfOrder.concat(name);
    }
    
    //console.log('addPage:' + name + " Page Order:" + pdfOrder);
    console.log('addPage:' + name);
    pdfContent[name] = content;
    pdfContent[name + '_'] = content; // we need also to add the main page
    

    
  }



  // will be called at the end to generate the pdf
  function convertMdToPdf(content, outputDir) {
      
      
      var text = '';
      for (var pageNo = 0; pageNo < pdfOrder.length; pageNo++) {
         
         var pageName = pdfOrder[pageNo];
         console.log("converting to pdf pageName="+ pageName);
        
        var pageContent = content[pageName];
        
        
        // replace assets/help/assets with /src/help/assets/help/assets cause of the images

        pageContent = pageContent.replace(new RegExp("assets/help/assets", "gi"), "./src/help/assets/help/assets");
         
        
         pageContent = pageContent.concat('<div style="page-break-after: always;"></div>');
         pageContent = pageContent.concat("\r\n");
         
         //console.log(pageContent);
         
         text = text.concat(pageContent); 
      }

      
    
      console.log('convertToPdf:' + outputDir + "/assets/HelpCenter.pdf");
       
      markdownpdf(pdfOptions).from.string(text).to(outputDir + "/assets/HelpCenter.pdf", function () {
        console.log("converting MD to PDF Done")
      })


      return;
  }





  //read Directory recursive
  function readDirAndCreateHelpCenter(dir, outputDir, folderPath, folderName) {
    
    let file, order;
    convertMdToHtml(dir, outputDir);
    order = readOrderFile(dir);
    if (order) {
      createNavigationContent(order, folderPath, folderName);
    };

    const getDirectories = dir => fs.readdirSync(dir).filter(file => (fs.statSync(path.join(dir, file)).isDirectory() && !file.startsWith('.')));
    getDirectories(dir).forEach(function (directory) {
      folderPath = folderName.length > 0 && folderName !== 'Help-Center' ? folderName + '/' + directory : directory;
      readDirAndCreateHelpCenter(path.join(dir, directory), path.join(outputDir, directory), folderPath, directory);
    });
  };


  function readOrderFile(dir) {
    let file, order, text;
    try {
      file = path.join(dir, '/.order');
      text = fs.readFileSync(file, 'utf8');
      text = text.replace(new RegExp(("\\n"), "gi"), ";"); 
      text = text.replace(new RegExp(("\\r"), "gi"), "");
      order = text.split(";");
    }
    catch (e) {
      console.log('No .order file on path ' + dir);
    }
    return order;
  }

  /**
   * Copy all assets which are referenced in a md file into an assets folder
   * 
   */
  function copyFileInAssetFolder(filename) {
    let readStream = fs.createReadStream(path.join(argvImageDir, filename));
    readStream.once('error', (err) => {
      console.log(err);
    });

    readStream.pipe(fs.createWriteStream(path.join(argvOutputDir, '/assets', filename)));
  }

  function handleImageLinks(content) {

    let images;

    images = content.match(new RegExp("(?=\\.attachments\\/).*?(?=\\))", "gi")) || [];
    content = content.replace(new RegExp("(?:\\.attachments)", "gi"), imgSrcPath);

    images.forEach(function (image) {
      image = image.replace(new RegExp("(?:\\.attachments\\/)", "gi"), '');
      copyFileInAssetFolder(image);
    });

    return content;
  }

  function handleTemplateLinks(html) {
    // console.log('\n\n######################################');
    let hrefs, path, regexp;
    regexp = new RegExp('(?=href).*?(?=">)', "gi");
    hrefs = html.match(regexp) || [];
    hrefs.forEach(function (href) {
      let relativePath = 'href="/help';
      path = href.split('/');
      // console.log('handleTemplateLinks: ORIG: ' + href);

      var pos = path.indexOf('Help-Center');
      
      if (pos != -1) {

      
        for (let i = pos+1; i < path.length; i++) {
          //console.log('relPath:' + relativePath + ' /Pathi: ' + path[i].toLowerCase());
          relativePath += '/' + path[i].toLowerCase();
          //console.log('relPath: ' + relativePath);
        }
        // console.log('handleTemplateLinks: NEW:'  + relativePath);
        html = html.replace(href, relativePath);
    
      }
    
    
    });
    return html;
  }


})();
