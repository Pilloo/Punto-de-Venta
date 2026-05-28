// This will always be a number.
export function length() {
    return localStorage.length;
}

// This could be either the name of the key, or null.
export function key(index) {
    return localStorage.key(index);
}

// This could be either the value of the key, or null.
export function getItem(keyName) {
    const item = localStorage.getItem(keyName);

    try {
        return JSON.parse(item);
    } catch {
        return item;
    }
}

// This function return Undefined (could be called with InvokeVoidAsync)
export function setItem(keyName, value) {
    const valueToString = typeof value === object ? JSON.stringify(value) : value;

    return localStorage.setItem(keyName, valueToString);
}

// This function return Undefined (could be called with InvokeVoidAsync too)
export function removeItem(keyName) {
    return localStorage.removeItem(keyName);
}

// This function return Undefined (could be called with InvokeVoidAsync as well)
export function clear() {
    return localStorage.clear();
}