# Pyr2
New type of Pyr Language more optimized and writed in C# can be compiled to JS for now but in future other language would be supported

Example code (Custom class with custom operators):
```
external function alert(string message) -> void;
external function js(string code) -> void;

external class console {
	function log(string text) -> void;
}

class Integer {
	int internal = 5;
	
	function Integer(int number){
		this.internal = number;
	}
	
	operator function plus(Integer sec) -> int {
		return new Integer(this.internal + sec.internal);
	}
	operator function equal(Integer sec) -> bool {
		if(this.internal == sec.internal)
		{
			return true;
		}
		return false;
	}
	operator function compareTo(Integer sec) -> int {
		return (this.internal - sec.internal);
	}
}

interface Iterator {
	function next() -> string;
	function hasNext() -> bool;
}

class StringIterator:Iterator {
	int index  = 0;
	string self;
	
	function StringIterator(string self) {
		this.self = self;
	}
	
	function next() -> string {
		return this.self[++this.index];
	}
	function hasNext() -> bool {
		if(this.index > this.self.length - 2){
			return false;
		}
		return true;
	}
}

function string.iterator() -> Iterator {
	return new StringIterator(this);
}

function main(){
	Integer t = new Integer(2);
	Integer x = new Integer(5);
	if(t < x){
		console.log("2 > 5");
	}
	string test = "test";
	console.log(test[0]);
	
	for(string ch in test){
		console.log(ch);
	}
}
```

js is external function defined in C# compiler
for calling main after page is loaded you need set Interpreter._WAITFORPAGELOAD = true (Default setting)
other the main call instantly and don't wait for element in page
