external class Array {}
static function Array.Clear(Array array, int start, int lenght) -> void { }

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
	
	function Find((T a) -> match) -> T { 
		for(int i in range(0, this._size)) {
            if(match(this._items[i])) {
                return this._items[i];
            }
        }
        return _default(T);
    }

	function FindLast((T a) -> match) -> T {            
        for(int i in range(this._size - 1, 0, -1)) {
            if(match(this._items[i])) {
                return this._items[i];
            }
        }
        return _default(T);
    }	
	
	function FindLastIndex(int startIndex, int count, (T a) -> match) -> T {
		int endIndex = startIndex - count;
        for(int i in range(startIndex, endIndex, -1)) {
			if(match(this._items[i])) return i;
        }
        return -1;
    }

	function FindLastIndex(int startIndex, (T a) -> match) -> T {
		return FindLastIndex(startIndex, startIndex + 1, match);
    }	
	
	function FindIndex(int startIndex, int count, (T a) -> match) -> int {
		int endIndex = startIndex + count;
        for(int i in range(startIndex, endIndex)) {
            if( match(this._items[i])) return i;
        }
        return -1;
	}
	
	function FindIndex(int startIndex, (T a) -> match) -> int {
		return FindIndex(startIndex, this._size - startIndex, match);
	}	
	
	function ForEach(){
		//TODO: dodelat
	}
	
	function ToArray() -> T[]{
		T[] array = new T[this._size];
		return array;
	}
	
	function TrueForAll((T a) -> match){
		for(int i in range(0, this._size)) {
            if(match(this._items[i]) != true) {
                return false;
            }
        }
        return true;
	}
	
	function IsCompatibleObject(object value) -> bool {
		return ((value is T) || (value == null && _default(T) == null));
	}
}