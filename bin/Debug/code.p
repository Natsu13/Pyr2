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