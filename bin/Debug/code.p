external function alert(string message) -> void;

class Type { 
	int test = 1;
}

class Integer : Type {
	int internal = 5;

	static function sayWho(string name) {
		string p = 'none {$name}';
		alert("test");		
		return p;
	}
}

Integer p = new Integer;

function test(int cislo) -> int {
	int ret = cislo; 
	return ret;
}
test(5);