#!/usr/bin/env python3
"""
정식 릴리스 배포: MSBuild `/t:Build`(Rebuild 아님), Release, RatkinDevFeatures=false → ZIP → GitHub 태그 에셋 교체.
프리릴리스 기본(모드 A)과 컴파일 속성은 동일; 프리릴리스는 `/prerelease` 문서의 ZIP·`gh` 단계를 따른다.

저장소 루트: python tools/release.py
"""
from __future__ import annotations

import datetime as _dt
import json
import os
import re
import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
REPO = os.environ.get("RATKIN_RELEASE_REPO", "solaris0115/NewRatkin")
TAG = os.environ.get("RATKIN_RELEASE_TAG", "1.6")
OUT_DIR = ROOT / "Build" / "TestBuild"
CSPROJ_DIR = ROOT / "Project" / "1.6" / "Source"


def die(msg: str, code: int = 1) -> None:
    print(msg, file=sys.stderr)
    raise SystemExit(code)


def run(cmd: list[str], *, cwd: Path | None = None) -> None:
    print("+", " ".join(cmd))
    r = subprocess.run(cmd, cwd=cwd or ROOT)
    if r.returncode != 0:
        die(f"명령 실패 (exit {r.returncode})", r.returncode)


def run_out(cmd: list[str], *, cwd: Path | None = None) -> str:
    r = subprocess.run(
        cmd,
        cwd=cwd or ROOT,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    if r.returncode != 0:
        die((r.stderr or "").strip() or f"명령 실패: {cmd}")
    return r.stdout or ""


def need_exe(name: str) -> str:
    p = shutil.which(name)
    if not p:
        die(f"PATH에 없음: {name}")
    return p


def find_msbuild() -> Path:
    env = os.environ.get("MSBUILD")
    if env:
        p = Path(env)
        if p.is_file():
            return p
    pf86 = os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)")
    vswhere = Path(pf86) / "Microsoft Visual Studio" / "Installer" / "vswhere.exe"
    if vswhere.is_file():
        r = subprocess.run(
            [
                str(vswhere),
                "-latest",
                "-requires",
                "Microsoft.Component.MSBuild",
                "-find",
                r"MSBuild\**\Bin\MSBuild.exe",
            ],
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
        )
        line = (r.stdout or "").strip().splitlines()
        if r.returncode == 0 and line:
            p = Path(line[0].strip())
            if p.is_file():
                return p
    fb = Path(
        r"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    )
    if fb.is_file():
        return fb
    die("MSBuild를 찾을 수 없음. VS 설치 또는 MSBUILD 환경 변수로 exe 경로 지정.")


def next_version() -> str:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    pat = re.compile(r"Ratkin_TestBuild_\d+_(\d+)\.(\d+)\.(\d+)\.zip$", re.I)
    best: tuple[int, int, int] | None = None
    for z in OUT_DIR.glob("Ratkin_TestBuild_*.zip"):
        m = pat.match(z.name)
        if not m:
            continue
        t = (int(m.group(1)), int(m.group(2)), int(m.group(3)))
        if best is None or t > best:
            best = t
    if best is None:
        print("기존 TestBuild ZIP 없음 → 0.0.1", file=sys.stderr)
        return "0.0.1"
    a, b, c = best
    return f"{a}.{b}.{c + 1}"


def release_zip_asset_name() -> str | None:
    raw = run_out(["gh", "release", "view", TAG, "-R", REPO, "--json", "assets"])
    data = json.loads(raw)
    for a in data.get("assets") or []:
        n = a.get("name") or ""
        if not n.lower().endswith(".zip"):
            continue
        if n.startswith("Source code"):
            continue
        return n
    return None


def main() -> None:
    need_exe("gh")
    need_exe("7z")
    run_out(["gh", "auth", "status"])
    run_out(["gh", "release", "view", TAG, "-R", REPO])

    ver = os.environ.get("RATKIN_RELEASE_VERSION") or next_version()
    day = _dt.datetime.now().strftime("%y%m%d")
    built = OUT_DIR / f"Ratkin_TestBuild_{day}_{ver}.zip"

    msbuild = find_msbuild()
    run(
        [
            str(msbuild),
            str(CSPROJ_DIR / "NewRatkin.csproj"),
            "/t:Build",
            "/p:Configuration=Release",
            "/p:RatkinDevFeatures=false",
            "/restore:false",
        ],
        cwd=CSPROJ_DIR,
    )
    # NewRatkin.csproj OutputPath → Project/1.6/Assemblies (bin/Release 미사용)
    dll = CSPROJ_DIR.parent / "Assemblies" / "NewRatkin.dll"
    if not dll.is_file():
        die(f"빌드 산출 없음: {dll}")

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    run(
        [
            "7z",
            "a",
            "-tzip",
            str(built),
            "./Project/*",
            "-xr!Project/1.5",
        ],
    )

    upload = built
    target_name = release_zip_asset_name()
    if target_name:
        upload = OUT_DIR / target_name
        shutil.copy2(built, upload)
        print(f"GitHub 기존 ZIP 이름에 맞춤: {target_name}", file=sys.stderr)
    else:
        print(
            "기존 커스텀 .zip 에셋 없음 → 새 파일명으로 업로드",
            file=sys.stderr,
        )

    run(
        [
            "gh",
            "release",
            "upload",
            TAG,
            str(upload),
            "-R",
            REPO,
            "--clobber",
        ],
    )
    print(f"완료: {upload} → {REPO} {TAG}")


if __name__ == "__main__":
    os.chdir(ROOT)
    main()
