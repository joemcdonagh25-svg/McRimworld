"""Command-line entry point: `python -m mcrimworld_workshop <command> [args]`.

See workshop/README.md for the full command reference.
"""

from __future__ import annotations

import argparse
import sys

from . import discovery, launcher
from .manifest import load_manifest


def cmd_env(args: argparse.Namespace) -> int:
    report = discovery.discover_environment()
    if args.json:
        print(discovery.report_as_json(report))
    else:
        print(discovery.render_report(report))
    return 0


def cmd_list(args: argparse.Namespace) -> int:
    paths = launcher.list_experience_paths()
    if not paths:
        print(f"No experiences found in {launcher.EXPERIENCES_DIR}")
        return 0
    for path in paths:
        result = load_manifest(path)
        if result.data:
            status = result.data.get("status", "specified")
            flag = "" if result.valid else "  [SCHEMA ERRORS]"
            print(f"{result.data.get('id', path.stem):<24} {result.data.get('name', ''):<32} status={status}{flag}")
        else:
            print(f"{path.stem:<24} <failed to parse>")
    return 0


def _resolve(experience_id: str):
    path = launcher.find_experience(experience_id)
    if path is None:
        print(f"No experience named '{experience_id}' in {launcher.EXPERIENCES_DIR}", file=sys.stderr)
        return None
    return load_manifest(path)


def cmd_inspect(args: argparse.Namespace) -> int:
    result = _resolve(args.experience_id)
    if result is None:
        return 1
    print(launcher.inspect_text(result))
    return 0


def cmd_validate(args: argparse.Namespace) -> int:
    result = _resolve(args.experience_id)
    if result is None:
        return 1
    print(launcher.inspect_text(result))
    print()
    print("Environment / mod requirements:")
    env = discovery.discover_environment()
    report = launcher.validate_experience(result, env)
    print(launcher.validation_text(report))
    return 0 if result.valid and report and report.ok else 1


def cmd_activate(args: argparse.Namespace) -> int:
    result = _resolve(args.experience_id)
    if result is None:
        return 1
    if not result.valid:
        print("Refusing to activate: manifest has schema errors. Run `validate` first.", file=sys.stderr)
        return 1
    env = discovery.discover_environment()
    record = launcher.activate_experience(result, env)
    print(f"Activated profile for '{record['experienceId']}'.")
    print(f"Backup of previous ModsConfig.xml: {record['backupPath'] or '(none existed)'}")
    print("Run `restore` to undo this.")
    return 0


def cmd_restore(args: argparse.Namespace) -> int:
    env = discovery.discover_environment()
    record = launcher.restore_profile(env)
    print(f"Restored ModsConfig.xml to the state before activating '{record['experienceId']}'.")
    return 0


def cmd_launch(args: argparse.Namespace) -> int:
    env = discovery.discover_environment()
    uri = launcher.launch_rimworld(env)
    print(f"Launching RimWorld via {uri}")
    print("This starts Steam's normal launch flow with whichever profile is currently active.")
    print("It does not select a scenario or resume a save for you (planned for M4).")
    return 0


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="mcrimworld-workshop", description="McRimworld Workshop launcher")
    sub = parser.add_subparsers(dest="command", required=True)

    p_env = sub.add_parser("env", help="Discover and report the local RimWorld/Steam/.NET environment")
    p_env.add_argument("--json", action="store_true", help="Emit machine-readable JSON")
    p_env.set_defaults(func=cmd_env)

    p_list = sub.add_parser("list", help="List available experiences")
    p_list.set_defaults(func=cmd_list)

    p_inspect = sub.add_parser("inspect", help="Show a single experience's manifest")
    p_inspect.add_argument("experience_id")
    p_inspect.set_defaults(func=cmd_inspect)

    p_validate = sub.add_parser("validate", help="Validate a manifest and its mod requirements against this machine")
    p_validate.add_argument("experience_id")
    p_validate.set_defaults(func=cmd_validate)

    p_activate = sub.add_parser("activate", help="Back up the current mod profile and activate this experience's")
    p_activate.add_argument("experience_id")
    p_activate.set_defaults(func=cmd_activate)

    p_restore = sub.add_parser("restore", help="Restore the mod profile that was active before the last activate")
    p_restore.set_defaults(func=cmd_restore)

    p_launch = sub.add_parser("launch", help="Start RimWorld via Steam with the currently active profile")
    p_launch.set_defaults(func=cmd_launch)

    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)
    try:
        return args.func(args)
    except (FileNotFoundError, RuntimeError, ValueError) as exc:
        print(f"Error: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
