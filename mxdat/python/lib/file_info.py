import os, binascii
from pathlib import Path
from typing import List


def get_file_size(file_path: Path):
    return os.stat(file_path).st_size


def get_file_crc(file_path: Path):
    with open(file_path, "rb") as f:
        buf = f.read()
    crc32 = binascii.crc32(buf)
    return crc32 & 0xFFFFFFFF


def get_folder_leaves(folder_path: Path):
    leaves: List[Path] = []
    for dirpath, _dirnames, filenames in os.walk(folder_path):
        for fn in filenames:
            leaves.append(Path(dirpath) / fn)
    return leaves


def format_bytes(bytes: int):
    if bytes < 1000:
        return f"{bytes}B"
    b = float(bytes)
    units = ["", "K", "M", "G", "T"]
    i = 0
    while b > 1000:
        i += 1
        b /= 1024
        if i == len(units) - 1:
            break
    return f"{round(b, 1)}{units[i]}B"
