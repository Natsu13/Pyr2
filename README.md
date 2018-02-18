# Pyr2
New type of Pyr Language more optimized and writed in C# can be compiled to JS for now but in future other language would be supported

Example code:
```
import System;
import System.Type;
import System.Range;
import System.action;
import System.Generic.List;
import HTML.DOM;

[OnPageLoad]
function main(){
	document.title = "Test HTML DOM modelu";
	console.log(document.title);

	Div box = new Div("box", "Tady to je div");
	box.onClick = (e) => {
		console.log("Toto je DIV!");
	};
	Input input = new Input("name", "text", "Zadej nazev...");
	input.onClick = (e) => {
		console.log("Kliknul jsi na input!");
		console.log(e);
		console.log(this);
	};

	$.AppendBodyChild(box);
	$.AppendBodyChild(input);
}
```

for calling main after page is loaded you need to add Attribute on main function named `[OnPageLoad]`
other the main call instantly and don't wait for element in page
this example show you import and using lambda and adding stuff on page!
