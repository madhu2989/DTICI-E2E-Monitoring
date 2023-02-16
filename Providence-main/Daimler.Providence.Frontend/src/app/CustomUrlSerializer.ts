import { UrlSerializer, UrlTree, DefaultUrlSerializer } from '@angular/router';

export class CustomUrlSerializer implements UrlSerializer {
  parse(url: any): UrlTree {
    
    const dus = new DefaultUrlSerializer();
        
    var ret = dus.parse(url);
    
    return ret;

  }

  serialize(tree: UrlTree): any {
    const dus = new DefaultUrlSerializer();
    
    var url = dus.serialize(tree).replace(/%252F/gi, '%2F');
     

    return url;
  }
}