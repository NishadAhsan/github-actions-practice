const { spawn } = require('node:child_process');
const core = require('@actions/core');

async function run() {
  try {
    const solution = core.getInput('solution', { required: true });
    core.info(`Verifying formatting for ${solution}`);

    await new Promise((resolve, reject) => {
      const format = spawn('dotnet', ['format', solution, '--verify-no-changes'], {
        stdio: 'inherit',
      });

      format.on('error', reject);
      format.on('exit', (code, signal) => {
        if (signal) {
          reject(new Error(`dotnet format terminated by signal ${signal}`));
        } else if (code !== 0) {
          reject(new Error(`dotnet format exited with code ${code ?? 1}`));
        } else {
          resolve();
        }
      });
    });

    core.info('Formatting verification passed.');
    core.setOutput('result', 'Formatting verification passed');
  } catch (error) {
    core.setFailed(error.message);
  }
}

run();
