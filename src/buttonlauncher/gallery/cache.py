"""Thumbnail caching utilities for the gallery tool."""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from shutil import copyfile
from typing import Iterable, List

__all__ = ["GalleryCache", "GalleryItem"]


@dataclass(frozen=True)
class GalleryItem:
    """Represents a single original image and its cached thumbnail."""

    source: Path
    thumbnail: Path


class GalleryCache:
    """Maintain a deduplicated thumbnail cache for gallery images.

    The previous implementation generated new thumbnails every time the
    application started. As a consequence the gallery UI displayed the same
    image multiple times because the freshly created thumbnails were treated as
    new items.  The cache now inspects the source directory for candidate
    images, filters out existing thumbnails and only regenerates thumbnails when
    the original file changed.
    """

    def __init__(self, image_dir: Path, thumbnail_dir: Path | None = None) -> None:
        self.image_dir = Path(image_dir)
        self.thumbnail_dir = Path(thumbnail_dir or self.image_dir / "thumbnails")
        self.thumbnail_dir.mkdir(parents=True, exist_ok=True)
        self._items: List[GalleryItem] = []

    def refresh(self) -> List[GalleryItem]:
        """Rebuild the cache and return the list of gallery items."""

        self._items.clear()
        for path in self._iter_sources():
            thumb_path = self.thumbnail_dir / f"{path.stem}_thumb{path.suffix}"
            self._ensure_thumbnail(path, thumb_path)
            self._items.append(GalleryItem(source=path, thumbnail=thumb_path))
        return list(self._items)

    def _iter_sources(self) -> Iterable[Path]:
        """Yield original image files skipping existing thumbnails."""

        if not self.image_dir.exists():
            return []
        candidates = sorted(self.image_dir.glob("*"))
        for candidate in candidates:
            if candidate.is_dir():
                continue
            if candidate.suffix.lower() not in {".png", ".jpg", ".jpeg", ".gif", ".bmp"}:
                continue
            if self._looks_like_thumbnail(candidate):
                continue
            yield candidate

    @staticmethod
    def _looks_like_thumbnail(path: Path) -> bool:
        stem = path.stem.lower()
        return stem.endswith("_thumb") or stem.endswith("_thumbnail")

    def _ensure_thumbnail(self, source: Path, thumbnail: Path) -> None:
        """Create or update the thumbnail when required."""

        if not thumbnail.exists() or thumbnail.stat().st_mtime < source.stat().st_mtime:
            copyfile(source, thumbnail)

