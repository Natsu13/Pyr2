# Pyr2
New type of Pyr Language more optimized and writed in C# can be compiled to JS for not but in future other language would be supported

Example code (How to control webpage):
```
external function alert(string message) -> void;
external function js(string code) -> void;

external interface Element {
	string innerHTML;
	string id;
	string className;
	
	function getAttribute(string name) -> int;
}

external class Doctype {
	string name;
	string internalSubset;
	string publicId;
	string systemId;
}

external class document {
	string title;
	string location;
	int width;
	int height;
	string characterSet;
	Doctype doctype;

	function getElementById(string id) -> Element;
	function getElementsByClassName(string name) -> Element;
}

function main() -> int {
	Element e = document.getElementById("test");
	e.innerHTML = "changed";
	
	string id = e.getAttribute("id");
	
	string pid = null;
	if(document.doctype != null){
		pid = document.doctype.publicId;
	}
	document.title = "Webpage in PYR2 {$pid?}";
	
	return 0;
}
```

js is external function defined in C# compiler
for calling main after page is loaded you need set Interpreter._WAITFORPAGELOAD = true (Default setting)
other the main call instantly and don't wait for element in page
