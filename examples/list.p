import System;
import System.Type;
import System.Range;
import System.action;

class List<T>: IIterable, Iterator {
	T[] _items;
	int capacity = 0;
	int _size = 0;
	int _version = 0;
	int current = 0;

	function List(int capacity = 0){
		this._items = new T[capacity];
		this.capacity = capacity;
	}

	operator function get(int index) -> T{
		return this._items[index];
	}

	operator function get(string index) -> bool{
		if(this.Find(x -> x == index) == index)
			return true;
		return false;
	}

	function iterator() -> Iterator {
		this.current = 0;
		return this;
	}

	function next() -> T {
		int ret = this.current;
		this.current = ret + 1;
		return this._items[ret];
	}

	function hasNext() -> bool {
		if(this.current + 1 > this._size){
			return false;
		}
		return true;
	}

	function Add(T item) -> void {
    	    if (this._size == this._items.Length)
	    {
		this.capacity++;
	    }
	    this._items[this._size++] = item;
	    this._version++;
  	}

	function Clear() -> void{
	    if (this._size > 0)
    	    {
      		Array.Clear(this._items, 0, this._size);
      		this._size = 0;
	    }
	    this._version++;
  	}

	function Find(Predicate<T> match) -> T {
		for(int i in Range.range(0, this._size - 1)) {
		      if(match(this._items[i])) {
				return this._items[i];
		      }
    		}
    		return _default<T>(T);
  	}

	function FindLast(Predicate<T> match) -> T {
    		for(int i in Range.range(this._size - 1, 0, -1)) {
      			if(match(this._items[i])) {
       				return this._items[i];
      			}
  		}
   		return _default<T>(T);
  	}

	function FindLastIndex(int startIndex, int count, Predicate<T> match) -> T {
		int endIndex = startIndex - count;
    		for(int i in Range.range(startIndex, endIndex, -1)) {
			if(match(this._items[i])) return i;
    		}
    		return -1;
  	}

	function FindLastIndex(int startIndex, Predicate<T> match) -> T {
		return this.FindLastIndex(startIndex, startIndex + 1, match);
  	}

	function FindIndex(int startIndex, int count, Predicate<T> match) -> int {
		int endIndex = startIndex + count;
      		for(int i in Range.range(startIndex, endIndex)) {
        		if(match(this._items[i])) return i;
      		}
   		return -1;
	}

	function FindIndex(int startIndex, Predicate<T> match) -> int {
		return this.FindIndex(startIndex, this._size - startIndex, match);
	}

	function ForEach(Action<T> acti){
		int version = this._version;

		for(int i in Range.range(0, this._size - 1)) {
	    if (version != this._version) {
	        //break;
	    }
	    acti(this._items[i]);
		}
	}

	function ToArray() -> T[]{
		T[] array = new T[this._size];
		return array;
	}

	function TrueForAll(Predicate<T> match){
		for(int i in Range.range(0, this._size - 1)) {
      			if(match(this._items[i]) != true) {
        			return false;
      			}
    		}
   		return true;
	}

	function IsCompatibleObject(object value) -> bool {
		return ((value is T) || (value == null && T == null));
	}
}
