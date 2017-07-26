# Pyr2
New type of Pyr Language more optimized and writed in C# can be compiled to JS for not but in future other language would be supported

Example code:
```
external function alert(string message) -> void;
external function js(string code) -> void;

class Type { 
	int test = 1;
}

class Integer : Type {
	int internal = 5;

	function sayWho(string name) {
		string p = 'none {$name}';
		if(name + "oj" == "ahoj" && 1 == 1){
			js("alert('ahoj')");
		}else{
			p = "nope";
		}
		return p;
	}
}

function main() -> int {
	Integer p = new Integer;
	p.sayWho("ahoj");
	return 0;
}
```

js is external function defined in C# compiler
