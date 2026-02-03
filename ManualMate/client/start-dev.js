// Fix for paths with spaces
const path = require('path');
const fs = require('fs');

// Get absolute path to this directory
const currentDir = __dirname.replace(/\\/g, '/');

// Load crypto polyfill
const cryptoPolyfillPath = path.join(currentDir, 'crypto-polyfill.js');
if (fs.existsSync(cryptoPolyfillPath)) {
  require(cryptoPolyfillPath);
}

// Spawn vite
const { spawn } = require('child_process');
const vitePath = path.join(currentDir, 'node_modules', '.bin', 'vite');

// Use npx or direct node execution
const vite = spawn(process.platform === 'win32' ? 'npx.cmd' : 'npx', ['vite'], { 
  stdio: 'inherit', 
  shell: true,
  cwd: currentDir
});

vite.on('error', (err) => {
  console.error('Failed to start Vite:', err);
  process.exit(1);
});

process.on('SIGINT', () => {
  vite.kill();
  process.exit(0);
});
