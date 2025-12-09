const crypto = require('crypto');

if (!globalThis.crypto) {
  globalThis.crypto = {};
}

if (!globalThis.crypto.getRandomValues) {
  globalThis.crypto.getRandomValues = function(arr) {
    const bytes = crypto.randomBytes(arr.length);
    for (let i = 0; i < arr.length; i++) {
      arr[i] = bytes[i];
    }
    return arr;
  };
}

if (!global.crypto) {
  global.crypto = globalThis.crypto;
}
