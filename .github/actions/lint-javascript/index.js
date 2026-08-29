const { spawn } = require('node:child_process');

const solution = process.env.INPUT_SOLUTION || 'GitHubActionsPractice.sln';
const format = spawn('dotnet', ['format', solution, '--verify-no-changes'], {
  stdio: 'inherit',
});

format.on('error', (error) => {
  console.error(`Unable to run dotnet format: ${error.message}`);
  process.exitCode = 1;
});

format.on('exit', (code, signal) => {
  if (signal) {
    console.error(`dotnet format terminated by signal ${signal}`);
    process.exitCode = 1;
    return;
  }

  process.exitCode = code ?? 1;
});
