from __future__ import annotations

import time
from pathlib import Path

import pytest

from buttonlauncher.gallery import GalleryCache


@pytest.fixture()
def image_dir(tmp_path: Path) -> Path:
    for idx in range(3):
        (tmp_path / f"image_{idx}.png").write_text(f"image-{idx}")
    return tmp_path


def _thumbnail_paths(cache: GalleryCache) -> list[Path]:
    return sorted(cache.thumbnail_dir.glob("*.png"))


def test_refresh_does_not_duplicate_items(image_dir: Path) -> None:
    cache = GalleryCache(image_dir)

    first_pass = cache.refresh()
    assert len(first_pass) == 3
    initial_thumbs = _thumbnail_paths(cache)
    assert len(initial_thumbs) == 3
    mtimes = {thumb: thumb.stat().st_mtime for thumb in initial_thumbs}

    second_pass = cache.refresh()
    assert len(second_pass) == 3
    second_thumbs = _thumbnail_paths(cache)
    assert second_thumbs == initial_thumbs
    # Thumbnails should not be recreated and therefore keep their mtimes
    assert {thumb: thumb.stat().st_mtime for thumb in second_thumbs} == mtimes


def test_refresh_updates_thumbnail_when_source_changes(image_dir: Path) -> None:
    cache = GalleryCache(image_dir)
    cache.refresh()
    thumb = next(iter(_thumbnail_paths(cache)))
    original = image_dir / "image_0.png"
    original.write_text("updated")
    time.sleep(1.0)

    cache.refresh()
    assert thumb.stat().st_mtime >= original.stat().st_mtime
