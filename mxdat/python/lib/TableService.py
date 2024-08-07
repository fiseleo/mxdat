# from _typeshed import StrPath
from io import BytesIO
from zipfile import ZipFile
import os
from .XXHashService import CalculateHash
from base64 import b64encode
from .MersenneTwister import MersenneTwister  # 替换为实际的模块导入语句
from typing import Union

class TableZipFile(ZipFile):
    def __init__(self, file: Union[str, BytesIO], name: str = None) -> None:
        super().__init__(file)
        mersenne_twister_instance = MersenneTwister(CalculateHash(name if not isinstance(file, str) else os.path.basename(file)))
        self.password = b64encode(mersenne_twister_instance.NextBytes(15))

    def open(self, name: str, mode: str = "r", force_zip64=False):
        return super(self.__class__, self).open(
            name, mode, pwd=self.password, force_zip64=force_zip64
        )