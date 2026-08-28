#!/usr/bin/env python3
import json
from . import run_preflight


def main():
    report = run_preflight()
    print(json.dumps(report.to_dict(), indent=2))


if __name__ == "__main__":
    main()
