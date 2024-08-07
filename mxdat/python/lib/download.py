import requests
from dataclasses import dataclass
from pathlib import Path
from tqdm.contrib.concurrent import process_map  # type: ignore
from typing import List
from .env import ENV
from .filepath import FP_RAW, write_json
from .file_info import format_bytes

FP_META = FP_RAW / "meta.json"


def download_from_url(url: str, save_to: Path):
    res = requests.get(url)
    res.raise_for_status()
    save_to.parent.mkdir(parents=True, exist_ok=True)
    if save_to.suffix == ".json":
        write_json(save_to, res.json())
    elif save_to.suffix == ".bytes":
        with open(save_to, "wb") as f:
            f.write(res.content)
    else:
        raise NotImplementedError()
    return save_to


@dataclass
class DownloadInfo:
    url: str
    output: Path
    bytes: int
    crc: int


def download(info: DownloadInfo):
    r = requests.get(info.url)
    info.output.parent.mkdir(parents=True, exist_ok=True)
    with open(info.output, "wb") as f:
        f.write(r.content)


def pretty_download(
    title: str, download_infos: List[DownloadInfo], need_permission: bool = False
):
    total_size = sum(map(lambda o: o.bytes, download_infos))
    if total_size == 0:
        print("Nothing to download.")
        return
    bs = format_bytes(total_size)

    message = f"Download {len(download_infos)} {title} files with total size {bs}"
    if need_permission:
        yn = input(f"{message}? (y/n) ")
        permission = yn.lower() in ["y", "yes"]
    else:
        print(f"{message}.")
        permission = True

    if permission:
        process_map(
            download, download_infos, desc=title, chunksize=1, disable=ENV.DISABLE_TQDM
        )
    else:
        print("Download is canceled.")


def discard_old_download_new(olds: List[DownloadInfo], news: List[DownloadInfo]):
    if len(olds) > 0:
        for info in olds:
            info.output.unlink()
        print(f"{len(olds)} stale files were deleted.")

    if len(news) > 0:
        pretty_download("Download files", news)
