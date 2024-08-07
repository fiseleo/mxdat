from typing import Sequence, TypeVar

T = TypeVar("T")
U = TypeVar("U")


def at(arr: Sequence[T], idx: int, default: U = None) -> T | U:
    if idx < len(arr):
        return arr[idx]
    else:
        return default
