'use strict';
var Type = function(){
	this.test = 1;
}

var Integer = function(){
	Type.call(this);
	this.internal = 5;
}
Integer.sayWho = function(name){
	var p = 'none ' + name + '';
	alert('test');
	return p;
}


var p = new Integer();
function test(cislo){
	var ret = cislo;
	return ret;
}

test(5);

