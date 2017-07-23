'use strict';
var Type = function(){
	this.test = 1;
}

var Integer = function(){
	Type.call(this);
	this.internal = 5;
}
Integer.prototype.sayWho = function(name){
	var p = 'none';
}


var p = new Integer();
function test(cislo){
	var ret = cislo;
	return ret;
}


