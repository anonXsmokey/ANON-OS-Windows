# ANON OS Windows — Developer Workflow

## Current status

The project is in the M0 shell prototype phase. The repository is being developed as a Windows-native project with a safe user-mode prototype before deeper OS integration.

## User involvement

The user does **not** need to perform work after every development step. Continue repository work autonomously when the required repository operations are available.

Ask the user to act only when a task genuinely requires access to their local machine, such as:

- installing Visual Studio / required Windows SDK tooling
- opening or running the prototype in their VirtualBox Windows VM
- providing build logs or screenshots
- testing hardware-specific behavior
- supplying a Windows ISO or other legally obtained installation media for local ISO construction
- making a GitHub account/security/permission change that cannot be performed through the connected repository tools

## First expected user action

The user will be notified when the M0 prototype is ready for local execution. At that point they should run the documented build/run procedure in the VirtualBox Windows VM and report the result.

## Working principle

Do not repeatedly ask for confirmation to continue normal repository work. Batch coherent implementation changes, keep documentation current, and surface only actionable blockers.

## Status vocabulary

Use these states in project reporting:

- SPECIFIED
- SCAFFOLDED
- IMPLEMENTED
- BUILT
- TESTED
- INTEGRATED
- RELEASE-READY

Never describe a stage as built or tested unless it has actually been built or tested.
