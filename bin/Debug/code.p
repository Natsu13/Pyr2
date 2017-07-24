class Type { 
	int test = 1;
}
class Integer:Type {
	int internal = 5;

	static function sayWho(string name) {
		string p = 'none {$name}';
		return p;
	}
}
Integer p = new Integer;
function test(int cislo) -> void {
	int ret = cislo; 
	return ret;
}