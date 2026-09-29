"""Check or restore separately obtained resources without downloading anything."""
import argparse
import json
from pathlib import Path
import re
import shutil
import sys


def contained(root, relative):
    path = (root / relative).resolve()
    if not path.is_relative_to(root) or path == root:
        raise ValueError(f'Path escapes its resource root: {relative}')
    return path


def plan_restore(project, manifest, source=None):
    """Check presence and metadata GUIDs before copying only missing files."""
    pending, errors = [], []
    for entry in manifest['files']:
        for relative in (entry['path'], entry['path'] + '.meta'):
            target = contained(project, relative)
            if target.exists():
                candidate = target
            elif source is None:
                errors.append(f'Missing: {relative}')
                continue
            else:
                candidate = contained(source, relative)
            if not candidate.is_file():
                errors.append(f'Missing or non-file resource: {relative}')
                continue
            if relative.endswith('.meta'):
                matches = re.findall(r'^guid:\s*([a-fA-F0-9]{32})\s*$',
                                     candidate.read_text(encoding='utf-8-sig'), re.MULTILINE)
                if len(matches) != 1 or matches[0].lower() != entry['guid'].lower():
                    errors.append(f'Unexpected metadata GUID: {relative}')
                    continue
            if not target.exists():
                pending.append((candidate, target))
    if errors:
        raise ValueError('\n'.join(errors))
    return pending


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--from', dest='source', type=Path,
                        help='Authorized resource folder containing the documented Assets/ tree')
    parser.add_argument('--apply', action='store_true', help='Copy validated missing files')
    parser.add_argument('--check', action='store_true', help='Check installed files without writing')
    args = parser.parse_args(argv)
    if args.apply and (args.check or args.source is None):
        parser.error('--apply requires --from and cannot be combined with --check')
    if args.check and args.source is not None:
        parser.error('--check inspects installed files; omit --from')
    project = Path(__file__).resolve().parents[1]
    source = args.source.resolve() if args.source else None
    manifest = json.loads(Path(__file__).with_name('local-resources.json').read_text(encoding='utf-8'))
    try:
        pending = plan_restore(project, manifest, source)
        if args.apply:
            for candidate, target in pending:
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(candidate, target)
        action = 'Restored' if args.apply else 'Ready to restore'
        print(f'{action}: {len(pending)} files.' if source else 'All required local resources are present and metadata GUIDs match.')
        return 0
    except (ValueError, OSError) as error:
        print(error, file=sys.stderr)
        return 1


if __name__ == '__main__':
    raise SystemExit(main())
