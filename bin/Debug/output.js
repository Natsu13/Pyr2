var module = function (_){
  'use strict';
  var Type = function(){
  	this.test = 1;
  }
  
  var Integer = function(){
  	Type.call(this);
  	this.internal = 5;
  }
  Integer.prototype.sayWho = function(name){
    var p = 'none ' + name + '';
    alert('ahoj');
    return p;
  }
  
  
  function main(){
    var p = new Integer();
    p.sayWho('test');
    return 0;
  }
  
  
  _.Type = Type;
  _.Integer = Integer;
  _.main = main;

  main();

  return _;
}(typeof module === 'undefined' ? {} : module);
