external function alert(string message) -> void;
external function js(string code) -> void;

class Type { 
	int test = 1;
}

class Integer : Type {
	int internal = 5;

	function sayWho(string name) {
		string p = 'none {$name}';
		js("alert('ahoj')");		
		return p;
	}
}

function main() -> int {
	Integer p = new Integer;
	p.sayWho("test");
	return 0;
}