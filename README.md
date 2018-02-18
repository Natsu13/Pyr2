# Pyr2
New type of Pyr Language more optimized and writed in C# can be compiled to JS for now but in future other language would be supported

Example code (Custom class with custom operators):
```
external function alert(string message) -> void;
external function js(string code) -> void;

external class console {
	function log(string text) -> void;
}

class StringIterator:Iterator {
	int index  = 0;
	string self;
	
	function StringIterator(string self) {
		this.self = self;
	}
	
	function next() -> string {
		return this.self[this.index++];
	}
	function hasNext() -> bool {
		if(this.index > this.self.length - 1){
			return false;
		}
		return true;
	}
}

function string.iterator() -> Iterator {
	return new StringIterator(this);
}

class Range: IIterable, Iterator {
	int start = 0;
	int step = 0;
	int end = 1;
	int current = 0;
	
	function Range(int start, int end, int step = 1) {
		this.start = start;
		this.end  = end;
		this.current = start;
		this.step = step;
	}
	
	function iterator() -> Iterator {
		return this;
	}
	
	function next() -> int {
		int ret = this.current;
		this.current = ret + this.step;
		return ret;
	}
	
	function hasNext() -> bool {
		if(this.current + this.step > this.end + this.step){
			return false;
		}
		return true;
	}
}

class Integer<T> {
	T internal;
	
	function Integer(T number){
		this.internal = number;
	}
	
	operator function plus(Integer sec) -> T {
		return new Integer<T>((this.internal is int) + (sec.internal is int));
	}
	operator function minus(Integer sec) -> T {
		return new Integer<T>((this.internal is int) - (sec.internal is int));
	}
	
	operator function equal(Integer sec) -> bool {
		if(this.internal == sec.internal)
		{
			return true;
		}
		return false;
	}
	operator function compareTo(Integer sec) -> int {
		return ((this.internal is int) - (sec.internal is int));
	}
}

function _test(){ console.log("test"); }

function main(){
	Integer<int> t = new Integer<int>(2);
	Integer<int> x = new Integer<int>(5);
	if(t < x){
		console.log("2 < 5");
	}
	
	lambda fnc = { (int a, int b) -> a + b };
	
	Integer<int>[10] pole = new Integer<int>[10](10);
	console.log(pole);
	console.log(pole[2]);
	
	q = t - x;
	console.log(q);
	
	console.log(fnc(2,5));
	
	console.log(5 + 2);
	
	string test = "test";
	
	for(string ch in test){
		console.log(ch);
	}
	for(int i in new Range(0,10,2)){
		console.log(i);
	}
}
```

js is external function defined in C# compiler
for calling main after page is loaded you need set Interpreter._WAITFORPAGELOAD = true (Default setting)
other the main call instantly and don't wait for element in page
